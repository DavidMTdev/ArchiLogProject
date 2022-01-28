using Archi.Api.Data;
using Archi.Api.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archi.Library;
using Serilog;
using Serilog.Events;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.IdentityModel.Logging;

namespace Archi.Api
{
    public class Startup : BaseStartup
    {

        public Startup(IConfiguration options) : base(options) {}

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddApiVersioning();

            //this.ConfigServices(services);
            //}

            //public override void ConfigServices(IServiceCollection services)
            //{

            // Add Authentication services
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(options =>
                   {
                       options.SaveToken = true;
                       options.TokenValidationParameters = new TokenValidationParameters()
                       {
                           ValidateIssuer = true,
                           ValidateAudience = true,
                           ValidateLifetime = false,
                           ValidateIssuerSigningKey = true,
                           ValidIssuer = Configuration["Jwt:Issuer"],
                           ValidAudience = Configuration["Jwt:Issuer"],
                           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                       };
                   });

            services.AddDbContext<ArchiDBContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Archi"));
            });

            /*services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });*/

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter the Bearer Authorization string as following: Bearer Generated-JWT-Token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
            });
        }

        public override void ConfigOptions(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var swaggerOptions = new SwaggerOptions();
            Configuration.GetSection(nameof(SwaggerOptions)).Bind(swaggerOptions);

            app.UseSwagger(option =>
            {
                option.RouteTemplate = swaggerOptions.JsonRoute;
            });

            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint(swaggerOptions.UiEndpoint, swaggerOptions.Description);
            });
        }
    }
}
