using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikVisionNetSDK.Models
{
    /// <summary>
    /// 通道
    /// </summary>
    public class CameraChannel
    {
        /// <summary>
        /// 设备是否启用
        /// </summary>
        public Boolean IsEnabled { get; set; }
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 ChannelNo { get; set; }
        /// <summary>
        /// 通道类型，0-数字通道DC，1-模拟通道AC
        /// </summary>
        public Int32 ChannelType { get; set; }
        /// <summary>
        /// 通道状态，0-空闲，1-在线，2-离线
        /// </summary>
        public Int32 ChannelState { get; set; }
    }
}
