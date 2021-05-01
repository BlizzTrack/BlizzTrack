using Newtonsoft.Json;

namespace BattleNet.Tools.Models
{
    public class ContentUINextModel
    {
        public class Logo
        {
            [JsonProperty("imageUrl")]
            public string ImageUrl { get; set; }

            [JsonProperty("imageFileType")]
            public string ImageFileType { get; set; }

            [JsonProperty("imageAltText")]
            public string ImageAltText { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("fileType")]
            public string FileType { get; set; }

            [JsonProperty("altText")]
            public string AltText { get; set; }
        }

        public class Context
        {
            [JsonProperty("locale")]
            public string Locale { get; set; }
        }

        public class Root
        {
            [JsonProperty("cxpProductId")]
            public string CxpProductId { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("productCode")]
            public string ProductCode { get; set; }

            [JsonProperty("segment")]
            public string Segment { get; set; }

            [JsonProperty("logo")]
            public Logo Logo { get; set; }

            [JsonProperty("context")]
            public Context Context { get; set; }

            [JsonProperty("titleId")]
            public int? TitleId { get; set; }
        }

    }
}
