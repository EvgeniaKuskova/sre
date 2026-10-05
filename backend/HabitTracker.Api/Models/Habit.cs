namespace HabitTracker.Api.Models;

public class Habit
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TargetCount { get; set; } = 7;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<HabitLog> Logs { get; set; } = new List<HabitLog>();
    public PeriodType Period { get; set; } = PeriodType.Week;
}
