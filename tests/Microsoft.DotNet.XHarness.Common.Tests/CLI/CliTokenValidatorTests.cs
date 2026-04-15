// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.CLI;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.CLI;

/// <summary>
/// Round 1 (attacker: argv injection via newlines into subprocess tokens).
/// </summary>
public class CliTokenValidatorTests
{
    [Fact]
    public void ThrowIfContainsControlCharacters_AllowsPrintable()
    {
        CliTokenValidator.ThrowIfContainsControlCharacters("com.example.app", nameof(CliTokenValidatorTests));
    }

    [Theory]
    [InlineData("a\u2028b")]
    [InlineData("a\u2029b")]
    public void ThrowIfContainsControlCharacters_RejectsUnicodeLineSeparators(string malicious)
    {
        Assert.Throws<ArgumentException>(() =>
            CliTokenValidator.ThrowIfContainsControlCharacters(malicious, "param"));
    }

    [Theory]
    [InlineData("a\nb")]
    [InlineData("a\rb")]
    [InlineData("\0")]
    [InlineData("x\u0001")]
    public void ThrowIfContainsControlCharacters_RejectsInjection(string malicious)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            CliTokenValidator.ThrowIfContainsControlCharacters(malicious, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Fact]
    public void ThrowIfContainsControlCharacters_RejectsNullOrEmpty()
    {
        Assert.Throws<System.ArgumentNullException>(() => CliTokenValidator.ThrowIfContainsControlCharacters(null, "p"));
        Assert.Throws<ArgumentException>(() => CliTokenValidator.ThrowIfContainsControlCharacters("", "p"));
    }

    [Theory]
    [InlineData(0xD800)] // isolated high surrogate
    [InlineData(0xDC00)] // isolated low surrogate
    public void ThrowIfContainsControlCharacters_RejectsIsolatedSurrogates(int surrogate)
    {
        var malicious = "safe" + new string((char)surrogate, 1) + "value";
        Assert.Throws<ArgumentException>(() =>
            CliTokenValidator.ThrowIfContainsControlCharacters(malicious, "param"));
    }
}
