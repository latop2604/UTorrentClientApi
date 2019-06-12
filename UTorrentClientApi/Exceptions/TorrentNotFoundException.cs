using System;
using System.Runtime.Serialization;

namespace UTorrent.Api
{
#if !PORTABLE
    [Serializable]
#endif
    public class TorrentNotFoundException : UTorrentException
    {
        public TorrentNotFoundException() { }
        public TorrentNotFoundException(string message) : base(message) { }
        public TorrentNotFoundException(string message, Exception innerException) : base(message, innerException) { }
#if !PORTABLE
        protected TorrentNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
