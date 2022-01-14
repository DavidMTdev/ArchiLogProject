using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Archi.Library
{
    public class BaseProgram<TStartup> where TStartup : BaseStartup
    {
        public static void StartupApp(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
              .WriteTo.Debug()
              .WriteTo.Console()
              .CreateBootstrapLogger();

            Log.Information("Starting up!");

            try
            {
                CreateHostBuilder(args).Build().Run();

                Log.Information("Stopped cleanly");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start correctly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration))
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<TStartup>();
               });
    }
}
