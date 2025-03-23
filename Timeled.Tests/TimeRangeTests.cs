using Timeled.Core;

namespace Timeled.Tests;

public class TimeRangeTests
{
    [Fact]
    public void Constructor_InvalidRange_ThrowsException()
    {
        var start = new DateTime(2025, 3, 24);
        var end = new DateTime(2025, 3, 23);
        
        Assert.Throws<ArgumentException>(() => new TimeRange(start, end));
    }

    [Fact]
    public void Duration_ReturnsCorrectValue()
    {
        var start = new DateTime(2025, 3, 23);
        var end = new DateTime(2025, 3, 24);
        var range = new TimeRange(start, end);
        
        Assert.Equal(TimeSpan.FromDays(1), range.Duration);
    }

    [Fact]
    public void Overlaps_ReturnsCorrectResult()
    {
        var range1 = new TimeRange(
            new DateTime(2025, 3, 23),
            new DateTime(2025, 3, 24));
        var range2 = new TimeRange(
            new DateTime(2025, 3, 23, 12, 0, 0),
            new DateTime(2025, 3, 24, 12, 0, 0));
        var range3 = new TimeRange(
            new DateTime(2025, 3, 24),
            new DateTime(2025, 3, 25));

        Assert.True(range1.Overlaps(range2));
        Assert.False(range1.Overlaps(range3));
    }
}