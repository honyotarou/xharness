// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;
using Microsoft.DotNet.XHarness.Common.Xml;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Xml;

/// <summary>
/// Cycle 1 — attacker: untrusted test-result XML with DTD / entity expansion (XXE-style surface).
/// </summary>
public class SecureXmlReaderSettingsTests
{
    [Fact]
    public void Create_ProhibitsDtdAndLimitsEntityExpansion()
    {
        var s = SecureXmlReaderSettings.Create();
        Assert.Equal(DtdProcessing.Prohibit, s.DtdProcessing);
        Assert.True(s.IgnoreComments);
        Assert.True(s.IgnoreProcessingInstructions);
        Assert.Equal(SecureXmlReaderSettings.DefaultMaxCharactersFromEntities, s.MaxCharactersFromEntities);
    }

    [Fact]
    public void Create_WithIgnoreWhitespace_SetsIgnoreWhitespace()
    {
        var s = SecureXmlReaderSettings.Create(ignoreWhitespace: true);
        Assert.True(s.IgnoreWhitespace);
    }
}
