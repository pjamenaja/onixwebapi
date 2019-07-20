using System;
using System.Text; 
using System.Threading.Tasks; 

using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Http;

using Onix.Api.Utils.Serializers;

namespace Onix.WebApi.Controllers.Commons
{    
    public class SerializerCTableFormatterXML : TextOutputFormatter  
    {        
        public SerializerCTableFormatterXML()
        {       
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));  
        
            SupportedEncodings.Add(Encoding.UTF8);  
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)  
        {  
            return (typeof(CRoot).IsAssignableFrom(type));  
        } 

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)  
        {  
            CRoot root = (CRoot) context.Object;
            var response = context.HttpContext.Response;  

            CTableToXml convert1 = new CTableToXml(root);
            string xml =convert1.Serialize();

            HttpResponseWritingExtensions.WriteAsync(context.HttpContext.Response, xml);

            return(Task.CompletedTask);
        }  
    }
}