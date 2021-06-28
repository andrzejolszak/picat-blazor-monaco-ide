using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PlaywrightSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ProjectionalBlazorMonaco.Tests
{
    public class BasicFeaturesTest : IClassFixture<DevHostServerFixture>
    {
        private readonly DevHostServerFixture _server;

        public BasicFeaturesTest(DevHostServerFixture server)
        {
            _server = server;
            _server.ContentRoot = Utils.GetPublishLocation();
        }

        [Fact]
        public async Task SiteIsLoaded()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync();
            var page = await (browser, _server).EnsureLoaded();

            await page.AssertTextContains("5. Reference nodes:");
        }
    }
}
