// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class RegexSecurityTests
{
    [Fact]
    public void Create_SetsDefaultMatchTimeout()
    {
        var r = RegexSecurity.Create("test", RegexOptions.None);
        Assert.Equal(RegexSecurity.DefaultMatchTimeout, r.MatchTimeout);
    }
}
