using System;
using System.IO;
using Microsoft.DotNet.XHarness.Common.Logging;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Microsoft.DotNet.XHarness.iOS.Shared.Listeners;
using Moq;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests.Listeners;

public class SimpleHttpListenerDoSTests
{
    [Fact]
    public void MaxRequestBodyBytes_IsAlignedWithTcpLimits()
    {
        Assert.Equal(Microsoft.DotNet.XHarness.Common.Networking.TcpStreamLimits.MaxTestLogStreamBytes, SimpleHttpListener.MaxRequestBodyBytes);
    }

    [Fact]
    public void BoundedReadStream_ThrowsWhenExceedingLimit()
    {
        // Construct a stream bigger than the limit and ensure bounded read fails.
        var big = new MemoryStream(new byte[SimpleHttpListener.MaxRequestBodyBytes + 1]);
        Assert.Throws<InvalidOperationException>(() =>
            StreamReadLimits.ReadToEndWithByteLimit(big, SimpleHttpListener.MaxRequestBodyBytes));
    }
}

