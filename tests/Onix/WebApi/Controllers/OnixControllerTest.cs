using System.Security.Claims;
using NUnit.Framework;

using Onix.Api.Utils;
using Onix.Api.Commons;
using Onix.Api.Factories;
using Onix.Api.Utils.Serializers;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Onix.WebApi.Controllers
{
    public class OnixControllerTest
    {
        [SetUp]
        public void Setup()
        {
            LibSetting instance = LibSetting.GetInstance();
            instance.OAuthKey = "HelloworldThisIsSecretKey";
            instance.ExternalApplicationKey = "44444444444";
        }

        private CTable getContextData(IHttpContextAccessor ctx)
        {
            CRoot tupple = (CRoot) ctx.HttpContext.Items["RESPONSE_TUPPLE"]; 
            return(tupple.Data);    
        }

        private OnixController createController(CTable data, string dbName, ClaimsIdentity ci)
        {
            CTable param = new CTable("PARAM");
            CRoot root = new CRoot(param, data);

            IHttpContextAccessor ctx = new HttpContextAccessor();
            ctx.HttpContext = new DefaultHttpContext();
            ctx.HttpContext.Items["REQUEST_TUPPLE"] = root;
            
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            DbContext dbContext = FactoryDbContext.GetDbContextForTesting("Onix", options);
/*
            ClaimsIdentity ci = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, "Seubpong"),
                    new Claim(ClaimTypes.Role, "Users")
                });
*/
            OnixController controller = new OnixController(ctx);
            controller.SetDbContext(dbContext);
            controller.SetIdentity(ci);

            return(controller);
        }
        
        [TestCase("MockedLogon")] //MUST not allow "MockedLogon" to be call via REST
        [TestCase("GetEmployeeList")]
        public void UnauthorizeIfNotLoginTest(string apiName)
        {
            CTable data = new CTable("DATA");
            data.SetFieldValue("FIELD1", "Seubpong");

            OnixController controller = createController(data, "Database" + apiName, null);
            controller.InvokeAction(apiName);

            IHttpContextAccessor ctx = controller.GetHttpContextAccessor();
            string result = (string) ctx.HttpContext.Items["AUTHENTICATION_RESULT"];        

            Assert.AreNotEqual("SUCCESS", result, "Should not get success if not authorized!!!");
        }

        [TestCase("Seubpong", "Test1234", "Test1234")]
        [TestCase("Seubpong", "abc1234", "abc1234")]
        public void GetTokenIfLogonSuccessTest(string userName, string password, string expectedPassword)
        {
            CTable data = new CTable("DATA");
            data.SetFieldValue("USER_NAME", userName);
            data.SetFieldValue("PASSWORD", password);
            data.SetFieldValue("EXPECTED_PASSWORD", expectedPassword);
            
            OnixController controller = createController(data, "Database" + userName, null);
            controller.SetLoginApiName("MockedLogon");
            controller.Logon();

            IHttpContextAccessor ctx = controller.GetHttpContextAccessor();
            string result = (string) ctx.HttpContext.Items["AUTHENTICATION_RESULT"];        

            Assert.AreEqual("SUCCESS", result, "Should get success authorization!!!");

            CTable d = getContextData(ctx);
            string token = d.GetFieldValue("TOKEN");
            string role = d.GetFieldValue("USER_ROLE");

            string expectedRole = "DO NOT CHANGE THIS!!!! - THIS ROLE WILL NOT BE ALLOWED FOR ALL";
            Assert.AreEqual(expectedRole, role, "Must return role for testing only!!!! (not use in real life)"); 

            Assert.Greater(token.Length, 80, "Token is too short!!!");
        }   

        [TestCase("Seubpong", "Test1234", "1111111111")]
        [TestCase("Seubpong", "abc1234", "1111111111")]
        public void GetNoTokenIfLogonFailedTest(string userName, string password, string expectedPassword)
        {
            CTable data = new CTable("DATA");
            data.SetFieldValue("USER_NAME", userName);
            data.SetFieldValue("PASSWORD", password);
            data.SetFieldValue("EXPECTED_PASSWORD", expectedPassword);
            
            OnixController controller = createController(data, "Database" + userName, null);
            controller.SetLoginApiName("MockedLogon");
            controller.Logon();

            IHttpContextAccessor ctx = controller.GetHttpContextAccessor();
            string result = (string) ctx.HttpContext.Items["AUTHENTICATION_RESULT"];        

            //Logon failed but still need "SUCCESS"
            Assert.AreEqual("SUCCESS", result, "Should get success authorization!!!");

            CTable d = getContextData(ctx);
            string token = d.GetFieldValue("TOKEN");

            Assert.AreEqual(0, token.Length, "Should not get any token if authentication failed!!!");
        }               
    }
}