using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikVisionNetSDK.Models
{
    /// <summary>
    /// 摄像头设备
    /// </summary>
    public class CameraDevice
    {
        /// <summary>
        /// 设备Id
        /// </summary>
        public String Id { get; set; }
        /// <summary>
        /// 设备IP
        /// </summary>
        public String IP { get; set; }
        /// <summary>
        /// 取流端口号
        /// </summary>
        public Int32 StreamPort { get; set; }
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 ChannelNo { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public String UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public String Password { get; set; }
    }
}
