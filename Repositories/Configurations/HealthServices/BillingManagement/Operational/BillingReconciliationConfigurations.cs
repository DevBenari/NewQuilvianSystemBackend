using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Configurations
{
    public class BilReconciliationCaseConfiguration : IEntityTypeConfiguration<BilReconciliationCase>
    {
        public void Configure(EntityTypeBuilder<BilReconciliationCase> builder)
        {
            builder.ToTable("BilReconciliationCase", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Version).IsConcurrencyToken();

            builder.Property(x => x.CaseNumber).HasMaxLength(40).IsRequired();
            builder.Property(x => x.SourceContext).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectType).HasMaxLength(80).IsRequired();
            builder.Property(x => x.ImpactDescription).HasMaxLength(500).IsRequired();
            builder.Property(x => x.NextAction).HasMaxLength(500);
            builder.Property(x => x.FailureReason).HasMaxLength(1000);
            builder.Property(x => x.ResolutionNote).HasMaxLength(1000);

            builder.Property(x => x.ImpactAmount).HasPrecision(18, 6);

            builder.HasIndex(x => x.CaseNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Satu case per kombinasi jenis dan identitas fakta. Inilah yang membuat pemindaian
            // ulang tidak menumpuk case duplikat untuk masalah yang sama: pemindaian berikutnya
            // menemukan case yang sudah ada, bukan membuat yang kedua.
            builder.HasIndex(x => new
                {
                    x.CaseType,
                    x.SourceContext,
                    x.MilestoneFactId,
                    x.MilestoneFactVersion,
                    x.EffectType
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Dipakai gerbang penutupan folio, sehingga sengaja diindeks.
            builder.HasIndex(x => new { x.FolioId, x.CaseStatus });
            builder.HasIndex(x => new { x.EncounterId, x.CaseStatus });
            builder.HasIndex(x => x.CaseStatus);
            builder.HasIndex(x => x.OwnerUserId);
        }
    }

    public class MstBillingReconciliationPolicyConfiguration
        : IEntityTypeConfiguration<MstBillingReconciliationPolicy>
    {
        public void Configure(EntityTypeBuilder<MstBillingReconciliationPolicy> builder)
        {
            builder.ToTable("MstBillingReconciliationPolicy", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MaterialityThresholdAmount).HasPrecision(18, 6);
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.CaseType)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
