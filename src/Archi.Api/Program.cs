using Archi.Library;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Archi.Api
{
    public class Program : BaseProgram<Startup>
    {
        public static void Main(string[] args)
        {
            StartupApp(args);
            
        }

        /*public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .UseSerilog((context, services, configuration) => configuration
                   .ReadFrom.Configuration(context.Configuration))
              .ConfigureWebHostDefaults(webBuilder =>
              {
                  webBuilder.UseStartup<Startup>();
              });*/
    }
}
