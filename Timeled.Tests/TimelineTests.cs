using Timeled.Core;

namespace Timeled.Tests;

public class TimelineTests
{
    private readonly DateTime _start = new(2025, 3, 23);
    private readonly DateTime _end = new(2025, 3, 26);

    [Fact]
    public void Constructor_InitializesWithSingleSegment()
    {
        var timeline = new Timeline(_start, _end);
        
        Assert.Single(timeline.Segments);
        Assert.Equal(_start, timeline.Start);
        Assert.Equal(_end, timeline.End);
        Assert.Equal(_start, timeline.Segments[0].Range.Start);
        Assert.Equal(_end, timeline.Segments[0].Range.End);
        Assert.Equal(SegmentStatus.Vacant, timeline.Segments[0].Status);
    }

    [Fact]
    public void Constructor_InvalidRange_ThrowsException() => 
        Assert.Throws<ArgumentException>(() => new Timeline(_end, _start));

    [Fact]
    public void GetAndCutSegment_CutsCorrectly()
    {
        var timeline = new Timeline(_start, _end);
        var duration = TimeSpan.FromDays(1);
        
        var segment = timeline.GetAndCutSegment(duration);
        
        Assert.Equal(2, timeline.Segments.Count);
        Assert.Equal(_start, segment.Range.Start);
        Assert.Equal(_start + duration, segment.Range.End);
        Assert.Equal(SegmentStatus.Vacant, segment.Status);
    }

    [Fact]
    public void OccupySegment_MarksAsOccupied()
    {
        var timeline = new Timeline(_start, _end);
        var duration = TimeSpan.FromDays(1);
        
        var segment = timeline.OccupySegment(duration);
        
        Assert.Equal(2, timeline.Segments.Count);
        Assert.Equal(SegmentStatus.Occupied, segment.Status);
    }

    [Fact]
    public void GetAndCutSegment_WithSearchRange_ReturnsWithinRange()
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start.AddDays(1), _start.AddDays(2));
        
        var segment = timeline.GetAndCutSegment(TimeSpan.FromHours(12), searchRange);
        
        Assert.True(segment.Range.Start >= searchRange.Start);
        Assert.True(segment.Range.End <= searchRange.End);
    }

    [Fact]
    public void GetAndCutSegment_NoAvailableSegment_ThrowsException()
    {
        var timeline = new Timeline(_start, _end);
        timeline.OccupySegment(TimeSpan.FromDays(3)); // Occupy entire timeline
        
        Assert.Throws<InvalidOperationException>(() => 
            timeline.GetAndCutSegment(TimeSpan.FromDays(1)));
    }
    
    [Fact]
    public void GetAndCutSegment_WithSearchRange_CutsWithinRange()
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start.AddDays(1), _start.AddDays(2));
        var duration = TimeSpan.FromHours(12);
        
        var segment = timeline.GetAndCutSegment(duration, searchRange);
        
        Assert.True(segment.Range.Start >= searchRange.Start);
        Assert.True(segment.Range.End <= searchRange.End);
        Assert.Equal(duration, segment.Range.Duration);
    }

    [Fact]
    public void GetAndCutSegment_WithSearchRange_RespectsExistingBoundaries()
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start.AddHours(12), _start.AddDays(2));
        var duration = TimeSpan.FromHours(6);
        
        // First cut outside the range
        timeline.OccupySegment(TimeSpan.FromHours(6));
        
        var segment = timeline.GetAndCutSegment(duration, searchRange);
        
        Assert.True(segment.Range.Start >= searchRange.Start);
        Assert.Equal(duration, segment.Range.Duration);
    }

    [Fact]
    public void GetAndCutSegment_InsufficientTimeInRange_ThrowsException()
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start, _start.AddHours(6));
        
        Assert.Throws<InvalidOperationException>(() => 
            timeline.GetAndCutSegment(TimeSpan.FromHours(12), searchRange));
    }
    
    [Fact]
    public void GetAndCutSegment_WithSearchRange_KeepsMiddleSegmentInRange()
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start.AddDays(1), _start.AddDays(2));
        var duration = TimeSpan.FromHours(12);
        
        var segment = timeline.GetAndCutSegment(duration, searchRange);
        
        // Check middle segment is within range
        Assert.True(segment.Range.Start >= searchRange.Start);
        Assert.True(segment.Range.End <= searchRange.End);
        Assert.Equal(duration, segment.Range.Duration);

        // Check we have three segments
        Assert.Equal(3, timeline.Segments.Count);
        
        // Check before segment extends before search range
        Assert.Equal(_start, timeline.Segments[0].Range.Start);
        Assert.Equal(segment.Range.Start, timeline.Segments[0].Range.End);
        
        // Check after segment extends after search range
        Assert.Equal(segment.Range.End, timeline.Segments[2].Range.Start);
        Assert.Equal(_end, timeline.Segments[2].Range.End);
    }

    [Fact]
    public void GetAndCutSegment_WithSearchRangeAtStart_MaintainsFullAfterSegment()
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start, _start.AddDays(1));
        var duration = TimeSpan.FromHours(12);
        
        var segment = timeline.GetAndCutSegment(duration, searchRange);
        
        Assert.Equal(2, timeline.Segments.Count);
        Assert.Equal(_start, segment.Range.Start);
        Assert.Equal(_start + duration, segment.Range.End);
        Assert.Equal(segment.Range.End, timeline.Segments[1].Range.Start);
        Assert.Equal(_end, timeline.Segments[1].Range.End);
    }

    [Fact]
    public void GetAndCutSegment_WithSearchRangeAtEnd_MaintainsFullBeforeSegment()
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_end.AddHours(-12), _end);
        var duration = TimeSpan.FromHours(12);
        
        var segment = timeline.GetAndCutSegment(duration, searchRange);
        
        Assert.Equal(2, timeline.Segments.Count);
        Assert.Equal(_start, timeline.Segments[0].Range.Start);
        Assert.Equal(segment.Range.Start, timeline.Segments[0].Range.End);
        Assert.Equal(segment.Range.End, _end);
    }
    
    [Theory]
    [InlineData(0, 12)]  // Start of timeline
    [InlineData(24, 12)] // Middle of timeline
    [InlineData(60, 12)] // Near end of timeline
    public void GetAndCutSegment_VariousPositions_CutsCorrectly(int startHours, int durationHours)
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start.AddHours(startHours), _end);
        var duration = TimeSpan.FromHours(durationHours);

        var segment = timeline.GetAndCutSegment(duration, searchRange);

        Assert.True(segment.Range.Start >= searchRange.Start);
        Assert.Equal(duration, segment.Range.Duration);
        Assert.Equal(SegmentStatus.Vacant, segment.Status);
        Assert.True(timeline.Segments.Count >= 2);
    }

    [Theory]
    [InlineData(6, 12)]  // Too large for remaining space
    [InlineData(12, 6)]  // Exact fit
    [InlineData(18, 1)]  // Small segment
    public void GetAndCutSegment_AfterOccupied_VariousSizes(int occupiedHours, int requestedHours)
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start.AddHours(occupiedHours), _end);
        
        timeline.OccupySegment(TimeSpan.FromHours(occupiedHours));
        var segment = timeline.GetAndCutSegment(TimeSpan.FromHours(requestedHours), searchRange);

        Assert.True(segment.Range.Start >= searchRange.Start);
        Assert.Equal(TimeSpan.FromHours(requestedHours), segment.Range.Duration);
        Assert.Equal(3, timeline.Segments.Count); // occupied + before + requested
    }

    [Theory]
    [InlineData(24, 48, 12)] // Search range fully within segment
    [InlineData(0, 24, 12)]  // Search range at start
    [InlineData(48, 72, 12)] // Search range at end
    public void GetAndCutSegment_SearchRangeBoundaries(int rangeStartHours, int rangeEndHours, int durationHours)
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start.AddHours(rangeStartHours), _start.AddHours(rangeEndHours));
        var duration = TimeSpan.FromHours(durationHours);

        var segment = timeline.GetAndCutSegment(duration, searchRange);

        Assert.True(segment.Range.Start >= searchRange.Start);
        Assert.True(segment.Range.End <= searchRange.End);
        Assert.Equal(duration, segment.Range.Duration);
    }

    [Theory]
    [InlineData(12, 12)] // Split in middle
    [InlineData(0, 24)]  // Take from start
    [InlineData(48, 24)] // Take from end
    public void OccupySegment_VariousPositions(int startHours, int durationHours)
    {
        var timeline = new Timeline(_start, _end);
        var searchRange = new TimeRange(_start.AddHours(startHours), _end);
        var duration = TimeSpan.FromHours(durationHours);

        var segment = timeline.OccupySegment(duration, searchRange);

        Assert.True(segment.Range.Start >= searchRange.Start);
        Assert.Equal(duration, segment.Range.Duration);
        Assert.Equal(SegmentStatus.Occupied, segment.Status);
        Assert.Contains(timeline.Segments, s => s.Equals(segment));
    }

    [Theory]
    [InlineData(24, 12, 12)] // Occupied before requested range
    [InlineData(36, 12, 12)] // Occupied within requested range (should throw)
    [InlineData(48, 12, 12)] // Occupied after requested range
    public void GetAndCutSegment_WithExistingOccupied(int occupiedStartHours, int occupiedDurationHours, int requestedHours)
    {
        var timeline = new Timeline(_start, _end);
        var occupiedStart = _start.AddHours(occupiedStartHours);
        var searchRange = new TimeRange(_start.AddHours(36), _start.AddHours(48));

        timeline.OccupySegment(TimeSpan.FromHours(occupiedDurationHours), 
            new TimeRange(occupiedStart, occupiedStart.AddDays(2)));

        if (occupiedStartHours == 36) // Overlaps with search range
        {
            Assert.Throws<InvalidOperationException>(() => 
                timeline.GetAndCutSegment(TimeSpan.FromHours(requestedHours), searchRange));
        }
        else
        {
            var segment = timeline.GetAndCutSegment(TimeSpan.FromHours(requestedHours), searchRange);
            Assert.True(segment.Range.Start >= searchRange.Start);
            Assert.True(segment.Range.End <= searchRange.End);
            Assert.Equal(TimeSpan.FromHours(requestedHours), segment.Range.Duration);
        }
    }
}