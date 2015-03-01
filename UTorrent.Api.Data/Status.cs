using System;

namespace UTorrent.Api.Data
{
    [Flags]
    public enum Status
    {
        Started = 1 << 0,         //   1
        Checking = 1 << 1,        //   2
        StartAfterCheck = 1 << 2, //   4
        Checked = 1 << 3,         //   8
        Error = 1 << 4,           //  16
        Paused = 1 << 5,          //  32
        Queued = 1 << 6,          //  64
        Loaded = 1 << 7,          // 128
    }
}
