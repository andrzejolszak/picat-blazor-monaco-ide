using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using WebApi.Controllers;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var conf = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            new ValuesController(conf)
                .Get("main", "main => writeln(hello_from_picat_compiler).");

            CreateWebHostBuilder(args)
                .Build()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://127.0.0.1:8701")
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = long.MaxValue;
                    options.Limits.MaxRequestLineSize = 1040000;
                });
    }
}
