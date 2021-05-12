using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HikVisionNetSDK.Enums;
using HikVisionNetSDK.Models;
using Office365Fx.Core;

namespace HikVisionNetSDK
{
    /// <summary>
    /// 摄像头服务接口。
    /// </summary>
    public interface ICameraService : IDisposable
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request">登录信息</param>
        OResult<Boolean> Login(CameraLoginRequest request);
        /// <summary>
        /// 注销登录
        /// </summary>
        OResult<Boolean> Logout();
        /// <summary>
        /// 截图。
        /// </summary>
        OResult<Byte[]> CapturePicture();
        /// <summary>
        /// 打开预览。
        /// </summary>
        /// <param name="hPlayWnd">预览窗口的指针</param>
        /// <param name="streamType">码流类型</param>
        OResult<ICameraPreviewService> OpenPreview(IntPtr hPlayWnd, StreamType streamType);
        /// <summary>
        /// 打开回放
        /// </summary>
        /// <param name="hPlayWnd">播放器句柄</param>
        /// <param name="startTime">起始时间</param>
        /// <param name="endTime">截止时间</param>
        OResult<ICameraPlaybackService> OpenPlayback(IntPtr hPlayWnd, DateTime startTime, DateTime endTime);
        /// <summary>
        /// 开始执行云台控制命令
        /// </summary>
        OResult<Boolean> StartPTZControl(PTZCommand cmd, Int32 speed = 1);
        /// <summary>
        /// 停止执行云台控制命令
        /// </summary>
        OResult<Boolean> StopPTZControl(PTZCommand cmd, Int32 speed = 1);
        /// <summary>
        /// 重启摄像头
        /// </summary>
        OResult<Boolean> Reboot();
        /// <summary>
        /// 开启录像
        /// </summary>
        OResult<Boolean> StartRecord();
        /// <summary>
        /// 停止录像
        /// </summary>
        OResult<Boolean> StopRecord();
        /// <summary>
        /// 根据时间下载录像文件
        /// </summary>
        Task<OResult<Boolean>> DownloadByTimeAsync(DownloadFileByTimeRequest request, CancellationToken cancellationToken = default);
        /// <summary>
        /// 根据时间备份录像文件
        /// </summary>
        Task<OResult<Boolean>> BackupByTimeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
        /// <summary>
        /// 即时刷新录像索引
        /// </summary>
        OResult<Boolean> UpdateRecordIndex();
        /// <summary>
        /// 设置预置点。
        /// </summary>
        /// <param name="presetIndex">预置点序号，从1开始</param>
        OResult<Boolean> SetPreset(Int32 presetIndex);
        /// <summary>
        /// 清除预置点。
        /// </summary>
        /// <param name="presetIndex">预置点序号，从1开始</param>
        OResult<Boolean> RemovePreset(Int32 presetIndex);
        /// <summary>
        /// 转到预置点。
        /// </summary>
        /// <param name="presetIndex">预置点序号，从1开始</param>
        OResult<Boolean> GotoPreset(Int32 presetIndex);
        /// <summary>
        /// 获取月历录像分布
        /// </summary>
        OResult<IEnumerable<CameraPlaybackMonthRecordDistResult>> GetMonthRecordDistributions(Int32 channelNo, Int32 year, Int32 month);
        /// <summary>
        /// 获取指定日期的录像分布
        /// </summary>
        OResult<IEnumerable<CameraPlaybackRecordTimeRange>> GetDayRecordDistributions(Int32 channelNo, DateTime date);
        /// <summary>
        /// 获取设备在线状态，true-在线，false-不在线或故障
        /// </summary>
        OResult<Boolean> GetDeviceOnlineStatus();
        /// <summary>
        /// 获取通道信息
        /// </summary>
        OResult<IEnumerable<CameraChannel>> GetChannels();
    }
}
