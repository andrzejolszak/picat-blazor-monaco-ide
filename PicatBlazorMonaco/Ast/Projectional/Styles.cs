using BlazorMonaco;
using BlazorMonaco.Editor;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Ast2
{
    public static class Styles
    {
        // TODO
        public static readonly ModelDecorationOptions NormalTextStyle = new ModelDecorationOptions
        {
            InlineClassName = "colorBlack",
        };

        public static readonly ModelDecorationOptions NormalTextGrayBackgroundStyle = new ModelDecorationOptions
        {
            InlineClassName = "backgroundLightGray",
        };

        public static readonly ModelDecorationOptions NormalTextRedBackgroundStyle = new ModelDecorationOptions
        {
            InlineClassName = "backgroundIndianRed",
        };

        public static readonly ModelDecorationOptions NormalTextBlueBackgroundStyle = new ModelDecorationOptions
        {
            InlineClassName = "backgroundLightBlue",
        };

        public static readonly ModelDecorationOptions GrayTextStyle = new ModelDecorationOptions
        {
            InlineClassName = "colorGray",
        };

        public static readonly ModelDecorationOptions Scratch;
        public static readonly ModelDecorationOptions UnderlineStyle = new ModelDecorationOptions
        {
            InlineClassName = "underline",
        };

        public static readonly ModelDecorationOptions SelectedParentNodeText = new ModelDecorationOptions
        {
            InlineClassName = "backgroundYellowImportant",
        };

        public static readonly ModelDecorationOptions SelectedNodeText = new ModelDecorationOptions
        {
            InlineClassName = "backgroundLightGreenImportant",
        };

        public static readonly ModelDecorationOptions InvisibleCharsStyle;

        public static async Task CreateCssStyles(IJSRuntime jsRuntime)
        {
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".decorationPointer { cursor: pointer; }");
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".colorGray { color: gray; }");
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".colorBlack { color: black; }");
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".backgroundLightGray { background: lightgray; }");
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".backgroundLightBlue { background: lightblue; }");
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".backgroundIndianRed { background: indianred; }");
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".underline { text-decoration: underline; }");
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".backgroundLightGreenImportant { background: lightgreen !important; }");
            await jsRuntime.InvokeVoidAsync("createCssStyle", ".backgroundYellowImportant { background: yellow; }");
        }
    }
}
