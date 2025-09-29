using MediaAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaAPI.Infrastructure.Data
{
    public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
    {
        public DbSet<MediaItem> MediaItems => Set<MediaItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            

            modelBuilder.Entity<MediaItem>(entity =>
            {
                entity.ToTable("media_items");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.Platform).HasMaxLength(100);
                entity.Property(x => x.Source).HasMaxLength(100);
                entity.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("active");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });
        }
    }
}
