using HabitTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>().HasMany(h => h.Logs).WithOne(l => l.Habit).HasForeignKey(l => l.HabitId);
    }
}