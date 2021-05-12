using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HikVisionNetSDK.Models
{
    /// <summary>
    /// 按起止时间请求下载文件
    /// </summary>
    public class DownloadFileByTimeRequest
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        public String FilePath { get; set; }
        /// <summary>
        /// 下载起始时间
        /// </summary>
        public DateTime From { get; set; }
        /// <summary>
        /// 下载截止时间
        /// </summary>
        public DateTime To { get; set; }
    }
}
