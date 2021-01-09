using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BNetLib.Models.Patchnotes
{
    public class Overwatch
    {
        public class UpdateChanges
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("dev_comment")]
            public string DevComment { get; set; }

            [JsonProperty("display_type")]
            public string DisplayType { get; set; }
        }

        public class Update
        {
            [JsonProperty("update")]
            public UpdateChanges UpdateChanges { get; set; }
        }

        public class GenericUpdate
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("dev_comment")]
            public string DevComment { get; set; }

            [JsonProperty("updates")]
            public List<Update> Updates { get; set; }
        }

        public class Metadata
        {
            [JsonProperty("asset_guid")]
            public string AssetGuid { get; set; }

            [JsonProperty("icon_guid")]
            public string IconGuid { get; set; }
        }

        public class Metadata2
        {
            [JsonProperty("asset_guid")]
            public string AssetGuid { get; set; }

            [JsonProperty("icon_guid")]
            public string IconGuid { get; set; }
        }

        public class AbilityChanges
        {
            [JsonProperty("ability_name")]
            public string AbilityName { get; set; }

            [JsonProperty("change_description")]
            public string ChangeDescription { get; set; }

            [JsonProperty("dev_comment")]
            public string DevComment { get; set; }

            [JsonProperty("metadata")]
            public Metadata2 Metadata { get; set; }
        }

        public class Ability
        {
            [JsonProperty("ability")]
            public AbilityChanges AbilityChanges { get; set; }
        }

        public class HeroChanges
        {
            [JsonProperty("hero_name")]
            public string HeroName { get; set; }

            [JsonProperty("change_description")]
            public string ChangeDescription { get; set; }

            [JsonProperty("dev_comment")]
            public string DevComment { get; set; }

            [JsonProperty("metadata")]
            public Metadata Metadata { get; set; }

            [JsonProperty("abilities")]
            public List<Ability> Abilities { get; set; }
        }

        public class Hero
        {
            [JsonProperty("hero")]
            public HeroChanges HeroChanges { get; set; }
        }

        public class HeroUpdate
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("dev_comment")]
            public string DevComment { get; set; }

            [JsonProperty("heroes")]
            public List<Hero> Heroes { get; set; }
        }

        public class Section
        {
            [JsonProperty("generic_update")]
            public GenericUpdate GenericUpdate { get; set; }

            [JsonProperty("hero_update")]
            public HeroUpdate HeroUpdate { get; set; }
        }

        public class PublishDetails
        {
            [JsonProperty("environment")]
            public string Environment { get; set; }

            [JsonProperty("locale")]
            public string Locale { get; set; }

            [JsonProperty("time")]
            public DateTime Time { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }
        }

        public class Entry
        {
            [JsonProperty("_version")]
            public int Version { get; set; }

            [JsonProperty("locale")]
            public string Locale { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("expired")]
            public bool Expired { get; set; }

            [JsonProperty("namespace")]
            public string Namespace { get; set; }

            [JsonProperty("post_date")]
            public DateTime PostDate { get; set; }

            [JsonProperty("sections")]
            public List<Section> Sections { get; set; }

            [JsonProperty("tags")]
            public List<object> Tags { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("updated_at")]
            public DateTime UpdatedAt { get; set; }

            [JsonProperty("updated_by")]
            public string UpdatedBy { get; set; }

            [JsonProperty("publish_details")]
            public PublishDetails PublishDetails { get; set; }
        }

        public class Root
        {
            [JsonProperty("entries")]
            public List<Entry> Entries { get; set; }
        }
    }
}