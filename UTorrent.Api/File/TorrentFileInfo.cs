using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UTorrent.Api.File.Bencoding;

namespace UTorrent.Api.File
{
    public class TorrentFileInfo
    {
        [ContractInvariantMethod]
        void Invariants()
        {
            Contract.Invariant(this.Duration != null);
            Contract.Invariant(this.Media != null);
            Contract.Invariant(this.Profiles != null);
        }
        public long Length { get; set; }

        private readonly IList<string> _path = new List<String>();
        public IList<string> Path
        {
            get { return _path; }
        }

        private readonly IList<long> _duration = new List<long>();
        public IList<long> Duration
        {
            get { return _duration; }
        }

        private readonly IList<long> _media = new List<long>();
        public IList<long> Media
        {
            get { return _media; }
        }

        private readonly IList<TorrentFileProfileCollection> _profiles = new List<TorrentFileProfileCollection>();
        public IList<TorrentFileProfileCollection> Profiles
        {
            get { return _profiles; }
        }

        public static TorrentFileInfo Parse(BDictionary dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            var file = new TorrentFileInfo();

            foreach (var item in dictionary)
            {
                if (item.Key == null)
                    continue;

                switch (item.Key.Value)
                {
                    case "length":
                        BInteger integer = item.Value as BInteger;
                        if (integer != null)
                        {
                            file.Length = integer.Value;
                        }
                        break;
                    case "path":
                        BList listItems = item.Value as BList;
                        if (listItems != null)
                        {
                            foreach (var listItem in listItems)
                            {
                                if (listItem != null)
                                {
                                    file.Path.Add(listItem.ToString());
                                }
                            }
                        }
                        break;
                }
            }

            return file;
        }
    }
}
