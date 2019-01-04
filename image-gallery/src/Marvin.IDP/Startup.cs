using System.Security.Claims;

using IdentityServer4;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.IDP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var authenticationBuilder = services.AddAuthentication();

            if (Configuration.GetValue<bool>("Authentication:Google:Enabled"))
            {
                authenticationBuilder.AddGoogle(
                    GoogleDefaults.AuthenticationScheme,
                    o =>
                    {
                        o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                        // Create the app in Google API Console
                        // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-2.2
                        //
                        // Use Secret Manager to define Google ClientId and ClientSecret
                        // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows
                        // > dotnet user-secrets set "Authentication:Google:ClientId" "......"
                        // > dotnet user-secrets set "Authentication:Google:ClientSecret" "......"
                        //
                        // Check secrets with:
                        // > dotnet user-secrets list
                        o.ClientId = Configuration["Authentication:Google:ClientId"];
                        o.ClientSecret = Configuration["Authentication:Google:ClientSecret"];

                        o.UserInformationEndpoint =
                            "https://openidconnect.googleapis.com/v1/userinfo";
                        o.ClaimActions.Clear();
                        o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
                        o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                        o.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_Name");
                        o.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_Name");
                        o.ClaimActions.MapJsonKey("urn:google:profile", "profile");
                        o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                        o.ClaimActions.MapJsonKey("urn:google:image", "picture");
                    });
            }

            if (Configuration.GetValue<bool>("Authentication:Microsoft:Enabled"))
            {
                authenticationBuilder.AddMicrosoftAccount(
                    MicrosoftAccountDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.SignInScheme =
                            IdentityServerConstants.ExternalCookieAuthenticationScheme;

                        // Create the app in Microsoft Developer Portal
                        // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins?view=aspnetcore-2.2
                        //
                        // Use Secret Manager to define Microsoft ApplicationId and Password
                        // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows
                        // > dotnet user-secrets set "Authentication:Microsoft:ApplicationId" "......"
                        // > dotnet user-secrets set "Authentication:Microsoft:Password" "......"
                        //
                        // Check secrets with:
                        // > dotnet user-secrets list
                        options.ClientId = Configuration["Authentication:Microsoft:ApplicationId"];
                        options.ClientSecret = Configuration["Authentication:Microsoft:Password"];
                    });
            }

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddTestUsers(Config.GetUsers())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
