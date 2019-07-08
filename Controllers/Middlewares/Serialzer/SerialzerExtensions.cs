using Onix.WebApi.Controllers.Middlewares.Serialzer;

namespace Microsoft.AspNetCore.Builder
{
    public static class SerialzerExtensions
    {
        public static IApplicationBuilder UseOnixSerialzer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerialzerMiddleware>();
        }
    }
}