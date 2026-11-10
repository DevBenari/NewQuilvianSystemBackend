using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableInstallmentDeductionEvidenceConfiguration : IEntityTypeConfiguration<FinReceivableInstallmentDeductionEvidence>
{
    public void Configure(EntityTypeBuilder<FinReceivableInstallmentDeductionEvidence> entity)
    {
        entity.ToTable("FinReceivableInstallmentDeductionEvidence", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_FinReceivableInstallmentDeductionEvidence_Status",
                "\"Status\" IN ('BUKTI_DIUNGGAH','TERVERIFIKASI','DITOLAK')");
            table.HasCheckConstraint(
                "CK_FinReceivableInstallmentDeductionEvidence_ResultStatus",
                "\"ResultStatus\" IN ('BERHASIL','SEBAGIAN','GAGAL')");
            table.HasCheckConstraint(
                "CK_FinReceivableInstallmentDeductionEvidence_Amount",
                "\"DeductedAmount\" >= 0");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.DeductionPeriod).HasMaxLength(7).IsRequired();
        entity.Property(x => x.DocumentNumber).HasMaxLength(100).IsRequired();
        entity.Property(x => x.ResultStatus).HasMaxLength(10).IsRequired();
        entity.Property(x => x.DeductedAmount).HasPrecision(18, 2);
        entity.Property(x => x.DeductionDate).HasColumnType("timestamp with time zone");
        entity.Property(x => x.Notes).HasMaxLength(500);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinInstallmentDeductionEvidenceStatuses.Diunggah);
        entity.Property(x => x.UploadedAt).HasColumnType("timestamp with time zone").IsRequired();
        entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.VerificationNotes).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Installment)
            .WithMany()
            .HasForeignKey(x => x.InstallmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FinReceivableInstallmentDeductionEvidence_Installment");

        entity.HasOne(x => x.Proof)
            .WithMany()
            .HasForeignKey(x => x.ProofId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FinReceivableInstallmentDeductionEvidence_Proof");

        // Satu angsuran tidak menyimpan dua bukti aktif dengan nomor laporan yang sama (unggah ganda/klik ganda).
        // Bukti yang DITOLAK dilepas dari batasan ini sehingga dokumen yang benar dapat diunggah ulang dengan nomor yang sama.
        // Semua nama constraint/index sengaja pendek: batas identifier PostgreSQL 63 karakter (nama bawaan EF terlalu panjang).
        entity.HasIndex(x => new { x.InstallmentId, x.DocumentNumber })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false AND \"Status\" <> 'DITOLAK'")
            .HasDatabaseName("UX_FinReceivableInstallmentDeductionEvidence_Active");

        entity.HasIndex(x => x.PlanId).HasDatabaseName("IX_FinReceivableInstallmentDeductionEvidence_PlanId");
        entity.HasIndex(x => x.ReceivableId).HasDatabaseName("IX_FinReceivableInstallmentDeductionEvidence_ReceivableId");
        entity.HasIndex(x => x.ProofId).HasDatabaseName("IX_FinReceivableInstallmentDeductionEvidence_ProofId");
    }
}
