// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// Attacker: huge error-pattern / config file read into memory as strings.
/// </summary>
public class FilePayloadSecurityTests : IDisposable
{
    private readonly string _path;

    public FilePayloadSecurityTests()
    {
        _path = Path.Combine(Path.GetTempPath(), "fps-" + Guid.NewGuid().ToString("N") + ".txt");
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
        catch
        {
            // best effort
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ThrowIfFileExceedsMaxBytes_AllowsUnderLimit()
    {
        File.WriteAllBytes(_path, new byte[100]);
        FilePayloadSecurity.ThrowIfFileExceedsMaxBytes(_path, nameof(FilePayloadSecurityTests), maxBytes: 200);
    }

    [Fact]
    public void ThrowIfFileExceedsMaxBytes_RejectsOverLimit()
    {
        File.WriteAllBytes(_path, new byte[300]);
        var ex = Assert.Throws<ArgumentException>(() =>
            FilePayloadSecurity.ThrowIfFileExceedsMaxBytes(_path, nameof(FilePayloadSecurityTests), maxBytes: 200));
        Assert.Equal(nameof(FilePayloadSecurityTests), ex.ParamName);
    }

    [Fact]
    public void ThrowIfFileExceedsMaxBytes_RejectsTraversalPath()
    {
        Assert.Throws<ArgumentException>(() =>
            FilePayloadSecurity.ThrowIfFileExceedsMaxBytes("/tmp/../etc/passwd", "p", maxBytes: 100));
    }

    [Fact]
    public void ThrowIfFileExceedsMaxBytes_AllowsExactlyAtLimit()
    {
        File.WriteAllBytes(_path, new byte[500]);
        FilePayloadSecurity.ThrowIfFileExceedsMaxBytes(_path, nameof(FilePayloadSecurityTests), maxBytes: 500);
    }

    [Fact]
    public void DefaultMaxTextFileBytes_IsPositive()
    {
        Assert.True(FilePayloadSecurity.DefaultMaxTextFileBytes > 0);
    }

    [Fact]
    public void DefaultMaxCrashListFileBytes_IsPositive()
    {
        Assert.InRange(FilePayloadSecurity.DefaultMaxCrashListFileBytes, 1024, long.MaxValue);
    }

    [Fact]
    public void DefaultMaxCrashReportTextBytes_IsPositive()
    {
        Assert.InRange(FilePayloadSecurity.DefaultMaxCrashReportTextBytes, FilePayloadSecurity.DefaultMaxCrashListFileBytes, long.MaxValue);
    }

    [Fact]
    public void ThrowIfFileExceedsMaxBytes_RejectsWhenOverCrashListDefault()
    {
        File.WriteAllBytes(_path, new byte[600]);
        Assert.Throws<ArgumentException>(() =>
            FilePayloadSecurity.ThrowIfFileExceedsMaxBytes(_path, nameof(FilePayloadSecurityTests), maxBytes: 500));
    }

    [Fact]
    public void ThrowIfFileExceedsMaxBytes_AllowsJustUnderCrashListCap()
    {
        File.WriteAllBytes(_path, new byte[100]);
        FilePayloadSecurity.ThrowIfFileExceedsMaxBytes(
            _path,
            nameof(FilePayloadSecurityTests),
            maxBytes: FilePayloadSecurity.DefaultMaxCrashListFileBytes);
    }

    [Fact]
    public void CrashReportMax_IsLargerThanWasmTextDefault()
    {
        Assert.True(FilePayloadSecurity.DefaultMaxCrashReportTextBytes >= FilePayloadSecurity.DefaultMaxTextFileBytes);
    }
}

