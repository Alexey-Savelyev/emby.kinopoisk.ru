using System.Collections.Generic;

namespace EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie
{
    public class KpVideos
    {
        public List<KpVideo> Trailers { get; set; } = new List<KpVideo>();
        public List<KpVideo> Teasers { get; set; } = new List<KpVideo>();
    }

}
