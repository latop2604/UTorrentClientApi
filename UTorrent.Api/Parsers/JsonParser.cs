using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UTorrent.Api.Data;

namespace UTorrent.Api
{
    public static class JsonParser
    {
        public static Result ParseJsonResult(string json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            JObject o = JObject.Parse(json);
            return ParseJsonResult(o);
        }

        public static Result ParseJsonResult(JObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Result result = new Result(obj);
            result.Build = ParseBase(obj, "build", j => j.Value<int>());
            result.Error = ParseBase(obj, "error", ParseError);
            result.CacheId = ParseBase(obj, "torrentc", j => j.Value<int>());
            result.Label.AddRangeIfNotNull(ParseBase(obj, "label", ParseLabels));
            result.RssFeeds.AddRangeIfNotNull(ParseBase(obj, "rssfeeds", ParseRssFeeds));
            result.Messages.AddRangeIfNotNull(ParseBase(obj, "messages", ParseMessages));
            result.Torrents.AddRangeIfNotNull(ParseBase(obj, "torrents", ParseTorrents));
            result.ChangedTorrents.AddRangeIfNotNull(ParseBase(obj, "torrentp", ParseTorrents));
            result.Files.AddRangeIfNotNull(ParseBase(obj, "files", ParseFiles));
            result.Settings.AddRangeIfNotNull(ParseBase(obj, "settings", ParseSettings));

            return result;
        }

        private static T ParseBase<T>(JObject obj, string token, Func<JToken, T> parser)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (token == null)
                throw new ArgumentNullException("token");
            if (parser == null)
                throw new ArgumentNullException("parser");

            var jsonToken = obj.SelectToken(token, false);
            if (jsonToken == null)
                return default(T);

            return parser(jsonToken);
        }

        public static UTorrentException ParseError(JToken obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var error = obj.Value<string>();

            if (string.IsNullOrEmpty(error))
                return null;

            return new UTorrentException(error);
        }

        public static IDictionary<string, FileCollection> ParseFiles(JToken obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var result = new Dictionary<string, FileCollection>();

            for (int i = 0; i + 1 < obj.Count(); i += 2)
            {
                var oKey = obj[i].Value<string>();
                if (oKey == null)
                    continue;

                string hash = oKey.ToUpperInvariant();
                var files = new List<Data.File>();

                var oValue = obj[i + 1];
                if (oValue == null)
                    continue;

                foreach (var jfile in oValue)
                {
                    Data.File file = new Data.File();
                    file.Name = jfile[0] != null ? jfile[0].Value<string>() : null;
                    file.Size = jfile[1] != null ? jfile[1].Value<long>() : 0;
                    file.Downloaded = jfile[2] != null ? jfile[2].Value<long>() : 0;
                    int priority = jfile[3] != null ? jfile[3].Value<int>() : 0;
                    if (priority <= 3 && priority >= 0)
                    {
                        file.Priority = (Priority)priority;
                    }
                    files.Add(file);
                }

                result.Add(hash, new FileCollection(files.OrderBy(f => f.Name)));
            }
            return result;
        }

        public static IList<Torrent> ParseTorrents(JToken obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var list = obj.Select(t => new Torrent
            {
                Hash = t[0].Value<string>().ToUpperInvariant(),
                Status = (Status)t[1].Value<int>(),
                Name = t[2].Value<string>(),
                Size = t[3].Value<long>(),
                Progress = t[4].Value<int>(),
                Downloaded = t[5].Value<long>(),
                Uploaded = t[6].Value<long>(),
                Ratio = t[7].Value<int>(),
                UploadSpeed = t[8].Value<int>(),
                DownloadSpeed = t[9].Value<int>(),
                Eta = t[10].Value<int>(),
                Label = t[11].Value<string>(),
                PeersConnected = t[12].Value<int>(),
                PeersInSwarm = t[13].Value<int>(),
                SeedsConnected = t[14].Value<int>(),
                SeedsInSwarm = t[15].Value<int>(),
                Availability = t[16].Value<int>(),
                TorrentQueueOrder = t[17].Value<int>(),
                Remaining = t[18].Value<long>(),
                AddedDate = (t.Count() <= 23) ? DateTime.MinValue : new DateTime(1970, 1, 1).AddSeconds(t[23].Value<int>()),
                Path = (t.Count() <= 26) ? "" : t[26].Value<string>(),
            }).ToList();

            return list;
        }

        private static IList<string> ParseMessages(JToken obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            List<string> result = obj.Select(l => l.ToString()).ToList();
            return result;
        }

        private static IList<RssFeed> ParseRssFeeds(JToken obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            List<RssFeed> result = obj.Select(l =>
            {
                var rssFedd = new RssFeed();
                rssFedd.Id = l[7].Value<int>();
                rssFedd.CustomAlias = l[2].Value<bool>();

                var urlAlias = l[6].Value<string>();
                var tabUrlAlias = urlAlias.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (tabUrlAlias.Length >= 2)
                {
                    rssFedd.Alias = tabUrlAlias[0];
                    rssFedd.Url = new Uri(tabUrlAlias[1]);
                }
                else
                {
                    rssFedd.Url = new Uri(tabUrlAlias[0]);
                }
                return rssFedd;
            }).ToList();
            return result;
        }
        private static IList<Label> ParseLabels(JToken obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            List<Label> result = obj.Select(l => new Label { Count = l[1].Value<int>(), Name = l[0].Value<string>() }).ToList();
            return result;
        }

        public static IList<Torrent> ParseTorrents(JObject obj)
        {
            if (obj == null)
                return new List<Torrent>();

            var oTorrents = obj.SelectToken("torrents");

            if (oTorrents == null || !oTorrents.Any())
            {
                return new List<Torrent>();
            }

            var list = oTorrents.Select(t => new Torrent
            {
                Hash = t[0].Value<string>().ToUpperInvariant(),
                Status = (Status)t[1].Value<int>(),
                Name = t[2].Value<string>(),
                Size = t[3].Value<long>(),
                Progress = t[4].Value<int>(),
                Downloaded = t[5].Value<long>(),
                Uploaded = t[6].Value<long>(),
                Ratio = t[7].Value<int>(),
                UploadSpeed = t[8].Value<int>(),
                DownloadSpeed = t[9].Value<int>(),
                Eta = t[10].Value<int>(),
                Label = t[11].Value<string>(),
                PeersConnected = t[12].Value<int>(),
                PeersInSwarm = t[13].Value<int>(),
                SeedsConnected = t[14].Value<int>(),
                SeedsInSwarm = t[15].Value<int>(),
                Availability = t[16].Value<int>(),
                TorrentQueueOrder = t[17].Value<int>(),
                Remaining = t[18].Value<long>(),
                AddedDate = (t.Count() <= 23) ? DateTime.MinValue : new DateTime(1970, 1, 1).AddSeconds(t[23].Value<int>()),
                Path = (t.Count() <= 26) ? "" : t[26].Value<string>(),
            }).ToList();

            return list;
        }

        public static IList<Data.File> ParseFiles(JObject obj)
        {
            if (obj == null)
                return new List<Data.File>();

            var ofiles = obj.SelectToken("files[1]");

            if (ofiles == null || !ofiles.Any())
            {
                return new List<Data.File>();
            }

            var files = ofiles.Select(f => new Data.File
            {
                Name = f[0].Value<string>(),
                Size = f[1].Value<long>(),
                Downloaded = f[2].Value<long>(),
                Priority = (Priority)f[3].Value<int>(),
            }).ToList();

            return files;
        }

        public static IEnumerable<Setting> ParseSettings(JToken obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            foreach (var jToken in obj)
            {
                Setting setting = new Setting();
                setting.Key = jToken[0].Value<string>();
                setting.Type = (SettingType)jToken[1].Value<int>();

                string value = jToken[2].Value<string>();
                switch (setting.Type)
                {
                    case SettingType.Integer:
                        int i;
                        if (int.TryParse(value, out i))
                        {
                            setting.Value = i;
                        }
                        else
                        {
                            setting.Value = value;
                        }
                        break;
                    case SettingType.Boolean:
                        setting.Value = value == "true";
                        break;
                    case SettingType.String:
                        setting.Value = value;
                        break;
                }

				if (jToken.Count() > 3)
				{
					var accessJtoken = jToken[3].SelectToken("access");
					setting.Access = accessJtoken.Value<string>();
				}

                yield return setting;
            }
        }
    }
}
