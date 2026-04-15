// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.XHarness.Common.Logging;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Microsoft.DotNet.XHarness.iOS.Shared.Utilities;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Logging;

public class Logs : List<IFileBackedLog>, ILogs
{
    private readonly IHelpers _helpers = new Helpers();

    public string Directory { get; set; }

    public Logs(string directory)
    {
        ArgumentNullException.ThrowIfNull(directory);
        HostPathSecurity.ThrowIfUnsafeHostPath(directory, nameof(directory));
        Directory = directory;
    }

    public IFileBackedLog Create(string filename, string description, bool? timestamp = null)
    {
        System.IO.Directory.CreateDirectory(Directory);
        var full = Path.GetFullPath(Path.Combine(Directory, filename));
        HostPathSecurity.ThrowIfResolvedPathNotUnderBase(Directory, full, nameof(filename));
        var rv = new LogFile(description, full);
        if (timestamp.HasValue)
        {
            rv.Timestamp = timestamp.Value;
        }

        Add(rv);
        return rv;
    }

    // Adds an existing file to this collection of logs.
    // If the file is not inside the log directory, then it's copied there.
    // 'path' must be a full path to the file.
    public IFileBackedLog AddFile(string path) => AddFile(path, Path.GetFileName(path));

    // Adds an existing file to this collection of logs.
    // If the file is not inside the log directory, then it's copied there.
    // 'path' must be a full path to the file.
    public IFileBackedLog AddFile(string path, string name)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        HostPathSecurity.ThrowIfUnsafeHostPath(path, nameof(path));
        string fullPath = Path.GetFullPath(path);
        string baseResolved = Path.GetFullPath(Directory);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        string basePrefix = baseResolved.EndsWith(Path.DirectorySeparatorChar) || baseResolved.EndsWith(Path.AltDirectorySeparatorChar)
            ? baseResolved
            : baseResolved + Path.DirectorySeparatorChar;
        bool underBase = fullPath.Equals(baseResolved, comparison) || fullPath.StartsWith(basePrefix, comparison);

        if (!underBase)
        {
            var newPath = Path.Combine(Directory, Path.GetFileNameWithoutExtension(path) + "-" + _helpers.Timestamp + Path.GetExtension(path));
            fullPath = Path.GetFullPath(newPath);
            HostPathSecurity.ThrowIfUnsafeHostPath(newPath, nameof(path));
            HostPathSecurity.ThrowIfResolvedPathNotUnderBase(Directory, fullPath, nameof(path));
            File.Copy(path, fullPath, true);
        }
        else
        {
            HostPathSecurity.ThrowIfResolvedPathNotUnderBase(Directory, fullPath, nameof(path));
        }

        var log = new LogFile(name, fullPath, true);
        Add(log);
        return log;
    }

    // Create an empty file in the log directory and return the full path to the file
    public string CreateFile(string path, string description)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        var full = Path.GetFullPath(Path.Combine(Directory, path));
        HostPathSecurity.ThrowIfResolvedPathNotUnderBase(Directory, full, nameof(path));
        using (var rv = new LogFile(description, full, false))
        {
            Add(rv);
            return rv.FullPath;
        }
    }

    public string CreateFile(string path, LogType type) => CreateFile(path, type.ToString());

    public void Dispose()
    {
        foreach (var log in this)
        {
            log.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
