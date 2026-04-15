// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.XHarness.CLI.Commands;

/// <summary>
/// Bound addresses returned after the loopback test server starts.
/// </summary>
public record ServerURLs(string Http, string? Https, string? StatefulSessionToken = null);
