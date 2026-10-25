using Prometheus;

namespace HabitTracker.Api.Monitoring;

public static class HabitMetrics
{
    public static readonly Counter HabitsCreatedTotal = Metrics.CreateCounter("habit_created_total", "Total number of habits created");
    public static readonly Counter HabitsCompletedTotal = Metrics.CreateCounter("habit_completed_total", "Total number of habits marked as completed");
    public static readonly Counter HabitsDeletedTotal = Metrics.CreateCounter("habit_deleted_total", "Total number of habits deleted");
    public static readonly Counter HabitCompletionFailedTotal = Metrics.CreateCounter("habit_completion_failed_total", "Total number of failed attempts to mark a habit as completed");
}