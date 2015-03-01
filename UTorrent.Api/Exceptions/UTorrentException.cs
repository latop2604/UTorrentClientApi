using System;
using System.Runtime.Serialization;

namespace UTorrent.Api
{
    [Serializable]
    public class UTorrentException : Exception
    {
        public UTorrentException() { }
        public UTorrentException(string message) : base(message) { }
        public UTorrentException(string message, Exception innerException) : base(message, innerException) { }
        protected UTorrentException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}
