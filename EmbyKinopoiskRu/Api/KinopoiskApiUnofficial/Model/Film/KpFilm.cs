using System.Collections.Generic;
using System.Diagnostics;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class KpFilm
    {
        public List<KpCountry> Countries { get; set; } = new List<KpCountry>();
        public List<KpGenre> Genres { get; set; } = new List<KpGenre>();
        public string ImdbId { get; set; }
        public long KinopoiskId { get; set; }
        public string NameOriginal { get; set; }
        public string NameRu { get; set; }
        public string PosterUrl { get; set; }
        public string PosterUrlPreview { get; set; }
        public float? RatingKinopoisk { get; set; }
        public string Type { get; set; } // FILM, VIDEO, TV_SERIES, MINI_SERIES, TV_SHOW
        public int? Year { get; set; }

        public string CoverUrl { get; set; } // backdrop
        public string Description { get; set; }
        public int? EndYear { get; set; }
        public int? FilmLength { get; set; }
        public string LogoUrl { get; set; }
        public string ProductionStatus { get; set; } // FILMING, PRE_PRODUCTION, COMPLETED, ANNOUNCED, UNKNOWN, POST_PRODUCTION
        public float? RatingFilmCritics { get; set; }
        public string RatingMpaa { get; set; }
        public bool? Serial { get; set; }
        public string Slogan { get; set; }
        public int? StartYear { get; set; }


        private string DebuggerDisplay => $"#{KinopoiskId}, {NameRu}";

    }
}
