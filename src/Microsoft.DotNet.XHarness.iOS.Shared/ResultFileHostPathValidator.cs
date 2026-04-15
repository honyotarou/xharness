// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.Utilities;

namespace Microsoft.DotNet.XHarness.iOS.Shared;

/// <summary>
/// Rejects host paths that are likely path-traversal or injection attempts for result file copies.
/// </summary>
public static class ResultFileHostPathValidator
{
    public static void ValidateHostDestinationPath(string path) =>
        HostPathSecurity.ThrowIfUnsafeHostPath(path, nameof(path));
}
