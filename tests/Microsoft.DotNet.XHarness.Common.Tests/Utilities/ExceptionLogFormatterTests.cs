// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class ExceptionLogFormatterTests
{
    [Fact]
    public void FormatSummary_CollapsesNewlines_AndIncludesType()
    {
        var ex = new InvalidOperationException("line1\r\nline2\nend");
        var s = ExceptionLogFormatter.FormatSummary(ex);
        Assert.DoesNotContain("\r", s);
        Assert.DoesNotContain("\n", s);
        Assert.Contains("InvalidOperationException", s);
        Assert.Contains("line1", s);
        Assert.Contains("line2", s);
        Assert.Contains("end", s);
    }

    [Fact]
    public void FormatSummary_DoesNotIncludeStackTrace()
    {
        try
        {
            throw new IOException("boom");
        }
        catch (Exception ex)
        {
            var s = ExceptionLogFormatter.FormatSummary(ex);
            Assert.DoesNotContain("   at ", s);
            Assert.DoesNotContain("ExceptionLogFormatterTests", s);
        }
    }
}
