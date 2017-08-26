using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace UTorrent.Api
{
    using System.Globalization;

    public abstract class BaseRequest<T> where T : BaseResponse, new()
    {
        #region Properties

        private string _baseUrl;
        protected Uri BaseUrl
        {
            get
            {
                if (_baseUrl == null)
                    return null;
                return new Uri(_baseUrl);
            }
            set
            {
                _baseUrl = value?.ToString();
            }
        }

        protected string Token { get; set; }

        protected UrlAction UrlAction { get; set; } = UrlAction.Default;

        protected IList<String> TorrentHash { get; } = new List<String>();

        protected Dictionary<string, string> Settings { get; } = new Dictionary<string, string>();

        #region Input

        #endregion

        #region Output

        protected bool UseCacheId { get; set; } = true;

        protected int CacheId { get; set; }

        protected bool HasTorrentList { get; set; }

        #endregion


        #endregion

        #region Fluent Setter

        public BaseRequest<T> SetBaseUrl(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            BaseUrl = uri;
            return this;
        }

        public BaseRequest<T> SetAction(UrlAction urlAction)
        {
            if (urlAction == null)
                throw new ArgumentNullException(nameof(urlAction));

            if (!CheckAction(urlAction))
                throw new InvalidOperationException(nameof(urlAction) + " invalide for this request");

            this.UrlAction = urlAction;
            return this;
        }

        public BaseRequest<T> SetTorrentHash(string hash)
        {
            Contract.Requires(hash != null);

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException(nameof(hash));

            hash = hash.Trim().ToUpperInvariant();
            if (!this.TorrentHash.Contains(hash))
                this.TorrentHash.Add(hash);
            return this;
        }

        public BaseRequest<T> SetTorrentHash(IEnumerable<string> hashs)
        {
            if (hashs == null)
                throw new ArgumentNullException(nameof(hashs));

            foreach (string hash in hashs)
            {
                if (string.IsNullOrWhiteSpace(hash))
                    throw new FormatException("Invalide hash format");

                var temphash = hash.Trim();
                if (!this.TorrentHash.Any(h => h.Equals(temphash, StringComparison.OrdinalIgnoreCase)))
                    this.TorrentHash.Add(temphash);
            }

            return this;
        }

        public BaseRequest<T> SetSetting(string key, string value)
        {
            Contract.Requires(key != null);
            Contract.Requires(value != null);

            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Invalid key", nameof(key));

            key = key.Trim();
            this.Settings[key] = value;
            return this;
        }

        public BaseRequest<T> SetSetting(string key, bool value)
        {
            return SetSetting(key, value ? "true" : "false");
        }

        public BaseRequest<T> SetSetting(string key, int value)
        {
            return SetSetting(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public BaseRequest<T> IncludeTorrentList(bool value)
        {
            this.HasTorrentList = value;
            return this;
        }

        public BaseRequest<T> SetCacheId(int cacheId)
        {
            this.CacheId = cacheId;
            return this;
        }

        public BaseRequest<T> UnableCache()
        {
            this.UseCacheId = true;
            return this;
        }

        public BaseRequest<T> DisableCache()
        {
            this.UseCacheId = false;
            return this;
        }

        #endregion

        protected abstract bool CheckAction(UrlAction action);

        protected abstract void ToUrl(StringBuilder sb);

        public Uri ToUrl()
        {
            return ToUrl(this.Token);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Lower case required")]
        public Uri ToUrl(string token)
        {
            if (token == null)
                throw new InvalidOperationException("Token can't be empty.");

            if (_baseUrl == null)
                throw new InvalidOperationException("BaseUrl not set.");

            StringBuilder sb = new StringBuilder();

            sb.Append(_baseUrl);
            sb.Append("?token=").Append(token);

            if (this.UrlAction != UrlAction.Default)
                sb.Append("&action=").Append(this.UrlAction.ActionValue.ToLowerInvariant());

            foreach (string torrentHash in this.TorrentHash)
                sb.Append("&hash=").Append(torrentHash);

            foreach (var setting in this.Settings)
            {
                sb.Append("&s=").Append(Uri.EscapeUriString(setting.Key));
                sb.Append("&v=").Append(setting.Value);
            }

            if (this.HasTorrentList)
                sb.Append("&list=1");

            if (this.UseCacheId)
                sb.Append("&cid=").Append(this.CacheId);

            ToUrl(sb);

            return new Uri(sb.ToString());
        }

        protected abstract void OnProcessingRequest(System.Net.HttpWebRequest wr);
        protected abstract void OnProcessedRequest(T result);

        public T ProcessRequest(string token, string logOn, string password, System.Net.Cookie cookie)
        {
            Uri uri = ToUrl(token);
            System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
            wr.Credentials = new System.Net.NetworkCredential(logOn, password);
            wr.CookieContainer = new System.Net.CookieContainer();
            if (cookie != null)
            {
                var cookiUri = cookie.Domain != null ? new Uri(cookie.Domain) : BaseUrl;
                wr.CookieContainer.SetCookies(uri, cookie.ToString());
            }

            OnProcessingRequest(wr);

#if !PORTABLE
            using (var response = wr.GetResponse())
#else
            using (var response = wr.GetResponseAsync().Result)
#endif
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                    throw new InvalidOperationException("Response stream is null");

                var sr = new System.IO.StreamReader(stream);
                var jsonResult = sr.ReadToEnd();

                var result = JsonParser.ParseJsonResult(jsonResult);

                if (result != null && result.CacheId != 0)
                    this.CacheId = result.CacheId;

                var ret = new T { Result = result };
                OnProcessedRequest(ret);
                return ret;
            }
        }

    }
}
