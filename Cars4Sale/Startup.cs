using Cars4Sale.Services;
using Cars4Sale.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace Cars4Sale
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
            services.AddMvc();
            services.AddScoped<ICarsService, CarsService>();
            services.AddScoped<ICarsRepository, CarsRepository>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Cars4Sale",
                    Version = "v1",
                    Description = "API for accessing Cars4Sale",
                    Contact = new OpenApiContact
                    {
                        Name = "Joe Ren",
                        Email = "joe.ren@gmail.com",
                    }
                });

                var name = Assembly.GetExecutingAssembly().GetName().Name;
                var xmlFile = $"{name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("ApiKeyAuth", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header",
                    Name = "ApiKey",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Scheme = "ApiKeyAuth"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKeyAuth"
                            }
                        }, new string[]{ }
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger(c => c.RouteTemplate = "api/Cars4Sale/{documentname}/swagger.json");
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/api/Cars4Sale/v1/swagger.json", "Cars4Sale v1");
                c.RoutePrefix = "api/Cars4Sale";
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
