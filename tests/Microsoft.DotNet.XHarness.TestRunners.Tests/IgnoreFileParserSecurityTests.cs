// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Xunit;

namespace Microsoft.DotNet.XHarness.TestRunners.Tests;

/// <summary>
/// Attacker: poison ignore paths via content-dir containing traversal or control chars.
/// </summary>
public class IgnoreFileParserSecurityTests
{
    [Fact]
    public async Task ParseContentFilesAsync_RejectsTraversalInContentDir()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            IgnoreFileParser.ParseContentFilesAsync("/tmp/../etc"));
    }

    [Fact]
    public void ParseContentFiles_RejectsTraversalInContentDir()
    {
        Assert.Throws<ArgumentException>(() =>
            IgnoreFileParser.ParseContentFiles("../out"));
    }

    [Fact]
    public void ParseTraitsContentFile_RejectsTraversalInContentDir()
    {
        Assert.Throws<ArgumentException>(() =>
            IgnoreFileParser.ParseTraitsContentFile("/x/../y", isXUnit: true));
    }

    [Fact]
    public async Task ParseTraitsFileAsync_RejectsTraversalInPath()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            IgnoreFileParser.ParseTraitsFileAsync("/tmp/../etc/traits.txt"));
    }

    [Fact]
    public async Task ParseContentFilesAsync_AllowsSafeTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "xht-ignore-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var result = await IgnoreFileParser.ParseContentFilesAsync(dir);
            Assert.NotNull(result);
        }
        finally
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {
                // best effort
            }
        }
    }
}
