using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HikVisionNetSDK.Common;
using HikVisionNetSDK.Enums;
using HikVisionNetSDK.Models;
using Office365Fx.Core;

namespace HikVisionNetSDK.Services
{
    public class CameraService : ICameraService
    {
        private readonly Boolean _sdkInit = false;
        private readonly List<ICameraPlaybackService> _playbackServices;
        private readonly List<ICameraPreviewService> _previewServices;

        private CameraLoginRequest _loginRequest;
        private Int32 _userId = -1;
        private CHCNetSDK.NET_DVR_DEVICEINFO_V40 _deviceInfo;

        public CameraService()
        {
            _sdkInit = CHCNetSDK.NET_DVR_Init();
            _playbackServices = new List<ICameraPlaybackService>();
            _previewServices = new List<ICameraPreviewService>();
        }

        public OResult<Boolean> Login(CameraLoginRequest request)
        {
            try
            {
                var struLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();

                //设备IP地址或者域名
                Byte[] byIP = Encoding.Default.GetBytes(request.IP);
                struLogInfo.sDeviceAddress = new Byte[129];
                byIP.CopyTo(struLogInfo.sDeviceAddress, 0);

                //设备服务端口号
                struLogInfo.wPort = (UInt16)request.LoginPort;

                //设备用户名
                Byte[] byUserName = Encoding.Default.GetBytes(request.UserName);
                struLogInfo.sUserName = new Byte[64];
                byUserName.CopyTo(struLogInfo.sUserName, 0);

                //设备密码
                Byte[] byPassword = Encoding.Default.GetBytes(request.Password);
                struLogInfo.sPassword = new Byte[64];
                byPassword.CopyTo(struLogInfo.sPassword, 0);

                struLogInfo.cbLoginResult = new CHCNetSDK.LOGINRESULTCALLBACK(LoginCallBack);
                struLogInfo.bUseAsynLogin = false;

                _deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();
                _userId = CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref _deviceInfo);

                if (_userId < 0)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"登录失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _loginRequest = request;
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Byte[]> CapturePicture()
        {
            if (_userId < 0)
            {
                return new OResult<Byte[]>(null, "用户未登录");
            }

            try
            {
                //图片保存路径和文件名
                //var picFileName = $"User_{_userId}_{DateTimeHelper.GetTimestamp()}.jpg";

                CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
                lpJpegPara.wPicQuality = 0; //图像质量 Image quality
                lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 2- 4CIF，0xff- Auto(使用当前码流分辨率)，抓图分辨率需要设备支持，更多取值请参考SDK文档

                //var captureSucc = CHCNetSDK.NET_DVR_CaptureJPEGPicture(_userId, _loginRequest.ChannelNo, ref lpJpegPara, picFileName);
                //if (!captureSucc)
                //{
                //    return new OResult<Byte[]>(null, $"抓图失败：{GetLastErrorCode()}");
                //}

                UInt32 iBuffSize = 10 * 1024 * 1024; //缓冲区大小需要不小于一张图片数据的大小 The buffer size should not be less than the picture size
                Byte[] byJpegPicBuffer = new Byte[iBuffSize];
                UInt32 dwSizeReturned = 0;

                var captureSucc = CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(_userId, _loginRequest.ChannelNo, ref lpJpegPara, byJpegPicBuffer, iBuffSize, ref dwSizeReturned);

                if (!captureSucc)
                {
                    return new OResult<Byte[]>(null, HkvsErrorCode.GetLastErrorCode(), $"抓图失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                var data = new Byte[dwSizeReturned];

                Buffer.BlockCopy(byJpegPicBuffer, 0, data, 0, data.Length);

                return new OResult<Byte[]>(data);
            }
            catch (Exception ex)
            {
                return new OResult<Byte[]>(null, ex);
            }
        }

        private void LoginCallBack(Int32 lUserID, Int32 dwResult, IntPtr lpDeviceInfo, IntPtr pUser)
        {
            var strLoginCallBack = "登录设备，lUserID：" + lUserID + "，dwResult：" + dwResult;

            if (dwResult == 0)
            {
                var iErrCode = CHCNetSDK.NET_DVR_GetLastError();
                strLoginCallBack += "，错误号:" + iErrCode;
            }
        }

        public OResult<Boolean> Logout()
        {
            if (_userId < 0)
            {
                return new OResult<Boolean>(true);
            }

            try
            {
                StopPreviewService();
                StopPlaybackService();

                var succ = CHCNetSDK.NET_DVR_Logout(_userId);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"注销失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                _userId = -1;
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        private void StopPlaybackService()
        {
            foreach (var playbackService in _playbackServices.ToArray())
            {
                var stopResult = playbackService.Stop();
                if (stopResult.Success)
                {
                    _playbackServices.Remove(playbackService);
                }
            }
        }

        private void StopPreviewService()
        {
            foreach (var previewService in _previewServices.ToArray())
            {
                var stopResult = previewService.Close();
                if (stopResult.Success)
                {
                    _previewServices.Remove(previewService);
                }
            }
        }

        public OResult<ICameraPreviewService> OpenPreview(IntPtr hPlayWnd, StreamType streamType)
        {
            if (_userId < 0)
            {
                return new OResult<ICameraPreviewService>(null, "请先登录");
            }

            var previewService = new CameraPreviewService(_userId, _loginRequest.ChannelNo, streamType, hPlayWnd);
            var openResult = previewService.Open();
            if (!openResult.Success)
            {
                return new OResult<ICameraPreviewService>(null, openResult.ErrorCode, openResult.Message);
            }

            _previewServices.Add(previewService);
            return new OResult<ICameraPreviewService>(previewService);
        }

        public OResult<ICameraPlaybackService> OpenPlayback(IntPtr hPlayWnd, DateTime startTime, DateTime endTime)
        {
            if (_userId < 0)
            {
                return new OResult<ICameraPlaybackService>(null, "请先登录");
            }

            var playbackService = new CameraPlaybackService(_userId, _loginRequest.ChannelNo, hPlayWnd);

            var openResult = playbackService.Play(startTime, endTime);
            if (!openResult.Success)
            {
                return new OResult<ICameraPlaybackService>(null, openResult.ErrorCode, openResult.Message);
            }

            _playbackServices.Add(playbackService);

            return new OResult<ICameraPlaybackService>(playbackService);
        }

        public OResult<Boolean> StartPTZControl(PTZCommand cmd, Int32 speed = 4)
        {
            try
            {
                var succ = CHCNetSDK.NET_DVR_PTZControlWithSpeed_Other(_userId, _loginRequest.ChannelNo, (UInt32)cmd, 0, (UInt32)speed);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"开始摄像头移动失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> StopPTZControl(PTZCommand cmd, Int32 speed = 4)
        {
            try
            {
                var succ = CHCNetSDK.NET_DVR_PTZControlWithSpeed_Other(_userId, _loginRequest.ChannelNo, (UInt32)cmd, 1, (UInt32)speed);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"停止摄像头移动失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public void Dispose()
        {
            if (_userId >= 0)
            {
                Logout();
            }

            if (_sdkInit)
            {
                CHCNetSDK.NET_DVR_Cleanup();
            }

            GC.SuppressFinalize(this);
        }

        public OResult<Boolean> Shutdown()
        {
            try
            {
                if (_userId < 0)
                {
                    return new OResult<Boolean>(false, "用户未登录");
                }

                var succ = CHCNetSDK.NET_DVR_ShutDownDVR(_userId);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"关闭摄像头失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> Reboot()
        {
            try
            {
                if (_userId < 0)
                {
                    return new OResult<Boolean>(false, "用户未登录");
                }

                var succ = CHCNetSDK.NET_DVR_RebootDVR(_userId);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"重启摄像头失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> StartRecord()
        {
            try
            {
                var succ = CHCNetSDK.NET_DVR_StartDVRRecord(_userId, _loginRequest.ChannelNo, 0);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"开启录像失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> StopRecord()
        {
            try
            {
                var succ = CHCNetSDK.NET_DVR_StopDVRRecord(_userId, _loginRequest.ChannelNo);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"停止录像失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public async Task<OResult<Boolean>> DownloadByTimeAsync(DownloadFileByTimeRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var now = DateTime.Now;

                if (request.From > now)
                {
                    request.From = now;
                }

                if (request.To > now)
                {
                    request.To = now;
                }

                if (request.From > request.To)
                {
                    request.To = DateTime.Now;
                }

                CHCNetSDK.NET_DVR_PLAYCOND struDownPara = new CHCNetSDK.NET_DVR_PLAYCOND();
                struDownPara.dwChannel = (UInt32)_loginRequest.ChannelNo;

                //设置下载的开始时间
                struDownPara.struStartTime.dwYear = (UInt32)request.From.Year;
                struDownPara.struStartTime.dwMonth = (UInt32)request.From.Month;
                struDownPara.struStartTime.dwDay = (UInt32)request.From.Day;
                struDownPara.struStartTime.dwHour = (UInt32)request.From.Hour;
                struDownPara.struStartTime.dwMinute = (UInt32)request.From.Minute;
                struDownPara.struStartTime.dwSecond = (UInt32)request.From.Second;

                //设置下载的结束时间
                struDownPara.struStopTime.dwYear = (UInt32)request.To.Year;
                struDownPara.struStopTime.dwMonth = (UInt32)request.To.Month;
                struDownPara.struStopTime.dwDay = (UInt32)request.To.Day;
                struDownPara.struStopTime.dwHour = (UInt32)request.To.Hour;
                struDownPara.struStopTime.dwMinute = (UInt32)request.To.Minute;
                struDownPara.struStopTime.dwSecond = (UInt32)request.To.Second;

                var m_lDownHandle = CHCNetSDK.NET_DVR_GetFileByTime_V40(_userId, request.FilePath, ref struDownPara);
                if (m_lDownHandle < 0)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"按时间下载失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                UInt32 iOutValue = 0;
                var succ = CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lDownHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"按时间下载失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                var tcs = new TaskCompletionSource<Boolean>();

                _ = Task.Run(async () =>
                {
                    var iPos = 0;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        iPos = CHCNetSDK.NET_DVR_GetDownloadPos(m_lDownHandle);

                        switch (iPos)
                        {
                            case 100:
                                CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle);
                                tcs.SetResult(true);
                                return;
                            case 200:
                                tcs.SetException(new SystemException("网络中断"));
                                CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle);
                                return;
                        }

                        await Task.Delay(1000);
                    }
                }, CancellationToken.None);

                succ = await tcs.Task;

                if (!succ)
                {
                    return new OResult<Boolean>(false, "按时间下载失败");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public async Task<OResult<Boolean>> BackupByTimeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            try
            {
                var lpDiskList = new CHCNetSDK.NET_DVR_DISKABILITY_LIST();
                lpDiskList.dwSize = (UInt32)Marshal.SizeOf(lpDiskList);

                var succ = CHCNetSDK.NET_DVR_GetDiskList(_userId, ref lpDiskList);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"获取磁盘列表失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                if (!lpDiskList.struDescNode.Any())
                {
                    return new OResult<Boolean>(false, "可用磁盘列表为空");
                }

                var availableDesc = lpDiskList.struDescNode.Where(x => x.dwFreeSpace > 0).Select(x => x.byDescribe).FirstOrDefault();
                if (availableDesc == null || !availableDesc.Any())
                {
                    return new OResult<Boolean>(false, "磁盘空间不足");
                }

                var lpBackupByTime = new CHCNetSDK.NET_DVR_BACKUP_TIME_PARAM();

                lpBackupByTime.lChannel = _loginRequest.ChannelNo;

                lpBackupByTime.struStartTime.dwYear = (UInt32)from.Year;
                lpBackupByTime.struStartTime.dwMonth = (UInt32)from.Month;
                lpBackupByTime.struStartTime.dwDay = (UInt32)from.Day;
                lpBackupByTime.struStartTime.dwHour = (UInt32)from.Hour;
                lpBackupByTime.struStartTime.dwMinute = (UInt32)from.Minute;
                lpBackupByTime.struStartTime.dwSecond = (UInt32)from.Second;

                lpBackupByTime.struStopTime.dwYear = (UInt32)to.Year;
                lpBackupByTime.struStopTime.dwMonth = (UInt32)to.Month;
                lpBackupByTime.struStopTime.dwDay = (UInt32)to.Day;
                lpBackupByTime.struStopTime.dwHour = (UInt32)to.Hour;
                lpBackupByTime.struStopTime.dwMinute = (UInt32)to.Minute;
                lpBackupByTime.struStopTime.dwSecond = (UInt32)to.Second;

                lpBackupByTime.byDiskDes = availableDesc;   //备份磁盘的描述
                lpBackupByTime.byWithPlayer = 0;            //是否备份播放器，0-不备份，1-备份
                lpBackupByTime.byContinue = 0;              //是否继续备份，0-不继续，1-继续
                lpBackupByTime.byDrawFrame = 0;             //0-不抽帧，1-抽帧

                var backUpHandle = CHCNetSDK.NET_DVR_BackupByTime(_userId, ref lpBackupByTime);
                if (backUpHandle < 0)
                {
                    return new OResult<Boolean>(false, $"按时间备份录像失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                var tcs = new TaskCompletionSource<Boolean>();

                _ = Task.Run(async () =>
                {

                    UInt32 iPos = 0;
                    IntPtr pState = Marshal.AllocHGlobal((Int32)iPos);
                    Marshal.StructureToPtr(iPos, pState, false);

                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var succ = CHCNetSDK.NET_DVR_GetBackupProgress(backUpHandle, pState);
                            if (succ)
                            {
                                iPos = (UInt32)Marshal.PtrToStructure(pState, typeof(UInt32));

                                if (iPos == 100)
                                {
                                    CHCNetSDK.NET_DVR_StopBackup(backUpHandle);
                                    tcs.SetResult(true);
                                    return;
                                }
                                else if (iPos == 101 || iPos >= 400)
                                {
                                    CHCNetSDK.NET_DVR_StopBackup(backUpHandle);
                                    return;
                                }
                            }

                            await Task.Delay(1000);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pState);
                    }
                }, CancellationToken.None);

                succ = await tcs.Task;

                if (!succ)
                {
                    return new OResult<Boolean>(false, "按时间下载失败");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> UpdateRecordIndex()
        {
            try
            {
                var succ = CHCNetSDK.NET_DVR_UpdateRecordIndex(_userId, _loginRequest.ChannelNo);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"即时刷新录像索引失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<Boolean> SetPreset(Int32 presetIndex)
        {
            return PTZPreset(CHCNetSDK.SET_PRESET, presetIndex);
        }

        public OResult<Boolean> RemovePreset(Int32 presetIndex)
        {
            return PTZPreset(CHCNetSDK.CLE_PRESET, presetIndex);
        }

        public OResult<Boolean> GotoPreset(Int32 presetIndex)
        {
            return PTZPreset(CHCNetSDK.GOTO_PRESET, presetIndex);
        }

        private OResult<Boolean> PTZPreset(Int32 command, Int32 presetIndex)
        {
            try
            {
                if (presetIndex < 1 || presetIndex > 255)
                {
                    return new OResult<Boolean>(false, "预置点超出范围[1-255]");
                }

                var succ = CHCNetSDK.NET_DVR_PTZPreset_Other(_userId, _loginRequest.ChannelNo, (UInt32)command, (UInt32)presetIndex);
                if (!succ)
                {
                    return new OResult<Boolean>(false, HkvsErrorCode.GetLastErrorCode(), $"操作预置点失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<IEnumerable<CameraPlaybackMonthRecordDistResult>> GetMonthRecordDistributions(Int32 channelNo, Int32 year, Int32 month)
        {
            try
            {
                var startDate = DateTime.Parse($"{year}-{month}-01");
                var endDate = startDate.AddMonths(1);
                var monthDist = new List<CameraPlaybackMonthRecordDistResult>();

                while (startDate < endDate)
                {
                    var item = new CameraPlaybackMonthRecordDistResult()
                    {
                        Date = startDate
                    };

                    var getRangesResult = GetDayRecordTimeRanges(channelNo, startDate);
                    if (getRangesResult.Success)
                    {
                        item.Ranges = getRangesResult.Value;
                    }

                    monthDist.Add(item);
                    startDate = startDate.AddDays(1);
                }

                return new OResult<IEnumerable<CameraPlaybackMonthRecordDistResult>>(monthDist);
            }
            catch (Exception ex)
            {
                return new OResult<IEnumerable<CameraPlaybackMonthRecordDistResult>>(null, ex);
            }
        }

        private OResult<IEnumerable<CameraPlaybackRecordTimeRange>> GetDayRecordTimeRanges(Int32 channelNo, DateTime date)
        {
            try
            {
                CHCNetSDK.NET_DVR_FILECOND_V40 struFileCond_V40 = new CHCNetSDK.NET_DVR_FILECOND_V40();

                struFileCond_V40.lChannel = channelNo; //通道号
                struFileCond_V40.dwFileType = 0xff; //0xff-全部，0-定时录像，1-移动侦测，2-报警触发，...
                struFileCond_V40.dwIsLocked = 0xff; //0-未锁定文件，1-锁定文件，0xff表示所有文件（包括锁定和未锁定）

                //设置录像查找的开始时间
                struFileCond_V40.struStartTime.dwYear = (UInt32)date.Year;
                struFileCond_V40.struStartTime.dwMonth = (UInt32)date.Month;
                struFileCond_V40.struStartTime.dwDay = (UInt32)date.Day;
                struFileCond_V40.struStartTime.dwHour = 0;
                struFileCond_V40.struStartTime.dwMinute = 0;
                struFileCond_V40.struStartTime.dwSecond = 0;

                //设置录像查找的结束时间
                struFileCond_V40.struStopTime.dwYear = (UInt32)date.Year;
                struFileCond_V40.struStopTime.dwMonth = (UInt32)date.Month;
                struFileCond_V40.struStopTime.dwDay = (UInt32)date.Day;
                struFileCond_V40.struStopTime.dwHour = 23;
                struFileCond_V40.struStopTime.dwMinute = 59;
                struFileCond_V40.struStopTime.dwSecond = 59;

                //开始录像文件查找 Start to search video files 
                var m_lFindHandle = CHCNetSDK.NET_DVR_FindFile_V40(_userId, ref struFileCond_V40);

                if (m_lFindHandle < 0)
                {
                    return new OResult<IEnumerable<CameraPlaybackRecordTimeRange>>(null, HkvsErrorCode.GetLastErrorCode(), $"查找录像文件失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }
                else
                {
                    CHCNetSDK.NET_DVR_FINDDATA_V30 struFileData = new CHCNetSDK.NET_DVR_FINDDATA_V30();

                    var ranges = new List<CameraPlaybackRecordTimeRange>();

                    while (true)
                    {
                        //逐个获取查找到的文件信息
                        var result = CHCNetSDK.NET_DVR_FindNextFile_V30(m_lFindHandle, ref struFileData);
                        if (result == CHCNetSDK.NET_DVR_ISFINDING)  //正在查找请等待
                        {
                            continue;
                        }
                        else if (result == CHCNetSDK.NET_DVR_FILE_SUCCESS) //获取文件信息成功
                        {
                            var startTime = new DateTime((Int32)struFileData.struStartTime.dwYear, (Int32)struFileData.struStartTime.dwMonth, (Int32)struFileData.struStartTime.dwDay, (Int32)struFileData.struStartTime.dwHour, (Int32)struFileData.struStartTime.dwMinute, (Int32)struFileData.struStartTime.dwSecond);
                            var endTime = new DateTime((Int32)struFileData.struStopTime.dwYear, (Int32)struFileData.struStopTime.dwMonth, (Int32)struFileData.struStopTime.dwDay, (Int32)struFileData.struStopTime.dwHour, (Int32)struFileData.struStopTime.dwMinute, (Int32)struFileData.struStopTime.dwSecond);
                            ranges.Add(new CameraPlaybackRecordTimeRange()
                            {
                                StartTime = startTime,
                                EndTime = endTime
                            });
                        }
                        else if (result == CHCNetSDK.NET_DVR_FILE_NOFIND || result == CHCNetSDK.NET_DVR_NOMOREFILE)
                        {
                            break; //未查找到文件或者查找结束，退出
                        }
                        else
                        {
                            break;
                        }
                    }

                    MergetRanges(ref ranges);

                    return new OResult<IEnumerable<CameraPlaybackRecordTimeRange>>(ranges.Any() ? ranges : null);
                }
            }
            catch (Exception ex)
            {
                return new OResult<IEnumerable<CameraPlaybackRecordTimeRange>>(null, ex);
            }
        }

        private static void MergetRanges(ref List<CameraPlaybackRecordTimeRange> ranges)
        {
            if (!ranges.Any() || ranges.Count < 2) { return; }

            var stk = new Stack<CameraPlaybackRecordTimeRange>(ranges.Count);

            foreach (var range in ranges)
            {
                if (!stk.Any())
                {
                    stk.Push(range);
                    continue;
                }

                var last = stk.Peek();

                if (last.EndTime >= range.StartTime)
                {
                    last.EndTime = range.EndTime;
                    continue;
                }

                stk.Push(range);
            }

            ranges.Clear();

            foreach (var item in stk.Reverse())
            {
                ranges.Add(item);
            }
        }

        public OResult<IEnumerable<CameraPlaybackRecordTimeRange>> GetDayRecordDistributions(Int32 channelNo, DateTime date)
        {
            return GetDayRecordTimeRanges(channelNo, date);
        }

        public OResult<Boolean> GetDeviceOnlineStatus()
        {
            try
            {
                var succ = CHCNetSDK.NET_DVR_RemoteControl(_userId, 20005, IntPtr.Zero, 0);
                if (!succ)
                {
                    return new OResult<Boolean>(false, $"获取设备在线状态失败：{HkvsErrorCode.GetLastErrorMessage()}");
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(false, ex);
            }
        }

        public OResult<IEnumerable<CameraChannel>> GetChannels()
        {
            UInt32 dwReturn = 0;
            var iGroupNo = 0;  //0表示仅获取第一组64个通道，如果设备IP通道大于64路，需要按组号0~i多次调用NET_DVR_GET_IPPARACFG_V40获取
            var iIPDevID = new Int32[96];
            var dwAChanTotalNum = (UInt32)_deviceInfo.struDeviceV30.byChanNum;
            var dwDChanTotalNum = (UInt32)(_deviceInfo.struDeviceV30.byIPChanNum + 256 * _deviceInfo.struDeviceV30.byHighDChanNum);
            IntPtr ptrIpParaCfgV40 = IntPtr.Zero;
            var channels = new List<CameraChannel>(16);

            try
            {
                if (dwDChanTotalNum > 0)
                {
                    var m_struIpParaCfgV40 = new CHCNetSDK.NET_DVR_IPPARACFG_V40();
                    var dwSize = (UInt32)Marshal.SizeOf(m_struIpParaCfgV40);

                    ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(m_struIpParaCfgV40, ptrIpParaCfgV40, false);

                    if (CHCNetSDK.NET_DVR_GetDVRConfig(_userId, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
                    {
                        m_struIpParaCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));

                        if (dwAChanTotalNum > 0)
                        {
                            for (var i = 0; i < dwAChanTotalNum; i++)
                            {
                                channels.Add(new CameraChannel()
                                {
                                    ChannelNo = _deviceInfo.struDeviceV30.byStartChan + i,
                                    ChannelType = (Int32)CameraChannelType.AC,
                                    IsEnabled = m_struIpParaCfgV40.byAnalogChanEnable[i] != 0
                                });
                            }
                        }

                        UInt32 iDChanNum = 64;
                        if (dwDChanTotalNum < 64)
                        {
                            iDChanNum = dwDChanTotalNum; //如果设备IP通道小于64路，按实际路数获取
                        }

                        CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;
                        CHCNetSDK.NET_DVR_PU_STREAM_URL m_struStreamURL;
                        CHCNetSDK.NET_DVR_IPCHANINFO_V40 m_struChanInfoV40;

                        if (iDChanNum > 0)
                        {
                            for (var i = 0; i < iDChanNum; i++)
                            {
                                var byStreamType = m_struIpParaCfgV40.struStreamMode[i].byGetStreamType;
                                dwSize = (UInt32)Marshal.SizeOf(m_struIpParaCfgV40.struStreamMode[i].uGetStream);

                                var dcChannel = new CameraChannel()
                                {
                                    ChannelNo = (Int32)m_struIpParaCfgV40.dwStartDChan + i,
                                    ChannelType = (Int32)CameraChannelType.DC
                                };

                                switch (byStreamType)
                                {
                                    //目前NVR仅支持直接从设备取流
                                    case 0:
                                        IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwSize);
                                        Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                                        m_struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));
                                        UpdateChannel(ref dcChannel, m_struChanInfo.byEnable, m_struChanInfo.byIPID);
                                        iIPDevID[i] = m_struChanInfo.byIPID + m_struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;
                                        Marshal.FreeHGlobal(ptrChanInfo);
                                        break;
                                    case 4:
                                        IntPtr ptrStreamURL = Marshal.AllocHGlobal((Int32)dwSize);
                                        Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrStreamURL, false);
                                        m_struStreamURL = (CHCNetSDK.NET_DVR_PU_STREAM_URL)Marshal.PtrToStructure(ptrStreamURL, typeof(CHCNetSDK.NET_DVR_PU_STREAM_URL));
                                        UpdateChannel(ref dcChannel, m_struStreamURL.byEnable, m_struStreamURL.wIPID);
                                        iIPDevID[i] = m_struStreamURL.wIPID - iGroupNo * 64 - 1;
                                        Marshal.FreeHGlobal(ptrStreamURL);
                                        break;
                                    case 6:
                                        IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal((Int32)dwSize);
                                        Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                                        m_struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));
                                        UpdateChannel(ref dcChannel, m_struChanInfoV40.byEnable, m_struChanInfoV40.wIPID);
                                        iIPDevID[i] = m_struChanInfoV40.wIPID - iGroupNo * 64 - 1;
                                        Marshal.FreeHGlobal(ptrChanInfoV40);
                                        break;
                                    default:
                                        break;
                                }

                                channels.Add(dcChannel);
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < dwAChanTotalNum; i++)
                    {
                        channels.Add(new CameraChannel()
                        {
                            ChannelNo = _deviceInfo.struDeviceV30.byStartChan + i,
                            ChannelType = (Int32)CameraChannelType.AC,
                            IsEnabled = true
                        });
                    }
                }

                return new OResult<IEnumerable<CameraChannel>>(channels);
            }
            finally
            {
                Marshal.FreeHGlobal(ptrIpParaCfgV40);
            }
        }

        private static void UpdateChannel(ref CameraChannel channel, Byte byEnable, Int32 byIPID)
        {
            if (byIPID == 0)
            {
                channel.ChannelState = (Int32)CameraChannelState.IDLE;
                channel.IsEnabled = false;
            }
            else
            {
                if (byEnable == 0)
                {
                    channel.ChannelState = (Int32)CameraChannelState.Offline;
                    channel.IsEnabled = false;
                }
                else
                {
                    channel.ChannelState = (Int32)CameraChannelState.Online;
                    channel.IsEnabled = true;
                }
            }
        }

    }
}
