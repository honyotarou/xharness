// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.Networking;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Networking;

/// <summary>
/// Rounds 4–5 (attacker: unbounded TCP reads).
/// </summary>
public class TcpStreamLimitsTests
{
    [Theory]
    [InlineData(0, 100, 1000, 100)]
    [InlineData(900, 200, 1000, 100)]
    [InlineData(1000, 200, 1000, 0)]
    [InlineData(1000, 0, 1000, 0)]
    public void AllowedReadLength_Clamp(int current, int read, int max, int expected)
    {
        Assert.Equal(expected, TcpStreamLimits.AllowedReadLength(current, read, max));
    }
}
