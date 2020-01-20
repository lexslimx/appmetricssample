using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Filtering;
using App.Metrics.Formatters.InfluxDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace metricssample
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

            var filter = new MetricsFilter().WhereType(MetricType.Timer);
            var metrics = new MetricsBuilder()
                         .Configuration.Configure(
                options =>
                {
                    options.WithGlobalTags((globalTags, info) =>
                    {
                        globalTags.Add("app", "metricssample");
                        globalTags.Add("env", "Development");
                        globalTags.Add("server", Environment.MachineName);
                    });
                    options.ReportingEnabled = true;
                    options.Enabled = true;                    
                })                         
                .Report.ToInfluxDb(
                    options =>
                    {                        
                        options.InfluxDb.BaseUri = new Uri("http://WIN-IR190QMVTJ6:8086");
                        options.InfluxDb.CreateDataBaseIfNotExists = true;
                        options.InfluxDb.Database = "netcoredb";
                        //options.InfluxDb.UserName = "admin";
                        //options.InfluxDb.Password = "password";
                        //options.InfluxDb.Consistenency = "consistency";
                        //options.InfluxDb.RetentionPolicy = retentionpolicy
                        options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                        options.HttpPolicy.FailuresBeforeBackoff = 5;
                        options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                        options.MetricsOutputFormatter = new  MetricsInfluxDbLineProtocolOutputFormatter();
                        options.Filter = filter;
                        options.FlushInterval = TimeSpan.FromSeconds(20);
                    })
                .Build();

            services.AddMetrics(metrics);
            services.AddMetricsReportingHostedService();
            services.AddMvcCore().AddMetricsCore();
            services.AddControllersWithViews();            
            services.AddMetricsTrackingMiddleware();
            services.AddMvc().AddMetrics();
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMetricsAllMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
