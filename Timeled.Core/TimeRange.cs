namespace Timeled.Core;

/// <summary>
/// Представляет диапазон времени с начальной и конечной датой.
/// </summary>
public class TimeRange
{
    /// <summary>
    /// Начальная дата и время диапазона.
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// Конечная дата и время диапазона.
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// Создает новый диапазон времени с указанным началом и концом.
    /// </summary>
    /// <param name="start">Начальная дата и время</param>
    /// <param name="end">Конечная дата и время</param>
    /// <exception cref="ArgumentException">Выбрасывается, если конечное время предшествует начальному</exception>
    public TimeRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("Конечное время должно быть позже начального");
        
        Start = start;
        End = end;
    }

    /// <summary>
    /// Вычисляет длительность диапазона времени.
    /// </summary>
    public TimeSpan Duration => End - Start;

    /// <summary>
    /// Проверяет, пересекается ли текущий диапазон с другим диапазоном.
    /// </summary>
    /// <param name="other">Другой диапазон для проверки</param>
    /// <returns>true, если диапазоны пересекаются; иначе false</returns>
    public bool Overlaps(TimeRange other) =>
        Start < other.End && End > other.Start;
}