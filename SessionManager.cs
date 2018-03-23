using Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using WebApiHelpers;

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
             StartClass.Instance.Log.WriteInfo("当前在线人数:" + list.Count);
         });
        timer.Start();
    }
    public static string GetTicket(this HttpActionContext actionContext)
    {
        lock (lockObj)
        {
            string ticket = GetHeadersTicket(actionContext);

            SessionModel model = new SessionModel();
            model.TimeOut = false;
            model.Obj = null;
            if (string.IsNullOrEmpty(ticket) || !list.Any(a => a.Ticket == ticket))
            {
                model.Ticket = Guid.NewGuid().ToString().Replace("-", "");
            }
            else
            {
                model.Ticket = ticket;
            }

            model.LastOprTime = DateTime.Now;
            model.SessionData = new Hashtable();
            lock (lockObj)
            {
                list.Add(model);
            }
            return model.Ticket;
        }
    }
    public static void Login<T>(this HttpActionContext actionContext,T userInfo, string userKey)
    {
        string ticket = GetHeadersTicket(actionContext);
        var tmp = list.Find(a => a.Ticket == ticket);
        if (tmp != null)
        {
            lock (lockObj)
            {
                tmp.LastOprTime = DateTime.Now;
                tmp.TimeOut = false;
                tmp.UserKey = userKey;
                T t = userInfo;
                tmp.Obj = t;
            }

        }
        else
        {
            SessionModel model = new SessionModel();
            model.TimeOut = false;
            model.UserKey = userKey;
            model.Obj = userInfo;
            model.Ticket = ticket;
            model.LastOprTime = DateTime.Now;
            lock (lockObj)
            {
                list.Add(model);
            }
        }
    }
    public static void Logout(string userKey)
    {
        lock (lockObj)
        {
           var tmp = list.Find(o => o.UserKey == userKey);
            if (tmp != null)
            {
                lock (lockObj)
                {
                    tmp.TimeOut = true;
                    tmp.Obj = null;
                    tmp.UserKey = "";
                }
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
                tmp.TimeOut = true;
                tmp.Obj = null;
                tmp.UserKey = "";
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
    public static T GetUserInfo<T>(this HttpActionContext actionContext)
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
    public static string GetUserKey(this HttpActionContext actionContext)
    {
        string ticket = GetHeadersTicket(actionContext);
        var tmp = list.Find(a => a.Ticket == ticket);
        if (tmp != null)
        {
            return tmp.UserKey;
        }
        else
        {
            return "";
        }
    }
    public static string GetHeadersTicket(this HttpActionContext actionContext)
    {
        IEnumerable<string> values = null;
        if (actionContext.Request.Headers.TryGetValues("ticket", out values))
        {
            //判断是否授权
            return values.FirstOrDefault();
        }
        else
        {
            return null;
        }
    }
    public static bool Set(this HttpActionContext actionContext, string key, object value)
    {
        string ticket = GetHeadersTicket(actionContext);
        var tmp = list.Find(a => a.Ticket == ticket);
        if (tmp != null)
        {
            tmp.SessionData.Remove(key);
            tmp.SessionData.Add(key, value);
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool TryGet<T>(this HttpActionContext actionContext, string key, out T value)
    {
        value = default(T);
        object outValue = null;
        if (TryGet(actionContext, key, out outValue))
        {
            if (outValue == null)
            {
                value = default(T);
            }
            else
            {
                value = (T)outValue;
            }
            
            return true;
        }
        return false;
    }
    public static bool TryGet(this HttpActionContext actionContext, string key, out object value)
    {
        value = null;
        string ticket = GetHeadersTicket(actionContext);
        var tmp = list.Find(a => a.Ticket == ticket);
        if (tmp != null)
        {
            value = tmp.SessionData[key];
            return true;
        }
        else
        {
            return false;
        }
    }
}
public class SessionModel
{
    public string Ticket { get; set; }
    public DateTime LastOprTime { get; set; }
    public object Obj { get; set; }
    public bool TimeOut { get; set; }
    public Hashtable SessionData { get; set; }
    public string UserKey { get; set; }
}