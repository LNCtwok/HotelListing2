using HotelListing2.Data;
using HotelListing2.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Marvin.Cache.Headers;
using AspNetCoreRateLimit;

namespace HotelListing2
{
    public static class ServiceExtension
    {
        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentityCore<ApiUser>(q =>{ q.User.RequireUniqueEmail = true; });

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), services);
            builder.AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();

        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration Configuration)
        {
            var jwtSettings = Configuration.GetSection("Jwt");
            var key = Environment.GetEnvironmentVariable("KEY"); 
            
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;//I'm adding authontication to the application and the default scheme is Jwt
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,//validate who issued the token 
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.GetSection("Issuer").Value,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),//hashing the key
                    };
                });

        }

        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(error => {
                error.Run(async context=> {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if(contextFeature != null)
                    {
                        Log.Error($"Something went wrong {contextFeature.Error}");

                        await context.Response.WriteAsync(new Error { 
                        
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal server error please try again later",
                        }.ToString());
                    }
                });
            });
        }

        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(opt => {//opt==option
                opt.ReportApiVersions = true;    //this means that there will be a header in our responses saying this is the version that you're usin
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });
        }

        public static void ConfigureHttpCacheHeaders(this IServiceCollection services)
        {
            services.AddResponseCaching();
            services.AddHttpCacheHeaders(
                (expirationOpt) =>
                {
                    expirationOpt.MaxAge = 120;
                    expirationOpt.CacheLocation = CacheLocation.Private;
                },
                (validateOpt) =>
                {
                    validateOpt.MustRevalidate = true;//once data changed must go to the process again
                }
             );
        }

        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            var rateLimitRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Limit= 1,
                    Period = "5s" //so this means : within 5 seconds you're only allowed to make one call on any endpoint(because of "*")
                 //one hunderd call every 10 seconds and it can be per minute by putting an 'm' instead of 's'
                }/* //we can add multiple rules for multiple endpoints
                ,
                new RateLimitRule
                {
                    Endpoint = "*",
                    Limit=1,
                    Period = "1s"
                }*/
            };
            services.Configure<IpRateLimitOptions>(opt => {

                opt.GeneralRules = rateLimitRules;//this is the limit rules that we defined above
            });
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }
    }
}
