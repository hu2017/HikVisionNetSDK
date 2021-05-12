using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HikVisionNetSDK;
using HikVisionNetSDK.Enums;
using HikVisionNetSDK.Models;
using HikVisionNetSDK.Services;
using Xunit;
using Xunit.Abstractions;

namespace RTUDriverTests
{
    /// <summary>
    /// 摄像头测试
    /// </summary>
    public class CameraTest
    {
        public ITestOutputHelper Console { get; }

        public CameraTest(ITestOutputHelper console)
        {
            this.Console = console;
        }

        //测试NVR登录
        [Fact]
        public ICameraService TestNVRLogin()
        {
            var cameraService = new CameraService();
            Assert.NotNull(cameraService);

            var loginResult = cameraService.Login(new CameraLoginRequest()
            {
                IP = "192.168.8.168",
                LoginPort = 8000, //登录端口号
                UserName = "admin",
                Password = "lct12345",
                ChannelNo = 33
            });
            Assert.True(loginResult.Success);
            return cameraService;
        }

        //测试摄像头直连登录
        [Fact]
        public ICameraService TestCameraLogin()
        {
            var cameraService = new CameraService();
            Assert.NotNull(cameraService);

            var loginResult = cameraService.Login(new CameraLoginRequest()
            {
                IP = "192.168.8.170",
                LoginPort = 8000,
                UserName = "admin",
                Password = "lct12345",
                ChannelNo = 1
            });
            Assert.True(loginResult.Success);
            return cameraService;
        }

        //测试登出
        [Fact]
        public void TestCameraLogout()
        {
            var cameraService = TestNVRLogin();
            var result = cameraService.Logout();
            Assert.True(result.Success);
        }

        //抓图
        [Fact]
        public void TestCapture()
        {
            var cameraService = TestNVRLogin();
            var result = cameraService.CapturePicture();
            Assert.True(result.Success);
        }

        //测试开始预览和停止预览
        [Fact]
        public void TestStartPreviewAndStopPreview()
        {
            var cameraService = TestNVRLogin();

            var openResult = cameraService.OpenPreview(IntPtr.Zero, StreamType.Main);
            Assert.True(openResult.Success);

            var prevService = openResult.Value;
            Assert.NotNull(prevService);

            var closeResult = prevService.Close();
            Assert.True(closeResult.Success);
        }

        //测试开始预览和停止预览
        [Fact]
        public void TestStartPreviewAndStopPreviewAndSaveData()
        {
            var cameraService = TestNVRLogin();

            var previewResult = cameraService.OpenPreview(IntPtr.Zero, StreamType.Main);

            Assert.True(previewResult.Success);

            var prevService = previewResult.Value;
            Assert.NotNull(prevService);

            var result = prevService.StartRecord("aaa.mp4");
            Assert.True(result.Success);

            Thread.Sleep(5000);

            var stopResult = prevService.StopRecord();
            Assert.True(stopResult.Success);

            var closeResult = prevService.Close();
            Assert.True(closeResult.Success);
        }

        //测试开始回放和停止回放
        [Fact]
        public void TestStartPlaybackAndStopPlayback()
        {
            var cameraService = TestNVRLogin();

            var from = DateTime.Parse("2021-03-29 09:30:00");
            var to = DateTime.Parse("2021-03-29 09:35:00");

            var openResult = cameraService.OpenPlayback(IntPtr.Zero, from, to);
            Assert.True(openResult.Success);

            var playbackService = openResult.Value;
            Assert.NotNull(playbackService);

            var stopResult = playbackService.Stop();
            Assert.True(stopResult.Success);
        }

        #region 测试锁流
        ////测试锁流
        //[Fact]
        //public void TestLockStream()
        //{
        //    var cameraService = TestCameraLogin();

        //    var result = cameraService.LockStream(new StreamTimeLockRequest()
        //    {
        //        ChannelNo = 33,
        //        BeginTime = DateTime.Parse("2021-02-24 09:30:00"),
        //        EndTime = DateTime.Parse("2021-02-24 09:30:10"),
        //        ExpiredTime = DateTime.Parse("2021-02-24 12:00:00")
        //    });

        //    Console.WriteLine(result.Message);
        //    Assert.True(result.Success);
        //}

        ////测试解锁流
        //[Fact]
        //public void TestUnLockStream()
        //{
        //    var cameraService = TestCameraLogin();

        //    var result = cameraService.UnLockStream(new StreamTimeLockRequest()
        //    {
        //        BeginTime = DateTime.Parse("2021-02-09 11:30:00"),
        //        EndTime = DateTime.Parse("2021-02-09 11:30:10"),
        //        ChannelNo = 33
        //    });

        //    Console.WriteLine(result.Message);
        //    Assert.True(result.Success);
        //} 
        #endregion

        //测试开启录像
        [Fact]
        public void TestStartRecord()
        {
            var cameraService = TestNVRLogin();
            var startResult = cameraService.StartRecord();
            Assert.True(startResult.Success);
        }

        //测试停止录像
        [Fact]
        public void TestStopRecord()
        {
            var cameraService = TestNVRLogin();
            var stopResult = cameraService.StopRecord();
            Assert.True(stopResult.Success);
        }

        //即时刷新录像索引
        [Fact]
        public void TestUpdateRecordIndex()
        {
            var cameraService = TestNVRLogin();
            var updateResult = cameraService.UpdateRecordIndex();
            Assert.True(updateResult.Success);
        }

        //测试按时间下载
        [Fact]
        public async void TestDownloadByTime()
        {
            var cameraService = TestNVRLogin();
            var dlResult = await cameraService.DownloadByTimeAsync(new DownloadFileByTimeRequest()
            {
                FilePath = Path.Combine(AppContext.BaseDirectory, DateTime.Now.ToString("yyMMddHHmmss") + ".mp4"),
                From = DateTime.Parse("2021-03-01 09:00:00"),
                To = DateTime.Parse("2021-03-01 09:00:40")
            });
            Assert.True(dlResult.Success);
        }

        //测试按时间备份
        [Fact]
        public async void TestBackupByTime()
        {
            var cameraService = TestNVRLogin();
            var from = DateTime.Parse("2021-03-01 09:00:00");
            var to = DateTime.Parse("2021-03-01 09:00:40");
            var backUpResult = await cameraService.BackupByTimeAsync(from, to);
            Assert.True(backUpResult.Success);
        }

        //测试开始移动和停止移动
        [Fact]
        public void TestStartMoveAndStopMove()
        {
            Int32 delay = 2000;
            var cameraService = TestNVRLogin();
            Assert.NotNull(cameraService);

            var moveResult = cameraService.StartPTZControl(PTZCommand.TILT_UP);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.TILT_UP);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.TILT_DOWN);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.TILT_DOWN);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.PAN_LEFT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.PAN_LEFT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.PAN_RIGHT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.PAN_RIGHT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.UP_LEFT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.UP_LEFT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.UP_RIGHT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.UP_RIGHT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.DOWN_LEFT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.DOWN_LEFT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.DOWN_RIGHT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.DOWN_RIGHT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.PAN_AUTO);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.PAN_AUTO);
            Assert.True(moveResult.Success);
        }

        //测试开始变焦和停止变焦
        [Fact]
        public void TestStartZoomAndStopZoom()
        {
            Int32 delay = 2000;
            var cameraService = TestNVRLogin();
            Assert.NotNull(cameraService);

            var moveResult = cameraService.StartPTZControl(PTZCommand.ZOOM_IN);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.ZOOM_IN);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StartPTZControl(PTZCommand.ZOOM_OUT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);

            moveResult = cameraService.StopPTZControl(PTZCommand.ZOOM_OUT);
            Assert.True(moveResult.Success);
            Thread.Sleep(delay);
        }

        //测试关机和重启
        [Fact]
        public void TestShutdownAndReboot()
        {
            var cameraService = TestNVRLogin();
            Assert.NotNull(cameraService);

            var result = cameraService.Reboot();
            Assert.True(result.Success);
        }

        //测试设置预置点
        [Fact]
        public void TestSetPreset()
        {
            var cameraService = TestNVRLogin();
            var result = cameraService.SetPreset(1);
            Assert.True(result.Success);
        }

        //测试移除预置点
        [Fact]
        public void TestRemovePreset()
        {
            var cameraService = TestNVRLogin();
            var result = cameraService.SetPreset(2);
            Assert.True(result.Success);

            result = cameraService.RemovePreset(2);
            Assert.True(result.Success);
        }

        //测试转到预置点
        [Fact]
        public void TestGotoPreset()
        {
            var cameraService = TestNVRLogin();
            var result = cameraService.SetPreset(3);
            Assert.True(result.Success);

            result = cameraService.GotoPreset(3);
            Assert.True(result.Success);
        }

        //测试月历录像分布
        [Fact]
        public void TestGetMonthRecordDist()
        {
            var cameraService = TestNVRLogin();
            var result = cameraService.GetMonthRecordDistributions(33, 2021, 3);
            Assert.True(result.Success);
        }

        //测试设备(NVR/CVR)在线状态
        [Fact]
        public void TestGetDeviceOnlineStatus()
        {
            var cameraService = TestCameraLogin();
            var result = cameraService.GetDeviceOnlineStatus();
            Assert.True(result.Success);
        }

        //测试摄像头在线状态
        [Fact]
        public void TestGetCameraOnlineStatus()
        {
            var cameraService = TestNVRLogin();
            var result = cameraService.GetDeviceOnlineStatus();
            Assert.True(result.Success);

            var captureResult = cameraService.CapturePicture();
            Assert.True(captureResult.Success);
        }

        [Fact]
        public void TestGetChannels()
        {
            var cameraService = TestNVRLogin();
            var result = cameraService.GetChannels();
            Assert.True(result.Success);

            cameraService = TestCameraLogin();
            result = cameraService.GetChannels();
            Assert.True(result.Success);
        }
    }
}
