using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

using Onix.Api.Commons;
using Onix.Api.Utils.Serializers;
using Onix.WebApi.Controllers.Commons;

namespace Onix.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]    
    public class OnixController : ControllerServiceBase
    {        
        private IHttpContextAccessor ctxAccessor = null;

        public OnixController(IHttpContextAccessor httpContextAccessor) : base()
        {
            setHttpContext(httpContextAccessor);
            ctxAccessor = httpContextAccessor;
        }

        //Use GET method for all API to simplify things  
        [AllowAnonymous]      
        [HttpGet("Init")]
        public ActionResult<string> Init()
        {
            CRoot requestTupple = (CRoot) ctxAccessor.HttpContext.Items["REQUEST_TUPPLE"];

            CTable input = requestTupple.Data;
            CRoot tupple = init(input, ctxAccessor.HttpContext);

            ctxAccessor.HttpContext.Items["RESPONSE_TUPPLE"] = tupple;

            return "";
        } 

        [AllowAnonymous]      
        [HttpGet("Logon")]
        public ActionResult<string> Logon()
        {
            CRoot requestTupple = (CRoot) ctxAccessor.HttpContext.Items["REQUEST_TUPPLE"];

            CTable input = requestTupple.Data;
            CRoot tupple = logon(input, ctxAccessor.HttpContext);

            ctxAccessor.HttpContext.Items["RESPONSE_TUPPLE"] = tupple;

            return "";
        } 

        //Use GET method for all API to simplify things 
        [HttpGet("{apiName}")]
        public ActionResult<string> InvokeAction(string apiName)
        {   
            IIdentity identity = GetIdentity();
            bool isAuthen = false;
            if (identity != null)
            {
                isAuthen = identity.IsAuthenticated;
            }

            if (!isAuthen)
            {
                //Make sure code will not execute if no authentication from Middleware
                return "";
            }

            CRoot requestTupple = (CRoot) ctxAccessor.HttpContext.Items["REQUEST_TUPPLE"];

            CTable input = requestTupple.Data;
            CRoot tupple = apply(apiName, input);            
            ctxAccessor.HttpContext.Items["RESPONSE_TUPPLE"] = tupple;                      

            return "";
        }        
    }
}
