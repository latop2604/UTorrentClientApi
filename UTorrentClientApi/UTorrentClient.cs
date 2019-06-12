using System;
using System.Collections.Generic;
#if !PORTABLE
using System.Configuration;
#endif
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UTorrent.Api.Data;

namespace UTorrent.Api
{
    public class UTorrentClient
    {
        protected string BaseUrl;

        private readonly string _logOn;
        private readonly string _password;
        private Cookie _cookie;
        private string _token;
        private int _cacheId;

        protected string Token
        {
            get
            {
                if (_token == null)
                {
                    GetToken();
                }
                return _token;
            }
        }

        protected Uri TokenUrl => new Uri(BaseUrl + "token.html");

        /// <summary>
        /// If True, activate µTorrent client cache
        /// </summary>
        public bool UseCache { get; set; }

#if !PORTABLE
        /// <summary>
        /// Create new <see cref="UTorrentClient"/> instence with credential from configuration's AppSettings.
        /// </summary>
        public UTorrentClient()
        {
            UseCache = false;
            string logOn = ConfigurationManager.AppSettings["UTORRENT.LOGIN"];
            string password = ConfigurationManager.AppSettings["UTORRENT.PASSWORD"];

            _logOn = logOn;
            _password = password;

            if (logOn == null || password == null)
                throw new InvalidOperationException("UTORRENT.LOGIN and UTORRENT.PASSWORD configuration key not found.");

            InitBaseUrl("127.0.0.1", 8080);
        }
#endif

        /// <summary>
        /// Create new <see cref="UTorrentClient"/>
        /// </summary>
        public UTorrentClient(string logOn, string password)
        {
            UseCache = false;
            _logOn = logOn;
            _password = password;
            InitBaseUrl("127.0.0.1", 8080);
        }

        /// <summary>
        /// Create new <see cref="UTorrentClient"/>
        /// </summary>
        public UTorrentClient(string ip, int port, string logOn, string password)
            : this(logOn, password)
        {
            InitBaseUrl(ip, port);
        }

#if !PORTABLE
        /// <summary>
        /// Create new <see cref="UTorrentClient"/>
        /// </summary>
        public UTorrentClient(IPAddress ipAddress, int port, string logOn, string password)
            : this(logOn, password)
        {
            if (ipAddress == null)
                throw new ArgumentNullException(nameof(ipAddress));
            InitBaseUrl(ipAddress.ToString(), port);
        }

        /// <summary>
        /// Create new <see cref="UTorrentClient"/>
        /// </summary>
        public UTorrentClient(IPEndPoint ipEndPoint, string logOn, string password)
            : this(logOn, password)
        {
            if (ipEndPoint == null)
                throw new ArgumentNullException(nameof(ipEndPoint));
            InitBaseUrl(ipEndPoint.Address.ToString(), ipEndPoint.Port);
        }
#endif

        private void InitBaseUrl(string ip, int port)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));
            if (port <= 0 || port >= 65536)
                throw new ArgumentOutOfRangeException(nameof(port));
            if (!ip.StartsWith("http"))
            {
                BaseUrl = string.Format(System.Globalization.CultureInfo.InvariantCulture, "http://{0}:{1}/gui/", ip, port);
            }
            else
            {
                BaseUrl = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:{1}/gui/", ip, port);
            }
        }

        /// <summary>
        /// Get all torrents from µTorrent client without file's details
        /// </summary>
        /// <param name="fetchProps">Also get props for all torrents, default is false</param>
        /// <returns></returns>
        public Response GetList(bool fetchProps = false)
        {
            var response = GetList(_cacheId);
            if (fetchProps == true)
            {
                response.Result.Props.Clear();
                foreach (Torrent t in response.Result.Torrents)
                {
                    try { response.Result.Props.Add(GetProps(t.Hash)); }
                    catch { }
                }
            }
            return response;
        }

        /// <summary>
        /// Get all torrents from µTorrent client without file's details
        /// </summary>
        /// <returns></returns>
        public Task<Response> GetListAsync()
        {
            return GetListAsync(_cacheId);
        }

        /// <summary>
        /// Get all torrents from µTorrent client without file's details
        /// </summary>
        /// <param name="cacheId">The cache id</param>
        /// <returns></returns>
        public Response GetList(int cacheId)
        {
            var request = new Request();
            request.IncludeTorrentList(true);
            if (UseCache)
            {
                SetCacheId(request, cacheId);
            }
            return ProcessRequest(request);
        }

        /// <summary>
        /// Get all torrents from µTorrent client without file's details
        /// </summary>
        /// <param name="cacheId">The cache id</param>
        /// <returns></returns>
        public Task<Response> GetListAsync(int cacheId)
        {
            return Task.Factory.StartNew(() => GetList(cacheId));
        }

        /// <summary>
        /// Get all torrents and specific torrent's files from µTorrent client
        /// </summary>
        /// <param name="hash">The torrent id</param>
        /// <returns></returns>
        public Torrent GetTorrent(string hash)
        {
            Contract.Requires(hash != null);
            var request = new Request();
            request.SetAction(UrlAction.GetFiles);
            request.IncludeTorrentList(true);
            request.SetTorrentHash(hash);
            IList<Torrent> TempList = ProcessRequest(request).Result.Torrents;

            if (TempList.Count > 0)
                return (from v in TempList where v.Hash == hash select v).First();

            throw new TorrentNotFoundException("Torrent with the hash " + hash + " was not found.");
        }

        /// <summary>
        /// Get all props from specific torrent
        /// </summary>
        /// <param name="hash">The torrent id</param>
        /// <returns></returns>
        public Props GetProps(string hash)
        {
            Contract.Requires(hash != null);
            var request = new Request();
            request.SetAction(UrlAction.GetProps);
            request.IncludeTorrentList(true);
            request.SetTorrentHash(hash);
            List<Props> TempProps = ProcessRequest(request).Result.Props;

            if (TempProps.Count > 0)
                return (from v in TempProps where v.Hash == hash select v).First();

            throw new TorrentNotFoundException("Torrent with the hash " + hash + " was not found.");
        }

        /// <summary>
        /// Get torrent's files from µTorrent client
        /// </summary>
        /// <param name="hash">The torrent id</param>
        /// <returns></returns>
        public Task<Torrent> GetTorrentAsync(string hash)
        {
            return Task.Factory.StartNew(() => GetTorrent(hash));
        }

        /// <summary>
        /// Get torrent's files from µTorrent client
        /// </summary>
        /// <param name="hash">The torrent id</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Response GetFiles(string hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));

            var request = new Request();
            request.SetAction(UrlAction.GetFiles);
            request.SetTorrentHash(hash);
            //request.IncludeTorrentList(true);

            return ProcessRequest(request);
        }

        /// <summary>
        /// Get torrent's files from µTorrent client async
        /// </summary>
        /// <param name="hash">The torrent id</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task<Response> GetFilesAsync(string hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));
            return Task.Factory.StartNew(() => GetFiles(hash));
        }

        #region Command

        /// <summary>
        /// Start the torrent
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Response StartTorrent(string hash)
        {
            Contract.Requires(hash != null);
            return ActionTorrent(UrlAction.Start, hash);
        }

        /// <summary>
        /// Start the torrent
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Task<Response> StartTorrentAsync(string hash)
        {
            return Task.Factory.StartNew(() => StartTorrent(hash));
        }

        /// <summary>
        /// Start torrent list
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Response StartTorrent(IEnumerable<string> hashs)
        {
            return ActionTorrent(UrlAction.Start, hashs);
        }

        /// <summary>
        /// Start torrent list async
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Task<Response> StartTorrentAsync(IEnumerable<string> hashs)
        {
            return Task.Factory.StartNew(() => StartTorrent(hashs));
        }

        /// <summary>
        /// Stop the torrent
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Response StopTorrent(string hash)
        {
            Contract.Requires(hash != null);
            return ActionTorrent(UrlAction.Stop, hash);
        }

        /// <summary>
        /// Stop the torrent async
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Task<Response> StopTorrentAsync(string hash)
        {
            return Task.Factory.StartNew(() => StopTorrent(hash));
        }

        /// <summary>
        /// Stop torrent list
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Response StopTorrent(IEnumerable<string> hashs)
        {
            return ActionTorrent(UrlAction.Stop, hashs);
        }

        /// <summary>
        /// Stop torrent list async
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Task<Response> StopTorrentAsync(IEnumerable<string> hashs)
        {
            return Task.Factory.StartNew(() => StopTorrent(hashs));
        }

        /// <summary>
        /// Pause the torrent
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Response PauseTorrent(string hash)
        {
            Contract.Requires(hash != null);
            return ActionTorrent(UrlAction.Pause, hash);
        }

        /// <summary>
        /// Pause the torrent async
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Task<Response> PauseTorrentAsync(string hash)
        {
            return Task.Factory.StartNew(() => PauseTorrent(hash));
        }

        /// <summary>
        /// Pause torrent list
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Response PauseTorrent(IEnumerable<string> hashs)
        {
            return ActionTorrent(UrlAction.Pause, hashs);
        }

        /// <summary>
        /// Pause torrent list async
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Task<Response> PauseTorrentAsync(IEnumerable<string> hashs)
        {
            return Task.Factory.StartNew(() => PauseTorrent(hashs));
        }

        /// <summary>
        /// Deletes the torrent and the data
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Response DeleteTorrent(string hash)
        {
            Contract.Requires(hash != null);
            return ActionTorrent(UrlAction.RemoveDataTorrent, hash);
        }

        /// <summary>
        /// Deletes the torrent and removes the data async
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Task<Response> DeleteTorrentAsync(string hash)
        {
            return Task.Factory.StartNew(() => DeleteTorrent(hash));
        }

        /// <summary>
        /// Deletes torrent list and removes the data
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Response DeleteTorrent(IEnumerable<string> hashs)
        {
            return ActionTorrent(UrlAction.RemoveDataTorrent, hashs);
        }

        /// <summary>
        /// Deletes torrent list and removes the data async
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Task<Response> DeleteTorrentAsync(IEnumerable<string> hashs)
        {
            return Task.Factory.StartNew(() => DeleteTorrent(hashs));
        }

        /// <summary>
        /// Recheck the torrent
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Response RecheckTorrent(string hash)
        {
            Contract.Requires(hash != null);
            return ActionTorrent(UrlAction.Recheck, hash);
        }

        /// <summary>
        /// Recheck the torrent async
        /// </summary>
        /// <param name="hash">The torrent hash</param>
        /// <returns></returns>
        public Task<Response> RecheckTorrentAsync(string hash)
        {
            return Task.Factory.StartNew(() => RecheckTorrent(hash));
        }

        /// <summary>
        /// Recheck torrent list
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Response RecheckTorrent(IEnumerable<string> hashs)
        {
            return ActionTorrent(UrlAction.Recheck, hashs);
        }

        /// <summary>
        /// Recheck torrent list async
        /// </summary>
        /// <param name="hashs">Torrents hash</param>
        /// <returns></returns>
        public Task<Response> RecheckTorrentAsync(IEnumerable<string> hashs)
        {
            return Task.Factory.StartNew(() => RecheckTorrent(hashs));
        }

        /// <summary>
        /// Removes (but does not delete) the torrent from the client
        /// </summary>
        public Response Remove(string hash)
        {
            Contract.Requires(hash != null);

            return ActionTorrent(UrlAction.Remove, hash);
        }

        /// <summary>
        /// Removes (but does not delete) the torrent from the client
        /// </summary>
        public Task<Response> RemoveAsync(string hash)
        {
            return Task.Factory.StartNew(() => Remove(hash));
        }

        /// <summary>
        /// Removes (but does not delete) the torrent from the client
        /// </summary>
        public Response Remove(IEnumerable<string> hashs)
        {
            Contract.Requires(hashs != null);

            return ActionTorrent(UrlAction.Remove, hashs);
        }

        /// <summary>
        /// Removes (but does not delete) the torrent from the client
        /// </summary>
        public Task<Response> RemoveAsync(IEnumerable<string> hashs)
        {
            return Task.Factory.StartNew(() => Remove(hashs));
        }

        /// <summary>
        /// Removes the torrent from the client (but does not delete the torrent file) and deletes the downloaded data
        /// </summary>
        public Response RemoveData(string hash)
        {
            Contract.Requires(hash != null);

            return ActionTorrent(UrlAction.RemoveData, hash);
        }

        /// <summary>
        /// Removes the torrent from the client (but does not delete the torrent file) and deletes the downloaded data
        /// </summary>
        public Task<Response> RemoveDataAsync(string hash)
        {
            return Task.Factory.StartNew(() => RemoveData(hash));
        }

        /// <summary>
        /// Removes the torrent from the client (but does not delete the torrent file) and deletes the downloaded data
        /// </summary>
        public Response RemoveData(IEnumerable<string> hashs)
        {
            Contract.Requires(hashs != null);

            return ActionTorrent(UrlAction.RemoveData, hashs);
        }

        /// <summary>
        /// Removes the torrent from the client (but does not delete the torrent file) and deletes the downloaded data
        /// </summary>
        public Task<Response> RemoveDataAsync(IEnumerable<string> hashs)
        {
            return Task.Factory.StartNew(() => RemoveData(hashs));
        }

        /// <summary>
        /// Removes the torrent from the client and deletes torrent file, yet perserves the downloaded file(s)
        /// </summary>
        public Response RemoveTorrent(string hash)
        {
            Contract.Requires(hash != null);

            return ActionTorrent(UrlAction.RemoveTorrent, hash);
        }

        /// <summary>
        /// Removes the torrent from the client and deletes torrent file, yet perserves the downloaded file(s) async
        /// </summary>
        public Task<Response> RemoveTorrentAsync(string hash)
        {
            return Task.Factory.StartNew(() => RemoveTorrent(hash));
        }

        /// <summary>
        /// Removes the torrent from the client and deletes torrent file, yet perserves the downloaded file(s)
        /// </summary>
        public Response RemoveTorrent(IEnumerable<string> hashs)
        {
            Contract.Requires(hashs != null);

            return ActionTorrent(UrlAction.RemoveTorrent, hashs);
        }

        /// <summary>
        /// Removes the torrent from the client and deletes torrent file, yet perserves the downloaded file(s) async
        /// </summary>
        public Task<Response> RemoveTorrentAsync(IEnumerable<string> hashs)
        {
            return Task.Factory.StartNew(() => RemoveTorrent(hashs));
        }

        private Response ActionTorrent(UrlAction urlAction, string hash)
        {
            Contract.Requires(hash != null);

            var request = new Request()
                .SetAction(urlAction)
                .IncludeTorrentList(true)
                .SetTorrentHash(hash);

            return ProcessRequest(request);
        }

        private Response ActionTorrent(UrlAction urlAction, IEnumerable<string> hashs)
        {
            var request = new Request()
                .SetAction(urlAction)
                .IncludeTorrentList(true)
                .SetTorrentHash(hashs);

            return ProcessRequest(request);
        }


        #endregion

        #region New Torrent

        /// <summary>
        /// Send torrent url to the µTorrent client
        /// </summary>
        /// <param name="uri">The torrent url</param>
        /// <param name="path">The torrent destination sub-folder</param>
        /// <returns></returns>
        public AddUrlResponse AddUrlTorrent(Uri uri, string path)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var request = new AddUrlRequest();
            request.SetUri(uri);
            request.SetAction(UrlAction.AddUrl);
            request.IncludeTorrentList(true);

            if (!string.IsNullOrEmpty(path))
            {
                request.SetTorrentPath(path);
            }
            return ProcessRequest(request);
        }

        /// <summary>
        /// Send torrent url to the µTorrent client
        /// </summary>
        /// <param name="uri">The torrent url</param>
        /// <param name="path">The torrent destination sub-folder</param>
        /// <returns></returns>
        public Task<AddUrlResponse> AddUrlTorrentAsync(Uri uri, string path)
        {
            return Task.Factory.StartNew(() => AddUrlTorrent(uri, path));
        }

        /// <summary>
        /// Send torrent url to the µTorrent client
        /// </summary>
        /// <param name="uri">The torrent url</param>
        /// <returns></returns>
        public AddUrlResponse AddUrlTorrent(string uri)
        {
            Contract.Requires(uri != null);

            return AddUrlTorrent(new Uri(uri), null);
        }

        /// <summary>
        /// Send torrent url to the µTorrent client
        /// </summary>
        /// <param name="uri">The torrent url</param>
        /// <param name="path">The torrent destination sub-folder</param>
        /// <returns></returns>
        public AddUrlResponse AddUrlTorrent(string uri, string path)
        {
            Contract.Requires(uri != null);

            return AddUrlTorrent(new Uri(uri), path);
        }

        /// <summary>
        /// Send torrent file to the µTorrent client
        /// </summary>
        /// <param name="inputStream">The torrent file stream</param>
        /// <returns></returns>
        public AddStreamResponse PostTorrent(System.IO.Stream inputStream)
        {
            Contract.Requires(inputStream != null);
            return PostTorrent(inputStream, null, 0);
        }

        /// <summary>
        /// Send torrent file to the µTorrent client
        /// </summary>
        /// <param name="inputStream">The torrent file stream</param>
        /// <param name="path">The torrent destination sub-folder</param>
        /// <returns></returns>
        public AddStreamResponse PostTorrent(System.IO.Stream inputStream, string path)
        {
            Contract.Requires(inputStream != null);
            return PostTorrent(inputStream, path, 0);
        }

        /// <summary>
        /// Send torrent file to the µTorrent client
        /// </summary>
        /// <param name="inputStream">The torrent file stream</param>
        /// <param name="path">The torrent destination sub-folder</param>
        /// <returns></returns>
        public Task<AddStreamResponse> PostTorrentAsync(System.IO.Stream inputStream, string path)
        {
            return Task.Factory.StartNew(() => PostTorrent(inputStream, path, 0));
        }

        /// <summary>
        /// Send torrent file to the µTorrent client
        /// </summary>
        /// <param name="inputStream">The torrent file stream</param>
        /// <param name="path">The torrent destination sub-folder</param>
        /// <param name="cacheId"></param>
        /// <returns></returns>
        public AddStreamResponse PostTorrent(System.IO.Stream inputStream, string path, int cacheId)
        {
            Contract.Requires(inputStream != null);

            GetToken();
            AddStreamResponse result;
            using (var request = new AddStreamRequest())
            {
                request.SetFile(inputStream);
                request.SetAction(UrlAction.AddFile);
                request.IncludeTorrentList(true);

                if (!string.IsNullOrEmpty(path))
                {
                    request.SetTorrentPath(path);
                }

                if (UseCache)
                {
                    SetCacheId(request, cacheId);
                }
                result = ProcessRequest(request);
            }
            return result;
        }

        #endregion

        #region Settings

        public Response GetSettings()
        {
            Request request = new Request();
            request.SetAction(UrlAction.GetSettings);
            return ProcessRequest(request);
        }

        public Task<Response> GetSettingsAsync()
        {
            return Task.Factory.StartNew(() => GetSettings());
        }

        public Response SetSetting(string key, string value)
        {
            Request request = new Request();
            request.SetAction(UrlAction.SetSetting);
            request.SetSetting(key, value);
            return ProcessRequest(request);
        }

        public Response SetSetting(string key, bool value)
        {
            Request request = new Request();
            request.SetAction(UrlAction.SetSetting);
            request.SetSetting(key, value);
            return ProcessRequest(request);
        }

        public Response SetSetting(string key, int value)
        {
            Request request = new Request();
            request.SetAction(UrlAction.SetSetting);
            request.SetSetting(key, value);
            return ProcessRequest(request);
        }

        public Response SetSetting(Dictionary<string, string> settings)
        {
            Request request = new Request();
            request.SetAction(UrlAction.SetSetting);
            foreach (var setting in settings)
            {
                request.SetSetting(setting.Key, setting.Value);
            }
            return ProcessRequest(request);
        }

        #endregion

        private void GetToken()
        {
            var wr = (HttpWebRequest)WebRequest.Create(TokenUrl);
            wr.Method = "GET";
            wr.Credentials = new NetworkCredential(_logOn, _password);

            try
            {
#if !PORTABLE
                using (var response = wr.GetResponse())
#else
                using (var response = wr.GetResponseAsync().Result)
#endif
                {
                    string result;
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        result = stream.ReadToEnd();
                    }

                    if (result == null)
                    {
                        throw new ServerUnavailableException("Unable to retreive WebUI token");
                    }

                    var cookies = response.Headers != null ? response.Headers["Set-Cookie"] : null;
                    if (cookies != null && cookies.Contains("GUID"))
                    {
                        var tab1 = cookies.Split(';');
                        if (tab1.Length >= 1)
                        {
                            var cookiestab = tab1[0].Split('=');
                            if (cookiestab.Length >= 2)
                            {
                                _cookie = new Cookie(cookiestab[0], cookiestab[1]) { Domain = BaseUrl };
                            }
                        }
                    }

                    int indexStart = result.IndexOf('<');
                    int indexEnd = result.IndexOf('>');
                    while (indexStart >= 0 && indexEnd >= 0 && indexStart <= indexEnd)
                    {
                        result = result.Remove(indexStart, indexEnd - indexStart + 1);

                        indexStart = result.IndexOf('<');
                        indexEnd = result.IndexOf('>');
                    }
                    _token = result;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse webResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new InvalidCredentialException();
                    }
                }

                throw new ServerUnavailableException("Unable to retreive WebUI token", ex);
            }
        }

        /// <summary>
        /// Available for test only
        /// </summary>
        /// <returns></returns>
        public string TestGetToken()
        {
            GetToken();
            return _token;
        }

        /// <summary>
        /// Send request to the µTorrent client
        /// </summary>
        /// <typeparam name="TResponse">The response type</typeparam>
        /// <param name="request">The request</param>
        /// <returns>The response data</returns>
        public TResponse ProcessRequest<TResponse>(BaseRequest<TResponse> request) where TResponse : BaseResponse, new()
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string token = Token;
            request.SetBaseUrl(new Uri(BaseUrl));
            var response = request.ProcessRequest(token, _logOn, _password, _cookie);

            if (response.Result != null && response.Result.CacheId != 0)
            {
                _cacheId = response.Result.CacheId;
            }

            return response;
        }

        /// <summary>
        /// Find Specific torrent with his files in an UTorrenre response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="hash">The torrent id</param>
        /// <returns>A <see cref="Torrent"/></returns>
        public static Torrent ConsolidateTorrent(BaseResponse response, string hash)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            if (response.Result == null)
                throw new ArgumentNullException(nameof(response), "response.Result is null");
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));

            if (response.Result.Torrents == null || response.Result.Torrents.Count == 0)
                return null;

            var torrent = response.Result.Torrents.FirstOrDefault(t => t.Hash.Equals(hash, StringComparison.OrdinalIgnoreCase));

            if (torrent != null && response.Result.Files != null && response.Result.Files.Keys.Any(k => k.Equals(hash, StringComparison.OrdinalIgnoreCase)))
            {
                torrent.Files.Clear();
                torrent.Files.AddRangeIfNotNull(response.Result.Files.First(pair => pair.Key.Equals(hash, StringComparison.OrdinalIgnoreCase)).Value);
            }

            return torrent;
        }

        private static void SetCacheId<T>(BaseRequest<T> request, int cacheId) where T : BaseResponse, new()
        {
            Contract.Requires(request != null);

            if (cacheId != 0)
            {
                request.UnableCache();
                request.SetCacheId(cacheId);
            }
        }
    }
}
