using System.Collections.Generic;
namespace UTorrent.Api.Data
{
   public class Props
    {
        public string Hash { get; set; }
        public string Trackers { get; set; }
        public int UlRate { get; set; }
        public int DlRate { get; set; }
        public int Superseed { get; set; }
        public int DHT { get; set; }
        public int PEX { get; set; }
        public int SeedOverride { get; set; }
        public int SeedRatio { get; set; }
        public int SeedTime { get; set; }
        public int UlSlots { get; set; }
    }
}
