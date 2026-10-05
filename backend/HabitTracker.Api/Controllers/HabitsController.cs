using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using HabitTracker.Api.Data;
using HabitTracker.Api.Models;
using HabitTracker.Api.DTOs;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HabitsController(AppDbContext db, ILogger<HabitsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HabitDto>>> GetAll()
    {
        logger.LogInformation("Запрос списка всех привычек");
        var habits = await db.Habits.Include(h => h.Logs).ToListAsync();
        
        var result = habits.Select(h => 
        {
            var periodDays = (int)h.Period;
            var periodStart = DateTime.UtcNow.Date.AddDays(-periodDays + 1);
            var logsInPeriod = h.Logs.Where(l => l.CompletedAt >= periodStart).ToList();
            
            return new HabitDto(
                h.Id, h.Name, h.Period, h.TargetCount, h.CreatedAt,
                logsInPeriod.Count,
                logsInPeriod.Select(l => l.CompletedAt).ToList()
            );
        });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Habit>> Create([FromBody] CreateHabitDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Название привычки обязательно" });
        }
        
        if (dto.TargetCount < 1)
        {
            return BadRequest(new { message = "Цель должна быть не менее 1" });
        }
        var habit = new Habit { Name = dto.Name.Trim(), Period = dto.Period, TargetCount = dto.TargetCount };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();
        logger.LogInformation("Привычка успешно создана. Id: {Id}", habit.Id);
        return CreatedAtAction(nameof(GetAll), new { id = habit.Id }, habit);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var habit = await db.Habits.FindAsync(id);
        if (habit == null)
        {
            logger.LogWarning("Привычка с Id  не найдена: {Id}", id);
            return NotFound(new { message = "Привычка не найдена" });
        }
        
        db.Habits.Remove(habit);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var habit = await db.Habits.Include(h => h.Logs).FirstOrDefaultAsync(h => h.Id == id);
        if (habit == null)
        {
            logger.LogWarning("Попытка отметить несуществующую привычку с Id: {Id}", id);
            return NotFound(new { message = "Привычка не найдена" });
        }

        var periodStart = DateTime.UtcNow.Date.AddDays(-((int)habit.Period) + 1);
        var completedCount = habit.Logs.Count(l => l.CompletedAt >= periodStart);

        if (completedCount >= habit.TargetCount)
        {
            return BadRequest(new { message = "Цель на этот период уже достигнута" });
        }

        var today = DateTime.UtcNow.Date;
        if (habit.Logs.Any(l => l.CompletedAt.Date == today))
        {
            return BadRequest(new { message = "Вы уже отмечали эту привычку сегодня" });
        }

        habit.Logs.Add(new HabitLog { HabitId = id, CompletedAt = DateTime.UtcNow });
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Проверки выше — это read-check-write, они не атомарны.
            // Гонку закрывает уникальный индекс (HabitId, дата выполнения по UTC),
            // поэтому параллельная вставка «теряется» на уровне СУБД (фактор 8).
            logger.LogInformation("Конкурентная попытка отметить привычку {HabitName} дважды за день", habit.Name);
            return BadRequest(new { message = "Вы уже отмечали эту привычку сегодня" });
        }

        logger.LogInformation("Привычка {HabitName} успешно отмечена как выполненная", habit.Name);
        return Ok();
    }
}