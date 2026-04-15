using Microsoft.DotNet.XHarness.CLI.Commands;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands.LoopbackTestServer;

public class ServerURLsRedactionTests
{
    [Fact]
    public void ToString_RedactsStatefulSessionToken()
    {
        var urls = new ServerURLs("127.0.0.1:1234", "127.0.0.1:5678", "SuperSecretToken");
        string s = urls.ToString();
        Assert.Contains("ServerURLs", s);
        Assert.DoesNotContain("SuperSecretToken", s);
        Assert.Contains("[REDACTED]", s);
    }
}

