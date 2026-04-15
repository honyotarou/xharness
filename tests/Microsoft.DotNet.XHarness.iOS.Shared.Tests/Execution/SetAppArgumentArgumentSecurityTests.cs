using Microsoft.DotNet.XHarness.iOS.Shared.Execution;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests.Execution;

public class SetAppArgumentArgumentSecurityTests
{
    [Fact]
    public void AsCommandLineArgument_EscapesSpacesAndQuotes()
    {
        var arg = new SetAppArgumentArgument("--foo bar\"baz");
        string cmd = arg.AsCommandLineArgument();

        Assert.Contains("-argument=", cmd);
        Assert.Contains("\"", cmd); // should be quoted/escaped for process argument string
        Assert.Contains("bar", cmd);
    }
}

