using System;

using MediaBrowser.Model.Plugins;

namespace EmbyKinopoiskRu.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        private static readonly string DefaultUnofficialToken = "0f162131-81c1-4979-b46c-3eea4263fb11";
        private static readonly string DefaultDevToken = "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7";
        public const string KINOPOISKDEV = "kinopoisk.dev";
        public const string KINOPOISKAPIUNOFFICIALTECH = "kinopoiskapiunofficial.tech";

        public string Token { get; set; } = string.Empty;
        public string ApiType { get; set; } = KINOPOISKAPIUNOFFICIALTECH;

        public string GetToken()
        {
            return !string.IsNullOrWhiteSpace(Token)
                ? Token
                : KINOPOISKAPIUNOFFICIALTECH.Equals(ApiType, StringComparison.Ordinal)
                    ? DefaultUnofficialToken
                    : DefaultDevToken;
        }

    }
}
