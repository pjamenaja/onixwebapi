using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Onix.WebApi.Controllers.Middlewares.Serialzer
{
    public class ContentTypeMiddleware
    {
        private readonly RequestDelegate nextDelegate;

        public ContentTypeMiddleware(RequestDelegate next)
        {
            nextDelegate = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //Response content type can be based on request content type

            context.Response.OnStarting((state) =>
            {
                context.Response.ContentType = "application/xml; charset=UTF-8";
                return Task.FromResult(0);
                
            }, null); 

            await nextDelegate(context);
        }
    }
}
