using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BattleNet.Tools.Models
{
    public class ContentBlogModel
    {
        public class Context
        {
            [JsonProperty("locale")] public string Locale { get; set; }

            [JsonProperty("layoutTemplateId")] public string LayoutTemplateId { get; set; }

            [JsonProperty("includeFeed")] public bool IncludeFeed { get; set; }

            [JsonProperty("cxpProductId")] public string CxpProductId { get; set; }

            [JsonProperty("region")] public string Region { get; set; }
        }

        public class Logo
        {
            [JsonProperty("imageUrl")] public string ImageUrl { get; set; }

            [JsonProperty("imageFileType")] public string ImageFileType { get; set; }

            [JsonProperty("imageAltText")] public string ImageAltText { get; set; }

            [JsonProperty("url")] public string Url { get; set; }

            [JsonProperty("fileType")] public string FileType { get; set; }

            [JsonProperty("altText")] public string AltText { get; set; }
        }

        public class CxpProduct
        {
            [JsonProperty("cxpProductId")] public string CxpProductId { get; set; }

            [JsonProperty("title")] public string Title { get; set; }

            [JsonProperty("productCode")] public string ProductCode { get; set; }

            [JsonProperty("segment")] public string Segment { get; set; }

            [JsonProperty("logo")] public Logo Logo { get; set; }

            [JsonProperty("context")] public Context Context { get; set; }

            [JsonProperty("titleId")] public int? TitleId { get; set; }

            [JsonProperty("subTitleId")] public int? SubTitleId { get; set; }
        }

        public class StaticAsset
        {
            [JsonProperty("imageUrl")] public string ImageUrl { get; set; }

            [JsonProperty("imageFileType")] public string ImageFileType { get; set; }

            [JsonProperty("imageAltText")] public string ImageAltText { get; set; }

            [JsonProperty("url")] public string Url { get; set; }

            [JsonProperty("fileType")] public string FileType { get; set; }

            [JsonProperty("altText")] public string AltText { get; set; }
        }

        public class Properties
        {
            [JsonProperty("cxpProduct")] public CxpProduct CxpProduct { get; set; }

            [JsonProperty("staticAsset")] public StaticAsset StaticAsset { get; set; }

            [JsonProperty("summary")] public string Summary { get; set; }

            [JsonProperty("lastUpdated")] public DateTime LastUpdated { get; set; }

            [JsonProperty("author")] public string Author { get; set; }

            [JsonProperty("cxpProductId")] public string CxpProductId { get; set; }

            [JsonProperty("internalTitle")] public string InternalTitle { get; set; }

            [JsonProperty("title")] public string Title { get; set; }

            [JsonProperty("category")] public string Category { get; set; }

            [JsonProperty("callToAction")] public string CallToAction { get; set; }

            [JsonProperty("campaignId")] public string CampaignId { get; set; }

            [JsonProperty("url")] public string Url { get; set; }

            [JsonProperty("videoId")] public string VideoId { get; set; }

            [JsonProperty("listType")] public string ListType { get; set; }
        }

        public class ContentItem
        {
            [JsonProperty("contentId")] public string ContentId { get; set; }

            [JsonProperty("contentType")] public string ContentType { get; set; }

            [JsonProperty("context")] public Context Context { get; set; }

            [JsonProperty("properties")] public Properties Properties { get; set; }
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

            [JsonProperty("context")] public Context Context { get; set; }

            [JsonProperty("pagination")] public Pagination Pagination { get; set; }
        }

        public class Root
        {
            [JsonProperty("layoutId")] public string LayoutId { get; set; }

            [JsonProperty("sections")] public List<Section> Sections { get; set; }

            [JsonProperty("context")] public Context Context { get; set; }

            [JsonProperty("feed")] public Feed Feed { get; set; }
        }
    }
}