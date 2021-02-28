using Newtonsoft.Json;
using System;

namespace mchua.dev.galleria
{
    public class ImageMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "metadata";

        [JsonProperty("imageId")]
        public string ImageId { get; set; }

        [JsonProperty("originalName")]
        public string OriginalName { get; set; }

        [JsonProperty("uploadTimestamp")]
        public DateTimeOffset UploadTimestamp { get; set; }
    }
}