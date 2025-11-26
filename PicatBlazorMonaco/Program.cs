using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PicatBlazorMonaco;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectionalBlazorMonaco
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            // builder.Services.AddHttpContextAccessor();

            await builder.Build().RunAsync();
        }
    }
}
