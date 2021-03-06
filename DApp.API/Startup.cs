﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DApp.API.Data;
using DApp.API.Helpers;
using DApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DApp.API
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
            services.AddDbContext<DataContext>(x => x
                .UseSqlServer(Configuration.GetConnectionString("DApp"))
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning)));
            
            // Do not do the below. This is only so that we can use the current very weak password
            IdentityBuilder builder = services.AddIdentityCore<User>(opt => {
               opt.Password.RequireDigit = false;
               opt.Password.RequiredLength = 4;
               opt.Password.RequireNonAlphanumeric = false;
               opt.Password.RequireUppercase = false; 
            });

            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            builder.AddEntityFrameworkStores<DataContext>();
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();

            services.AddAuthorization(options => {
                options.AddPolicy("RequireAdmin", policy => {
                    policy.RequireRole("Admin");
                });
                options.AddPolicy("ModeratPhotoRole", policy => {
                    policy.RequireRole("Admin", "Moderator");
                });
                options.AddPolicy("VipOnly", policy => {
                    policy.RequireRole("VIP");
                });
            });

            services.AddMvc(opt => {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(opt => {
                    opt.SerializerSettings.ReferenceLoopHandling =
                        Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.AddCors();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            Mapper.Reset();
            services.AddAutoMapper();
            services.AddTransient<Seed>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters()
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)
                            ),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                        };
                });

                services.AddScoped<LogUserActivity>();
        }

        // public void ConfigureDevelopmentServices(IServiceCollection services)
        // {
        //     services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
        //     services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
        //         .AddJsonOptions(opt => {
        //             opt.SerializerSettings.ReferenceLoopHandling =
        //                 Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        //     });
        //     services.AddCors();
        //     services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
        //     services.AddAutoMapper();
        //     services.AddTransient<Seed>();
        //     services.AddScoped<IAuthRepository, AuthRepository>();
        //     services.AddScoped<IDatingRepository, DatingRepository>();
        //     services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //         .AddJwtBearer(options =>
        //         {
        //             options.TokenValidationParameters =
        //                 new TokenValidationParameters()
        //                 {
        //                     ValidateIssuerSigningKey = true,
        //                     IssuerSigningKey = new SymmetricSecurityKey(
        //                         Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)
        //                     ),
        //                     ValidateIssuer = false,
        //                     ValidateAudience = false,
        //                 };
        //         });

        //         services.AddScoped<LogUserActivity>();
        // }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(builder =>
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();

                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    })
                );
                // app.UseHsts();
            }

            seeder.SeedUsers();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            // app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc(routes => {
                routes.MapSpaFallbackRoute(
                  name: "spa-fallback",
                  defaults: new {Controller = "FallBack", Action = "Index"}  
                );
            });
        }
    }
}
