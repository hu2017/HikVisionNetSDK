using System;
using HikVisionNetSDK;
using HikVisionNetSDK.Codec;
using HikVisionNetSDK.Models;
using Xunit;

namespace HkvsNetDrvierTests
{
    public class WebSocketTranscoderTest
    {
        /// <summary>
        /// 直连摄像头取流
        /// </summary>
        [Fact]
        public void TestStartCVR()
        {
            var transcoder = new WebSocketTranscoder(@"C:\ffmpeg\bin\ffmpeg.exe", @"C:\nodejs\node.exe", "192.168.8.89");
            var device1 = new CameraDevice()
            {
                Id = "001",
                IP = "192.168.8.170",
                StreamPort = 554,//取流端口号
                ChannelNo = 1,
                UserName = "admin",
                Password = "lct12345"
            };

            var startResult = transcoder.Start(device1, "123456");
            Assert.True(startResult.Success);

            transcoder.Stop(device1.Id);
            Assert.NotNull(startResult.Value);
        }

        /// <summary>
        /// 通过NVR连接摄像头取流
        /// </summary>
        [Fact]
        public void TestStartNVR()
        {
            var transcoder = new WebSocketTranscoder(@"C:\ffmpeg\bin\ffmpeg.exe", @"C:\nodejs\node.exe", "192.168.8.89");
            var device2 = new CameraDevice()
            {
                Id = "002",
                IP = "192.168.8.168",
                StreamPort = 554,
                ChannelNo = 33,
                UserName = "admin",
                Password = "lct12345"
            };

            var startResult = transcoder.Start(device2, "78910");
            Assert.True(startResult.Success);

            transcoder.Stop(device2.Id);
            Assert.NotNull(startResult.Value);
        }
    }
}
