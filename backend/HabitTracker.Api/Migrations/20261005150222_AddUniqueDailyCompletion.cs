using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDailyCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Гарантия «не более одного выполнения привычки в день» переносится в СУБД:
            // уникальный индекс по паре (привычка, календарная дата по UTC).
            // Без него два параллельных запроса проходят проверку read-check-write
            // в приложении и оба вставят запись (гонка, фактор 8).
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "IX_HabitLogs_HabitId_Day"
                ON "HabitLogs" ("HabitId", (("CompletedAt" AT TIME ZONE 'UTC')::date));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_HabitLogs_HabitId_Day";""");
        }
    }
}
