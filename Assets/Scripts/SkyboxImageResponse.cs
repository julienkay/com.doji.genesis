using System;
using Newtonsoft.Json;

namespace Genesis {
    public partial class SkyboxImageResponse {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("file_url")]
        public Uri FileUrl { get; set; }

        [JsonProperty("thumb_url")]
        public Uri ThumbUrl { get; set; }

        [JsonProperty("video_url")]
        public object VideoUrl { get; set; }

        [JsonProperty("progress")]
        public long Progress { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}