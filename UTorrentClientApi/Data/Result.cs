using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UTorrent.Api.Data;

namespace UTorrent.Api
{
    public class Result
    {
        public JObject Source { get; }

        public int Build { get; set; }
        public UTorrentException Error { get; set; }
        public int CacheId { get; set; }

        public IList<Label> Label { get; } = new List<Label>();

        public IList<string> Messages { get; } = new List<string>();

        public IList<Torrent> Torrents { get; } = new List<Torrent>();

        public IList<Torrent> ChangedTorrents { get; } = new List<Torrent>();

        public IList<RssFeed> RssFeeds { get; } = new List<RssFeed>();

        public IList<object> RssFilters { get; } = new List<object>();

        public IDictionary<string, FileCollection> Files { get; } = new Dictionary<string, FileCollection>();

        public List<Setting> Settings { get; } = new List<Setting>();

        public List<Props> Props { get; } = new List<Props>();

        public Result(JObject source)
        {
            this.Source = source;
        }
    }
}
