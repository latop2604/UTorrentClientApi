using UTorrent.Api.Data;

namespace UTorrent.Api
{
    public abstract class BaseAddResponse : BaseResponse
    {
        public Torrent AddedTorrent { get; set; }
    }
}
