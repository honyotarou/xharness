// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;

namespace Microsoft.DotNet.XHarness.Common.Xml;

/// <summary>
/// Safe defaults for parsing untrusted XML (mitigates XXE / DTD expansion when combined with XmlResolver = null).
/// </summary>
public static class SecureXmlReaderSettings
{
    /// <summary>
    /// Caps expansion from internal entities if DTD processing is ever enabled (defense in depth).
    /// </summary>
    public const int DefaultMaxCharactersFromEntities = 100_000;

    public static XmlReaderSettings Create(bool ignoreWhitespace = false)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            MaxCharactersFromEntities = DefaultMaxCharactersFromEntities,
        };
        if (ignoreWhitespace)
        {
            settings.IgnoreWhitespace = true;
        }
        return settings;
    }
}
