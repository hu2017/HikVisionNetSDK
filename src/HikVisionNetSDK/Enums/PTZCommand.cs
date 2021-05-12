using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikVisionNetSDK.Enums
{
    /// <summary>
    /// 云台控制命令
    /// </summary>
    public enum PTZCommand
    {
        /// <summary>
        /// 接通灯光电源
        /// </summary>
        [Description("接通灯光电源")]
        LIGHT_PWRON = 2,
        /// <summary>
        /// 接通雨刷开关
        /// </summary>
        [Description("接通雨刷开关")]
        WIPER_PWRON = 3,
        /// <summary>
        /// 接通风扇开关
        /// </summary>
        [Description("接通风扇开关")]
        FAN_PWRON = 4,
        /// <summary>
        /// 接通加热器开关
        /// </summary>
        [Description("接通加热器开关")]
        HEATER_PWRON = 5,
        /// <summary>
        /// 接通辅助设备开关
        /// </summary>
        [Description("接通辅助设备开关")]
        AUX_PWRON1 = 6,
        /// <summary>
        /// 接通辅助设备开关
        /// </summary>
        [Description("接通辅助设备开关")]
        AUX_PWRON2 = 7,
        /// <summary>
        /// 焦距变大
        /// </summary>
        [Description("焦距变大")]
        ZOOM_IN = 11,
        /// <summary>
        /// 焦距变小
        /// </summary>
        [Description("焦距变小")]
        ZOOM_OUT = 12,
        /// <summary>
        /// 焦点前调
        /// </summary>
        [Description("焦点前调")]
        FOCUS_NEAR = 13,
        /// <summary>
        /// 焦点后调
        /// </summary>
        [Description("焦点后调")]
        FOCUS_FAR = 14,
        /// <summary>
        /// 光圈扩大
        /// </summary>
        [Description("光圈扩大")]
        IRIS_OPEN = 15,
        /// <summary>
        /// 光圈缩小
        /// </summary>
        [Description("光圈缩小")]
        IRIS_CLOSE = 16,
        /// <summary>
        /// 云台以SS的速度上仰
        /// </summary>
        [Description("上仰")]
        TILT_UP = 21,
        /// <summary>
        /// 云台以SS的速度下俯
        /// </summary>
        [Description("下俯")]
        TILT_DOWN = 22,
        /// <summary>
        /// 云台以SS的速度左转
        /// </summary>
        [Description("左转")]
        PAN_LEFT = 23,
        /// <summary>
        /// 云台以SS的速度右转
        /// </summary>
        [Description("右转")]
        PAN_RIGHT = 24,
        /// <summary>
        /// 云台以SS的速度上仰和左转
        /// </summary>
        [Description("上仰和左转")]
        UP_LEFT = 25,
        /// <summary>
        /// 云台以SS的速度上仰和右转
        /// </summary>
        [Description("上仰和右转")]
        UP_RIGHT = 26,
        /// <summary>
        /// 云台以SS的速度下俯和左转
        /// </summary>
        [Description("下俯和左转")]
        DOWN_LEFT = 27,
        /// <summary>
        /// 云台以SS的速度下俯和右转
        /// </summary>
        [Description("下俯和右转")]
        DOWN_RIGHT = 28,
        /// <summary>
        /// 云台以SS的速度左右自动扫描
        /// </summary>
        [Description("左右自动扫描")]
        PAN_AUTO = 29
    }
}
