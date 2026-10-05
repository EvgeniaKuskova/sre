using HabitTracker.Api.Models;

namespace HabitTracker.Api.DTOs;

public record CreateHabitDto(
    string Name,
    PeriodType Period, 
    int TargetCount);

public record HabitDto(
    Guid Id, 
    string Name,
    PeriodType Period,
    int TargetCount, 
    DateTime CreatedAt, 
    int CompletedInPeriod,
    List<DateTime> CompletedDates
);