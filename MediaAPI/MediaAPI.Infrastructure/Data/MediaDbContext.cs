using MediaAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaAPI.Infrastructure.Data
{
    public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
    {
        public DbSet<MediaItem> MediaItems => Set<MediaItem>();
        public DbSet<UserLike> UserLikes => Set<UserLike>();
        public DbSet<UserView> UserViews => Set<UserView>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("media");

            modelBuilder.Entity<MediaItem>(entity =>
            {
                entity.ToTable("media_items");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.Platform).HasMaxLength(100);
                entity.Property(x => x.Source).HasMaxLength(100);
                entity.Property(x => x.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("active");

                entity.Property(x => x.Category).HasMaxLength(100);
                entity.Property(x => x.Duration).HasMaxLength(50);
                entity.Property(x => x.Thumbnail).HasColumnType("text");
                entity.Property(x => x.Views).HasDefaultValue(0);
                entity.Property(x => x.Likes).HasDefaultValue(0);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<UserLike>(entity =>
            {
                entity.ToTable("user_likes");
                entity.HasKey(x => new { x.UserId, x.MediaItemId });

                entity.HasOne(x => x.MediaItem)
                    .WithMany(x => x.UserLikes)
                    .HasForeignKey(x => x.MediaItemId);
            });

            modelBuilder.Entity<UserView>(entity =>
            {
                entity.ToTable("user_views");
                entity.HasKey(x => new { x.UserId, x.MediaItemId });

                entity.HasOne(x => x.MediaItem)
                    .WithMany(x => x.UserViews)
                    .HasForeignKey(x => x.MediaItemId);
            });
        }
    }
}
