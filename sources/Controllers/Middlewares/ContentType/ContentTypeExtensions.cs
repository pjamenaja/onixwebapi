using Onix.WebApi.Controllers.Middlewares.Serialzer;

namespace Microsoft.AspNetCore.Builder
{
    public static class ContentTypeExtensions
    {
        public static IApplicationBuilder UseOnixContentType(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ContentTypeMiddleware>();
        }
    }
}