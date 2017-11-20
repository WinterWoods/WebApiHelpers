using Helpers;
using Microsoft.Owin.Hosting;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using SZORM;

namespace MonitorServices
{
    public class StartClass
    {
        public static ConsoleHelper log = null;
        public static void Start()
        {
            try
            {
                Directory.SetCurrentDirectory("C:\\");
                //获取当前文件目录
                FileInfo file = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
                //设置工作目录为文件目录
                Directory.SetCurrentDirectory(file.DirectoryName);
                log = new ConsoleHelper();
                log.WriteInfo("获取启动地址");
                var httpUrl = ConfigurationManager.AppSettings["HttpUrl"].ToString();
                
                WebApp.Start(httpUrl);
                log.WriteInfo("启动成功:" + httpUrl);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    if (e.InnerException.Message.IndexOf("拒绝访问") > -1)
                    {
                        log.WriteError("启动失败:请以管理员方式启动");
                    }
                    else
                    {
                        log.WriteError("启动失败:" + e.InnerException.Message);
                    }
                }
                else
                {
                    log.WriteError("启动失败:" + e.Message);
                }
                //throw e;
            }
        }
    }

}
