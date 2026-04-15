// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.DotNet.XHarness.CLI.CommandArguments.Android;

internal class PackageNameArgument : RequiredStringArgument
{
    private static readonly Regex s_safeAndroidPackageName = new(
        @"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public PackageNameArgument()
        : base("package-name=|p=", "Package name contained within the supplied APK")
    {
    }

    public override void Validate()
    {
        base.Validate();

        // Defense-in-depth: this value can reach tooling and logs; keep it strict and predictable.
        if (Value.Length > 255 || Value.StartsWith("-", StringComparison.Ordinal) || !s_safeAndroidPackageName.IsMatch(Value))
        {
            throw new ArgumentException("Unsafe Android package name.", nameof(PackageNameArgument));
        }
    }
}
