// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Best-effort file delete with optional warning callback (never silent empty catch).
/// </summary>
public static class SafeFileDelete
{
    public static void TryDelete(string? path, Action<string>? logWarning)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            logWarning?.Invoke($"Warning: could not delete file '{path}': {ExceptionLogFormatter.FormatSummary(ex)}");
        }
    }
}
