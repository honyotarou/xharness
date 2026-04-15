using System.Linq;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests.Utilities;

public class ShuffleExtensionsTests
{
    [Fact]
    public void Shuffle_PreservesAllItems()
    {
        var input = Enumerable.Range(0, 100).ToArray();
        var shuffled = Microsoft.DotNet.XHarness.iOS.Shared.Utilities.Extensions.Shuffle(input).ToArray();
        Assert.Equal(input.Length, shuffled.Length);
        Assert.Equal(input.OrderBy(x => x), shuffled.OrderBy(x => x));
    }
}

