// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// Attacker: huge tool stdout/stderr merged into diagnostics JSON — bound before JsonDocument.Parse.
/// </summary>
public class JsonPayloadSecurityTests
{
    [Fact]
    public void ThrowIfJsonStringTooLong_AllowsWithinLimit()
    {
        JsonPayloadSecurity.ThrowIfJsonStringTooLong("{}", nameof(JsonPayloadSecurityTests), maxChars: 10);
    }

    [Fact]
    public void ThrowIfJsonStringTooLong_RejectsOverLimit()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            JsonPayloadSecurity.ThrowIfJsonStringTooLong("0123456789", nameof(JsonPayloadSecurityTests), maxChars: 9));
        Assert.Equal(nameof(JsonPayloadSecurityTests), ex.ParamName);
    }

    [Fact]
    public void ThrowIfJsonStringTooLong_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JsonPayloadSecurity.ThrowIfJsonStringTooLong(null!, "json"));
    }
}
