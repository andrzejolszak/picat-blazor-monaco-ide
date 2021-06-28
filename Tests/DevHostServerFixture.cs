using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using ProjectionalBlazorMonaco.Tests.BlazorDevServer;

namespace ProjectionalBlazorMonaco.Tests
{
    // Based on https://github.com/dotnet/aspnetcore/blob/master/src/Components/test/E2ETest/Infrastructure/ServerFixtures/DevHostServerFixture.cs
    public class DevHostServerFixture : IDisposable
    {
        private readonly Lazy<Uri> _rootUriInitializer;

        public DevHostServerFixture()
        {
            _rootUriInitializer = new Lazy<Uri>(() => new Uri(StartAndGetRootUri()));
        }

        public Uri RootUri => _rootUriInitializer.Value;
        public IHost Host { get; set; }
        public string PathBase { get; set; }
        public string ContentRoot { get; set; }


        protected string StartAndGetRootUri()
        {
            Host = CreateWebHost();
            RunInBackgroundThread(Host.Start);
            return Host.Services.GetRequiredService<IServer>().Features
                .Get<IServerAddressesFeature>()
                .Addresses.Single();
        }

        protected IHost CreateWebHost()
        {
            var host = "127.0.0.1";

            var args = new List<string>
            {
                "--urls", $"http://{host}:0",
                "--contentroot", ContentRoot,
                "--pathbase", PathBase,
            };

            return DevServer.BuildWebHost(args.ToArray());
        }

        protected static void RunInBackgroundThread(Action action)
        {
            var isDone = new ManualResetEvent(false);

            ExceptionDispatchInfo edi = null;
            new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    edi = ExceptionDispatchInfo.Capture(ex);
                }

                isDone.Set();
            }).Start();

            if (!isDone.WaitOne(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException("Timed out waiting for: " + action);
            }

            if (edi != null)
            {
                throw edi.SourceException;
            }
        }

        public void Dispose()
        {
            // This can be null if creating the webhost throws, we don't want to throw here and hide
            // the original exception.
            Host?.Dispose();
            Host?.StopAsync();
        }
    }
}
