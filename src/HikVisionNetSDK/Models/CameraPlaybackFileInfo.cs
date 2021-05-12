using System;
using System.Collections.Generic;
using System.Text;

namespace HikVisionNetSDK.Models
{
    /// <summary>
    /// 回放文件信息
    /// </summary>
    public class CameraPlaybackFileInfo
    {
        public String FileName { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
