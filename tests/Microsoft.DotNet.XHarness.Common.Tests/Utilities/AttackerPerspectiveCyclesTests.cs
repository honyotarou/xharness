// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.DotNet.XHarness.Common.CLI;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// Fifty attacker-view review cycles: path/JSON/CLI token/regex bounds — each row is review → test → behavior locked by implementation.
/// </summary>
public class AttackerPerspectiveCyclesTests
{
    public static IEnumerable<object[]> HostPathAndRelatedCycles()
    {
        foreach (var path in TraversalAndBadPaths)
        {
            yield return new object[] { path, true };
        }

        foreach (var path in SafePaths)
        {
            yield return new object[] { path, false };
        }
    }

    private static readonly string[] TraversalAndBadPaths =
    {
        "/out/../etc",
        "../rel",
        "a/../b",
        "/x/y/../z/../..",
        "..",
        "/safe/../..",
        "prefix/../suffix",
        "/a/b/c/../../../d",
        "\\server\\..\\share",
        "C:\\a\\..\\b",
        "/tmp/a\u2028b",
        "/tmp/a\u2029b",
        "/x/\0y",
        "/x/\ny",
        "a/../b/c",
        "/var/../root",
        "./../x",
        "foo/../",
        "/../abs",
        "mid/../end",
        "a/b/../c/../..",
        "/usr/local/../bin/../..",
        "segment/..",
        "..\\windows",
        "/a/b/c/d/../../../e/../..",
    };

    private static readonly string[] SafePaths =
    {
        "/tmp/xharness-out",
        "/var/folders/abc/T/x",
        "C:/Users/dev/out",
        "/Users/me/Projects/app",
        "/a/b/c/d",
        "relative/subdir",
        "single",
        "/",
        "/opt/dotnet",
        "/Volumes/Data/build",
        "/home/user/.cache/xharness",
        "/tmp/.hidden/out",
        "subdir/name-with-dashes",
        "Unicode/テスト/out",
        "/Library/Developer/Xcode",
        "/android/sdk/platforms",
        "artifacts/bin/Debug",
        "C:/Program Files/dotnet",
        "/usr/share",
        "/mnt/wsl/projects/x",
        "/a.b/c.d/e",
        "deep/nested/path/ok",
        "/var/tmp/build",
        "D:/build/out",
        "/snap/dotnet/123",
    };

    [Theory]
    [MemberData(nameof(HostPathAndRelatedCycles))]
    public void Cycle_HostPathSecurity(string path, bool expectThrow)
    {
        if (expectThrow)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                HostPathSecurity.ThrowIfUnsafeHostPath(path, "p"));
            Assert.Equal("p", ex.ParamName);
        }
        else
        {
            HostPathSecurity.ThrowIfUnsafeHostPath(path, "p");
        }
    }

    public static IEnumerable<object[]> JsonCycles()
    {
        yield return new object[] { "{\"a\":1}", 100, false };
        yield return new object[] { new string('x', 11), 10, true };
        yield return new object[] { "[]", 2, false };
        yield return new object[] { new string('{', 5000), 100, true };
        yield return new object[] { "{}", 10, false };
    }

    [Theory]
    [MemberData(nameof(JsonCycles))]
    public void Cycle_JsonPayloadBounds(string json, int maxChars, bool expectThrow)
    {
        if (expectThrow)
        {
            Assert.Throws<ArgumentException>(() =>
                JsonPayloadSecurity.ThrowIfJsonStringTooLong(json, "j", maxChars));
        }
        else
        {
            JsonPayloadSecurity.ThrowIfJsonStringTooLong(json, "j", maxChars);
        }
    }

    public static IEnumerable<object[]> CliTokenCycles()
    {
        yield return new object[] { "safe.token", false };
        yield return new object[] { "a\tb", true };
        yield return new object[] { "x\vz", true };
        yield return new object[] { "ok-123", false };
        yield return new object[] { "a\u007fb", true };
    }

    [Theory]
    [MemberData(nameof(CliTokenCycles))]
    public void Cycle_CliTokenControlChars(string token, bool expectThrow)
    {
        if (expectThrow)
        {
            Assert.Throws<ArgumentException>(() =>
                CliTokenValidator.ThrowIfContainsControlCharacters(token, "t"));
        }
        else
        {
            CliTokenValidator.ThrowIfContainsControlCharacters(token, "t");
        }
    }

    [Fact]
    public void Cycle_Regex_ReDoSMitigation_UsesBoundedMatchTimeout()
    {
        var r = RegexSecurity.Create(@"^(a+)+$");
        Assert.Equal(RegexSecurity.DefaultMatchTimeout, r.MatchTimeout);
        Assert.Equal(TimeSpan.FromSeconds(1), RegexSecurity.DefaultMatchTimeout);
    }
}
