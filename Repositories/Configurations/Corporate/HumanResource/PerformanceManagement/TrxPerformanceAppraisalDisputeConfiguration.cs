using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxPerformanceAppraisalDisputeConfiguration : IEntityTypeConfiguration<TrxPerformanceAppraisalDispute>
    {
        public void Configure(EntityTypeBuilder<TrxPerformanceAppraisalDispute> entity)
        {
            entity.ToTable("trx_performance_appraisal_disputes", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DisputeNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DisputeDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DisputedComponent).HasMaxLength(30).IsRequired();
            entity.Property(x => x.GrievanceReason).IsRequired();
            entity.Property(x => x.EvidenceDocumentUrl).HasMaxLength(250).IsRequired();
            entity.Property(x => x.FinalVerdict).HasMaxLength(30);
            entity.Property(x => x.AdjustedCompositeScore).HasColumnType("decimal(5,2)");
            entity.Property(x => x.VerdictDeliveredAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Submitted");

            entity.HasOne(x => x.PerformanceAppraisal)
                .WithMany()
                .HasForeignKey(x => x.PerformanceAppraisalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DecidedByProfile)
                .WithMany()
                .HasForeignKey(x => x.DecidedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DisputeNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PerformanceAppraisalId);
            entity.HasIndex(x => x.WorkforceProfileId);
            entity.HasIndex(x => x.Status);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxPerformanceAppraisalDispute> entity)
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
