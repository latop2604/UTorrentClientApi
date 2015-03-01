using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UTorrent.Api.Data;

namespace UTorrent.Api
{
    public class Result
    {
        private readonly JObject _source;
        public JObject Source
        {
            get { return _source; }
        }

        public int Build { get; set; }
        public UTorrentException Error { get; set; }
        public int CacheId { get; set; }

        private readonly IList<Label> _label = new List<Label>();
        public IList<Label> Label
        {
            get { return _label; }
        }

        private readonly IList<string> _messages = new List<string>();
        public IList<string> Messages
        {
            get { return _messages; }
        }

        private readonly IList<Torrent> _torrents = new List<Torrent>();
        public IList<Torrent> Torrents
        {
            get { return _torrents; }
        }

        private readonly IList<Torrent> _changedTorrents = new List<Torrent>();
        public IList<Torrent> ChangedTorrents
        {
            get { return _changedTorrents; }
        }

        private readonly IList<RssFeed> _rssFeeds = new List<RssFeed>();
        public IList<RssFeed> RssFeeds
        {
            get { return _rssFeeds; }
        }

        private readonly IList<object> _rssFilters = new List<object>();
        public IList<object> RssFilters
        {
            get { return _rssFilters; }
        }

        private readonly IDictionary<string, FileCollection> _files = new Dictionary<string, FileCollection>();
        public IDictionary<string, FileCollection> Files
        {
            get { return _files; }
        }

        private readonly List<Setting> _settings = new List<Setting>();
        public List<Setting> Settings
        {
            get { return _settings; }
        }

        public Result(JObject source)
        {
            _source = source;
        }
    }
}
