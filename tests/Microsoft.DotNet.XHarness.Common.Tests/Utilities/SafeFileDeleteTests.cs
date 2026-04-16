// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class SafeFileDeleteTests
{
    [Fact]
    public void TryDelete_LogsWhenFileIsLocked_Windows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var path = Path.GetTempFileName();
        var warnings = new List<string>();
        using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            SafeFileDelete.TryDelete(path, warnings.Add);
        }

        Assert.Single(warnings);
        Assert.Contains("could not delete", warnings[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryDelete_DeletesExistingFile_NoWarning()
    {
        var path = Path.GetTempFileName();
        var warnings = new List<string>();
        SafeFileDelete.TryDelete(path, warnings.Add);
        Assert.Empty(warnings);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void TryDelete_NullPath_NoOp()
    {
        SafeFileDelete.TryDelete(null, null);
    }
}
