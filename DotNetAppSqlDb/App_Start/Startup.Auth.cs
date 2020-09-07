using DotNetAppSqlDb.Utils;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;

namespace DotNetAppSqlDb
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

        public static readonly string Authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        private static string graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];


        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = Authority,
                    PostLogoutRedirectUri = redirectUri,
                    RedirectUri = redirectUri,


                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        //
                        // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                        //
                       // AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                        AuthenticationFailed = OnAuthenticationFailed
                    },

                    TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false
                    }

                }); ;
        }

        private static Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Exception.Message);
            return Task.FromResult(0);
        }

        private static async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            var code = context.Code;

            ClientCredential credential = new ClientCredential(clientId, appKey);
            string userObjectID = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            AuthenticationContext authContext = new AuthenticationContext(Authority, new NaiveSessionCache(userObjectID));

            // If you create the redirectUri this way, it will contain a trailing slash.  
            // Make sure you've registered the same exact Uri in the Azure Portal (including the slash).
            Uri uri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));

            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, uri, credential, graphResourceId);
        }
    }

}
