using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Filtering;
using App.Metrics.Formatters.InfluxDB;

namespace metricssample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
          
            CreateHostBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseMetrics(); webBuilder.UseMetricsWebTracking(); })
                .Build().Run();      
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder                    
                    .UseStartup<Startup>()                    
                    .ConfigureKestrel((context, options) =>
                    {
                        options.AllowSynchronousIO = true;
                    });              
                });
    }
}
