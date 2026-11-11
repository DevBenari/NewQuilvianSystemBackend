using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxOppeStaffEvaluationDetailConfiguration : IEntityTypeConfiguration<TrxOppeStaffEvaluationDetail>
    {
        public void Configure(EntityTypeBuilder<TrxOppeStaffEvaluationDetail> builder)
        {
            builder.ToTable("trx_oppe_staff_evaluation_details", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.TargetValue).HasColumnType("decimal(8,2)");
            builder.Property(x => x.ActualValue).HasColumnType("decimal(8,2)");
            builder.Property(x => x.ComputedScore).HasColumnType("decimal(5,2)");

            builder.HasOne(x => x.StaffEvaluation)
                .WithMany()
                .HasForeignKey(x => x.StaffEvaluationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MetricCatalog)
                .WithMany()
                .HasForeignKey(x => x.MetricCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.StaffEvaluationId);
            builder.HasIndex(x => x.MetricCatalogId);
        }
    }
}
