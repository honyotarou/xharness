// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.Common.CLI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests;

/// <summary>
/// Attacker: multi-gigabyte diagnostics JSON to exhaust memory on merge.
/// </summary>
public class CommandDiagnosticsMergeSizeTests
{
    [Fact]
    public void SaveToJsonFile_SkipsMergeWhenExistingFileExceedsMaxBytes()
    {
        var path = Path.Combine(Path.GetTempPath(), "diag-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.SetLength(CommandDiagnostics.MaxExistingDiagnosticsJsonFileBytes + 1);
            }

            var diag = new CommandDiagnostics(NullLogger.Instance, TargetPlatform.Android, "test");
            diag.SaveToJsonFile(path);
            // Merge skipped; existing oversized file must not be read or rewritten.
            Assert.True(new FileInfo(path).Length > CommandDiagnostics.MaxExistingDiagnosticsJsonFileBytes);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
