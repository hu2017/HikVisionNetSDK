using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using HikVisionNetSDK.Common;
using HikVisionNetSDK.Enums;
using Office365Fx.Core;

namespace HikVisionNetSDK.Services
{
    public class CameraPreviewService : ICameraPreviewService
    {
        private readonly Int32 _userId = -1;
        private readonly Int32 _channelNo = 33;
        private readonly StreamType _streamType;
        private readonly IntPtr _hPlayWnd;

        private Boolean _inRecord = false;
        private Boolean _inRealPlay = false;

        private Int32 _realHandle = -1;

        public CameraPreviewService(Int32 userId, Int32 channelNo, StreamType streamType, IntPtr hPlayWnd)
        {
            _userId = userId;
            _channelNo = channelNo;
            _streamType = streamType;
            _hPlayWnd = hPlayWnd;
        }

        public OResult<String> StartRecord(String fileName)
        {
            if (_userId < 0)
            {
                return new OResult<String>(null, "用户未登录");
            }

            if (_inRecord)
            {
                return new OResult<String>(null, "正在录制");
            }

            try
            {
                CHCNetSDK.NET_DVR_MakeKeyFrame(_userId, _channelNo);
                var succ = CHCNetSDK.NET_DVR_SaveRealData(_realHandle, fileName);
                if (!succ)
                {
                    return new OResult<String>(null, HkvsErrorCode.GetLastErrorCode(), $"录制失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _inRecord = true;
                return new OResult<String>(fileName);//TODO:返回文件路径
            }
            catch (Exception ex)
            {
                return new OResult<String>(null, ex);
            }
        }

        public OResult<Boolean> StopRecord()
        {
            if (_userId < 0)
            {
                return new OResult<Boolean>(false, "用户未登录");
            }

            if (!_inRecord)
            {
                return new OResult<Boolean>(true);
            }

            try
            {
                var succ = CHCNetSDK.NET_DVR_StopSaveRealData(_realHandle);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"停止录像失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _inRecord = false;
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public void Dispose()
        {
            if (_inRecord)
            {
                StopRecord();
            }

            if (_inRealPlay)
            {
                Close();
            }

            GC.SuppressFinalize(this);
        }

        public OResult<Boolean> Close()
        {
            if (_userId < 0)
            {
                return new OResult<Boolean>(false, "用户未登录");
            }

            try
            {
                this.StopRecord();
                var succ = CHCNetSDK.NET_DVR_StopRealPlay(_realHandle);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"停止预览失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _inRealPlay = false;

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> Open()
        {
            if (_userId < 0)
            {
                return new OResult<Boolean>(false, "用户未登录");
            }

            if (_inRealPlay)
            {
                return new OResult<Boolean>(true);
            }

            try
            {
                var lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = _hPlayWnd;  //预览窗口
                lpPreviewInfo.lChannel = _channelNo;//预览的设备通道
                lpPreviewInfo.dwStreamType = (UInt32)_streamType;     //码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;       //连接方式：0-TCP方式，1-UDP方式，2-多播方式，3-RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true;      //0-非阻塞取流，1-阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 15;  //播放库播放缓冲区最大缓冲帧数
                lpPreviewInfo.byProtoType = 0;
                lpPreviewInfo.byPreviewMode = 0;


                _realHandle = CHCNetSDK.NET_DVR_RealPlay_V40(_userId, ref lpPreviewInfo, null, IntPtr.Zero);
                if (_realHandle < 0)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"开始预览失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _inRealPlay = true;

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }
    }
}
