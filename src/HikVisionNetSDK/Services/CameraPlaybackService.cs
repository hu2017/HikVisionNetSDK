using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using HikVisionNetSDK.Common;
using HikVisionNetSDK.Enums;
using HikVisionNetSDK.Models;
using Office365Fx.Core;
using Office365Fx.Core.Helpers;

namespace HikVisionNetSDK.Services
{
    public class CameraPlaybackService : ICameraPlaybackService
    {
        private readonly Int32 _channelNo = 33;
        private readonly IntPtr _hPlayWnd;
        private readonly Int32 _userId = -1;

        private Int32 _playbackHandle = -1;
        private PlayerStatus _playerStatus = PlayerStatus.Stop;

        public CameraPlaybackService(Int32 userId, Int32 channelNo, IntPtr hPlayWnd)
        {
            _userId = userId;
            _channelNo = channelNo;
            _hPlayWnd = hPlayWnd;
        }

        public OResult<Boolean> Fast()
        {
            if (_playerStatus == PlayerStatus.Stop)
            {
                return new OResult<Boolean>(false, "播放已停止");
            }

            try
            {
                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, CHCNetSDK.NET_DVR_PLAYFAST, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"快进失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<IEnumerable<CameraPlaybackFileInfo>> GetPlaybackFiles(DateTime beginTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public OResult<Boolean> Pause()
        {
            if (_playerStatus != PlayerStatus.Play)
            {
                return new OResult<Boolean>(true);
            }

            try
            {
                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, CHCNetSDK.NET_DVR_PLAYPAUSE, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"暂停播放失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _playerStatus = PlayerStatus.Pause;
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> Resume()
        {
            if (_playerStatus != PlayerStatus.Pause)
            {
                return new OResult<Boolean>(true);
            }

            try
            {
                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, CHCNetSDK.NET_DVR_PLAYNORMAL, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"暂停播放失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _playerStatus = PlayerStatus.Play;
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> Play(DateTime startTime, DateTime endTime)
        {
            var stopResult = Stop();
            if (!stopResult.Success)
            {
                return new OResult<Boolean>(false, stopResult.ErrorCode, stopResult.Message);
            }

            try
            {
                var playParameter = CreateVideoPlayParameter(startTime, endTime);
                _playbackHandle = CHCNetSDK.NET_DVR_PlayBackByTime_V40(_userId, ref playParameter);
                if (_playbackHandle < 0)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"回放播放失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"回放播放失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _playerStatus = PlayerStatus.Play;
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> Play(String fileName)
        {
            var stopResult = Stop();
            if (!stopResult.Success)
            {
                return new OResult<Boolean>(false, stopResult.ErrorCode, stopResult.Message);
            }

            try
            {
                _playbackHandle = CHCNetSDK.NET_DVR_PlayBackByName(_userId, fileName, _hPlayWnd);
                if (_playbackHandle < 0)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"回放播放失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"回放播放失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _playerStatus = PlayerStatus.Play;
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> Slow()
        {
            if (_playerStatus == PlayerStatus.Stop)
            {
                return new OResult<Boolean>(false, "播放已停止");
            }

            try
            {
                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, CHCNetSDK.NET_DVR_PLAYSLOW, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"快退失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> Stop()
        {
            if (_playbackHandle < 0)
            {
                return new OResult<Boolean>(true);
            }

            if (_playerStatus == PlayerStatus.Stop)
            {
                return new OResult<Boolean>(true);
            }

            try
            {
                var succ = CHCNetSDK.NET_DVR_StopPlayBack(_playbackHandle);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"停止播放失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _playerStatus = PlayerStatus.Stop;
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> PlayFrame()
        {
            if (_playerStatus == PlayerStatus.Stop)
            {
                return new OResult<Boolean>(false, "播放已停止");
            }

            try
            {
                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, CHCNetSDK.NET_DVR_PLAYFRAME, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"逐帧播放失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Int32> GetPlayProgress()
        {
            try
            {
                IntPtr lpOutBuffer = Marshal.AllocHGlobal(4);
                UInt32 iOutValue = 0;

                CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, CHCNetSDK.NET_DVR_PLAYGETPOS, IntPtr.Zero, 0, lpOutBuffer, ref iOutValue);

                var pos = (Int32)Marshal.PtrToStructure(lpOutBuffer, typeof(Int32));
                Marshal.FreeHGlobal(lpOutBuffer);

                if (pos == 200) //网络异常，回放失败
                {
                    return new OResult<Int32>(-1, "网络异常");
                }

                return new OResult<Int32>(pos);
            }
            catch (Exception ex)
            {
                return new OResult<Int32>(0, ex);
            }
        }

        private CHCNetSDK.NET_DVR_VOD_PARA CreateVideoPlayParameter(DateTime beginTime, DateTime endTime)
        {
            CHCNetSDK.NET_DVR_VOD_PARA struVodPara = new CHCNetSDK.NET_DVR_VOD_PARA();
            struVodPara.dwSize = (UInt32)Marshal.SizeOf(struVodPara);
            struVodPara.struIDInfo.dwChannel = (UInt32)_channelNo; //通道号
            struVodPara.hWnd = _hPlayWnd;//回放窗口句柄

            //设置回放的开始时间 Set the starting time to search video files
            struVodPara.struBeginTime.dwYear = (UInt32)beginTime.Year;
            struVodPara.struBeginTime.dwMonth = (UInt32)beginTime.Month;
            struVodPara.struBeginTime.dwDay = (UInt32)beginTime.Day;
            struVodPara.struBeginTime.dwHour = (UInt32)beginTime.Hour;
            struVodPara.struBeginTime.dwMinute = (UInt32)beginTime.Minute;
            struVodPara.struBeginTime.dwSecond = (UInt32)beginTime.Second;

            //设置回放的结束时间 Set the stopping time to search video files
            struVodPara.struEndTime.dwYear = (UInt32)endTime.Year;
            struVodPara.struEndTime.dwMonth = (UInt32)endTime.Month;
            struVodPara.struEndTime.dwDay = (UInt32)endTime.Day;
            struVodPara.struEndTime.dwHour = (UInt32)endTime.Hour;
            struVodPara.struEndTime.dwMinute = (UInt32)endTime.Minute;
            struVodPara.struEndTime.dwSecond = (UInt32)endTime.Second;

            return struVodPara;
        }

        public OResult<Boolean> CapturePicture(String savePath)
        {
            if (_userId < 0)
            {
                return new OResult<Boolean>(false, "用户未登录");
            }

            try
            {
                var captureSucc = CHCNetSDK.NET_DVR_PlayBackCaptureFile(_playbackHandle, savePath);

                if (!captureSucc)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"抓图失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> Control(Int32 cmd)
        {
            try
            {
                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(_playbackHandle, (UInt32)cmd, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"操作失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<DateTime> GetPlayBackOsdTime()
        {
            try
            {
                if (_playbackHandle < 0)
                {
                    return new OResult<DateTime>(DateTime.MinValue, "未找到的视频");
                }

                var lpOsdTime = new CHCNetSDK.NET_DVR_TIME();
                var succ = CHCNetSDK.NET_DVR_GetPlayBackOsdTime(_playbackHandle, ref lpOsdTime);

                if (!succ)
                {
                    return new OResult<DateTime>(DateTime.MinValue, HkvsErrorCode.GetLastErrorCode(), $"获取OSD时间失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                var dt = new DateTime((Int32)lpOsdTime.dwYear, (Int32)lpOsdTime.dwMonth, (Int32)lpOsdTime.dwDay, (Int32)lpOsdTime.dwHour, (Int32)lpOsdTime.dwMinute, (Int32)lpOsdTime.dwSecond);
                return new OResult<DateTime>(dt);
            }
            catch (Exception ex)
            {
                return new OResult<DateTime>(DateTime.MinValue, ex);
            }
        }
    }
}
