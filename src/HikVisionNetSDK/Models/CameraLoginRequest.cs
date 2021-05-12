using System;
using System.Collections.Generic;
using System.Text;

namespace HikVisionNetSDK.Models
{
    /// <summary>
    /// 摄像头登录请求
    /// </summary>
    public class CameraLoginRequest
    {
        /// <summary>
        /// 摄像头/NVR的IP地址
        /// </summary>
        public String IP { get; set; }
        /// <summary>
        /// 登录端口号
        /// </summary>
        public Int32 LoginPort { get; set; }
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 ChannelNo { get; set; } = 33;
        /// <summary>
        /// 登录用户名
        /// </summary>
        public String UserName { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        public String Password { get; set; }
    }
}
