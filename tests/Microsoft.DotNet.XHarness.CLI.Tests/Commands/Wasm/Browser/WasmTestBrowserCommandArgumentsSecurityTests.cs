using System;
using Microsoft.DotNet.XHarness.CLI.CommandArguments.Wasm;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands.Wasm.Browser;

public class WasmTestBrowserCommandArgumentsSecurityTests
{
    [Theory]
    [InlineData("--disable-web-security")]
    [InlineData("--allow-file-access-from-files")]
    [InlineData("--disable-site-isolation-trials")]
    [InlineData("--user-data-dir=/tmp/profile")]
    [InlineData("--unsafely-treat-insecure-origin-as-secure=http://127.0.0.1")]
    public void ValidateBrowserArgs_RejectsDangerousFlags(string arg)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            WasmTestBrowserCommandArguments.ValidateBrowserArgs(new[] { arg }));
        Assert.Contains("Rejected dangerous", ex.Message);
    }

    [Fact]
    public void ValidateBrowserArgs_AllowsBenignFlags()
    {
        WasmTestBrowserCommandArguments.ValidateBrowserArgs(new[]
        {
            "--lang=en-US",
            "--window-size=1200,800",
        });
    }
}

