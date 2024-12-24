using Newtonsoft.Json;

namespace ATM10Updater.Data
{
    public class ModMetadataInfo
    {
        [JsonProperty("serverPackFileId")]
        public int ServerId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonIgnore]
        public string DownloadLink { get; set; } = string.Empty;
    }
}
