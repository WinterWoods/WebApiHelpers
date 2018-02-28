using Newtonsoft.Json.Converters;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using WebApiHelpers.Filters;

namespace WebApiHelpers
{
    public class Startup
    {
        public static string startUpPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\www\\";
        public static Dictionary<string, byte[]> pageCache = new Dictionary<string, byte[]>();
        public static bool StartCache { get; set; }
        public static HttpConfiguration config = new HttpConfiguration();
        private object LockObject = new object();
        public void Configuration(IAppBuilder app)
        {
            if (!Directory.Exists(startUpPath))
            {
                Directory.CreateDirectory(startUpPath);
            }
            config.Services.Replace(typeof(IAssembliesResolver), new ExtendedDefaultAssembliesResolver());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
            config.Filters.Add(new ExceptionActionFilter());
            //跨域配置
            //config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}"
            );
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new IsoDateTimeConverter()
            {
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
            });
            
            //config.Filters.Add(new ApiAuthorizeAttribute(new string[] { "User/UserLogin", "User/GetTicket", "User/IsLogin" }));
            app.UseWebApi(config);

            bool IsCache = StartClass.Instance.Config.Get("WebInfo", "CacheFile", "false") == "true" ? true : false;
            string defaultPathFile = StartClass.Instance.Config.Get("WebInfo", "DefaultPage", "index.html");
            string file404= StartClass.Instance.Config.Get("WebInfo", "File_404", "404.html");
            //检测是否监听https,如果监听,自动跳转到https
            bool AutoRedirect = StartClass.Instance.Config.Get("WebInfo", "AutoRedirectHttps", "true") == "true";
            var httpUrls = StartClass.Instance.Config.Get("WebInfo", "Url", "http://127.0.0.1:8080");
            var urls = httpUrls.Split(new char[] { ',' });
            var httpsUrl = string.Empty;
            if (urls.Any(a => a.StartsWith("https")))
            {
                httpsUrl = urls.FirstOrDefault(f => f.StartsWith("https"));
            }
            app.Run(context =>
            {
                string path = defaultPathFile;

                context.Response.StatusCode = 200;
                if (context.Request.Path.Value != "/")
                {
                    if (context.Request.Path.Value.IndexOf('.') > -1)
                    {
                        path = context.Request.Path.Value;
                    }
                }

                if (!File.Exists(startUpPath + path))
                {
                    path = file404;
                }

                FileInfo fileInfo = new FileInfo(startUpPath + path);
                byte[] msg = null;
                if (IsCache)
                {
                    if (!pageCache.ContainsKey(path))
                    {
                        lock (LockObject)
                        {
                            msg = File.ReadAllBytes(startUpPath + path);
                            pageCache.Add(path, msg);
                        }
                    }
                    msg = pageCache[path];
                }
                else
                {
                    if (File.Exists(startUpPath + path))
                        msg = File.ReadAllBytes(startUpPath + path);
                    else
                        msg = new byte[0];
                }

                if (AutoRedirect && !string.IsNullOrEmpty(httpsUrl) && context.Request.Scheme == "http")
                {
                    context.Response.Redirect(httpsUrl + context.Request.Path.Value);
                    context.Response.StatusCode = 301;
                    return context.Response.WriteAsync(msg);
                }


                context.Response.ContentType = MimeMapping.GetMimeMapping(fileInfo.Name);
                context.Response.StatusCode = 200;
                context.Response.ContentLength = msg.Length;
                return context.Response.WriteAsync(msg);
            });
        }
    }
}
