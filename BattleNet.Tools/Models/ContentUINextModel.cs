using Newtonsoft.Json;

namespace BattleNet.Tools.Models
{
    public class ContentUINextModel
    {
        public class Root
        {
            [JsonProperty("cxpProductId")] public string CxpProductId { get; set; }

            [JsonProperty("title")] public string Title { get; set; }

            [JsonProperty("productCode")] public string ProductCode { get; set; }
            
            [JsonProperty("segment")] public string Segment { get; set; }

            [JsonProperty("logo")] public SharedModels.Logo Logo { get; set; }

            [JsonProperty("context")] public SharedModels.Context Context { get; set; }

            [JsonProperty("titleId")] public int? TitleId { get; set; }
        }
    }
}
