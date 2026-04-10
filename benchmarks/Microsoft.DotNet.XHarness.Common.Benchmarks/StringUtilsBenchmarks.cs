// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using Microsoft.DotNet.XHarness.Common.Utilities;

namespace Microsoft.DotNet.XHarness.Common.Benchmarks;

[MemoryDiagnoser]
public class StringUtilsBenchmarks
{
    private readonly string[] _simple = new[] { "-n", "foo", "bar" };
    private readonly string[] _withSpaces = new[] { "--output", "path with spaces", "suffix" };

    [Benchmark]
    public string FormatArguments_Simple() => StringUtils.FormatArguments(_simple);

    [Benchmark]
    public string FormatArguments_WithSpaces() => StringUtils.FormatArguments(_withSpaces);

    [Benchmark]
    public string Quote_ShellChars() => StringUtils.Quote("foo bar's, $path\\");
}
