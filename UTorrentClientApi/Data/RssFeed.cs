using System;

namespace UTorrent.Api.Data
{
    public class RssFeed
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public Uri Url { get; set; }
        public bool CustomAlias { get; set; }
    }
}
