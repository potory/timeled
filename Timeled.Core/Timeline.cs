namespace Timeled.Core;

/// <summary>
/// Представляет временную линию, состоящую из сегментов, которые могут быть свободными или занятыми.
/// Позволяет резервировать временные интервалы и управлять доступными отрезками времени.
/// </summary>
public class Timeline
{
    private readonly List<TimelineSegment> _segments;

    public DateTime Start { get; }
    public DateTime End { get; }

    /// <summary>
    /// Создает новую временную линию с указанным началом и концом.
    /// </summary>
    /// <param name="start">Начальная дата и время</param>
    /// <param name="end">Конечная дата и время</param>
    /// <exception cref="ArgumentException">Выбрасывается, если конечное время предшествует начальному</exception>
    public Timeline(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("Конечное время должно быть позже начального");

        Start = start;
        End = end;
        
        _segments = new List<TimelineSegment> { new(start, end) };
    }

    /// <summary>
    /// Получает список сегментов временной линии в режиме только для чтения.
    /// </summary>
    public IReadOnlyList<TimelineSegment> Segments => _segments;

    /// <summary>
    /// Находит и вырезает первый доступный сегмент заданной длительности.
    /// </summary>
    /// <param name="duration">Требуемая длительность сегмента</param>
    /// <param name="searchRange">Опциональный диапазон поиска</param>
    /// <returns>Вырезанный сегмент временной линии</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если подходящий сегмент не найден</exception>
    public TimelineSegment GetAndCutSegment(TimeSpan duration, TimeRange? searchRange = null)
    {
        var availableSegment = FindShortestAvailableSegment(duration, searchRange);
        if (availableSegment == null)
            throw new InvalidOperationException("Доступный сегмент не найден");

        return CutSegment(availableSegment, duration, searchRange);
    }

    /// <summary>
    /// Занимает сегмент заданной длительности, помечая его как занятый.
    /// </summary>
    /// <param name="duration">Требуемая длительность сегмента</param>
    /// <param name="searchRange">Опциональный диапазон поиска</param>
    /// <returns>Занятый сегмент временной линии</returns>
    public TimelineSegment OccupySegment(TimeSpan duration, TimeRange? searchRange = null)
    {
        var segment = GetAndCutSegment(duration, searchRange);
        segment.SetStatus(SegmentStatus.Occupied);
        return segment;
    }

    private TimelineSegment FindShortestAvailableSegment(TimeSpan duration, TimeRange? searchRange)
    {
        TimelineSegment shortest = null;
        TimeSpan? shortestDuration = null;

        foreach (var segment in _segments.Where(segment => segment.Status != SegmentStatus.Occupied))
        {
            TimeSpan availableDuration;
            DateTime availableStart;
            
            if (searchRange != null)
            {
                if (!searchRange.Overlaps(segment.Range)) continue;
                
                availableStart = segment.Range.Start > searchRange.Start ? 
                    segment.Range.Start : searchRange.Start;
                
                DateTime availableEnd = segment.Range.End < searchRange.End ? 
                    segment.Range.End : searchRange.End;
                
                availableDuration = availableEnd - availableStart;
            }
            else
            {
                availableStart = segment.Range.Start;
                availableDuration = segment.Range.Duration;
            }

            if (availableDuration < duration) continue;

            if (shortest == null || availableDuration < shortestDuration)
            {
                shortest = segment;
                shortestDuration = availableDuration;
            }
        }

        return shortest ?? throw new InvalidOperationException("Доступный сегмент не найден");
    }

    private TimelineSegment CutSegment(TimelineSegment segment, TimeSpan duration, TimeRange? searchRange)
    {
        int index = _segments.IndexOf(segment);
        _segments.RemoveAt(index);

        DateTime newStart = searchRange != null ? 
            (segment.Range.Start > searchRange.Start ? segment.Range.Start : searchRange.Start) : 
            segment.Range.Start;

        var newSegment = new TimelineSegment(newStart, newStart + duration);
        
        // Добавить сегмент до нового, если есть место (может расширяться перед диапазоном поиска)
        if (segment.Range.Start < newSegment.Range.Start)
        {
            _segments.Insert(index, new TimelineSegment(segment.Range.Start, newSegment.Range.Start));
            index++;
        }

        // Добавить новый сегмент (будет находиться в пределах диапазона поиска, если он указан)
        _segments.Insert(index, newSegment);
        index++;

        // Добавить сегмент после нового, если есть место (может расширяться после диапазона поиска)
        if (newSegment.Range.End < segment.Range.End)
        {
            _segments.Insert(index, new TimelineSegment(newSegment.Range.End, segment.Range.End));
        }

        return newSegment;
    }
}