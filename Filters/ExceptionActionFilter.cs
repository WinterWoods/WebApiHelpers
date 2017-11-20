using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace WebApiHelpers.Filters
{
    public class ExceptionActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                ResultBasicModel result = new ResultBasicModel();
                result.code = ResultState.Errror;

                if (filterContext.Exception.InnerException != null)
                {
                    result.message = filterContext.Exception.InnerException.Message;
                }
                else
                {
                    result.message = filterContext.Exception.Message;
                }
                filterContext.Response = new HttpResponseMessage()
                {
                    Content = new ObjectContent(typeof(ResultBasicModel), result, new JsonMediaTypeFormatter()),
                    RequestMessage = filterContext.Request,
                    StatusCode = HttpStatusCode.OK,

                };
            }
        }
    }
}
