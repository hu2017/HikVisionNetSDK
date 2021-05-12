using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HikVisionNetSDK.Enums
{
    /// <summary>
    /// 播放器状态
    /// </summary>
    public enum PlayerStatus
    {
        /// <summary>
        /// 停止
        /// </summary>
        [Description("停止")]
        Stop = 0,
        /// <summary>
        /// 播放
        /// </summary>
        [Description("播放")]
        Play = 1,
        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Pause = 2
    }
}
