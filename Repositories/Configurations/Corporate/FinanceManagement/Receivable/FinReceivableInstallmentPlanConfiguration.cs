using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableInstallmentPlanConfiguration : IEntityTypeConfiguration<FinReceivableInstallmentPlan>
{
    public void Configure(EntityTypeBuilder<FinReceivableInstallmentPlan> entity)
    {
        entity.ToTable("FinReceivableInstallmentPlan", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_FinReceivableInstallmentPlan_Status",
                "\"Status\" IN ('MENUNGGU','DISETUJUI','DITOLAK','SELESAI','DIBATALKAN')");
            // FIN-DES-099: pengaju MUST NOT menjadi penyetuju.
            table.HasCheckConstraint(
                "CK_FinReceivableInstallmentPlan_MakerChecker",
                "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
            table.HasCheckConstraint(
                "CK_FinReceivableInstallmentPlan_Amount",
                "\"InstallmentCount\" > 0 AND \"InstallmentAmount\" > 0");
            // TotalAgreedAmount MUST sama dengan InstallmentCount x InstallmentAmount. Kesamaannya
            // dengan sisa piutang saat disetujui tidak dapat ditegakkan di sini — lintas tabel,
            // ditegakkan service (FinanceReceivableInstallmentPlanService, BE-FIN-093).
            table.HasCheckConstraint(
                "CK_FinReceivableInstallmentPlan_Total",
                "\"TotalAgreedAmount\" = \"InstallmentCount\" * \"InstallmentAmount\"");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.PlanNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.InstallmentAmount).HasPrecision(18, 2);
        entity.Property(x => x.TotalAgreedAmount).HasPrecision(18, 2);
        entity.Property(x => x.FirstDeductionPeriod).HasMaxLength(7).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableInstallmentPlanStatuses.Menunggu);
        entity.Property(x => x.AgreementDocumentPath).HasMaxLength(512);
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.CancelReason).HasMaxLength(500);
        entity.Property(x => x.Notes).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Receivable)
            .WithMany()
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PlanNumber).IsUnique().HasDatabaseName("IX_FinReceivableInstallmentPlan_PlanNumber");
        // FIN-DES-099: satu piutang hanya boleh punya satu perjanjian aktif.
        entity.HasIndex(x => x.ReceivableId)
            .IsUnique()
            .HasFilter("\"Status\" IN ('MENUNGGU','DISETUJUI')")
            .HasDatabaseName("IX_FinReceivableInstallmentPlan_ReceivableId_Active");
        entity.HasIndex(x => new { x.Status, x.RequestedAt }).HasDatabaseName("IX_FinReceivableInstallmentPlan_Status_RequestedAt");
    }
}
