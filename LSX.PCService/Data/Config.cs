using System;

using System.Configuration;
namespace LSX.PCService.Data
{
    /// <summary>
    /// 配置文件
    /// </summary>
    class Config
    {
        public static string QueuePath { get { return ConfigurationManager.AppSettings["QueuePath"]; } }
        public static string SqlServerConn { get { return ConfigurationManager.AppSettings["SqlServerConn"]; } }
        public static int LightCount
        {
            get
            {
                try { return int.Parse(ConfigurationManager.AppSettings["LightCount"]); }
                catch
                {
                    throw new Exception("配置中LightCount必须为大于0的整数");
                }
            }
        }
        public static int ChannelCount
        {
            get
            {
                try { return int.Parse(ConfigurationManager.AppSettings["ChannelCount"]); }
                catch
                {
                    throw new Exception("配置中ChannelCount必须为大于0的整数");
                }
            }
        }

        public static bool InitDataBase
        {
            get
            {
                try { return bool.Parse(ConfigurationManager.AppSettings["InitDataBase"]); }
                catch
                {
                    throw new Exception("配置中InitDataBase必须为0/1/false/true");
                }
            }
        }

        public static int LightServerPort
        {
            get
            {
                try { return int.Parse(ConfigurationManager.AppSettings["LightServerPort"]); }
                catch
                {
                    throw new Exception("配置中LightServerPort必须为大于1000的整数");
                }
            }
        }
        public static int CameraServerPort
        {
            get
            {
                try { return int.Parse(ConfigurationManager.AppSettings["CameraServerPort"]); }
                catch
                {
                    throw new Exception("配置中CameraServerPort必须为大于1000的整数");
                }
            }
        }
        public static int ChannelServerPort
        {
            get
            {
                try { return int.Parse(ConfigurationManager.AppSettings["ChannelServerPort"]); }
                catch
                {
                    throw new Exception("配置中ChannelServerPort必须为大于1000的整数");
                }
            }
        }
    }
}
