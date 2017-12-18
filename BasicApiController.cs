using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

public enum ResultState
{
    OK=0,
    Errror=1,
    NoLogin=2,
    NoAuth=3
}
public class ResultBasicModel
{
    public ResultState code { get; set; }
    public string message { get; set; }
}
public class ResultBadModel<T> : ResultBasicModel
{
    public T data { get; set; }
}
public class KeyModel
{
    public string Key { get; set; }
}
public class KeysModel
{
    public string Keys { get; set; }
}
public class BasicApiController : ApiController
{
    public IHttpActionResult Error(string msg)
    {
        ResultBasicModel result = new ResultBasicModel();
        result.code = ResultState.Errror;
        result.message = msg;
        return Ok(result);
    }
    public IHttpActionResult Success()
    {
        ResultBasicModel result = new ResultBasicModel();
        result.code = ResultState.OK;
        result.message = "";
        return Ok(result);
    }
    public IHttpActionResult Success<T>(T obj)
    {
        ResultBadModel<T> result = new ResultBadModel<T>();
        result.data = obj;
        result.code = ResultState.OK;
        result.message = "";
        return Ok(result);
    }
    public IHttpActionResult NoLogin(string msg)
    {
        ResultBasicModel result = new ResultBasicModel();
        result.code = ResultState.NoLogin;
        result.message = msg;
        return Ok(result);
    }
    public IHttpActionResult NoAuth(string msg)
    {
        ResultBasicModel result = new ResultBasicModel();
        result.code = ResultState.NoAuth;
        result.message = msg;
        return Ok(result);
    }
}