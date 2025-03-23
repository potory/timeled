using Timeled.Core;

namespace Timeled.Tests;

public class TimelineSegmentTests
{
    [Fact]
    public void Overlaps_ReturnsCorrectResult()
    {
        var segment1 = new TimelineSegment(
            new DateTime(2025, 3, 23),
            new DateTime(2025, 3, 24));
        var segment2 = new TimelineSegment(
            new DateTime(2025, 3, 23, 12, 0, 0),
            new DateTime(2025, 3, 24, 12, 0, 0));
        var segment3 = new TimelineSegment(
            new DateTime(2025, 3, 24),
            new DateTime(2025, 3, 25));

        Assert.True(segment1.Overlaps(segment2));
        Assert.False(segment1.Overlaps(segment3));
    }

    [Fact]
    public void Equals_ReturnsCorrectResult()
    {
        var segment1 = new TimelineSegment(
            new DateTime(2025, 3, 23),
            new DateTime(2025, 3, 24),
            SegmentStatus.Vacant);
        var segment2 = new TimelineSegment(
            new DateTime(2025, 3, 23),
            new DateTime(2025, 3, 24),
            SegmentStatus.Vacant);
        var segment3 = new TimelineSegment(
            new DateTime(2025, 3, 23),
            new DateTime(2025, 3, 24),
            SegmentStatus.Occupied);

        Assert.Equal(segment1, segment2);
        Assert.NotEqual(segment1, segment3);
    }
    
}