using Microsoft.EntityFrameworkCore;

namespace CafeBoard.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Veritabanında oluşacak tablolar
        public DbSet<Developer> Developers { get; set; }
        public DbSet<CafeTask> CafeTasks { get; set; }
        public DbSet<TaskLog> TaskLogs { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<Sprint> Sprints { get; set; }

        public DbSet<DeveloperFinance> DeveloperFinances { get; set; }
        public DbSet<SprintFinancialSummary> SprintFinancialSummaries { get; set; }
        public DbSet<DailyProgress> DailyProgresses { get; set; }
    }
}