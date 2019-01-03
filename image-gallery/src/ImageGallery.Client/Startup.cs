using System;
using System.IdentityModel.Tokens.Jwt;

using Flurl.Http.Configuration;

using IdentityModel;

using ImageGallery.Client.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ImageGallery.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy(
                        "CanOrderFrame", policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            policyBuilder.RequireClaim("country", "be");
                            policyBuilder.RequireClaim("subscriptionlevel", "PayingUser");
                        });
                    
                    options.AddPolicy("CanAddImage",
                        policyBuilder => { policyBuilder.RequireAuthenticatedUser(); });
                });

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryApiClient
            services.AddScoped<IImageGalleryApiClient, ImageGalleryApiClient>();

            // register a few required services, one of which will be an implementation of IHttpClientFactory
            services.AddHttpClient(
                "idp_client",
                client => client.BaseAddress = new Uri(Configuration.GetValue<string>("IdpUrl")));

            services.AddHttpClient(
                "api_client",
                client => client.BaseAddress =
                    new Uri(Configuration.GetValue<string>("ImageGalleryApiUrl")));

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                }).AddCookie("Cookies", options =>
                {
                    options.AccessDeniedPath = "/Authorization/AccessDenied";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    options.Authority = "https://localhost:5005";
                    options.ClientId = "imagegalleryclient";
                    options.ResponseType = "code id_token";
                    //options.CallbackPath = new PathString("...");
                    //options.SignedOutCallbackPath = new PathString("...");
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("address");
                    options.Scope.Add("roles");
                    options.Scope.Add("subscriptionlevel");
                    options.Scope.Add("country");
                    options.Scope.Add("imagegalleryapi");
                    options.Scope.Add("offline_access");
                    options.SaveTokens = true;
                    options.ClientSecret = "secret";
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ClaimActions.Remove("amr");
                    options.ClaimActions.DeleteClaim("sid");
                    options.ClaimActions.DeleteClaim("idp");
                    // options.ClaimActions.DeleteClaim("address");
                    options.ClaimActions.MapUniqueJsonKey("role", "role");
                    options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");
                    options.ClaimActions.MapUniqueJsonKey("country", "country");
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.GivenName,
                        RoleClaimType = JwtClaimTypes.Role
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}