using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BattleNet.Tools.Models
{
    public class ContentBlogModel
    {
        public class ContentItem
        {
            [JsonProperty("contentId")] public string ContentId { get; set; }

            [JsonProperty("contentType")] public string ContentType { get; set; }

            [JsonProperty("context")] public SharedModels.Context Context { get; set; }

            [JsonProperty("properties")] public SharedModels.Properties Properties { get; set; }
        }

        public class Section
        {
            [JsonProperty("name")] public string Name { get; set; }

            [JsonProperty("contentItems")] public List<ContentItem> ContentItems { get; set; }
        }

        public class Pagination
        {
            [JsonProperty("offset")] public int Offset { get; set; }

            [JsonProperty("hasNextPage")] public bool HasNextPage { get; set; }

            [JsonProperty("limit")] public int Limit { get; set; }
        }

        public class Feed
        {
            [JsonProperty("contentItems")] public List<ContentItem> ContentItems { get; set; }

            [JsonProperty("context")] public SharedModels.Context Context { get; set; }

            [JsonProperty("pagination")] public Pagination Pagination { get; set; }
        }

        public class Root
        {
            [JsonProperty("layoutId")] public string LayoutId { get; set; }

            [JsonProperty("sections")] public List<Section> Sections { get; set; }

            [JsonProperty("context")] public SharedModels.Context Context { get; set; }

            [JsonProperty("feed")] public Feed Feed { get; set; }
        }
    }
}