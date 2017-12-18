using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;

namespace WebApiHelpers.Filters
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public string Groups { get; set; }
        /// <summary>
        /// 权限
        /// </summary>
        public string Jurisdictions { get; set; }
        /// <summary>
        /// 判断是否登录,及有权限
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            string ActionName = actionContext.ActionDescriptor.ActionName;
            string ControllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            IEnumerable<string> values = null;
            if (actionContext.Request.Headers.TryGetValues("ticket", out values))
            {
                if (values.First().StartsWith("Pay_"))
                {
                    string _Ticket = values.First().Replace("Pay_", "");
                    if(MD51.PwdIsRight(_Ticket, "sz06181102#@!"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                //如果获取到
                return actionContext.IsLogin();
                //判断是否登录
            }
            else
            {
                //如果没有获取到
                return false;
            }
            
        }
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
            ResultBasicModel result = new ResultBasicModel();
            result.code = ResultState.NoLogin;
            actionContext.Response = new HttpResponseMessage()
            {

                Content = new ObjectContent(typeof(ResultBasicModel), result, new JsonMediaTypeFormatter()),
                RequestMessage = actionContext.Request,
                StatusCode = HttpStatusCode.OK,

            };
        }
        public class ttt
        {
            public string Message { get; set; }
        }
        /// <summary>
        /// 判断是否有权限
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
            string controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = actionContext.ActionDescriptor.ActionName;
            string roles ="";
            if (!string.IsNullOrWhiteSpace(roles))
            {
                //this.Roles = roles.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
            
        }

    }
}
