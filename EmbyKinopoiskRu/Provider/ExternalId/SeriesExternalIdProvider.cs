using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Provider.ExternalId
{
    /// <summary>
    /// Add link on kinopoisk page to metadate of the Series
    /// </summary>
    public class SeriesExternalIdProvider : IExternalId
    {
        public string Name => Plugin.PluginName;

        public string Key => Plugin.PluginKey;

        /// <summary>
        /// Used on paget for link
        /// </summary>
        public string UrlFormatString => "https://www.kinopoisk.ru/series/{0}/";

        public bool Supports(IHasProviderIds item)
        {
            return item is Series;
        }
    }
}
