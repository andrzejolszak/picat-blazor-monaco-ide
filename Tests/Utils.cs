using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PlaywrightSharp;
using PlaywrightSharp.Chromium;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

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

            string val = (await page.GetInnerTextAsync("div.lines-content")).Replace("·", " ").Replace("\n", "\r\n");
            int idx = val.IndexOf(text);
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

        public static async Task<IPage> EnsureLoaded(this (IChromiumBrowser, DevHostServerFixture) browser, int? exampleToLoad = null)
        {
            var page = await browser.Item1.NewPageAsync();
            page.PageError += Page_PageError;
            page.Console += Page_Console;

            await page.GoToAsync(browser.Item2.RootUri.AbsoluteUri + (exampleToLoad.HasValue ? $"{exampleToLoad}" : string.Empty));
            await page.WaitForSelectorAsync("#sample-code-editor-123");
            await page.ClickAsync("#sample-code-editor-123");
            await page.Press("Control+Home");

            return page;
        }

        private static void Page_Console(object sender, ConsoleEventArgs e)
        {
            if (e.Message.Type != "debug")
            {
                Console.WriteLine(e.Message.Text);
            }
        }

        private static void Page_PageError(object sender, PageErrorEventArgs e)
        {
            Console.WriteLine(e.Message);
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
            await page.TypeAsync("div.lines-content", text);
        }
    }
}
