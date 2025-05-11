using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DemonicBotWin.WinForms.Models
{
    public class DiscordServer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon")]
        public string IconUrl { get; set; }

        public string DisplayName => Name;

        public string InitialLetter => !string.IsNullOrEmpty(Name)
            ? Name.Substring(0, 1).ToUpper()
            : "?";
    }

    public class DiscordChannel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parentId")]
        public string ParentId { get; set; }

        public string DisplayName => $"#{Name}";
    }

    public class EmbedMessage
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; } = "#5865F2"; // Discord Blurple

        [JsonProperty("author")]
        public EmbedAuthor Author { get; set; }

        [JsonProperty("fields")]
        public List<EmbedField> Fields { get; set; } = new List<EmbedField>();

        [JsonProperty("image")]
        public EmbedImage Image { get; set; }

        [JsonProperty("thumbnail")]
        public EmbedThumbnail Thumbnail { get; set; }

        [JsonProperty("footer")]
        public EmbedFooter Footer { get; set; }

        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; } = DateTime.Now;
    }

    public class EmbedAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconURL")]
        public string IconUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class EmbedField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("inline")]
        public bool Inline { get; set; }
    }

    public class EmbedFooter
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("iconURL")]
        public string IconUrl { get; set; }
    }

    public class EmbedImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class EmbedThumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}