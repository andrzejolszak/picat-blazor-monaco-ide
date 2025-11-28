using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
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
            await page.Type("xxx y");

            await page.AssertTextContains("xxx y");
        }
    }
}
