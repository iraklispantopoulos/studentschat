using Microsoft.EntityFrameworkCore;
using Repository.Entities;

namespace Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<PromptUnit> PromptConfigurationUnits { get; set; }
        public DbSet<ChatHistory> ChatHistory { get; set; }
        public DbSet<Settings> Configurations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
