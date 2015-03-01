using System;
using System.Runtime.Serialization;

namespace UTorrent.Api
{
    [Serializable]
    public class ServerUnavailableException : UTorrentException
    {
        public ServerUnavailableException() { }
        public ServerUnavailableException(string message) : base(message) { }
        public ServerUnavailableException(string message, Exception innerException) : base (message, innerException) { }
        protected ServerUnavailableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
