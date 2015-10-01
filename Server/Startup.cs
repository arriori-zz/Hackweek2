using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using System.Web.Http;
using System.Configuration;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;

[assembly: OwinStartup(typeof(Server.Startup))]

namespace Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuthTokenGeneration(app);

            ConfigureOAuthTokenConsumption(app);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
        }

        private static string PublicClientId = "web";

        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                //For Dev enviroment only (on production should be AllowInsecureHttp = false)
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/Token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AccessTokenFormat = new CustomJwtFormat("http://localhost:26573/")
            };

            // OAuth 2.0 Bearer Access Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
        }

        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            var issuer = "http://localhost:26573/";
            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] { audienceId },
                IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                    {
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    }
            });
        }
    }
}
