// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.iOS.Shared;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests;

/// <summary>
/// Round 2 (attacker: path traversal / odd host paths for result copy destination).
/// </summary>
public class ResultFileHostPathValidatorTests
{
    [Theory]
    [InlineData("/tmp/out/../secret.xml")]
    [InlineData("../tmp/out.xml")]
    [InlineData("/safe/../evil.xml")]
    [InlineData("..")]
    [InlineData("/a/..")]
    public void ValidateHostDestinationPath_RejectsParentSegments(string path)
    {
        var ex = Assert.Throws<ArgumentException>(() => ResultFileHostPathValidator.ValidateHostDestinationPath(path));
        Assert.Equal("path", ex.ParamName);
        Assert.Contains("parent-directory", ex.Message);
    }

    [Fact]
    public void ValidateHostDestinationPath_AllowsSimpleAbsolutePath()
    {
        ResultFileHostPathValidator.ValidateHostDestinationPath("/tmp/results/test-results.xml");
    }

    [Fact]
    public void ValidateHostDestinationPath_RejectsControlCharacters()
    {
        Assert.Throws<ArgumentException>(() =>
            ResultFileHostPathValidator.ValidateHostDestinationPath("/tmp/x\ny.xml"));
    }
}
