using Microsoft.DotNet.XHarness.CLI.Commands.Wasm;
using Microsoft.DotNet.XHarness.Common.CLI;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands.Wasm.Browser;

public class WasmBrowserTestRunnerExitCodeValidationTests
{
    [Fact]
    public void SuccessDomExitCode_WithErrorPattern_FailsClosed()
    {
        var validated = WasmBrowserTestRunner.ValidateExitCodeAgainstLogs(
            domExitCode: ExitCode.SUCCESS,
            logProcessorExitCode: ExitCode.SUCCESS,
            forwardedExitCode: null,
            lineThatMatchedErrorPattern: "Assertion failed");

        Assert.Equal(ExitCode.TESTS_FAILED, validated);
    }

    [Fact]
    public void SuccessDomExitCode_WithForwardedNonZero_FailsClosed()
    {
        var validated = WasmBrowserTestRunner.ValidateExitCodeAgainstLogs(
            domExitCode: ExitCode.SUCCESS,
            logProcessorExitCode: ExitCode.SUCCESS,
            forwardedExitCode: 1,
            lineThatMatchedErrorPattern: null);

        Assert.Equal(ExitCode.TESTS_FAILED, validated);
    }
}

