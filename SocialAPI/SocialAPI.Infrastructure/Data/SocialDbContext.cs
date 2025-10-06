using Microsoft.EntityFrameworkCore;
using SocialAPI.Domain.Entities;

namespace SocialAPI.Infrastructure.Data
{
    public class SocialDbContext(DbContextOptions<SocialDbContext> options) : DbContext(options)
    {
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<PostTag> PostTags => Set<PostTag>();
        public DbSet<PostVote> PostVotes => Set<PostVote>();
        public DbSet<CommentVote> CommentVotes => Set<CommentVote>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<PostImage> PostImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Visibility).IsRequired().HasMaxLength(50);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("comments");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tags");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Slug).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            // Cấu hình many-to-many relationship tại đây
            modelBuilder.Entity<PostTag>(entity =>
            {
                entity.ToTable("post_tags");
                entity.HasKey(x => new { x.PostId, x.TagId });

                entity.HasOne(pt => pt.Post)
                    .WithMany(p => p.PostTags)
                    .HasForeignKey(pt => pt.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pt => pt.Tag)
                    .WithMany(t => t.PostTags)
                    .HasForeignKey(pt => pt.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PostVote>(entity =>
            {
                entity.ToTable("post_votes");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.VoteType).IsRequired().HasMaxLength(20);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<CommentVote>(entity =>
            {
                entity.ToTable("comment_votes");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.VoteType).IsRequired().HasMaxLength(20);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("reports");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TargetType).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Reason).IsRequired().HasMaxLength(500);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
            });

            modelBuilder.Entity<PostImage>(entity =>
            {
                entity.ToTable("post_images");

                entity.HasOne(pi => pi.Post)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}