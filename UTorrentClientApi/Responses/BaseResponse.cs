
namespace UTorrent.Api
{
    public abstract class BaseResponse
    {
        private Result _result;

        /// <summary>
        /// Request result
        /// </summary>
        public Result Result
        {
            get { return _result; }
            set
            {
                if (_result == value)
                    return;

                _result = value;
                OnResultChange();
            }
        }

        /// <summary>
        /// µTorrent result error
        /// </summary>
        public UTorrentException Error => _result?.Error;

        protected abstract void OnResultChange();
    }
}
