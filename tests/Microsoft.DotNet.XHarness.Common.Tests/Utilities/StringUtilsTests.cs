// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class StringUtilsTests
{
    private static readonly char s_shellQuoteChar =
        (int)Environment.OSVersion.Platform != 128
            && Environment.OSVersion.Platform != PlatformID.Unix
            && Environment.OSVersion.Platform != PlatformID.MacOSX
        ? '"'   // Windows
        : '\''; // !Windows

    [Fact]
    public void NoEscapingNeeded() => Assert.Equal("foo", StringUtils.Quote("foo"));

    [Fact]
    public void Quote_Null_ReturnsEmpty() => Assert.Equal(string.Empty, StringUtils.Quote(null));

    [Fact]
    public void Quote_Empty_ReturnsEmpty() => Assert.Equal(string.Empty, StringUtils.Quote(string.Empty));

    [Theory]
    [InlineData(",")]
    [InlineData("$")]
    [InlineData("\\")]
    public void Quote_ShellSpecialChars_Quoted(string special)
    {
        var q = StringUtils.Quote("x" + special + "y");
        Assert.StartsWith(s_shellQuoteChar.ToString(), q);
        Assert.EndsWith(s_shellQuoteChar.ToString(), q);
    }

    [Theory]
    [InlineData("foo bar", "foo bar")]
    [InlineData("foo \"bar\"", "foo \\\"bar\\\"")]
    [InlineData("foo bar's", "foo bar\\\'s")]
    [InlineData("foo $bar's", "foo $bar\\\'s")]
    public void QuoteForProcessTest(string input, string expected) => Assert.Equal(s_shellQuoteChar + expected + s_shellQuoteChar, StringUtils.Quote(input));

    [Fact]
    public void FormatArguments_ParamsNull_JoinsEmpty() => Assert.Equal(string.Empty, StringUtils.FormatArguments((string[])null));

    [Fact]
    public void FormatArguments_NoArgs_JoinsEmpty() => Assert.Equal(string.Empty, StringUtils.FormatArguments());

    [Fact]
    public void FormatArguments_ListNull_JoinsEmpty() => Assert.Equal(string.Empty, StringUtils.FormatArguments((IList<string>)null));

    [Fact]
    public void FormatArguments_ListEmpty_JoinsEmpty() => Assert.Equal(string.Empty, StringUtils.FormatArguments(new List<string>()));

    [Theory]
    [InlineData("a", "b", "a b")]
    [InlineData("-n", "x", "-n x")]
    public void FormatArguments_JoinsWithSpace(string a, string b, string expected) =>
        Assert.Equal(expected, StringUtils.FormatArguments(a, b));

    [Theory]
    [InlineData("one arg")]
    [InlineData("two words")]
    public void FormatArguments_ArgWithSpace_UsesProcessQuoting(string token)
    {
        var joined = StringUtils.FormatArguments("prefix", token, "suffix");
        Assert.Contains("prefix", joined);
        Assert.Contains("suffix", joined);
        Assert.Contains(token, joined);
    }

    [Fact]
    public void FormatArguments_ArgWithDoubleQuote_IsEscapedOnce()
    {
        var joined = StringUtils.FormatArguments("a\"b");
        Assert.Contains("\\\"", joined);
        // Previous bug produced a backslash followed by multiple quotes (e.g. \\\"\"\")
        Assert.DoesNotContain("\\\"\"\"", joined);
    }

    [Fact(Skip = "Only works on OSX/Linux")]
    public void FormatArgumentsTest()
    {
        var p = new Process();
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = "/bin/echo";

        var complexInput = "'";

        p.StartInfo.Arguments = StringUtils.FormatArguments("-n", "foo", complexInput, "bar");
        p.Start();
        var output = p.StandardOutput.ReadToEnd();
        Assert.Equal($"foo {complexInput} bar", output);
    }
}
