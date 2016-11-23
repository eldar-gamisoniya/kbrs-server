using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using kbsrserver.Helpers;
using System.IO;
using kbsrserver.Models;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Web.Http;

namespace kbsrserver.Attributes
{
    public class TwoFactorAuthorizeAttribute : AuthorizationFilterAttribute
    {
        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, System.Threading.CancellationToken cancellationToken)
        {
            await base.OnAuthorizationAsync(actionContext, cancellationToken);
            var principal = actionContext.RequestContext.Principal;
            var imei = actionContext.Request.GetImei();
            var signature = actionContext.Request.GetSignature();
            if (imei == null || signature == null)
            {
                actionContext.Response = actionContext.Request.CreateCustomErrorResponse(HttpStatusCode.BadRequest, "There is no imei or signature");
                return;
            }
            UserKey key;
            using (var context = new ApplicationDbContext())
            {
                key = await context.Keys.FirstOrDefaultAsync(k => k.Imei == imei && k.User.UserName == principal.Identity.Name);
                if (key == null || key.SignHalfKey == null || key.PublicSignKey == null)
                {
                    actionContext.Response = actionContext.Request.CreateCustomErrorResponse(HttpStatusCode.MethodNotAllowed, "Sign key is not found");
                    return;
                }
            }

            var stream = await actionContext.Request.Content.ReadAsStreamAsync();
            var reader = new StreamReader(stream);
            var jsonPostData = reader.ReadToEnd();
            stream.Seek(0, SeekOrigin.Begin);

            if (!BouncyCastleHelper.Verify(jsonPostData, signature, key.PublicSignKey))
            {
                actionContext.Response = actionContext.Request.CreateCustomErrorResponse(HttpStatusCode.BadRequest, "Sign is invalid");
                return;
            }
        }
    }
}