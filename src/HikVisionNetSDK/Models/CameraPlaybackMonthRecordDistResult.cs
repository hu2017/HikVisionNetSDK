using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikVisionNetSDK.Models
{
    /// <summary>
    /// 回放记录月历分布
    /// </summary>
    public class CameraPlaybackMonthRecordDistResult
    {
        /// <summary>
        /// 月份
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 分布区间
        /// </summary>
        public IEnumerable<CameraPlaybackRecordTimeRange> Ranges { get; set; }
    }
}
