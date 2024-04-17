using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
namespace Configuration
{

    internal static class AuthenticationConfigurer {

        public static void ConfigureAuthentication(WebApplicationBuilder builder) {


            var authenticationType = builder.Configuration.GetValue<string>("AuthenticationType");

            switch (authenticationType)
            {
                case "Negotiate":
                    builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme)
                        .AddNegotiate();
                    break;
                case "Cookie":
                    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                        .AddCookie(options =>
                        {
                            options.LoginPath = "/Account/Login/Index"; // Путь к странице входа
                            options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Время жизни cookie 
                        });
                    break;
                default:
                    throw new InvalidOperationException("Unsupported authentication type");
            }
        }
    
    }

}