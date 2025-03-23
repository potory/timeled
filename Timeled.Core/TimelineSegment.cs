namespace Timeled.Core;

/// <summary>
/// Представляет сегмент временной линии с определенным диапазоном времени и статусом (свободен или занят).
/// </summary>
public class TimelineSegment
{
    /// <summary>
    /// Диапазон времени сегмента.
    /// </summary>
    public TimeRange Range { get; private set; }

    /// <summary>
    /// Статус сегмента (свободен или занят).
    /// </summary>
    public SegmentStatus Status { get; private set; }

    /// <summary>
    /// Создает новый сегмент временной линии с указанным началом, концом и статусом.
    /// </summary>
    /// <param name="start">Начальная дата и время сегмента</param>
    /// <param name="end">Конечная дата и время сегмента</param>
    /// <param name="status">Статус сегмента (по умолчанию свободен)</param>
    public TimelineSegment(DateTime start, DateTime end, SegmentStatus status = SegmentStatus.Vacant)
    {
        Range = new TimeRange(start, end);
        Status = status;
    }

    /// <summary>
    /// Проверяет, пересекается ли текущий сегмент с другим сегментом.
    /// </summary>
    /// <param name="other">Другой сегмент для проверки</param>
    /// <returns>true, если сегменты пересекаются; иначе false</returns>
    public bool Overlaps(TimelineSegment other) => Range.Overlaps(other.Range);

    /// <summary>
    /// Сравнивает текущий сегмент с другим объектом на равенство.
    /// </summary>
    /// <param name="obj">Объект для сравнения</param>
    /// <returns>true, если объекты равны; иначе false</returns>
    public override bool Equals(object obj)
    {
        if (obj is TimelineSegment other)
        {
            return Range.Start == other.Range.Start && 
                   Range.End == other.Range.End && 
                   Status == other.Status;
        }
        return false;
    }
    
    /// <summary>
    /// Устанавливает новый статус для сегмента.
    /// </summary>
    /// <param name="status">Новый статус сегмента</param>
    public void SetStatus(SegmentStatus status) => Status = status;

    /// <summary>
    /// Вычисляет хэш-код для текущего сегмента.
    /// </summary>
    /// <returns>Хэш-код, основанный на начале, конце и статусе сегмента</returns>
    public override int GetHashCode() => HashCode.Combine(Range.Start, Range.End, Status);
}