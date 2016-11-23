using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using kbsrserver.Models;
using System.Data.Entity;
using kbsrserver.Helpers;

namespace kbsrserver.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }
            string[] values;
            if (!context.Request.Headers.TryGetValue("Imei", out values))
            {
                context.SetError("no_imei", "Imei not specified");
                return;
            }
            var imei = values.FirstOrDefault();
            UserKey key;

            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userEntity = await db.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);
                    key = await db.Keys.FirstOrDefaultAsync(k => k.User.UserName == userEntity.UserName && k.Imei == imei);
                    if (key == null)
                    {
                        key = new UserKey
                        {
                            Imei = imei,
                            UserStorageKey = BouncyCastleHelper.GenerateSerpentKey(),
                            User = userEntity
                        };
                        db.Keys.Add(key);
                        await db.SaveChangesAsync();
                    }
                    else if (key.UserStorageKey == null)
                    {
                        key.UserStorageKey = BouncyCastleHelper.GenerateSerpentKey();
                        db.Entry(key).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
                   OAuthDefaults.AuthenticationType);

                AuthenticationProperties properties = CreateProperties(user, key);
                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                context.Validated(ticket);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(ApplicationUser user, UserKey key)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", user.UserName }
            };
            if (key?.PublicSignKey != null)
                data.Add("signHalfKey", key.SignHalfKey);
            if (key?.UserStorageKey != null)
                data.Add("userStorageKey", key.UserStorageKey);
            return new AuthenticationProperties(data);
        }
    }
}