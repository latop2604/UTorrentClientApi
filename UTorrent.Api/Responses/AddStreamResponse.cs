using UTorrent.Api.File;

namespace UTorrent.Api
{
    public class AddStreamResponse : BaseAddResponse
    {
        public TorrentInfo AddedTorrentInfo { get; set; }

        protected override void OnResultChange()
        {
        }
    }
}
