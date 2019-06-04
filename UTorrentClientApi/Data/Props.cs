using System.Collections.Generic;
namespace UTorrent.Api.Data
{
    public class Props
    {
        public string Hash { get; set; }
        public string[] Trackers { get; set; }
        public int UlRate { get; set; }
        public int DlRate { get; set; }
        public PropsOption Superseed { get; set; }
        public PropsOption DHT { get; set; }
        public PropsOption PEX { get; set; }
        public PropsOption SeedOverride { get; set; }
        public int SeedRatio { get; set; }
        public int SeedTime { get; set; }
        public int UlSlots { get; set; }
    }

    public enum PropsOption
    {
        NotAllowed = -1,
        Disabled = 0,
        Enabled = 1,
    }
}
