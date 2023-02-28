using Microsoft.AspNetCore.Authentication;

namespace ThreeplyWebApi.Controllers.AuthenticationScheme
{
    public class GeneralUserAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public const string Name = "GeneralUserAuthenticationScheme";
        public GeneralUserAuthenticationSchemeOptions() : base()
        {
        }
    }
}
