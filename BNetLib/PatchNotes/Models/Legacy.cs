using System.Collections.Generic;
using Newtonsoft.Json;

namespace BNetLib.PatchNotes.Models
{
    public class Legacy
    {
        public class PatchNote
        {
            [JsonProperty("program")]
            public string Program { get; set; }

            [JsonProperty("locale")]
            public string Locale { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("patchVersion")]
            public string PatchVersion { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("detail")]
            public string Detail { get; set; }

            [JsonProperty("buildNumber")]
            public int BuildNumber { get; set; }

            [JsonProperty("publish")]
            public long Publish { get; set; }

            [JsonProperty("created")]
            public long Created { get; set; }

            [JsonProperty("updated")]
            public long Updated { get; set; }

            [JsonProperty("develop")]
            public bool Develop { get; set; }

            [JsonProperty("slug")]
            public string Slug { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }
        }

        public class Pagination
        {
            [JsonProperty("totalEntries")]
            public int TotalEntries { get; set; }

            [JsonProperty("totalPages")]
            public int TotalPages { get; set; }

            [JsonProperty("pageSize")]
            public int PageSize { get; set; }

            [JsonProperty("page")]
            public int Page { get; set; }
        }

        public class Root
        {
            [JsonProperty("patchNotes")]
            public List<PatchNote> PatchNotes { get; set; }

            [JsonProperty("pagination")]
            public Pagination Pagination { get; set; }
        }
    }
}