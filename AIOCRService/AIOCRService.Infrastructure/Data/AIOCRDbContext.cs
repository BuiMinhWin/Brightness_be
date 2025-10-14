using AIOCRService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIOCRService.Infrastructure.Data
{
    public class AIOCRDbContext : DbContext
    {
        public AIOCRDbContext(DbContextOptions<AIOCRDbContext> options) : base(options)
        {
        }

        public DbSet<OCRRequest> OCRRequests => Set<OCRRequest>();
        public DbSet<OCRResult> OCRResults => Set<OCRResult>();
        public DbSet<ProcessedData> ProcessedData => Set<ProcessedData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // OCRRequest configuration
            modelBuilder.Entity<OCRRequest>(entity =>
            {
                entity.ToTable("ocr_requests");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ImageUrl).HasMaxLength(500);
                entity.Property(x => x.FileName).IsRequired().HasMaxLength(255);
                entity.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
                entity.Property(x => x.ErrorMessage).HasMaxLength(1000);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.Status);
            });

            // OCRResult configuration
            modelBuilder.Entity<OCRResult>(entity =>
            {
                entity.ToTable("ocr_results");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ExtractedText).HasColumnType("text");
                entity.Property(x => x.StructuredData).HasColumnType("text");
                entity.Property(x => x.Confidence).HasMaxLength(50);
                entity.Property(x => x.Language).HasMaxLength(10);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasOne(x => x.OCRRequest)
                    .WithMany()
                    .HasForeignKey(x => x.OCRRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProcessedData configuration
            modelBuilder.Entity<ProcessedData>(entity =>
            {
                entity.ToTable("processed_data");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.DataType).IsRequired().HasMaxLength(50);
                entity.Property(x => x.ProcessedContent).HasColumnType("text");
                entity.Property(x => x.Summary).HasColumnType("text");
                entity.Property(x => x.Insights).HasColumnType("text");
                entity.Property(x => x.IsLatest).HasDefaultValue(true).HasColumnName("is_latest");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasOne(x => x.OCRResult)
                    .WithMany()
                    .HasForeignKey(x => x.OCRResultId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.DataType);
                entity.HasIndex(x => x.IsLatest);
            });
        }
    }
}

