// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using Microsoft.DotNet.XHarness.Common.Networking;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Networking;

public class TcpListenerAddressResolverTests
{
    [Fact]
    public void GetListenAddress_RoundTripsEnvironmentVariable()
    {
        string prev = System.Environment.GetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName);
        try
        {
            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, null);
            Assert.Equal(IPAddress.Loopback, TcpListenerAddressResolver.GetListenAddress());

            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "true");
            Assert.Equal(IPAddress.Loopback, TcpListenerAddressResolver.GetListenAddress());

            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "1");
            Assert.Equal(IPAddress.Loopback, TcpListenerAddressResolver.GetListenAddress());

            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "false");
            Assert.Equal(IPAddress.Any, TcpListenerAddressResolver.GetListenAddress());

            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "0");
            Assert.Equal(IPAddress.Any, TcpListenerAddressResolver.GetListenAddress());
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, prev);
        }
    }

    [Fact]
    public void GetHttpListenerPrefixHost_AlignsWithGetListenAddress()
    {
        string prev = System.Environment.GetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName);
        try
        {
            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, null);
            Assert.Equal(IPAddress.Loopback, TcpListenerAddressResolver.GetListenAddress());
            Assert.Equal("127.0.0.1", TcpListenerAddressResolver.GetHttpListenerPrefixHost());

            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "true");
            Assert.Equal(IPAddress.Loopback, TcpListenerAddressResolver.GetListenAddress());
            Assert.Equal("127.0.0.1", TcpListenerAddressResolver.GetHttpListenerPrefixHost());

            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "1");
            Assert.Equal("127.0.0.1", TcpListenerAddressResolver.GetHttpListenerPrefixHost());

            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "false");
            Assert.Equal("*", TcpListenerAddressResolver.GetHttpListenerPrefixHost());

            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "0");
            Assert.Equal("*", TcpListenerAddressResolver.GetHttpListenerPrefixHost());
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, prev);
        }
    }

    [Fact]
    public void GetListenAddress_FailClosed_UnrecognizedValuesBindLoopback()
    {
        string prev = System.Environment.GetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName);
        try
        {
            foreach (string invalid in new[] { "invalid", "yes", "2", "TRUEE" })
            {
                System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, invalid);
                Assert.Equal(IPAddress.Loopback, TcpListenerAddressResolver.GetListenAddress());
                Assert.Equal("127.0.0.1", TcpListenerAddressResolver.GetHttpListenerPrefixHost());
            }
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, prev);
        }
    }

    [Fact]
    public void GetListenAddress_WhitespaceOnly_TreatsAsUnset()
    {
        string prev = System.Environment.GetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName);
        try
        {
            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, "   ");
            Assert.Equal(IPAddress.Loopback, TcpListenerAddressResolver.GetListenAddress());
            Assert.Equal("127.0.0.1", TcpListenerAddressResolver.GetHttpListenerPrefixHost());
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(TcpListenerAddressResolver.BindLoopbackOnlyVariableName, prev);
        }
    }
}
