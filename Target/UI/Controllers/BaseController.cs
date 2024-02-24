using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
using NLog;
using BLL;

using System.Text;
using DAL.Core;

namespace Entity.Controllers
{
    public class BaseController : Controller
    {
        public string UserName{get;}

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            UserName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
            ViewBag.UserName = UserName;
            log.Debug("Current user =" + UserName);
        }

        protected Logger log = LogManager.GetCurrentClassLogger();

    //    protected UnitOfWork uow = new UnitOfWork();

        protected JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();

        public string GetJsonSafe(Func<object> function)
        {
            dynamic expOb = new ExpandoObject();
            try
            {
                expOb.Item = function();
            }
            catch (Exception ex)
            {
                expOb.Errors = new List<ObjectError>() { new ObjectError("", error: ex.GetAllMessages()) };
                log.Error(ex);
            }
            var json = JsonConvert.SerializeObject(expOb, JsonSerializerSettings);
            return json;
        }

        public string GetJsonSafe(Action function)
        {
            dynamic expOb = new ExpandoObject();
            try
            {
                function();

            }
            catch (Exception ex)
            {
                expOb.Errors = new List<ObjectError>() { new ObjectError("", ex.GetAllMessages()) };
                log.Error(ex);

            }
            var json = JsonConvert.SerializeObject(expOb, JsonSerializerSettings);
            return json;
        }

        protected string Body()
        {
            var request = HttpContext.Request;
            var bodyText = string.Empty;
            request.EnableBuffering();
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyText = reader.ReadToEnd();
            }
            request.Body.Position = 0;
            return bodyText;
        }

    }
}

