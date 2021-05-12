using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using HikVisionNetSDK.Models;
using Office365Fx.Core;

namespace HikVisionNetSDK.Codec
{
    /// <summary>
    /// WebSocket转码器，提供将RSTP流转换成WebSocket流的方法。
    /// </summary>
    public sealed class WebSocketTranscoder
    {
        private readonly ConcurrentDictionary<String, TranscodeChannel> _transcodeChannels = new ConcurrentDictionary<String, TranscodeChannel>();
        private readonly ConcurrentDictionary<Int32, Boolean> _usedPorts = new ConcurrentDictionary<Int32, Boolean>();

        private readonly String _ffMpegPath;
        private readonly String _nodePath;
        private readonly String _wsHost;

        /// <summary>
        /// 初始化一个转码器。
        /// </summary>
        /// <param name="ffmpegPath">ffmpeg.exe文件路径</param>
        /// <param name="nodePath">node.exe文件路径</param>
        /// <param name="wsHost">本机对应的局域网地址或公网地址，以便生成可以从网络其它位置访问的websocket取流地址。</param>
        public WebSocketTranscoder(String ffmpegPath, String nodePath, String wsHost)
        {
            if (ffmpegPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(ffmpegPath));
            }

            if (!File.Exists(ffmpegPath))
            {
                throw new FileNotFoundException($"文件\"{ffmpegPath}\"不存在");
            }

            if (nodePath.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(nodePath));
            }

            if (!File.Exists(nodePath))
            {
                throw new FileNotFoundException($"文件\"{nodePath}\"不存在");
            }

            if (wsHost.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(wsHost));
            }

            _ffMpegPath = ffmpegPath;
            _nodePath = nodePath;
            _wsHost = wsHost;
        }

        /// <summary>
        /// 开启对指定设备的转码。
        /// </summary>
        /// <param name="device">要进行转码的设备</param>
        /// <param name="secret">设置转码需要的密钥</param>
        /// <param name="width">转码后的画面宽度</param>
        /// <param name="height">转码后的画面高度</param>
        /// <param name="startTime">转码截取的起始时间</param>
        /// <param name="endTime">转码截取的截止时间</param>
        public OResult<String> Start(CameraDevice device, String secret, Int32 width = 1920, Int32 height = 1080, DateTime? startTime = null, DateTime? endTime = null)
        {
            if (_transcodeChannels.ContainsKey(device.Id))
            {
                var ch = _transcodeChannels[device.Id];
                var url = BuildWebsocketUrl(ch);
                return new OResult<String>(url);
            }

            if (secret.IsNullOrWhiteSpace())
            {
                return new OResult<String>(default, "未提供转码密钥");
            }

            var startNodeResult = StartNodeServer(secret.Replace(" ", String.Empty));
            if (!startNodeResult.Success)
            {
                return new OResult<String>(default, startNodeResult.Message);
            }

            var transcodeRequest = new TranscodeRequest()
            {
                IP = device.IP,
                LoginPort = device.StreamPort,
                ChannelNo = device.ChannelNo,
                UserName = device.UserName,
                Password = device.Password,
                Width = width,
                Height = height,
                StartTime = startTime,
                EndTime = endTime,
                WebSocketUrl = $"http://127.0.0.1:{startNodeResult.Value.StreamPort}/{startNodeResult.Value.Secret}"
            };

            var startTranscodeResult = StartFFMepegServer(transcodeRequest);

            if (!startTranscodeResult.Success)
            {
                return new OResult<String>(default, startTranscodeResult.Message);
            }

            var channel = new TranscodeChannel()
            {
                InputChannel = startTranscodeResult.Value,
                OutputChannel = startNodeResult.Value
            };

            _transcodeChannels.TryAdd(device.Id, channel);

            return new OResult<String>(BuildWebsocketUrl(channel));
        }

        /// <summary>
        /// 构造WebSocket流的访问地址。
        /// </summary>
        /// <param name="ch">转码通道</param>
        private String BuildWebsocketUrl(TranscodeChannel ch)
        {
            return $"ws://{_wsHost}:{ch.OutputChannel.WebSocketPort}/";
        }

        /// <summary>
        /// 停止对指定设备的转码。
        /// </summary>
        /// <param name="deviceId">设备Id</param>
        public void Stop(String deviceId)
        {
            if (_transcodeChannels.TryGetValue(deviceId.ToString(), out TranscodeChannel channel))
            {
                var stopFFMpegServerResult = StopFFMpegServer(channel.InputChannel.Process);
                var stopNodeServerResult = StopNodeServer(channel.OutputChannel.Process);

                if (stopFFMpegServerResult.Success
                    && stopNodeServerResult.Success)
                {
                    _transcodeChannels.TryRemove(deviceId.ToString(), out _);
                    _usedPorts.TryRemove(channel.OutputChannel.WebSocketPort, out _);
                    _usedPorts.TryRemove(channel.OutputChannel.StreamPort, out _);
                }
            }
        }

        /// <summary>
        /// 停止对所有设备的转码。
        /// </summary>
        public void StopAll()
        {
            if (_transcodeChannels.Any())
            {
                foreach (var deviceId in _transcodeChannels.Keys.ToList())
                {
                    Stop(deviceId);
                }
            }
        }

        private OResult<WebSocketChannel> StartNodeServer(String secret)
        {
            try
            {
                var wsLiveVideoPath = Path.Combine(AppContext.BaseDirectory, @"lib\wsLiveVideo\wsLiveVideo.js");
                if (!File.Exists(wsLiveVideoPath))
                {
                    return new OResult<WebSocketChannel>(null, new FileNotFoundException(wsLiveVideoPath));
                }

                var streamPort = GetValidPort();
                var websocketPort = GetValidPort();

                var startInfo = new ProcessStartInfo()
                {
                    FileName = _nodePath,
                    Arguments = $"\"{wsLiveVideoPath}\" {secret} {streamPort} {websocketPort}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = new Process() { StartInfo = startInfo };

                //process.OutputDataReceived += (s, e) =>
                //{
                //    if (e.Data.HasValue())
                //    {
                //        LogHelper.Info(_taskName, e.Data);
                //    }
                //};

                //process.ErrorDataReceived += (s, e) =>
                //{
                //    if (e.Data.HasValue())
                //    {
                //        LogHelper.Info(_taskName, e.Data);
                //    }
                //};

                //process.Exited += (s, e) =>
                //{
                //    LogHelper.Info(_taskName, "Node服务已退出");
                //};

                process.Start();

                process.BeginOutputReadLine();

                var channel = new WebSocketChannel()
                {
                    Process = process,
                    Secret = secret,
                    StreamPort = streamPort,
                    WebSocketPort = websocketPort
                };

                return new OResult<WebSocketChannel>(channel);
            }
            catch (Exception ex)
            {
                return new OResult<WebSocketChannel>(null, ex);
            }
        }

        private static String BuildRSTPUrl(TranscodeRequest request)
        {
            if (request.StartTime == null && request.EndTime == null)
            {
                //老版本的设备，端口号从33开始
                if (request.ChannelNo >= TranscodeRequest.CHANNEL_START_NO)
                {
                    return $"rtsp://{request.UserName}:{request.Password}@{request.IP}:{request.LoginPort}/h265/ch{request.ChannelNo}/main/av_stream";
                }
                else //新版本的设备，端口号从1开始
                {
                    return $"rtsp://{request.UserName}:{request.Password}@{request.IP}:{request.LoginPort}/Streaming/Channels/{request.ChannelNo}01";
                }
            }
            else
            {
                //按起止时间构造RTSP取流地址
                var starttime = $"{request.StartTime:yyyyMMdd}t{request.StartTime:HHmmss}z";
                var endtime = request.EndTime != null ? $"{request.EndTime:yyyyMMdd}t{request.EndTime:HHmmss}z" : null;

                var chNo = request.ChannelNo >= TranscodeRequest.CHANNEL_START_NO ? (request.ChannelNo - TranscodeRequest.CHANNEL_START_NO + 1) : request.ChannelNo;
                var url = $"rtsp://{request.UserName}:{request.Password}@{request.IP}:{request.LoginPort}/Streaming/tracks/{chNo}01?starttime={starttime}";
                if (endtime.HasValue())
                {
                    url += $"&endtime={endtime}";
                }

                return url;
            }
        }

        private OResult<StreamChannel> StartFFMepegServer(TranscodeRequest request)
        {
            try
            {
                var rtspUri = BuildRSTPUrl(request);

                var startInfo = new ProcessStartInfo()
                {
                    FileName = _ffMpegPath,
                    Arguments = $"-rtsp_transport tcp -i \"{rtspUri}\" -buffer_size 1024000 -max_delay 500000 -stimeout 20000000 -acodec copy -ar 44100 -b:a 128k -an -f mpegts -codec:v mpeg1video -vf scale={request.Width}:{request.Height} -s {request.Width}x{request.Height} {request.WebSocketUrl}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = new Process() { StartInfo = startInfo };

                //process.ErrorDataReceived += (s, e) =>
                //{
                //    if (e.Data.HasValue())
                //    {
                //        if (e.Data.StartsWith("frame=", StringComparison.OrdinalIgnoreCase))
                //        {
                //            return;
                //        }

                //        LogHelper.Info(_taskName, e.Data);
                //    }
                //};

                //process.OutputDataReceived += (s, e) =>
                //{
                //    if (e.Data.HasValue())
                //    {
                //        LogHelper.Info(_taskName, e.Data);
                //    }
                //};

                //process.Exited += (s, e) =>
                //{
                //    LogHelper.Info(_taskName, "转码服务已退出");
                //};

                process.Start();

                process.BeginErrorReadLine();

                var channel = new StreamChannel()
                {
                    Process = process,
                    WebSocketUrl = request.WebSocketUrl,
                    RTSPUri = rtspUri
                };

                return new OResult<StreamChannel>(channel);
            }
            catch (Exception ex)
            {
                return new OResult<StreamChannel>(null, ex);
            }
        }

        private static OResult<Boolean> StopNodeServer(Process process)
        {
            try
            {
                var existedProcesses = Process.GetProcessesByName(process.ProcessName);
                if (existedProcesses == null || !existedProcesses.Any())
                {
                    return new OResult<Boolean>(true);
                }

                foreach (var existeProc in existedProcesses)
                {
                    if (existeProc.HasExited)
                    {
                        continue;
                    }

                    if (existeProc.Id == process.Id)
                    {
                        KillProcess(existeProc.Id);
                    }
                }

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(default, ex);
            }
        }

        private static OResult<Boolean> StopFFMpegServer(Process process)
        {
            try
            {
                var existedProcesses = Process.GetProcessesByName(process.ProcessName);
                if (existedProcesses == null || !existedProcesses.Any())
                {
                    return new OResult<Boolean>(true);
                }

                var existedProc = existedProcesses.Where(x => x.Id == process.Id).FirstOrDefault();
                if (existedProc == null)
                {
                    return new OResult<Boolean>(true);
                }

                KillProcess(existedProc.Id);
                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(default, ex);
            }
        }

        private static OResult<Boolean> KillProcess(params Int32[] pids)
        {
            try
            {
                var pidsArgs = String.Join(' ', pids.Select(x => $"/pid {x}"));

                var proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = "taskkill",
                    Arguments = $"/f /t {pidsArgs}"
                });

                proc.WaitForExit();

                return new OResult<Boolean>(true);
            }
            catch (Exception ex)
            {
                return new OResult<Boolean>(default, ex);
            }
        }

        /// <summary>
        /// 获取一个未被使用的端口号。
        /// </summary>
        private Int32 GetValidPort()
        {
            for (var port = 8000; port < 50000; port++)
            {
                if (_usedPorts.ContainsKey(port))
                {
                    continue;
                }

                if (!PortInUse(port))
                {
                    _usedPorts.TryAdd(port, true);
                    return port;
                }
            }

            return -1;
        }

        /// <summary>
        /// 判断指定端口号是否已被使用。
        /// </summary>
        /// <param name="port">端口号</param>
        private static Boolean PortInUse(Int32 port)
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var ipEndPoints = ipProperties.GetActiveTcpListeners();
            return ipEndPoints.Where(x => x.Port == port).Any();
        }
    }
}
