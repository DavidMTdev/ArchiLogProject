using Archi.Library;
using Microsoft.Extensions.Configuration;

namespace Archi.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.Development.json")
               .Build();

            BaseProgram<Startup>.StartupApp(args, configuration);
        }

       
    }
}
