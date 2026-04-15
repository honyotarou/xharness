// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.CLI;

namespace Microsoft.DotNet.XHarness.iOS.Shared;

/// <summary>
/// argv-style arguments for copying test result files from iOS simulators / devices without invoking a shell.
/// </summary>
public static class ResultFileCopyArguments
{
    public const string XcrunExecutable = "/usr/bin/xcrun";
    public const string CpExecutable = "/bin/cp";

    /// <summary>
    /// Arguments for: xcrun simctl get_app_container &lt;udid&gt; &lt;bundleId&gt; data
    /// </summary>
    public static string[] XcrunSimctlGetAppContainer(string udid, string bundleIdentifier)
    {
        CliTokenValidator.ThrowIfContainsControlCharacters(udid, nameof(udid));
        CliTokenValidator.ThrowIfContainsControlCharacters(bundleIdentifier, nameof(bundleIdentifier));
        return new[] { "simctl", "get_app_container", udid, bundleIdentifier, "data" };
    }

    /// <summary>
    /// Arguments for: xcrun devicectl device copy from ... (iOS 18+ device path).
    /// </summary>
    public static string[] XcrunDevicectlCopyFrom(
        string udid,
        string sourcePath,
        string hostDestinationPath,
        string bundleIdentifier)
    {
        CliTokenValidator.ThrowIfContainsControlCharacters(udid, nameof(udid));
        CliTokenValidator.ThrowIfContainsControlCharacters(sourcePath, nameof(sourcePath));
        CliTokenValidator.ThrowIfContainsControlCharacters(hostDestinationPath, nameof(hostDestinationPath));
        CliTokenValidator.ThrowIfContainsControlCharacters(bundleIdentifier, nameof(bundleIdentifier));

        return new[]
        {
            "devicectl", "device", "copy", "from",
            "--device", udid,
            "--source", sourcePath,
            "--destination", hostDestinationPath,
            "--user", "mobile",
            "--domain-type", "appDataContainer",
            "--domain-identifier", bundleIdentifier,
        };
    }

    /// <summary>
    /// Builds the full host path to the test results file inside the simulator app data container.
    /// </summary>
    public static string CombineSimulatorContainerPath(string containerPath, string relativePathInsideContainer)
    {
        CliTokenValidator.ThrowIfContainsControlCharacters(containerPath, nameof(containerPath));
        ArgumentException.ThrowIfNullOrEmpty(relativePathInsideContainer);
        if (relativePathInsideContainer[0] != '/')
        {
            throw new ArgumentException("Relative path must start with '/'.", nameof(relativePathInsideContainer));
        }

        return containerPath.TrimEnd('/') + relativePathInsideContainer;
    }
}
