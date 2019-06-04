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
        public int Dht { get; set; }
        public int Pex { get; set; }
        public int Seed_Override { get; set; }
        public int Seed_Ratio { get; set; }
        public int Seed_Time { get; set; }
        public int UlSlots { get; set; }
    }
}
