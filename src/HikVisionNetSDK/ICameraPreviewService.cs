using System;
using System.Collections.Generic;
using System.Text;
using Office365Fx.Core;

namespace HikVisionNetSDK
{
    public interface ICameraPreviewService : IDisposable
    {
        /// <summary>
        /// 打开实时预览。
        /// </summary>
        OResult<Boolean> Open();
        /// <summary>
        /// 开始录像。
        /// </summary>
        /// <param name="fileName">录像文件名</param>
        OResult<String> StartRecord(String fileName);
        /// <summary>
        /// 停止录像。
        /// </summary>
        OResult<Boolean> StopRecord();
        /// <summary>
        /// 关闭实时预览。
        /// </summary>
        OResult<Boolean> Close();
    }
}
