using Microsoft.EntityFrameworkCore;
using TestCarBot.Models;

namespace TestCarBot.DbService
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserData> Users => Set<UserData>();
        public DbSet<DocumentData> Documents => Set<DocumentData>();
        public DbSet<InsurancePolicy> Policies => Set<InsurancePolicy>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Documents)
                .WithOne(d => d.User!)
                .HasForeignKey(d => d.TelegramUserId);

            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Policies)
                .WithOne(p => p.User!)
                .HasForeignKey(p => p.TelegramUserId);
        }
    }
}
