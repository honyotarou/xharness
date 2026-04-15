// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.Common.CLI;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Validates host file-system paths used for output artifacts (attacker: traversal or log injection via path strings).
/// </summary>
public static class HostPathSecurity
{
    /// <summary>
    /// Throws if <paramref name="path"/> is not suitable as a host output path (control chars, line separators, or <c>..</c> segments).
    /// </summary>
    public static void ThrowIfUnsafeHostPath(string path, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        CliTokenValidator.ThrowIfContainsControlCharacters(path, paramName);

        string normalized = path.Replace('\\', '/');
        if (normalized.Contains("/../", StringComparison.Ordinal) ||
            normalized.EndsWith("/..", StringComparison.Ordinal) ||
            normalized.StartsWith("../", StringComparison.Ordinal) ||
            normalized == "..")
        {
            throw new ArgumentException("Path cannot contain parent-directory (..) segments.", paramName);
        }
    }

    /// <summary>
    /// Returns true if <paramref name="candidatePath"/> resolves under <paramref name="baseDirectory"/> (attacker: zip-slip via relative filename segments).
    /// </summary>
    public static bool IsResolvedPathUnderBaseDirectory(string baseDirectory, string candidatePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
        ArgumentException.ThrowIfNullOrEmpty(candidatePath);
        ThrowIfUnsafeHostPath(baseDirectory, nameof(baseDirectory));
        ThrowIfUnsafeHostPath(candidatePath, nameof(candidatePath));

        string fullBase = GetPathForContainmentCheck(baseDirectory);
        string fullCandidate = GetPathForContainmentCheck(candidatePath);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (fullCandidate.Equals(fullBase, comparison))
        {
            return true;
        }

        if (!fullBase.EndsWith(Path.DirectorySeparatorChar) && !fullBase.EndsWith(Path.AltDirectorySeparatorChar))
        {
            fullBase += Path.DirectorySeparatorChar;
        }

        return fullCandidate.StartsWith(fullBase, comparison);
    }

    /// <summary>
    /// Resolves <see cref="Path.GetFullPath"/> and walks path components, resolving symbolic links at each step.
    /// If a path segment does not exist yet, returns the resolved prefix combined with the missing tail so
    /// containment checks cannot be fooled by a symlink under the base (attacker: symlink escape).
    /// </summary>
    private static string GetPathForContainmentCheck(string path)
    {
        path = Path.GetFullPath(path);

        try
        {
            return ResolvePathSymlinksWithOptionalTail(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return path;
    }

    private static string ResolvePathSymlinksWithOptionalTail(string path)
    {
        string? root = Path.GetPathRoot(path);
        if (string.IsNullOrEmpty(root))
        {
            return path;
        }

        bool resolvedSymlink = false;
        string current = root;
        string remainder = path.Length > root.Length ? path.Substring(root.Length) : string.Empty;
        string[] segments = remainder.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string segment in segments)
        {
            string next = Path.GetFullPath(Path.Combine(current, segment));
            if (!File.Exists(next) && !Directory.Exists(next))
            {
                // Without symlink resolution, return the full normalized path so prefix checks stay correct
                // (e.g. /srv/http vs /srv/https). After resolving a symlink, return the resolved prefix + tail.
                return resolvedSymlink ? next : path;
            }

            if (Directory.Exists(next))
            {
                var di = new DirectoryInfo(next);
                FileSystemInfo? resolved = di.ResolveLinkTarget(returnFinalTarget: true);
                if (resolved != null)
                {
                    current = resolved.FullName;
                    resolvedSymlink = true;
                }
                else
                {
                    current = di.FullName;
                }
            }
            else if (File.Exists(next))
            {
                var fi = new FileInfo(next);
                FileSystemInfo? resolved = fi.ResolveLinkTarget(returnFinalTarget: true);
                if (resolved != null)
                {
                    current = resolved.FullName;
                    resolvedSymlink = true;
                }
                else
                {
                    current = fi.FullName;
                }
            }
        }

        return current;
    }

    /// <summary>
    /// Throws if <paramref name="candidatePath"/> resolves outside <paramref name="baseDirectory"/>.
    /// </summary>
    public static void ThrowIfResolvedPathNotUnderBase(string baseDirectory, string candidatePath, string paramName)
    {
        if (!IsResolvedPathUnderBaseDirectory(baseDirectory, candidatePath))
        {
            throw new ArgumentException("Path escapes the base directory.", paramName);
        }
    }
}
