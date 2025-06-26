using Newtonsoft.Json;

namespace ATM10Updater.Data
{
    public class ModMetadata
    {
        [JsonProperty("serverPackFileId")]
        public int ServerId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonIgnore]
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
