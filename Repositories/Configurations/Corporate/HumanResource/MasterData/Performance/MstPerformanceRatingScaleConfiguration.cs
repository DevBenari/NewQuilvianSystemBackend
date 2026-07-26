using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Performance
{
    public class MstPerformanceRatingScaleConfiguration : IEntityTypeConfiguration<MstPerformanceRatingScale>
    {
        public void Configure(EntityTypeBuilder<MstPerformanceRatingScale> entity)
        {
            entity.ToTable("MstPerformanceRatingScale", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScaleCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ScaleName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ScaleType).HasMaxLength(50).HasDefaultValue("Numeric").IsRequired();
            entity.Property(x => x.MinimumScore).HasPrecision(10, 2).HasDefaultValue(1m);
            entity.Property(x => x.MaximumScore).HasPrecision(10, 2).HasDefaultValue(5m);
            entity.Property(x => x.PassingScore).HasPrecision(10, 2).IsRequired(false);
            entity.Property(x => x.DecimalPlaces).HasDefaultValue(2);
            entity.Property(x => x.IsHigherScoreBetter).HasDefaultValue(true);
            entity.Property(x => x.RatingDefinitionJson).HasColumnType("jsonb").IsRequired(false);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasIndex(x => x.ScaleCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ScaleName);
            entity.HasIndex(x => new { x.ScaleType, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.IsDefault, x.IsActive, x.IsDelete });
        }
    }
}
