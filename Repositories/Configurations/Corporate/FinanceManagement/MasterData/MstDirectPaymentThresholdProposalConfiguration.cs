using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.MasterData;

public sealed class MstDirectPaymentThresholdProposalConfiguration : IEntityTypeConfiguration<MstDirectPaymentThresholdProposal>
{
    public void Configure(EntityTypeBuilder<MstDirectPaymentThresholdProposal> entity)
    {
        entity.ToTable("MstDirectPaymentThresholdProposal", "public", table =>
        {
            table.HasCheckConstraint("CK_MstDirectPaymentThresholdProposal_Amount", "\"ProposedAmount\" > 0");
            table.HasCheckConstraint(
                "CK_MstDirectPaymentThresholdProposal_ApprovalStatus",
                "\"ApprovalStatus\" IN ('MENUNGGU', 'DISETUJUI', 'DITOLAK')");
            table.HasCheckConstraint(
                "CK_MstDirectPaymentThresholdProposal_ActivationStatus",
                "\"ActivationStatus\" IN ('BELUM_AKTIF', 'AKTIF', 'DIGANTIKAN')");
            // Aktif atau digantikan hanya mungkin bagi pengajuan yang disetujui: Ditolak/Menunggu tidak dapat diaktifkan.
            table.HasCheckConstraint(
                "CK_MstDirectPaymentThresholdProposal_ActivationRequiresApproval",
                "\"ActivationStatus\" = 'BELUM_AKTIF' OR \"ApprovalStatus\" = 'DISETUJUI'");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.ProposalNumber).HasMaxLength(40).IsRequired();
        entity.Property(x => x.ProposedAmount).HasPrecision(18, 2);
        entity.Property(x => x.PreviousActiveAmount).HasPrecision(18, 2);
        entity.Property(x => x.ActivatedPreviousAmount).HasPrecision(18, 2);
        entity.Property(x => x.ChangeReason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.ApprovalStatus).HasMaxLength(20).IsRequired();
        entity.Property(x => x.ActivationStatus).HasMaxLength(20).IsRequired();
        entity.Property(x => x.RejectionReason).HasMaxLength(500);

        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ActivatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.EffectiveFrom).HasColumnType("timestamp with time zone");
        entity.Property(x => x.SupersededAt).HasColumnType("timestamp with time zone");

        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Identitas pengajuan unik selamanya (tanpa filter IsDelete): nomor lama tidak dipakai ulang.
        entity.HasIndex(x => x.ProposalNumber)
            .IsUnique()
            .HasDatabaseName("IX_MstDirectPaymentThresholdProposal_ProposalNumber");

        // Invarian database: paling banyak SATU pengajuan berstatus AKTIF — pelengkap IX_MstDirectPaymentThreshold_Active.
        entity.HasIndex(x => x.ActivationStatus)
            .IsUnique()
            .HasFilter("\"ActivationStatus\" = 'AKTIF' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_MstDirectPaymentThresholdProposal_Active");

        entity.HasIndex(x => x.RequestedAt)
            .HasDatabaseName("IX_MstDirectPaymentThresholdProposal_RequestedAt");

        entity.HasIndex(x => new { x.ApprovalStatus, x.ActivationStatus })
            .HasDatabaseName("IX_MstDirectPaymentThresholdProposal_Status");
    }
}
