using Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;


    public static class SessionManager
    {
        private static List<SessionModel> list;
        private static object lockObj = new object();
        public static int SessionOutTimer = 50;
        public static int SessionClearTimer = 60 * 12;
        private static TimerTaskService timer;
        static SessionManager()
        {
            list = new List<SessionModel>();

            timer = TimerTaskService.CreateTimerTaskService(new TimerInfo { TimerType = TimerType.LoopStop, Hour = 1 }, () =>
             {
                 lock (lockObj)
                 {
                     list.RemoveAll(r => (DateTime.Now - r.LastOprTime) > new TimeSpan(0, SessionOutTimer, 0));
                 }
             });
            timer.Start();
        }
        public static string NewUser(this HttpActionContext actionContext)
        {
            lock (lockObj)
            {
                SessionModel model = new SessionModel();
                model.TimeOut = false;
                model.Obj = null;
                model.Ticket = Guid.NewGuid().ToString().Replace("-", "");
                model.LastOprTime = DateTime.Now;
                model.sessionData = new Hashtable();
                lock (lockObj)
                {
                    list.Add(model);
                }
                return model.Ticket;
            }
        }
        public static void Login<T>(this HttpActionContext actionContext, T user)
        {
            string ticket = GetHeadersTicket(actionContext);
            var tmp = list.Find(a => a.Ticket == ticket);
            if (tmp != null)
            {
                lock (lockObj)
                {
                    tmp.LastOprTime = DateTime.Now;
                    tmp.TimeOut = false;
                    tmp.Obj = user;
                }

            }
            else
            {
                SessionModel model = new SessionModel();
                model.TimeOut = false;
                model.Obj = user;
                model.Ticket = ticket;
                model.LastOprTime = DateTime.Now;
                lock (lockObj)
                {
                    list.Add(model);
                }
            }
        }
        public static void Logout(this HttpActionContext actionContext)
        {
            string ticket = GetHeadersTicket(actionContext);
            var tmp = list.Find(a => a.Ticket == ticket);
            if (tmp != null)
            {
                lock (lockObj)
                {
                    list.Remove(tmp);
                }
            }
        }
        public static bool IsLogin(this HttpActionContext actionContext)
        {
            string ticket = GetHeadersTicket(actionContext);
            var tmp = list.Find(a => a.Ticket == ticket);
            if (tmp != null)
            {
                if (!tmp.TimeOut)
                {
                    lock (lockObj)
                    {
                        tmp.LastOprTime = DateTime.Now;
                    }
                    return true;
                }
            }
            return false;
        }
        public static T GetUser<T>(this HttpActionContext actionContext)
        {
            string ticket = GetHeadersTicket(actionContext);
            var tmp = list.Find(a => a.Ticket == ticket);
            if (tmp != null)
            {
                return (T)tmp.Obj;
            }
            else
            {
                return default(T);
            }

        }
        public static string GetHeadersTicket(this HttpActionContext actionContext)
        {
            IEnumerable<string> values = null;
            if (actionContext.Request.Headers.TryGetValues("ticket",out values))
            {
                //判断是否授权
                return values.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
        public static void Set(this HttpActionContext actionContext, string key, object value)
        {
            string ticket = GetHeadersTicket(actionContext);
            var tmp = list.Find(a => a.Ticket == ticket);
            if (tmp != null)
            {
                tmp.sessionData.Add(key, value);
            }
        }
        public static object Get(this HttpActionContext actionContext, string key)
        {
            string ticket = GetHeadersTicket(actionContext);
            var tmp = list.Find(a => a.Ticket == ticket);
            if (tmp != null)
            {
                return tmp.sessionData[key];
            }
            return null;
        }
    }
public class SessionModel
{
    public string Ticket { get; set; }
    public DateTime LastOprTime { get; set; }
    public object Obj { get; set; }
    public bool TimeOut { get; set; }
    public Hashtable sessionData { get; set; }
}