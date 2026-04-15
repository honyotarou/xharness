// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.CLI.Commands.Wasm;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands.Wasm;

public class ErrorPatternScannerTests : IDisposable
{
    private readonly string _patternsPath;

    public ErrorPatternScannerTests()
    {
        _patternsPath = Path.Combine(Path.GetTempPath(), "errpat-" + Guid.NewGuid().ToString("N") + ".txt");
    }

    public void Dispose()
    {
        if (File.Exists(_patternsPath))
        {
            File.Delete(_patternsPath);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_RejectsUnsafePatternsFilePath()
    {
        Assert.Throws<ArgumentException>(() => new ErrorPatternScanner("/tmp/../evil.txt", NullLogger.Instance));
    }

    [Fact]
    public void SkipsRegexPatternLongerThanLimit()
    {
        File.WriteAllText(_patternsPath, "%" + new string('a', 100_001));
        var scanner = new ErrorPatternScanner(_patternsPath, NullLogger.Instance);
        Assert.False(scanner.IsError("any line", out _));
    }

    [Fact]
    public void CapsNumberOfCompiledRegexes()
    {
        using var sw = new StreamWriter(_patternsPath);
        for (int i = 0; i < 502; i++)
        {
            // Anchor so e.g. x50 does not match the line "x500".
            sw.WriteLine("%^x" + i + "$");
        }
        sw.Close();

        var scanner = new ErrorPatternScanner(_patternsPath, NullLogger.Instance);
        int matchCount = 0;
        for (int i = 0; i < 502; i++)
        {
            if (scanner.IsError("x" + i, out _))
            {
                matchCount++;
            }
        }
        Assert.True(matchCount <= 500);
    }
}
