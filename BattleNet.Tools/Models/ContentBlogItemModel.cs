using System;
using Newtonsoft.Json;

namespace BattleNet.Tools.Models
{
    public class ContentBlogItemModel
    {
        [JsonProperty("contentId")] public string ContentId { get; set; }

        [JsonProperty("contentType")] public string ContentType { get; set; }

        [JsonProperty("context")] public SharedModels.Context Context { get; set; }

        [JsonProperty("properties")] public SharedModels.Properties Properties { get; set; }
    }
}