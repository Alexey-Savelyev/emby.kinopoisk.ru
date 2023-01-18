using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season;
using EmbyKinopoiskRu.Helper;

using MediaBrowser.Common.Net;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial
{
    internal class KinopoiskUnofficialApi
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IActivityManager _activityManager;

        internal KinopoiskUnofficialApi(
            ILogger log
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager)
        {
            _httpClient = httpClient;
            _log = log;
            _jsonSerializer = jsonSerializer;
            _activityManager = activityManager;
        }

        internal async Task<KpFilm?> GetFilmById(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{movieId}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? null : _jsonSerializer.DeserializeFromString<KpFilm>(response);
        }
        internal async Task<KpSearchResult<KpFilm>> GetFilmsByName(string name, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films?keyword={name}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? new() : _jsonSerializer.DeserializeFromString<KpSearchResult<KpFilm>>(response);
        }
        internal async Task<List<KpFilmStaff>> GetStaffByFilmId(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/staff?filmId={movieId}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? new() : _jsonSerializer.DeserializeFromString<List<KpFilmStaff>>(response);
        }
        internal async Task<List<KpVideo>> GetVideosByFilmId(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{movieId}/videos";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? new() : _jsonSerializer.DeserializeFromString<List<KpVideo>>(response);
        }
        internal async Task<KpSearchResult<KpSeason>?> GetEpisodesBySeriesId(string seriesId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{seriesId}/seasons";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? null : _jsonSerializer.DeserializeFromString<KpSearchResult<KpSeason>>(response);
        }
        internal async Task<KpPerson?> GetPersonById(string personId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/staff/{personId}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? null : _jsonSerializer.DeserializeFromString<KpPerson>(response);
        }
        internal async Task<KpSearchResult<KpPerson>> GetPersonsByName(string name, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/persons?name={name}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? new() : _jsonSerializer.DeserializeFromString<KpSearchResult<KpPerson>>(response);
        }

        private async Task<string> SendRequest(string url, CancellationToken cancellationToken)
        {
            var token = Plugin.Instance?.Configuration.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                _log.Error($"The plugin {Plugin.PluginName} failed to start. Skip request");
                return string.Empty;
            }

            var response = await SendRequest(token, url, cancellationToken);
            if (string.IsNullOrEmpty(response))
            {
                return response;
            }
            if (response.Length == 3)
            {
                if (int.TryParse(response, out var statusCode))
                {
                    _log.Info($"Unable to parse response status code '{response}'");
                    return string.Empty;
                }
                else
                {
                    switch (statusCode)
                    {
                        case 401:
                            _log.Error($"Token is invalid: '{token}'");
                            AddToActivityLog($"Token '{token}' is invalid", "Token is invalid");
                            return string.Empty;
                        case 402:
                            _log.Warn("Request limit exceeded (either daily or total). Currently daily limit is 500 requests");
                            AddToActivityLog("Request limit exceeded (either daily or total). Currently daily limit is 500 requests", "Request limit exceeded");
                            return string.Empty;
                        case 404:
                            _log.Info($"Data not found for URL: {url}");
                            return string.Empty;
                        case 429:
                            _log.Info("Too many requests per second. Waiting 2 sec");
                            await Task.Delay(2000, cancellationToken);
                            return await SendRequest(token, url, cancellationToken);
                        default:
                            return string.Empty;
                    }
                }
            }
            return response;
        }
        private async Task<string> SendRequest(string token, string url, CancellationToken cancellationToken)
        {
            _log.Debug($"Sending request to {url}");
            HttpRequestOptions options = new()
            {
                CancellationToken = cancellationToken,
                Url = url,
                LogResponse = true,
                CacheLength = TimeSpan.FromHours(12),
                CacheMode = CacheMode.Unconditional,
                TimeoutMs = 180000,
            };
            options.Sanitation.SanitizeDefaultParams = false;
            options.RequestHeaders.Add("X-API-KEY", token);
            try
            {
                using HttpResponseInfo response = await _httpClient.SendAsync(options, "GET");
                var statusCode = (int)response.StatusCode;
                if ((int)response.StatusCode is >= 200 and < 300)
                {
                    using StreamReader reader = new(response.Content);
                    var result = await reader.ReadToEndAsync();
                    _log.Info($"Received response: '{result}'");
                    return result;
                }
                else
                {
                    _log.Error($"Received '{response.StatusCode}' from API");
                    return response.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorException($"Unable to fetch data from URL '{url}' due to {ex.Message}", ex);
                return string.Empty;
            }
        }
        private void AddToActivityLog(string overview, string shortOverview)
        {
            KpHelper.AddToActivityLog(_activityManager, overview, shortOverview);
        }
    }
}
