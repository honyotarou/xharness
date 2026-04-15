// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.CLI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests;

/// <summary>
/// Cycle 4 — attacker: diagnostics JSON path points at sensitive location.
/// </summary>
public class CommandDiagnosticsPathTests
{
    [Fact]
    public void SaveToJsonFile_RejectsUnsafeTargetPath()
    {
        var diag = new CommandDiagnostics(NullLogger.Instance, TargetPlatform.Android, "test");
        var ex = Assert.Throws<ArgumentException>(() => diag.SaveToJsonFile("/tmp/../etc/diag.json"));
        Assert.Equal("targetFile", ex.ParamName);
    }
}
