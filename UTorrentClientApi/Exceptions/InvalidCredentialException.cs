using System;
using System.Runtime.Serialization;

namespace UTorrent.Api
{
#if !PORTABLE
    [Serializable]
#endif
    public class InvalidCredentialException : UTorrentException
    {
        public InvalidCredentialException() { }
        public InvalidCredentialException(string message) : base(message) { }
        public InvalidCredentialException(string message, Exception innerException) : base (message, innerException) { }
#if !PORTABLE
        protected InvalidCredentialException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
