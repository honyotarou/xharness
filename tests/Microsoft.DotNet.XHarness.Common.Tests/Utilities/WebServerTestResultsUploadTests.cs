// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class WebServerTestResultsUploadTests
{
    [Fact]
    public async Task TryCopyStreamToStreamWithLimitAsync_WithinLimit_WritesAll()
    {
        var src = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
        await using var dest = new MemoryStream();
        var (ok, n) = await WebServerTestResultsUpload.TryCopyStreamToStreamWithLimitAsync(src, dest, maxBytes: 100);
        Assert.True(ok);
        Assert.Equal(5, n);
        Assert.Equal("hello", Encoding.UTF8.GetString(dest.ToArray()));
    }

    [Fact]
    public async Task TryCopyStreamToStreamWithLimitAsync_ExceedsLimit_ReturnsFalse()
    {
        var src = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
        await using var dest = new MemoryStream();
        var (ok, n) = await WebServerTestResultsUpload.TryCopyStreamToStreamWithLimitAsync(src, dest, maxBytes: 3);
        Assert.False(ok);
        Assert.Equal(0, n);
        Assert.Equal(0, dest.Length);
    }

    [Fact]
    public async Task TryCopyStreamToStreamWithLimitAsync_ExactlyAtLimit_Ok()
    {
        var src = new MemoryStream(Encoding.UTF8.GetBytes("abcd"));
        await using var dest = new MemoryStream();
        var (ok, n) = await WebServerTestResultsUpload.TryCopyStreamToStreamWithLimitAsync(src, dest, maxBytes: 4);
        Assert.True(ok);
        Assert.Equal(4, n);
    }
}
