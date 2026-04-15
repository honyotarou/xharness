// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.DotNet.XHarness.Common.CLI;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests;

/// <summary>
/// Cycle 3 — attacker: poison output directory path for JSON artifact.
/// </summary>
public class RunSummaryEmitterTests
{
    [Fact]
    public void WriteResultJsonFile_RejectsUnsafeOutputDirectory()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            RunSummaryEmitter.WriteResultJsonFile(
                "/tmp/../etc",
                ExitCode.SUCCESS,
                "android",
                null,
                null,
                null,
                null,
                Array.Empty<DiagnosticsFile>()));
        Assert.Equal("outputDirectory", ex.ParamName);
    }
}
