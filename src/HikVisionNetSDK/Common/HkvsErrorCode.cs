using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikVisionNetSDK.Common
{
    /// <summary>
    /// 海康错误码
    /// </summary>
    public static class HkvsErrorCode
    {
        private static readonly Dictionary<Int32, String> HkvsErrors;

        static HkvsErrorCode()
        {
            HkvsErrors = new Dictionary<Int32, String>(200);
            HkvsErrors[1] = "用户名密码错误";
            HkvsErrors[2] = "权限不足";
            HkvsErrors[3] = "SDK未初始化";
            HkvsErrors[4] = "通道号错误";
            HkvsErrors[5] = "设备总的连接数超过最大";
            HkvsErrors[6] = "SDK和设备的版本不匹配";
            HkvsErrors[7] = "连接设备失败";
            HkvsErrors[8] = "向设备发送失败";
            HkvsErrors[9] = "从设备接收数据失败";
            HkvsErrors[10] = "从设备接收数据超时";
            HkvsErrors[11] = "传送的数据有误";
            HkvsErrors[12] = "调用次序错误";
            HkvsErrors[13] = "无权限";
            HkvsErrors[14] = "设备命令执行超时";
            HkvsErrors[15] = "串口号错误，指定的设备串口号不存在";
            HkvsErrors[16] = "报警端口错误，指定的设备报警输入或者输出端口不存在";
            HkvsErrors[17] = "参数错误";
            HkvsErrors[18] = "设备通道处于错误状态";
            HkvsErrors[19] = "设备无硬盘";
            HkvsErrors[20] = "硬盘号错误";
            HkvsErrors[21] = "设备硬盘满";
            HkvsErrors[22] = "设备硬盘出错";
            HkvsErrors[23] = "设备不支持";
            HkvsErrors[24] = "设备忙";
            HkvsErrors[25] = "设备修改不成功";
            HkvsErrors[26] = "密码输入格式不正确";
            HkvsErrors[27] = "硬盘正在格式化，不能启动操作";
            HkvsErrors[28] = "设备资源不足";
            HkvsErrors[29] = "设备操作失败";
            HkvsErrors[30] = "语音对讲、语音广播操作中采集本地音频或打开音频输出失败";
            HkvsErrors[31] = "设备语音对讲被占用";
            HkvsErrors[32] = "时间输入不正确";
            HkvsErrors[33] = "回放时设备没有指定的文件";
            HkvsErrors[34] = "创建文件出错";
            HkvsErrors[35] = "打开文件出错";
            HkvsErrors[36] = "上次的操作还没有完成";
            HkvsErrors[37] = "获取当前播放的时间出错";
            HkvsErrors[38] = "播放出错";
            HkvsErrors[39] = "文件格式不正确";
            HkvsErrors[40] = "路径错误";
            HkvsErrors[41] = "SDK资源分配错误";
            HkvsErrors[42] = "声卡模式错误";
            HkvsErrors[43] = "缓冲区不足";
            HkvsErrors[44] = "创建SOCKET出错";
            HkvsErrors[45] = "设置SOCKET出错";
            HkvsErrors[46] = "分配的注册连接数、预览连接数超过SDK支持的最大数";
            HkvsErrors[47] = "用户不存在";
            HkvsErrors[48] = "设备升级时写FLASH失败";
            HkvsErrors[49] = "设备升级失败";
            HkvsErrors[50] = "解码卡已经初始化过";
            HkvsErrors[51] = "调用播放库中某个函数失败";
            HkvsErrors[52] = "登录设备的用户数达到最大";
            HkvsErrors[53] = "获得本地PC的IP地址或物理地址失败";
            HkvsErrors[54] = "设备该通道没有启动编码";
            HkvsErrors[55] = "IP地址不匹配";
            HkvsErrors[56] = "MAC地址不匹配";
            HkvsErrors[57] = "升级文件语言不匹配";
            HkvsErrors[58] = "播放器路数达到最大";
            HkvsErrors[59] = "备份设备中没有足够空间进行备份";
            HkvsErrors[60] = "没有找到指定的备份设备";
            HkvsErrors[61] = "图像素位数不符，限24色";
            HkvsErrors[62] = "图片高宽超限，限128256";
            HkvsErrors[63] = "图片大小超限，限100K";
            HkvsErrors[64] = "载入当前目录下播放库(PlayCtrl.dll、SuperRender.dll、AudioRender.dll)出错";
            HkvsErrors[65] = "找不到Player Sdk中某个函数入口";
            HkvsErrors[66] = "载入当前目录下DSsdk出错";
            HkvsErrors[67] = "找不到DsSdk中某个函数入口";
            HkvsErrors[68] = "调用硬解码库DsSdk中某个函数失败";
            HkvsErrors[69] = "声卡被独占";
            HkvsErrors[70] = "加入多播组失败";
            HkvsErrors[71] = "建立日志文件目录失败";
            HkvsErrors[72] = "绑定套接字失败";
            HkvsErrors[73] = "socket连接中断，此错误通常是由于连接中断或目的地不可达";
            HkvsErrors[74] = "注销时用户ID正在进行某操作";
            HkvsErrors[75] = "监听失败";
            HkvsErrors[76] = "程序异常";
            HkvsErrors[77] = "写文件失败";
            HkvsErrors[78] = "禁止格式化只读硬盘";
            HkvsErrors[79] = "远程用户配置结构中存在相同的用户名";
            HkvsErrors[80] = "导入参数时设备型号不匹配";
            HkvsErrors[81] = "导入参数时语言不匹配";
            HkvsErrors[82] = "导入参数时软件版本不匹配";
            HkvsErrors[83] = "预览时外接IP通道不在线";
            HkvsErrors[84] = "加载标准协议通讯库(StreamTransClient.dll)失败";
            HkvsErrors[85] = "加载转封装库(SystemTransform.dll)失败";
            HkvsErrors[86] = "超出最大的IP接入通道数";
            HkvsErrors[87] = "添加录像标签或者其他操作超出最多支持的个数";
            HkvsErrors[88] = "图像增强仪，参数模式错误";
            HkvsErrors[89] = "码分器不在线";
            HkvsErrors[90] = "设备正在备份";
            HkvsErrors[91] = "通道不支持该操作";
            HkvsErrors[92] = "高度线位置太集中或长度线不够倾斜";
            HkvsErrors[93] = "取消标定冲突，如果设置了规则及全局的实际大小尺寸过滤";
            HkvsErrors[94] = "标定点超出范围";
            HkvsErrors[95] = "尺寸过滤器不符合要求";
            HkvsErrors[96] = "设备没有注册到ddns上";
            HkvsErrors[97] = "DDNS 服务器内部错误";
            HkvsErrors[98] = "此功能不支持该操作系统";
            HkvsErrors[99] = "解码通道绑定显示输出次数受限";
            HkvsErrors[100] = "加载当前目录下的语音对讲库失败";
            HkvsErrors[101] = "没有正确的升级包";
            HkvsErrors[102] = "用户还没登录成功";
            HkvsErrors[103] = "正在使用日志开关文件";
            HkvsErrors[104] = "端口池中用于绑定的端口已耗尽";
            HkvsErrors[105] = "码流封装格式错误";
            HkvsErrors[106] = "IP接入配置时IPID有误";
            HkvsErrors[107] = "预览组件加载失败";
            HkvsErrors[108] = "语音组件加载失败";
            HkvsErrors[109] = "报警组件加载失败";
            HkvsErrors[110] = "回放组件加载失败";
            HkvsErrors[111] = "显示组件加载失败";
            HkvsErrors[112] = "行业应用组件加载失败";
            HkvsErrors[113] = "通用配置管理组件加载失败";
            HkvsErrors[114] = "设备配置核心组件加载失败";
            HkvsErrors[121] = "单独加载组件时，组件与core版本不匹配";
            HkvsErrors[122] = "预览组件与core版本不匹配";
            HkvsErrors[123] = "语音组件与core版本不匹配";
            HkvsErrors[124] = "报警组件与core版本不匹配";
            HkvsErrors[125] = "回放组件与core版本不匹配";
            HkvsErrors[126] = "显示组件与core版本不匹配";
            HkvsErrors[127] = "行业应用组件与core版本不匹配";
            HkvsErrors[128] = "通用配置管理组件与core版本不匹配";
            HkvsErrors[136] = "预览组件与HCNetSDK版本不匹配";
            HkvsErrors[137] = "语音组件与HCNetSDK版本不匹配";
            HkvsErrors[138] = "报警组件与HCNetSDK版本不匹配";
            HkvsErrors[139] = "回放组件与HCNetSDK版本不匹配";
            HkvsErrors[140] = "显示组件与HCNetSDK版本不匹配";
            HkvsErrors[141] = "行业应用组件与HCNetSDK版本不匹配";
            HkvsErrors[142] = "通用配置管理组件与HCNetSDK版本不匹配";
            HkvsErrors[150] = "别名重复";
            HkvsErrors[152] = "用户名不存在";
            HkvsErrors[153] = "用户名被锁定";
            HkvsErrors[154] = "无效用户ID";
            HkvsErrors[155] = "登录版本低";
            HkvsErrors[156] = "加载libeay32.dll库失败";
            HkvsErrors[157] = "加载ssleay32.dll库失败";
            HkvsErrors[158] = "加载libiconv.dll库失败";
            HkvsErrors[165] = "连接测试服务器失败";
            HkvsErrors[166] = "NAS服务器挂载目录失败，目录无效或者用户名密码错误";
            HkvsErrors[167] = "NAS服务器挂载目录失败，没有权限";
            HkvsErrors[168] = "没有配置DNS，域名无效";
            HkvsErrors[169] = "没有配置网关，可能造成发送邮件失败";
            HkvsErrors[170] = "用户名密码不正确";
            HkvsErrors[171] = "设备和smtp服务器交互异常";
            HkvsErrors[172] = "FTP服务器创建目录失败";
            HkvsErrors[173] = "FTP服务器没有写入权限";
            HkvsErrors[174] = "IP冲突";
            HkvsErrors[175] = "存储池空间已满";
            HkvsErrors[176] = "云服务器存储池无效，没有配置存储池或者存储池ID错误";
            HkvsErrors[177] = "生效需要重启";
            HkvsErrors[178] = "断网续传布防连接已经存在";
            HkvsErrors[179] = "断网续传上传连接已经存在";
            HkvsErrors[180] = "导入文件格式不正确";
            HkvsErrors[181] = "导入文件内容不正确";
            HkvsErrors[182] = "HRUDP连接数超过设备限制";
            HkvsErrors[183] = "已达到端口复用最大数目";
            HkvsErrors[184] = "创建端口复用失败";
            HkvsErrors[185] = "不支持无阻塞抓图";
            HkvsErrors[186] = "已开启异步，该功能无效";
            HkvsErrors[187] = "已达到端口复用最大数目";
            HkvsErrors[188] = "连接尚未建立或连接无效";
        }

        /// <summary>
        /// 获取指定错误码对应的描述信息。
        /// </summary>
        /// <param name="errorCode">错误码</param>
        public static String GetLastErrorMessage(Int32 errorCode)
        {
            if (HkvsErrors.TryGetValue(errorCode, out String msg))
            {
                return msg;
            }

            return errorCode.ToString();
        }

        /// <summary>
        /// 获取指定错误码对应的描述信息。
        /// </summary>
        /// <param name="errorCode">错误码</param>
        public static String GetLastErrorMessage()
        {
            var errorCode = GetLastErrorCode();

            if (HkvsErrors.TryGetValue(errorCode, out String msg))
            {
                return msg;
            }

            return errorCode.ToString();
        }

        /// <summary>
        /// 获取最后一条错误码。
        /// </summary>
        public static Int32 GetLastErrorCode()
        {
            return (Int32)CHCNetSDK.NET_DVR_GetLastError();
        }
    }
}
