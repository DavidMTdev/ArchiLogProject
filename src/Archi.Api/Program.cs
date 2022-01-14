using Archi.Library;
using Microsoft.Extensions.Configuration;

namespace Archi.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BaseProgram<Startup>.StartupApp(args);
        } 
    }
}
