using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using System.IO;
using Microsoft.Extensions.FileProviders;
using WebAPI.Repository;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.Extensions.Options;
using System;

namespace WebAPI
{
    public class Startup
    {
        private readonly string _contentRootPath;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _contentRootPath = env.ContentRootPath;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //---EFCoreSecondLevelCacheInterceptor begin---
            services.AddEFSecondLevelCache(options =>
                options.UseMemoryCacheProvider().DisableLogging(true).UseCacheKeyPrefix("EF_")
                       .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30))

            // Please use the `CacheManager.Core` or `EasyCaching.Redis` for the Redis cache provider.
            );

            var connectionString = Configuration["ConnectionStrings:EmployeeAppCon"];
            services.AddConfiguredMsSqlDbContext(connectionString);
            //---EFCoreSecondLevelCacheInterceptor end---

            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeAngularRepository, EmployeeAngularRepository>();

            services.AddDbContext<APIDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("EmployeeAppCon")));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "WEB API",
                        Version = "v1"
                    });
            });

            //Enable CORS
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            //JSON Serializer
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }           

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WEB API");
                c.DocumentTitle = "WEB API";
                c.DocExpansion(DocExpansion.List);
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Photos")),
                RequestPath = "/Photos"
            });
        }
    }
}
