// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.CLI.Commands.Wasm;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands.Wasm;

/// <summary>
/// Cycle 5 — attacker: malformed length, bad base64, or huge decoded payloads in STARTRESULTXML lines.
/// </summary>
public class WasmTestMessagesProcessorSecurityTests
{
    [Fact]
    public void Constructor_RejectsUnsafeXmlResultPath()
    {
        var okOut = Path.Combine(Path.GetTempPath(), "wasm-out-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            Assert.Throws<ArgumentException>(() =>
                new WasmTestMessagesProcessor("/tmp/../evil.xml", okOut, NullLogger.Instance));
        }
        finally
        {
            if (File.Exists(okOut)) File.Delete(okOut);
        }
    }

    [Fact]
    public void Constructor_RejectsUnsafeStdoutPath()
    {
        var okXml = Path.Combine(Path.GetTempPath(), "wasm-xml-" + Guid.NewGuid().ToString("N") + ".xml");
        File.WriteAllText(okXml, "");
        try
        {
            Assert.Throws<ArgumentException>(() =>
                new WasmTestMessagesProcessor(okXml, "/tmp/../evil.txt", NullLogger.Instance));
        }
        finally
        {
            if (File.Exists(okXml)) File.Delete(okXml);
        }
    }

    [Fact]
    public void XmlResultLineRegex_UsesBoundedMatchTimeout()
    {
        Assert.Equal(RegexSecurity.DefaultMatchTimeout, WasmTestMessagesProcessor.XmlResultLineRegex.MatchTimeout);
    }

    [Fact]
    public void TryDecodeXmlResultLine_AcceptsValidPayload()
    {
        var payload = new byte[] { 1, 2, 3 };
        var b64 = Convert.ToBase64String(payload);
        var line = $"STARTRESULTXML {payload.Length} {b64} ENDRESULTXML";
        var m = WasmTestMessagesProcessor.XmlResultLineRegex.Match(line);
        Assert.True(m.Success);
        Assert.True(WasmXmlResultPayloadDecoder.TryDecodeXmlResultLine(m, maxDecodedBytes: 10_000, out var bytes, out var expectedLength));
        Assert.Equal(payload.Length, expectedLength);
        Assert.Equal(payload, bytes);
    }

    [Fact]
    public void TryDecodeXmlResultLine_RejectsWhenDecodedExceedsMax()
    {
        var payload = new byte[200];
        var b64 = Convert.ToBase64String(payload);
        var line = $"STARTRESULTXML {payload.Length} {b64} ENDRESULTXML";
        var m = WasmTestMessagesProcessor.XmlResultLineRegex.Match(line);
        Assert.True(m.Success);
        Assert.False(WasmXmlResultPayloadDecoder.TryDecodeXmlResultLine(m, maxDecodedBytes: 100, out var bytes, out _));
        Assert.Null(bytes);
    }

    [Fact]
    public void TryDecodeXmlResultLine_RejectsEmptyLengthToken()
    {
        // Regex allows zero digits; int.TryParse("") fails.
        var line = "STARTRESULTXML  YQ== ENDRESULTXML";
        var m = WasmTestMessagesProcessor.XmlResultLineRegex.Match(line);
        Assert.True(m.Success);
        Assert.False(WasmXmlResultPayloadDecoder.TryDecodeXmlResultLine(m, maxDecodedBytes: 10_000, out var bytes, out _));
        Assert.Null(bytes);
    }

    [Fact]
    public void TryDecodeXmlResultLine_RejectsInvalidBase64()
    {
        var line = "STARTRESULTXML 4 not-valid-base64!!! ENDRESULTXML";
        var m = WasmTestMessagesProcessor.XmlResultLineRegex.Match(line);
        Assert.True(m.Success);
        Assert.False(WasmXmlResultPayloadDecoder.TryDecodeXmlResultLine(m, maxDecodedBytes: 10_000, out var bytes, out _));
        Assert.Null(bytes);
    }

    [Fact]
    public async Task ProcessMessage_IgnoresDuplicateStartResultXml()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "wasm-xml-" + Guid.NewGuid().ToString("N") + ".xml");
        var outTxt = Path.Combine(Path.GetTempPath(), "wasm-out-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            File.WriteAllText(tmp, "");
            var p = new WasmTestMessagesProcessor(tmp, outTxt, NullLogger.Instance);
            using var cts = new System.Threading.CancellationTokenSource();
            var runner = p.RunAsync(cts.Token);

            var payload1 = new byte[] { 1, 2, 3 };
            var payload2 = new byte[] { 9, 9, 9 };
            p.Invoke($"STARTRESULTXML {payload1.Length} {Convert.ToBase64String(payload1)} ENDRESULTXML");
            p.Invoke($"STARTRESULTXML {payload2.Length} {Convert.ToBase64String(payload2)} ENDRESULTXML");

            // Stop the processor and ensure pending messages were handled.
            _ = await p.CompleteAndFlushAsync();
            cts.Cancel();
            try { await runner; } catch { }

            var written = File.ReadAllBytes(tmp);
            Assert.Equal(payload1, written);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
            if (File.Exists(outTxt)) File.Delete(outTxt);
        }
    }
}
