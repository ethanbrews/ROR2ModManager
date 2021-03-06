﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using ROR2ModManager.API.Thunderstore;
//
//    var thunderstoreManifest = ThunderstoreManifest.FromJson(jsonString);

namespace ROR2ModManager.API.Thunderstore
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ThunderstoreManifest
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version_number", NullValueHandling = NullValueHandling.Ignore)]
        public string VersionNumber { get; set; }

        [JsonProperty("website_url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri WebsiteUrl { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("dependencies", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Dependencies { get; set; }
    }

    public partial class ThunderstoreManifest
    {
        public static ThunderstoreManifest FromJson(string json) => JsonConvert.DeserializeObject<ThunderstoreManifest>(json, ROR2ModManager.API.Thunderstore.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ThunderstoreManifest self) => JsonConvert.SerializeObject(self, ROR2ModManager.API.Thunderstore.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
