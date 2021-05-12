using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikVisionNetSDK.Models
{
    /// <summary>
    /// 回放时间区间
    /// </summary>
    public class CameraPlaybackRecordTimeRange
    {
        /// <summary>
        /// 回放起始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 回放截止时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}
