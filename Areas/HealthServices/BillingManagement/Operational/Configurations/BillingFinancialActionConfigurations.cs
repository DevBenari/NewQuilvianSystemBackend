using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Configurations
{
    public class BilFinancialActionRequestConfiguration
        : IEntityTypeConfiguration<BilFinancialActionRequest>
    {
        public void Configure(EntityTypeBuilder<BilFinancialActionRequest> builder)
        {
            builder.ToTable("BilFinancialActionRequest", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Version).IsConcurrencyToken();

            builder.Property(x => x.RequestNumber).HasMaxLength(40).IsRequired();
            builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            builder.Property(x => x.ReasonCode).HasMaxLength(60).IsRequired();
            builder.Property(x => x.ReasonNote).HasMaxLength(1000);
            builder.Property(x => x.PolicyBlockReason).HasMaxLength(500);
            builder.Property(x => x.ContentHash).HasMaxLength(64).IsRequired();
            builder.Property(x => x.IdempotencyKey).HasMaxLength(120);
            builder.Property(x => x.ExecutionNote).HasMaxLength(1000);

            builder.Property(x => x.RequestedAmount).HasPrecision(18, 6);
            builder.Property(x => x.ExecutedAmount).HasPrecision(18, 6);

            builder.HasIndex(x => x.RequestNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Pengiriman ulang dengan kunci yang sama tidak boleh melahirkan permintaan kedua.
            // Dijaga index, bukan pemeriksaan di memori, karena dua permintaan bersamaan tidak
            // saling melihat sampai salah satunya menyentuh database.
            builder.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            // Dipakai gerbang penutupan folio untuk menjawab "masih ada permintaan menggantung?".
            builder.HasIndex(x => new { x.FolioId, x.Status });
            builder.HasIndex(x => new { x.EncounterId, x.Status });
            builder.HasIndex(x => new { x.ActionType, x.Status });
            builder.HasIndex(x => x.MakerUserId);
            builder.HasIndex(x => x.ChargeLineId);
            builder.HasIndex(x => x.SupersedesRequestId);

            builder.HasMany(x => x.Approvals)
                .WithOne(x => x.Request)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class BilFinancialApprovalConfiguration
        : IEntityTypeConfiguration<BilFinancialApproval>
    {
        public void Configure(EntityTypeBuilder<BilFinancialApproval> builder)
        {
            builder.ToTable("BilFinancialApproval", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DecisionNote).HasMaxLength(1000);
            builder.Property(x => x.RequestContentHash).HasMaxLength(64).IsRequired();

            builder.Property(x => x.ApprovedAmount).HasPrecision(18, 6);
            builder.Property(x => x.RequestedAmount).HasPrecision(18, 6);

            builder.HasIndex(x => new { x.RequestId, x.DecidedAt });
            builder.HasIndex(x => x.CheckerUserId);

            // Satu permintaan hanya boleh punya satu keputusan Approve yang sah. Tanpa index ini,
            // dua checker yang menekan tombol bersamaan dapat menghasilkan dua persetujuan atas
            // satu permintaan yang sama.
            builder.HasIndex(x => new { x.RequestId, x.Decision })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"Decision\" = 1");
        }
    }

    public class MstBillingApprovalPolicyConfiguration
        : IEntityTypeConfiguration<MstBillingApprovalPolicy>
    {
        public void Configure(EntityTypeBuilder<MstBillingApprovalPolicy> builder)
        {
            builder.ToTable("MstBillingApprovalPolicy", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PolicyCode).HasMaxLength(60).IsRequired();
            builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.Property(x => x.MinimumAmount).HasPrecision(18, 6);
            builder.Property(x => x.MaximumAmount).HasPrecision(18, 6);

            // Versi adalah cara kebijakan diganti. Kombinasi kode dan versi karena itu unik,
            // dan penggantian dilakukan dengan menerbitkan versi baru — bukan menimpa baris lama.
            builder.HasIndex(x => new { x.PolicyCode, x.PolicyVersion })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.ActionType, x.IsApproved, x.IsActive });
        }
    }

    public class BilFolioClosureHistoryConfiguration
        : IEntityTypeConfiguration<BilFolioClosureHistory>
    {
        public void Configure(EntityTypeBuilder<BilFolioClosureHistory> builder)
        {
            builder.ToTable("BilFolioClosureHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Note).HasMaxLength(1000);
            builder.Property(x => x.ClosureEvidence).HasMaxLength(4000);

            builder.HasIndex(x => new { x.FolioId, x.PerformedAt });
            builder.HasIndex(x => x.FinancialActionRequestId);

            builder.HasOne(x => x.Folio)
                .WithMany()
                .HasForeignKey(x => x.FolioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
