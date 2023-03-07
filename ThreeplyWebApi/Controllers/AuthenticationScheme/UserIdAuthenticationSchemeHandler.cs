using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ThreeplyWebApi.Controllers.AuthenticationScheme
{
    public class GeneralUserAuthenticationSchemeHandler : AuthenticationHandler<GeneralUserAuthenticationSchemeOptions>
    {
        public GeneralUserAuthenticationSchemeHandler(IOptionsMonitor<GeneralUserAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }
        
        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            StringValues userId = "";
            if (Context.Request.Headers.TryGetValue("User-Id",out userId))
            {
                Claim[] claims =
                {
                    new Claim(ClaimTypes.Name,userId),
                };
                var claimsIdentity = new ClaimsIdentity(claims, "GeneralUserAuthentication");
                var principal = new ClaimsPrincipal(claimsIdentity);
                var ticket = new AuthenticationTicket(principal,this.Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("huevo");
            }
            
        }
    }
}
