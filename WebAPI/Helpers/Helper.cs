using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RestSharp;

namespace WebAPI.Helpers
{
    public class Helper
    {
        public static string IPAddress(HttpContext ctx)
        {
            var request = ctx.Request;
            if (request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return request.Headers["X-Forwarded-For"];
            }
            else
            {
                return ctx.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
        public static Task<RestResponse> ExecuteAPi(string endpoint, string header = "")
        {

            string api_key = "";
            var client = new RestClient(endpoint);
            client.AddDefaultHeader("x-api-key", api_key);
            return client.ExecuteAsync(new RestRequest());
        }

        public static string GetCurrentDirectory() { 
            var result = Directory.GetCurrentDirectory();
            return result;
        
        }
        public static string GetStaticDirectory() 
        {
            var result = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\");
            if(!Directory.Exists(result)) 
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }
        public static string GetFilePath(string FileName)
        {
            var GetStaticContentDirectory = GetStaticDirectory();
            var result = Path.Combine(GetStaticContentDirectory, FileName);
            return result;
        }
    }
}
