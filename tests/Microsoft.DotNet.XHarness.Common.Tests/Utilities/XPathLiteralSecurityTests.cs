// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class XPathLiteralSecurityTests
{
    [Theory]
    [InlineData("CFBundleIdentifier", "'CFBundleIdentifier'")]
    [InlineData("O'Reilly", "concat('O', \"'\", 'Reilly')")]
    [InlineData("a'b'c", "concat('a', \"'\", 'b', \"'\", 'c')")]
    public void QuoteForXPathStringLiteral_SafeForSelectSingleNode(string input, string expected) =>
        Assert.Equal(expected, XPathLiteralSecurity.QuoteForXPathStringLiteral(input));

    [Fact]
    public void QuoteForXPathStringLiteral_NullThrows() =>
        Assert.Throws<ArgumentNullException>(() => XPathLiteralSecurity.QuoteForXPathStringLiteral(null!));
}
