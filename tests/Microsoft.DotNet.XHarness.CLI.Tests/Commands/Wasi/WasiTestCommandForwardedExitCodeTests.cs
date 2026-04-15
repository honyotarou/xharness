using Microsoft.DotNet.XHarness.CLI.Commands.Wasi;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands.Wasi;

public class WasiTestCommandForwardedExitCodeTests
{
    [Theory]
    [InlineData(1, 0, 1)] // must not downgrade failure
    [InlineData(0, 1, 1)] // can upgrade to failure
    [InlineData(0, 0, 0)] // ok
    [InlineData(2, 3, 3)] // forwarded non-zero wins
    [InlineData(2, null, 2)] // no forwarded keeps process
    public void ApplyForwardedExitCode_RespectsFailure(int processExit, int? forwarded, int expected)
    {
        Assert.Equal(expected, WasiTestCommand.ApplyForwardedExitCode(processExit, forwarded));
    }
}

