using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Linq;

namespace Server
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

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
                else if (context.ClientId == "web")
                {
                    var expectedUri = new Uri(context.Request.Uri, "/");
                    context.Validated(expectedUri.AbsoluteUri);
                }
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowedOrigin = "*";
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            if (context.UserName != "reportplususer@infragistics.com" || context.Password != "%baG7cadence")
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            var endpoint = "asdasdasd";

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, context.UserName));
            claims.Add(new Claim("Endpoint", endpoint));
            claims.Add(new Claim("Password", context.Password));

            var data = await context.Request.ReadFormAsync();

            var identity = new ClaimsIdentity("JWT");

            identity.AddClaims(claims);

            int daysSignedIn = 14;
            context.Options.AccessTokenExpireTimeSpan = TimeSpan.FromDays(daysSignedIn);

            var ticket = new AuthenticationTicket(identity, null);
            context.Validated(ticket);
        }
    }
}
