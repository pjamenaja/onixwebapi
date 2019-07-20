using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Onix.Api.Utils.Serializers;

namespace Onix.WebApi.Controllers.Middlewares.Serialzer
{
    public class SerialzerMiddleware
    {
        private readonly RequestDelegate nextDelegate;

        public SerialzerMiddleware(RequestDelegate next)
        {
            nextDelegate = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {                                    
            CRoot requestTupple = null;
            string errCode = "";

            using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                string bodyStr = reader.ReadToEnd();
                ICTableDeserializer convert0 = new XmlToCTable(bodyStr);
                requestTupple = convert0.Deserialize();

                var param = requestTupple.Param;
                errCode = param.GetFieldValue("ERROR_CODE");

                context.Items["REQUEST_TUPPLE"] = requestTupple;
            }

            if (errCode.Equals(""))
            {
                await nextDelegate(context);
            }
            else
            {
                //Bypass the next middleware
                context.Items["RESPONSE_TUPPLE"] = requestTupple;
            }

            string authenResult = (string) context.Items["AUTHENTICATION_RESULT"];
            if (!"SUCCESS".Equals(authenResult))
            {
                //Not yet authentication from Middleware
                context.Response.StatusCode = 401;
                context.Response.Headers["WWW-Authenticate"] = "Unauthorized";
                return;
            }

            CRoot root = (CRoot)context.Items["RESPONSE_TUPPLE"];
            var response = context.Response;

            if (root != null)
            {
                ICTableSerializer convert1 = new CTableToXml(root);
                string xml = convert1.Serialize();
                await context.Response.WriteAsync(xml);
            }
        }
    }
}
