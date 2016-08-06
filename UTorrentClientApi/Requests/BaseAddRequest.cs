using System;
using System.Net;
using System.Text;

using UTorrent.Api.Data;

namespace UTorrent.Api
{
    public abstract class BaseAddRequest<T> : BaseRequest<T> where T : BaseAddResponse, new()
    {
        #region Properties

        protected string TorrentPath { get; set; }

        #endregion

        #region Fluent Setter

        public BaseRequest<T> SetTorrentPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Argument path can't be null or empty");

            TorrentPath = path;

            return this;
        }

        #endregion

        protected override void ToUrl(StringBuilder sb)
        {
            if (sb == null)
                throw new ArgumentNullException("sb");

            if (TorrentPath != null)
                sb.Append("&path=").Append(WebUtility.UrlEncode(TorrentPath));
        }

        protected abstract Torrent FindAddedTorrent(T result);

        protected override void OnProcessedRequest(T result)
        {
            if (result == null || result.Error != null)
                return;

            result.AddedTorrent = FindAddedTorrent(result);
        }
    }
}
