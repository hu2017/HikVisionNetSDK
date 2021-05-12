using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HikVisionNetSDK.Codec
{
    public class TranscodeChannel
    {
        public WebSocketChannel OutputChannel { get; set; }
        public StreamChannel InputChannel { get; set; }
    }

    public class WebSocketChannel
    {
        public Int32 StreamPort { get; set; }
        public Int32 WebSocketPort { get; set; }
        public String Secret { get; set; }
        public Process Process { get; set; }
    }

    public class StreamChannel
    {
        public String WebSocketUrl { get; set; }
        public String RTSPUri { get; set; }
        public Process Process { get; set; }
    }

    public class TranscodeRequest
    {
        /// <summary>
        /// 通道起始编号
        /// </summary>
        public const Int32 CHANNEL_START_NO = 33;
        /// <summary>
        /// 摄像头或网络录像机的IP地址
        /// </summary>
        public String IP { get; set; }
        /// <summary>
        /// 登录端口号
        /// </summary>
        public Int32 LoginPort { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public String UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public String Password { get; set; }
        /// <summary>
        /// 通道号
        /// </summary>
        public Int32 ChannelNo { get; set; }
        /// <summary>
        /// 画面宽度
        /// </summary>
        public Int32 Width { get; set; }
        /// <summary>
        /// 画面高度
        /// </summary>
        public Int32 Height { get; set; }
        public String WebSocketUrl { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
