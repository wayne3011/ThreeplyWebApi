using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ThreeplyWebApi.Controllers.AuthenticationScheme
{
    public class UserIdAuthenticationSchemeHandler : AuthenticationHandler<UserIdAuthenticationSchemeOptions>
    {
        public UserIdAuthenticationSchemeHandler(IOptionsMonitor<UserIdAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }



        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            StringValues userId = "";
            if (Context.Request.Headers.TryGetValue("User-Id",out userId))
            {
                var principal = new ClaimsPrincipal(new ClaimsIdentity(userId));
                var ticket = new AuthenticationTicket(principal, this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("huevo");
            }
            
        }
    }
}
