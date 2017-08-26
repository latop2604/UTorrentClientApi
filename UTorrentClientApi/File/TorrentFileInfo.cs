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

        public IList<string> Path { get; } = new List<String>();

        public IList<long> Duration { get; } = new List<long>();

        public IList<long> Media { get; } = new List<long>();

        public IList<TorrentFileProfileCollection> Profiles { get; } = new List<TorrentFileProfileCollection>();

        public static TorrentFileInfo Parse(BDictionary dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

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
                        if (item.Value is BList listItems)
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
