using log4net;
using log4net.Config;
using log4net.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json;

namespace WebAPI.Middleware
{

    public static class LogTraceFactory
    {

        private static ILog _logger = LogManager.GetLogger(typeof(LoggerManager));

        public static void LogMessage(string msg)
        {
            try
            {
                XmlDocument log4netConfig = new XmlDocument();

                using (var fs = File.OpenRead("log4net.config"))
                {
                    log4netConfig.Load(fs);
                    var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
                    XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
                    _logger.Error(msg);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error", ex);
            }
        }
    }
    public class ExceptionHandler
    {
        private readonly RequestDelegate next;

        public ExceptionHandler(RequestDelegate next)
        {
            this.next = next;

        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }


        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            LogTraceFactory.LogMessage(ex.Message);
            //logger.Error(ex);
            var result = JsonConvert.SerializeObject(new
            {
                status_code = context.Response.StatusCode,
                Message = "Error from the custom middleware =======> " + ex.Message + " Date : " + DateTime.Now + "<==========",
                data = ""
            });
            return context.Response.WriteAsync(result);

        }
        public class ValidateModelAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                if (!context.ModelState.IsValid)
                {
                    var model = new
                    {
                        Staus_Code = (int)HttpStatusCode.BadRequest,
                        Error = context.ModelState
                    .SelectMany(keyValuePair => keyValuePair.Value.Errors)
                    .Select(modelError => modelError.ErrorMessage)
                    .ToArray()
                    };

                    context.Result = new BadRequestObjectResult(model);
                    //context.Result = new BadRequestObjectResult(context.ModelState);
                }
            }
        }
    }
}
