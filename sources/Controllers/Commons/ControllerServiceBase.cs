using System;
using System.Diagnostics;
using System.Security.Principal;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

using Onix.Api.Commons;
using Onix.Api.Factories;
using Onix.Api.Commons.Business;
using Onix.Api.Utils.Serializers;

using Onix.Api.Utils;

namespace Onix.WebApi.Controllers.Commons
{
    public class ControllerServiceBase : ControllerBase
    {
        private string loginApiName = "Logon";

        private DbContext dbContext = null;
        private IHttpContextAccessor ctxAccessor = null;
        private IIdentity identity = null;

        public ControllerServiceBase()
        {            
        }

        protected void setHttpContext(IHttpContextAccessor ctx)
        {
            ctxAccessor = ctx;
        }

        public void SetLoginApiName(string apiName)
        {
            loginApiName = apiName;
        }

        public IHttpContextAccessor GetHttpContextAccessor()
        {
            return ctxAccessor;
        }

        public void SetDbContext(DbContext ctx)
        {
            dbContext = ctx;
        }

        public DbContext GetDbContext()
        {      
            DbContext ctx = dbContext;
            if (ctx == null)
            {
                ctx = FactoryDbContext.GetDbContext("Onix");
            }

            return(ctx);
        }

        public void SetIdentity(IIdentity id)
        {
            identity = id;
        }

        public IIdentity GetIdentity()
        {
            IIdentity id = identity;
            if (id == null)
            {
                if (User == null)
                {
                    return null;
                }
                id = User.Identity;
            }            
            return id;
        }

        protected CRoot init(CTable inputData, HttpContext context)
        {
            CRoot tupple = apply("Init", inputData);         
            context.Items["AUTHENTICATION_RESULT"] = "SUCCESS";

            return tupple;
        }

        protected CRoot logon(CTable inputData, HttpContext context)
        {
            inputData.SetFieldValue("API_KEY", LibSetting.GetInstance().ExternalApplicationKey);

            CRoot tupple = apply(loginApiName, inputData);
            CTable param = tupple.Param;
            CTable data = tupple.Data;            

            string errCode = param.GetFieldValue("ERROR_CODE");
            if (!errCode.Equals("0"))
            {
                string userName = inputData.GetFieldValue("USER_NAME");
                param.SetFieldValue("ERROR_DESC", String.Format("Authentication error for [{0}]!!!", userName));
                return (tupple);
            }

            string role = data.GetFieldValue("USER_ROLE");

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(LibSetting.GetInstance().OAuthKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, data.GetFieldValue("NAME")),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = System.DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            context.Items["AUTHENTICATION_RESULT"] = "SUCCESS";

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenStr = tokenHandler.WriteToken(token);
            
            data.SetFieldValue("TOKEN", tokenStr);

            return tupple;
        }

        protected CRoot apply(String operationName, CTable inputData)
        {
            CTable output = new CTable("TABLE");
            CTable param = new CTable("PARAM");

            string errorDesc = "SUCCESS";
            string errorCode = "0";

            Claim claim = null;
            ClaimsIdentity identity = (ClaimsIdentity) GetIdentity();            
            if (identity != null)
            {
                claim = identity.FindFirst(ClaimTypes.Role);
            }

            string role = "";
            if (claim != null)
            {
                //Have not yet login before
                role = claim.Value;    
            }

            BusinessOperationOption option = FactoryBusinessOperation.GetBusinessOperationAllowedRole(operationName);
            if ((option == null) || !option.IsRoleAllow(role))
            {
                ctxAccessor.HttpContext.Items["AUTHENTICATION_RESULT"] = "FAILED"; 
                return(null); 
            }

            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                DbContext ctx = GetDbContext();
                IBusinessOperation opr = FactoryBusinessOperation.CreateBusinessOperationObject(operationName);
                output = opr.Apply(inputData, ctx);
            }
            catch (Exception e)
            {
                //TODO : Log ther exception here

                errorCode = "1";
                errorDesc = e.Message;
            }

            watch.Stop();            

            ctxAccessor.HttpContext.Items["AUTHENTICATION_RESULT"] = "SUCCESS";  

            param.SetFieldValue("ERROR_CODE", errorCode);
            param.SetFieldValue("ERROR_DESC", errorDesc);
            param.SetFieldValue("FUNCTION_NAME", operationName);
            param.SetFieldValue("ENGINE", "DOTNET");
            param.SetFieldValue("DEBUG_EXECUTION_TIME_MILLISEC", watch.ElapsedMilliseconds); //Millisec

            CRoot root = new CRoot(param, output);
            return root;
        }
    }
}