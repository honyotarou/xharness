// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// Cycle 2 — attacker: write diagnostics / xharness-result.json outside intended output via ../
/// </summary>
public class HostPathSecurityTests
{
    [Theory]
    [InlineData("/out/../etc/passwd")]
    [InlineData("../tmp/x")]
    public void ThrowIfUnsafeHostPath_RejectsTraversal(string path)
    {
        var ex = Assert.Throws<ArgumentException>(() => HostPathSecurity.ThrowIfUnsafeHostPath(path, "p"));
        Assert.Equal("p", ex.ParamName);
    }

    [Fact]
    public void ThrowIfUnsafeHostPath_AllowsNormalDirectory()
    {
        HostPathSecurity.ThrowIfUnsafeHostPath("/tmp/xharness-out", nameof(HostPathSecurityTests));
    }

    [Fact]
    public void ThrowIfUnsafeHostPath_RejectsUnicodeLineSeparators()
    {
        Assert.Throws<ArgumentException>(() =>
            HostPathSecurity.ThrowIfUnsafeHostPath("/tmp/a\u2028b", "p"));
    }

    [Fact]
    public void ThrowIfResolvedPathNotUnderBase_RejectsOutsidePath()
    {
        if (System.OperatingSystem.IsWindows())
        {
            return;
        }

        Assert.Throws<ArgumentException>(() =>
            HostPathSecurity.ThrowIfResolvedPathNotUnderBase("/tmp/jail", "/tmp/other", "p"));
    }

    [Fact]
    public void ThrowIfResolvedPathNotUnderBase_AllowsChildPath()
    {
        if (System.OperatingSystem.IsWindows())
        {
            return;
        }

        HostPathSecurity.ThrowIfResolvedPathNotUnderBase("/tmp/jail", "/tmp/jail/sub/file", "p");
    }

    [Fact]
    public void IsResolvedPathUnderBaseDirectory_RejectsSymlinkEscape()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = Path.Combine(Path.GetTempPath(), "xharness-sym-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var baseDir = Path.Combine(root, "safe");
            Directory.CreateDirectory(baseDir);
            var outside = Path.Combine(root, "outside");
            Directory.CreateDirectory(outside);
            var link = Path.Combine(baseDir, "escape");
            File.CreateSymbolicLink(link, outside);

            File.WriteAllText(Path.Combine(outside, "note.txt"), "x");
            var candidate = Path.Combine(link, "note.txt");

            Assert.False(HostPathSecurity.IsResolvedPathUnderBaseDirectory(baseDir, candidate));
            Assert.Throws<ArgumentException>(() =>
                HostPathSecurity.ThrowIfResolvedPathNotUnderBase(baseDir, candidate, "p"));
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public void IsResolvedPathUnderBaseDirectory_RejectsSymlinkEscape_NonexistentTail()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = Path.Combine(Path.GetTempPath(), "xharness-sym3-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var baseDir = Path.Combine(root, "safe");
            Directory.CreateDirectory(baseDir);
            var outside = Path.Combine(root, "outside");
            Directory.CreateDirectory(outside);
            var link = Path.Combine(baseDir, "escape");
            File.CreateSymbolicLink(link, outside);

            var candidate = Path.Combine(link, "ghost-not-created.txt");

            Assert.False(HostPathSecurity.IsResolvedPathUnderBaseDirectory(baseDir, candidate));
            Assert.Throws<ArgumentException>(() =>
                HostPathSecurity.ThrowIfResolvedPathNotUnderBase(baseDir, candidate, "p"));
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public void IsResolvedPathUnderBaseDirectory_AllowsRealChildFile()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = Path.Combine(Path.GetTempPath(), "xharness-sym2-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var baseDir = Path.Combine(root, "safe");
            Directory.CreateDirectory(baseDir);
            var ok = Path.Combine(baseDir, "ok.txt");
            File.WriteAllText(ok, "x");

            Assert.True(HostPathSecurity.IsResolvedPathUnderBaseDirectory(baseDir, ok));
            HostPathSecurity.ThrowIfResolvedPathNotUnderBase(baseDir, ok, "p");
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
