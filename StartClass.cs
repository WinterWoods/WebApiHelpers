using Helpers;
using Microsoft.Owin.Hosting;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using SZORM;

namespace WebApiHelpers
{
    public class StartClass
    {
        private static StartClass instance = null;
        private static readonly object padlock = new object();
        ConsoleHelper log;
        ConfigHelper config;
        public static StartClass Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new StartClass();
                    }
                    return instance;
                }
            }
        }

        public ConsoleHelper Log { get => log; set => log = value; }
        public ConfigHelper Config { get => config; set => config = value; }

        public StartClass()
        {
            Directory.SetCurrentDirectory("C:\\");
            //获取当前文件目录
            FileInfo file = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            //设置工作目录为文件目录
            Directory.SetCurrentDirectory(file.DirectoryName);

            Log = new ConsoleHelper("Log\\");

            Config = new ConfigHelper("config.txt");
        }
        public void Start()
        {
            try
            {
                Directory.SetCurrentDirectory("C:\\");
                //获取当前文件目录
                FileInfo file = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
                //设置工作目录为文件目录
                Directory.SetCurrentDirectory(file.DirectoryName);
                Log = new ConsoleHelper();
                Log.WriteInfo("获取启动地址");
                string httpUrl = Config.Get("WebInfo", "Url", "http://127.0.0.1:8080");


                WebApp.Start(httpUrl);
                Log.WriteInfo("启动成功:" + httpUrl);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    if (e.InnerException.Message.IndexOf("拒绝访问") > -1)
                    {
                        Log.WriteError("启动失败:请以管理员方式启动");
                    }
                    else
                    {
                        Log.WriteError("启动失败:" + e.InnerException.Message);
                    }
                }
                else
                {
                    Log.WriteError("启动失败:" + e.Message);
                }
                //throw e;
            }
        }
    }

}
