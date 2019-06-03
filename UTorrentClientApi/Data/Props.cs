using System.Collections.Generic;
namespace UTorrent.Api.Data
{
   public class Props
    {
        public string hash { get; set; }
        public string trackers { get; set; }
        public int ulrate { get; set; }
        public int dlrate { get; set; }
        public int superseed { get; set; }
        public int dht { get; set; }
        public int pex { get; set; }
        public int seed_override { get; set; }
        public int seed_ratio { get; set; }
        public int seed_time { get; set; }
        public int ulslots { get; set; }
    }
}
