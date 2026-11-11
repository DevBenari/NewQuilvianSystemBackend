using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxPerformanceAppraisalDetailConfiguration : IEntityTypeConfiguration<TrxPerformanceAppraisalDetail>
    {
        public void Configure(EntityTypeBuilder<TrxPerformanceAppraisalDetail> entity)
        {
            entity.ToTable("trx_performance_appraisal_details", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ComponentType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.MetricOrCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ItemTitle).HasMaxLength(200).IsRequired();
            entity.Property(x => x.WeightPercentage).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(x => x.SelfScore).HasColumnType("decimal(5,2)");
            entity.Property(x => x.ManagerScore).HasColumnType("decimal(5,2)");
            entity.Property(x => x.FinalScore).HasColumnType("decimal(5,2)");
            entity.Property(x => x.EvidenceDocumentUrl).HasMaxLength(250);

            entity.HasOne(x => x.PerformanceAppraisal)
                .WithMany(a => a.Details)
                .HasForeignKey(x => x.PerformanceAppraisalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.PerformanceAppraisalId);
            entity.HasIndex(x => x.ComponentType);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxPerformanceAppraisalDetail> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
