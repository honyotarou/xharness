// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net;
using Microsoft.DotNet.XHarness.Common.Logging;
using Microsoft.DotNet.XHarness.Common.Networking;
using Microsoft.DotNet.XHarness.iOS.Shared.Listeners;
using Moq;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests.Listeners;

public class SimpleListenerBindAddressTests
{
    [Fact]
    public void SimpleListener_AddressMatchesTcpListenerAddressResolver()
    {
        string prev = Environment.GetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName);
        try
        {
            Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, null);
            using var any = new SimpleListenerAddressProbe(new Mock<ILog>().Object, new Mock<IFileBackedLog>().Object);
            Assert.Equal(IPAddress.Loopback, any.ListenAddress);

            Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "true");
            using var loopback = new SimpleListenerAddressProbe(new Mock<ILog>().Object, new Mock<IFileBackedLog>().Object);
            Assert.Equal(IPAddress.Loopback, loopback.ListenAddress);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, prev);
        }
    }

    private sealed class SimpleListenerAddressProbe : SimpleListener
    {
        public SimpleListenerAddressProbe(ILog log, IFileBackedLog testLog) : base(log, testLog)
        {
        }

        public IPAddress ListenAddress => Address;

        public override int InitializeAndGetPort() => 0;

        protected override void Start()
        {
        }

        protected override void Stop()
        {
        }
    }
}
