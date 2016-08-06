using System.Collections.Generic;

namespace UTorrent.Api.File
{
    public class TorrentFileProfile
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TorrentFileProfileCollection : List<TorrentFileProfile> { }
}
