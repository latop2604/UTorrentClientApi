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
                _baseUrl = value == null ? null : value.ToString();
            }
        }

        private string _token;
        protected string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        private UrlAction _urlAction = UrlAction.Default;
        protected UrlAction UrlAction
        {
            get { return _urlAction; }
            set { _urlAction = value; }
        }

        private readonly IList<String> _torrentHash = new List<String>();
        protected IList<String> TorrentHash
        {
            get { return _torrentHash; }
        }

        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();
        protected Dictionary<string, string> Settings
        {
            get { return _settings; }
        }


        #region Input

        #endregion

        #region Output

        private bool _useCacheId = true;
        protected bool UseCacheId
        {
            get { return _useCacheId; }
            set { _useCacheId = value; }
        }

        private int _cacheId;
        protected int CacheId
        {
            get { return _cacheId; }
            set { _cacheId = value; }
        }

        private bool _hasTorrentList;
        protected bool HasTorrentList
        {
            get { return _hasTorrentList; }
            set { _hasTorrentList = value; }
        }

        #endregion


        #endregion

        #region Fluent Setter

        public BaseRequest<T> SetBaseUrl(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            BaseUrl = uri;
            return this;
        }

        public BaseRequest<T> SetAction(UrlAction urlAction)
        {
            if (urlAction == null)
                throw new ArgumentNullException("urlAction");

            if (!CheckAction(urlAction))
                throw new InvalidOperationException("urlAction invalide for this request");

            _urlAction = urlAction;
            return this;
        }

        public BaseRequest<T> SetTorrentHash(string hash)
        {
            Contract.Requires(hash != null);

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException("hash");

            hash = hash.Trim().ToUpperInvariant();
            if (!_torrentHash.Contains(hash))
                _torrentHash.Add(hash);
            return this;
        }

        public BaseRequest<T> SetTorrentHash(IEnumerable<string> hashs)
        {
            if (hashs == null)
                throw new ArgumentNullException("hashs");

            foreach (string hash in hashs)
            {
                if (string.IsNullOrWhiteSpace(hash))
                    throw new FormatException("Invalide hash format");

                var temphash = hash.Trim();
                if (!_torrentHash.Any(h => h.Equals(temphash, StringComparison.OrdinalIgnoreCase)))
                    _torrentHash.Add(temphash);
            }

            return this;
        }

        public BaseRequest<T> SetSetting(string key, string value)
        {
            Contract.Requires(key != null);
            Contract.Requires(value != null);

            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Invalid key", "key");

            key = key.Trim();
            _settings[key] = value;
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
            _hasTorrentList = value;
            return this;
        }

        public BaseRequest<T> SetCacheId(int cacheId)
        {
            _cacheId = cacheId;
            return this;
        }

        public BaseRequest<T> UnableCache()
        {
            _useCacheId = true;
            return this;
        }

        public BaseRequest<T> DisableCache()
        {
            _useCacheId = false;
            return this;
        }

        #endregion

        protected abstract bool CheckAction(UrlAction action);

        protected abstract void ToUrl(StringBuilder sb);

        public Uri ToUrl()
        {
            return ToUrl(_token);
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

            if (_urlAction != UrlAction.Default)
                sb.Append("&action=").Append(_urlAction.ActionValue.ToLowerInvariant());

            foreach (string torrentHash in _torrentHash)
                sb.Append("&hash=").Append(torrentHash);

            foreach (var setting in _settings)
            {
                sb.Append("&s=").Append(Uri.EscapeUriString(setting.Key));
                sb.Append("&v=").Append(setting.Value);
            }

            if (_hasTorrentList)
                sb.Append("&list=1");

            if (_useCacheId)
                sb.Append("&cid=").Append(_cacheId);

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
                    _cacheId = result.CacheId;

                var ret = new T { Result = result };
                OnProcessedRequest(ret);
                return ret;
            }
        }

    }
}
