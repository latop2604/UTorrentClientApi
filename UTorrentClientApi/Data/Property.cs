
namespace UTorrent.Api.Data
{
    public class Property
    {
        public string Hash { get; set; }
        public string Trackers { get; set; }
        public long UploadLimit { get; set; }
        public long DownloadLimit { get; set; }
        public PropertyPriority SuperSeed { get; set; }
        public PropertyPriority Dht { get; set; }
        public PropertyPriority Pex { get; set; }
        public PropertyPriority OverrideQueueing { get; set; }
        public long SeedRatio { get; set; }
        public long SeedingTime { get; set; }
        public int UploadSlots { get; set; }
    }
    public enum PropertyPriority
    {
        NotAllowed = -1,
        Disabled = 0,
        Enabled = 1,
    }
}
