using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Roman015API.Hubs;
using Roman015API.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Roman015API
{
    public class Startup
    {       
        public Startup(IConfiguration configuration)
        {         
            Domain   = System.Environment.GetEnvironmentVariable(configuration["AzureAdEnvironmentVars:AzureAdDomain"]);
            TenantId = System.Environment.GetEnvironmentVariable(configuration["AzureAdEnvironmentVars:AzureAdTenantId"]);
            ClientId = System.Environment.GetEnvironmentVariable(configuration["AzureAdEnvironmentVars:AzureAdClientId"]);
            BlogBlobConnectionString = System.Environment.GetEnvironmentVariable(configuration["AzureStorageEnvironmentVars:AzureStorageBlobConnectionString"]);
            CorsOrigins = System.Environment.GetEnvironmentVariable(configuration["CorsOriginsEnvironmentVar"]);
            RedisBackplaneConnectionString = System.Environment.GetEnvironmentVariable(configuration["RedisBackplaneConnectionStringEnvironmentVar"]);
            RedisBackplanePassword = System.Environment.GetEnvironmentVariable(configuration["RedisBackplanePasswordEnvironmentVar"]);

            Configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddJsonStream(GetAzureAdSettings())
                .AddJsonStream(GetAzureBlogSettings())
                .AddJsonStream(GetRedisBackplaneConnectionStringSettings())
                .Build();
        }

        public IConfiguration Configuration { get; }
        public string Domain { get; }
        public string TenantId { get; }
        public string ClientId { get; }
        public string BlogBlobConnectionString { get; }
        public string CorsOrigins { get; }
        public string RedisBackplaneConnectionString { get; }
        public string RedisBackplanePassword { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)                
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));
            
            services.AddSignalR()
                .AddStackExchangeRedis(
                    //host:port
                    Configuration["RedisBackplane:ConnectionString"],
                    options =>
                    {
                        options.Configuration.ChannelPrefix = "roman015api";
                        options.Configuration.Password = Configuration["RedisBackplane:Password"];                        
                    });

            services.AddControllers().AddControllersAsServices();

            services.AddHostedService<CacheWarmupService>();
            services.AddHostedService<InstagramScraperService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors(builder =>
            {
                // https://www.roman015.com,https://blog.roman015.com
                builder.WithOrigins(CorsOrigins.Split(",")); 
                builder.AllowAnyHeader();
            }); 

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/NotificationHub");
            });
        }

        private Stream GetAzureAdSettings()
        {
            string AzureAdString = "{ \"AzureAd\": {" + System.Environment.NewLine
                + "\"Instance\"    : \"https://login.microsoftonline.com/\"," + System.Environment.NewLine
                + "\"Domain\"      : \"AzureAdDomain\"," + System.Environment.NewLine
                + "\"TenantId\"    : \"AzureAdTenantId\"," + System.Environment.NewLine
                + "\"ClientId\"    : \"AzureAdClientId\"," + System.Environment.NewLine
                + "\"Authority\"    : \"https://login.microsoftonline.com/" + TenantId + "\"," + System.Environment.NewLine
                //+ "\"CallbackPath\": \"/signin-oidc\"" + System.Environment.NewLine
                + "}}";

            string AzureAdJson = AzureAdString
                .Replace("AzureAdDomain", Domain)
                .Replace("AzureAdTenantId", TenantId)
                .Replace("AzureAdClientId", ClientId);

            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(AzureAdJson));

            return ms;
        }

        private Stream GetAzureBlogSettings()
        {
            string AzureBlobString = "{ \"AzureBlobConnectionString\": \"" 
                + BlogBlobConnectionString + "\"" + System.Environment.NewLine
                + "}";

            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(AzureBlobString));

            return ms;
        }

        private Stream GetRedisBackplaneConnectionStringSettings()
        {
            string jsonString = "{ \"RedisBackplane\": {" + System.Environment.NewLine
               + "\"ConnectionString\"  : \"RedisBackplaneConnectionString\"," + System.Environment.NewLine
               + "\"Password\"          : \"RedisBackplanePassword\"" + System.Environment.NewLine
               + "}}";

            jsonString = jsonString
                    .Replace("RedisBackplaneConnectionString", RedisBackplaneConnectionString)
                    .Replace("RedisBackplanePassword", RedisBackplanePassword);

            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(jsonString));

            return ms;
        }
    }
}
