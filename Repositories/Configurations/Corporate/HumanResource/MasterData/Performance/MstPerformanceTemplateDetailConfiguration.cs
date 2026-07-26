using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Performance
{
    public class MstPerformanceTemplateDetailConfiguration : IEntityTypeConfiguration<MstPerformanceTemplateDetail>
    {
        public void Configure(EntityTypeBuilder<MstPerformanceTemplateDetail> entity)
        {
            entity.ToTable("MstPerformanceTemplateDetail", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PerformanceTemplateId).IsRequired();
            entity.Property(x => x.DetailCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DetailName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.DetailType).HasMaxLength(50).HasDefaultValue("KPI").IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Weight).HasPrecision(7, 2).HasDefaultValue(0m);
            entity.Property(x => x.TargetValue).HasPrecision(18, 4).IsRequired(false);
            entity.Property(x => x.MinimumTargetValue).HasPrecision(18, 4).IsRequired(false);
            entity.Property(x => x.MaximumTargetValue).HasPrecision(18, 4).IsRequired(false);
            entity.Property(x => x.MeasurementUnit).HasMaxLength(100);
            entity.Property(x => x.ScoreMethod).HasMaxLength(50).HasDefaultValue("RatingScale").IsRequired();
            entity.Property(x => x.TargetDirection).HasMaxLength(50);
            entity.Property(x => x.EvidenceRequirement).HasMaxLength(500);
            entity.Property(x => x.IsRequired).HasDefaultValue(true);
            entity.Property(x => x.AllowEmployeeComment).HasDefaultValue(true);
            entity.Property(x => x.AllowReviewerComment).HasDefaultValue(true);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
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

            entity.HasOne(x => x.PerformanceTemplate)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.PerformanceTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ParentDetail)
                .WithMany(x => x.ChildDetails)
                .HasForeignKey(x => x.ParentDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.KpiCatalog)
                .WithMany(x => x.TemplateDetails)
                .HasForeignKey(x => x.KpiCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Competency)
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RatingScale)
                .WithMany(x => x.TemplateDetails)
                .HasForeignKey(x => x.RatingScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PerformanceTemplateId, x.DetailCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.PerformanceTemplateId, x.ParentDetailId, x.SortOrder });
            entity.HasIndex(x => new { x.KpiCatalogId, x.CompetencyId, x.RatingScaleId });
            entity.HasIndex(x => new { x.DetailType, x.IsRequired, x.IsActive, x.IsDelete });
        }
    }
}
