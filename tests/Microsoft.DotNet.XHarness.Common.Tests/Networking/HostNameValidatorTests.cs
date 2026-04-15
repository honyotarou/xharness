using Microsoft.DotNet.XHarness.Common.Networking;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Networking;

public class HostNameValidatorTests
{
    [Theory]
    [InlineData("example.com")]
    [InlineData("sub.example.com")]
    public void IsSafeHostName_AllowsNormalDnsNames(string host) =>
        Assert.True(HostNameValidator.IsSafeHostName(host));

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("169.254.169.254")] // cloud metadata
    [InlineData("0.0.0.0")]
    [InlineData("::1")]
    [InlineData("localhost")]
    public void IsSafeHostName_RejectsLocalOrMetadataHosts(string host) =>
        Assert.False(HostNameValidator.IsSafeHostName(host));
}

