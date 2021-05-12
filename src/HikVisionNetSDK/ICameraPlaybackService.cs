using System;
using System.Collections.Generic;
using System.Text;
using HikVisionNetSDK.Models;
using Office365Fx.Core;

namespace HikVisionNetSDK
{
    /// <summary>
    /// 摄像头回放接口。
    /// </summary>
    public interface ICameraPlaybackService
    {
        /// <summary>
        /// 获取指定时间段的回放文件。
        /// </summary>
        /// <param name="beginTime">起始时间。</param>
        /// <param name="endTime">截止时间。</param>
        OResult<IEnumerable<CameraPlaybackFileInfo>> GetPlaybackFiles(DateTime beginTime, DateTime endTime);
        /// <summary>
        /// 播放指定时间段的回放视频。
        /// </summary>
        /// <param name="startTime">起始时间。</param>
        /// <param name="endTime">截止时间。</param>
        OResult<Boolean> Play(DateTime startTime, DateTime endTime);
        /// <summary>
        /// 播放指定名称的回放文件。
        /// </summary>
        /// <param name="fileName">要回放的文件名。</param>
        OResult<Boolean> Play(String fileName);
        /// <summary>
        /// 逐帧播放。
        /// </summary>
        OResult<Boolean> PlayFrame();
        /// <summary>
        /// 获取播放进度。
        /// </summary>
        OResult<Int32> GetPlayProgress();
        /// <summary>
        /// 暂停播放。
        /// </summary>
        OResult<Boolean> Pause();
        /// <summary>
        /// 停止播放。
        /// </summary>
        OResult<Boolean> Stop();
        /// <summary>
        /// 快进播放。
        /// </summary>
        OResult<Boolean> Fast();
        /// <summary>
        /// 快退播放。
        /// </summary>
        OResult<Boolean> Slow();
        /// <summary>
        /// 控制
        /// </summary>
        OResult<Boolean> Control(Int32 cmd);
        /// <summary>
        /// 截图
        /// </summary>
        OResult<Boolean> CapturePicture(String savePath);
        /// <summary>
        /// 获取录像回放时显示的 OSD 时间
        /// </summary>
        OResult<DateTime> GetPlayBackOsdTime();
    }
}
