using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UTorrent.Api.File.Bencoding;

namespace UTorrent.Api.File
{
    public class TorrentInfo
    {
        [ContractInvariantMethod]
        void Invariants()
        {
            Contract.Invariant(this.Files != null);
        }

        public string Hash { get; set; }
        public string Announce { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string Encoding { get; set; }
        public String Name { get; set; }
        public bool Private { get; set; }
        public string Pieces { get; set; }
        public long PieceLength { get; set; }

        public IList<TorrentFileInfo> Files { get; } = new List<TorrentFileInfo>();

        public static TorrentInfo Parse(BDictionary dictionary)
        {
            Contract.Requires(dictionary != null);

            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            var torrent = new TorrentInfo();

            TorrentFileInfo singleFile = new TorrentFileInfo();
            bool isSingleFile = false;

            foreach (var item in dictionary)
            {
                if (item.Key == null)
                    continue;

                if (item.Key.Value == "announce")
                {
                    if (item.Value != null)
                    {
                        torrent.Announce = item.Value.ToString();
                    }
                }
                else if (item.Key.Value == "created by")
                {
                    if (item.Value != null)
                    {
                        torrent.CreatedBy = item.Value.ToString();
                    }
                }
                else if (item.Key.Value == "creation date")
                {
                    if (item.Value is BInteger integer)
                    {
                        torrent.CreationDate = new DateTime(1970, 1, 1).AddSeconds(integer.Value);
                    }
                }
                else if (item.Key.Value == "encoding")
                {
                    if (item.Value != null)
                    {
                        torrent.Encoding = item.Value.ToString();
                    }
                }
                else if (item.Key.Value == "info")
                {
                    if (item.Value is BDictionary dict)
                    {
                        ParseInfo(torrent, singleFile, ref isSingleFile, dict);
                    }
                }
            }

            if (isSingleFile)
            {
                singleFile.Path?.Add(torrent.Name);
                torrent.Files.Add(singleFile);
            }

            return torrent;
        }

        private static void ParseInfo(TorrentInfo torrent, TorrentFileInfo singleFile, ref bool isSingleFile, BDictionary dictionary)
        {
            Contract.Requires(torrent != null);
            Contract.Requires(singleFile != null);
            Contract.Requires(dictionary != null);

            foreach (var info in dictionary)
            {
                if (info.Key == null)
                    continue;

                if (info.Key.Value == "name")
                {
                    if (info.Value != null)
                    {
                        torrent.Name = info.Value.ToString();
                    }
                }
                else if (info.Key.Value == "piece length")
                {
                    BInteger integer = info.Value as BInteger;
                    if (integer != null)
                    {
                        torrent.PieceLength = integer.Value;
                    }
                }
                else if (info.Key.Value == "pieces")
                {
                    if (info.Value != null)
                    {
                        torrent.Pieces = info.Value.ToString();
                    }
                }
                else if (info.Key.Value == "private")
                {
                    BInteger integer = info.Value as BInteger;
                    if (integer != null)
                    {
                        torrent.Private = integer.Value != 0;
                    }
                }
                else if (info.Key.Value == "files")
                {
                    if (info.Value is BList files)
                    {
                        foreach (var file in files)
                        {
                            if (file is BDictionary dict)
                            {
                                torrent.Files.Add(TorrentFileInfo.Parse(dict));
                            }
                        }
                    }
                }
                else if (info.Key.Value == "file-duration")
                {
                    isSingleFile = true;
                    if (info.Value is BList items)
                    {
                        foreach (var item in items)
                        {
                            BInteger integer = item as BInteger;
                            if (integer != null)
                            {
                                singleFile.Duration.Add(integer.Value);
                            }
                        }
                    }
                }
                else if (info.Key.Value == "file-media")
                {
                    isSingleFile = true;
                    if (info.Value is BList items)
                    {
                        foreach (var item in items)
                        {
                            BInteger integer = item as BInteger;
                            if (integer != null)
                            {
                                singleFile.Media.Add(integer.Value);
                            }
                        }
                    }
                }
                else if (info.Key.Value == "profiles")
                {
                    isSingleFile = true;

                    if (info.Value is BList items)
                    {
                        foreach (var item in items)
                        {
                            if (item is BDictionary dictItems)
                            {
                                TorrentFileProfileCollection profiles = new TorrentFileProfileCollection();
                                profiles.AddRange(dictItems.Select(dictItem => new TorrentFileProfile
                                {
                                    Name = dictItem.Key.ToString(),
                                    Value = dictItem.Value.ToString()
                                }));
                                singleFile.Profiles.Add(profiles);
                            }
                        }
                    }
                }
            }
        }

        public static TorrentInfo Parse(Uri uri)
        {
            Contract.Requires(uri != null);
            Contract.Requires(1 <= uri.Query.Length);

            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var torrent = new TorrentInfo();

            string queryString = uri.Query.Substring(1);
            var queryParams = ParseQueryString((queryString.Length > 0 && queryString[0] == '?') ? queryString.Substring(1) : queryString);

            List<string> annouces = new List<string>();
            foreach (string key in queryParams.Keys)
            {
                if (key == "dn")
                {
                    torrent.Name = queryParams[key];
                }
                else if (key == "xt")
                {
                    string[] urn = queryParams[key].Split(':');
                    if (urn.Length == 3)
                    {
                        torrent.Hash = urn[2];
                    }
                }
                else if (key == "tr")
                {
                    annouces.Add(queryParams[key]);
                }
                else if (key == "xl")
                {
                    if (long.TryParse(queryParams[key], out var val))
                    {
                        torrent.PieceLength = val;
                    }
                }
            }


            return torrent;
        }

        private static Dictionary<string, string> ParseQueryString(string s)
        {
            var result = new Dictionary<string, string>();

            int l = s?.Length ?? 0;
            int i = 0;

            while (i < l)
            {
                // find next & while noting first = on the way (and if there are more)

                int si = i;
                int ti = -1;

                while (i < l)
                {
                    char ch = s[i];

                    if (ch == '=')
                    {
                        if (ti < 0)
                            ti = i;
                    }
                    else if (ch == '&')
                    {
                        break;
                    }

                    i++;
                }

                // extract the name / value pair

                String name = null;
                String value = null;

                if (ti >= 0)
                {
                    name = s.Substring(si, ti - si);
                    value = s.Substring(ti + 1, i - ti - 1);
                }
                else
                {
                    value = s.Substring(si, i - si);
                }

                if (result.ContainsKey(name))
                {
                    // add name / value pair to the collection
                    result.Add(name, value);
                }

                // trailing '&'

                if (i == l - 1 && s[i] == '&')
                    result.Add(null, String.Empty);

                i++;
            }
            return result;
        }
    }
}
