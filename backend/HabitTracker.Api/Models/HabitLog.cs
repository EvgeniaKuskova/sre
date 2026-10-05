namespace HabitTracker.Api.Models;

public class HabitLog
{
    public Guid Id { get; set; }
    public Guid HabitId { get; set; }
    public Habit Habit { get; set; } = null!;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}