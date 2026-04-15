// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Mitigates XPath injection when building predicates with user-controlled plist keys.
/// </summary>
public static class XPathLiteralSecurity
{
    /// <summary>
    /// Returns an XPath 1.0 string expression for use in predicates (e.g. <c>text()=…</c>).
    /// Keys without <c>'</c> use a single-quoted literal; keys with <c>'</c> use <c>concat()</c> because
    /// <see cref="System.Xml.XmlNode.SelectSingleNode(string)"/> rejects <c>text()='O''Reilly'</c>.
    /// </summary>
    public static string QuoteForXPathStringLiteral(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.IndexOf('\'', StringComparison.Ordinal) < 0)
        {
            return "'" + value + "'";
        }

        // e.g. O'Reilly → concat('O', "'", 'Reilly')
        var parts = value.Split('\'');
        var sb = new StringBuilder("concat(");
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", \"'\", ");
            }

            sb.Append('\'');
            sb.Append(parts[i]);
            sb.Append('\'');
        }

        sb.Append(')');
        return sb.ToString();
    }
}
