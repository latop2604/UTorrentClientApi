using System;
#if !PORTABLE
using System.Runtime.Serialization;
#endif

namespace UTorrent.Api
{
#if !PORTABLE
    [Serializable]
#endif
    public class UTorrentException : Exception
    {
        public UTorrentException() { }
        public UTorrentException(string message) : base(message) { }
        public UTorrentException(string message, Exception innerException) : base(message, innerException) { }
#if !PORTABLE
        protected UTorrentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
