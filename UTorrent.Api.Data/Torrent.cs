using System;
using System.Collections.Generic;

namespace UTorrent.Api.Data
{
    public class Torrent
    {
        public string Hash { get; set; }
        public Status Status { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Integer in bytes
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// integer in per mils
        /// </summary>
        public int Progress { get; set; }
        /// <summary>
        /// integer in bytes
        /// </summary>
        public long Downloaded { get; set; }
        /// <summary>
        /// integer in bytes
        /// </summary>
        public long Uploaded { get; set; }
        /// <summary>
        /// integer in per mils
        /// </summary>
        public int Ratio { get; set; }
        /// <summary>
        /// integer in bytes per second
        /// </summary>
        public int UploadSpeed { get; set; }
        /// <summary>
        /// integer in bytes per second
        /// </summary>
        public int DownloadSpeed { get; set; }
        /// <summary>
        /// integer in seconds
        /// </summary>
        public int Eta { get; set; }
        public string Label { get; set; }
        public int PeersConnected { get; set; }
        public int PeersInSwarm { get; set; }
        public int SeedsConnected { get; set; }
        public int SeedsInSwarm { get; set; }
        /// <summary>
        /// integer in 1/65535ths
        /// </summary>
        public int Availability { get; set; }
        public int TorrentQueueOrder { get; set; }
        /// <summary>
        /// integer in bytes
        /// </summary>
        public long Remaining { get; set; }
        public string Path { get; set; }

        public DateTime AddedDate { get; set; }

        private readonly FileCollection _files = new FileCollection();
        public IList<File> Files
        {
            get { return _files; }
            set { 
                _files.Clear();
                if (value != null) _files.AddRange(value);
            }
        }
    }

    public class TorrentCollection : List<Torrent>
    {
        public TorrentCollection() { }
        public TorrentCollection(IEnumerable<Torrent> collection) : base(collection) { }
        public TorrentCollection(int capacity) : base(capacity) { }
    }
}
