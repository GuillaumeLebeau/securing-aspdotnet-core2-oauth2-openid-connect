using AutoMapper;

using IdentityServer4.AccessTokenValidation;

using ImageGallery.API.Authorization;
using ImageGallery.API.Entities;
using ImageGallery.API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImageGallery.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("MustOwnImage", policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            policyBuilder.AddRequirements(new MustOwnImageRequirement());
                        });
                });

            services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(
                    options =>
                    {
                        options.Authority = Configuration.GetValue<string>("IdpUrl");
                        options.ApiName = "imagegalleryapi";
                        options.ApiSecret = "apisecret";
                    });
            

            var connection = Configuration.GetConnectionString("ImageGalleryDBConnectionString");
            services.AddDbContext<GalleryContext>(o => o.UseSqlServer(connection));
            
            // register the repository
            services.AddScoped<IGalleryRepository, GalleryRepository>();
            
            // register AutoMapper
            services.AddAutoMapper();
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();

            app.UseAuthentication();
            
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
