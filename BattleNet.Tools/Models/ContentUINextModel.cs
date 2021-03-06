using Newtonsoft.Json;

namespace BattleNet.Tools.Models
{
    public class ContentUINextModel
    {
        public class Logo
        {
            [JsonProperty("altText")]
            public string AltText { get; set; }

            [JsonProperty("fileType")]
            public string FileType { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public class Root
        {
            [JsonProperty("titleId")]
            public int TitleId { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("productCode")]
            public string ProductCode { get; set; }

            [JsonProperty("logo")]
            public Logo Logo { get; set; }
        }

    }
}
