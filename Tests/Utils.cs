using FluentAssertions;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace ProjectionalBlazorMonaco.Tests
{
    public static class Utils
    {
        public static string GetPublishLocation()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json")
                .AddJsonFile("testsettings.local.json", true)
                .Build();

            var root = Path.Combine(AppContext.BaseDirectory, "../../../../");
            var location = Path.GetFullPath(Path.Combine(root, config["contentRoot"]));

            Directory.Exists(location).Should().BeTrue();
            Directory.GetFiles(location).Length.Should().BeGreaterThan(0);

            return location;
        }

        public static async Task AssertTextContains(this IPage page, string text)
        {
            int caretPosition = text.IndexOf('\'');

            text = text.Replace("\'", "");

            string val = "";
            int idx = 0;

            for (int i = 0; i < 3; i++)
            {
                val = (await page.Locator("div.lines-content").InnerTextAsync()).Replace("·", " ").Replace("\n", "\r\n");
                idx = val.IndexOf(text);

                if (idx > 0)
                {
                    break;
                }

                await Task.Delay(100);
            }
            
            idx.Should().BeGreaterOrEqualTo(0, val);

            if (caretPosition >= 0)
            {
                int offset = await page.EvaluateAsync<int>("blazorMonaco.editors[0].editor.getModel().getOffsetAt(blazorMonaco.editors[0].editor.getPosition())");
                offset.Should().Be(caretPosition + idx, val.Insert(offset, "'"));
            }
        }

        public static async Task EnsureLoaded(this IPage page)
        {
            await page.WaitForSelectorAsync("#sample-code-editor-123");
            await page.ClickAsync("#sample-code-editor-123");
        }

        public static async Task<IPage> EnsureLoaded(this (IBrowser, DevHostServerFixture) browser, int? exampleToLoad = null)
        {
            var page = await browser.Item1.NewPageAsync();
            page.PageError += Page_PageError;
            page.Console += Page_Console;

            await page.GotoAsync(browser.Item2.RootUri.AbsoluteUri +  (exampleToLoad.HasValue ? $"proj/{exampleToLoad}" : string.Empty));
            await page.WaitForSelectorAsync("#sample-code-editor-123");
            await page.ClickAsync("#sample-code-editor-123");
            await page.Press("Control+Home");

            return page;
        }

        private static void Page_Console(object sender, IConsoleMessage e)
        {
            if (e.Type != "debug")
            {
                Console.WriteLine(e.Text);
            }
        }

        private static void Page_PageError(object sender, string e)
        {
            Console.WriteLine(e);
        }

        /// <summary>
        /// E.g. Control+ArrowRight
        /// Backquote, Enter, Control, Minus, Equal, Backslash, Backspace, Tab, Delete, Escape,
        /// ArrowDown, End, Enter, Home, Insert, PageDown, PageUp, ArrowRight,
        /// ArrowUp, F1 - F12, Digit0 - Digit9, KeyA - KeyZ, etc.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static async Task Press(this IPage page, string keys)
        {
            await page.PressAsync("#sample-code-editor-123", keys);
        }

        public static async Task PressArrowLeft(this IPage page, bool ctrl = false, bool alt = false, bool shift = false, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "ArrowLeft");
            }
        }

        public static async Task PressArrowRight(this IPage page, bool ctrl = false, bool alt = false, bool shift = false, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "ArrowRight");
            }
        }

        public static async Task PressEnter(this IPage page, bool ctrl = false, bool alt = false, bool shift = false)
        {
            await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "Enter");
        }

        public static async Task PressControl(this IPage page)
        {
            await Press(page, "Control");
        }

        public static async Task PressBackspace(this IPage page, bool ctrl = false, bool alt = false, bool shift = false, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "Backspace");
            }
        }

        public static async Task PressTab(this IPage page, bool ctrl = false, bool alt = false, bool shift = false)
        {
            await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "Tab");
        }

        public static async Task PressDelete(this IPage page, bool ctrl = false, bool alt = false, bool shift = false)
        {
            await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "Delete");
        }

        public static async Task PressEscape(this IPage page, bool ctrl = false, bool alt = false, bool shift = false)
        {
            await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "Escape");
        }

        public static async Task PressArrowDown(this IPage page, bool ctrl = false, bool alt = false, bool shift = false)
        {
            await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "ArrowDown");
        }

        public static async Task PressArrowUp(this IPage page, bool ctrl = false, bool alt = false, bool shift = false)
        {
            await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "ArrowUp");
        }

        public static async Task PressEnd(this IPage page, bool ctrl = false, bool alt = false, bool shift = false)
        {
            await Press(page, (ctrl ? "Control+" : string.Empty) + (alt ? "Alt+" : string.Empty) + (shift ? "Shift+" : string.Empty) + "End");
        }

        public static async Task PressCtrlSpace(this IPage page)
        {
            await Press(page, "Control+Space");
        }

        public static async Task Type(this IPage page, string text)
        {
            await page.Locator("div.lines-content").PressSequentiallyAsync(text);
            // page.FillAsync -> to fill values in forms
            // page.PressAsync -> To press a special key, like <c>Control</c> or <c>ArrowDown</c>
        }
    }
}
