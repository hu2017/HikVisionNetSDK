using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikVisionNetSDK.Enums
{
    /// <summary>
    /// 通道状态
    /// </summary>
    public enum CameraChannelState
    {
        /// <summary>
        /// 空闲
        /// </summary>
        IDLE = 0,
        /// <summary>
        /// 在线
        /// </summary>
        Online = 1,
        /// <summary>
        /// 离线
        /// </summary>
        Offline = 2
    }
}
