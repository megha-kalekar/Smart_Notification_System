using Microsoft.EntityFrameworkCore;

namespace Smart_Notification_System.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
                entity.Property(n => n.Type).IsRequired().HasMaxLength(100);
                entity.Property(n => n.Priority).IsRequired().HasMaxLength(50);
                entity.HasIndex(n => n.IsProcessed);
                entity.HasIndex(n => n.CreatedAt);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
                entity.Property(u => u.RefreshTokenHash).HasMaxLength(256);
            });
        }
    }
}
