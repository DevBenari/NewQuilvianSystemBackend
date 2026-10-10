using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableInstallmentConfiguration : IEntityTypeConfiguration<FinReceivableInstallment>
{
    public void Configure(EntityTypeBuilder<FinReceivableInstallment> entity)
    {
        entity.ToTable("FinReceivableInstallment", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_FinReceivableInstallment_Status",
                "\"Status\" IN ('DIJADWALKAN','TERBAYAR_SEBAGIAN','TERBAYAR','TERTUNGGAK','DIBATALKAN')");
            table.HasCheckConstraint(
                "CK_FinReceivableInstallment_Amount",
                "\"ScheduledAmount\" > 0 AND \"CarriedOverAmount\" >= 0");
            // FIN-DEC-168/179: sisa MUST sama dengan dijadwalkan ditambah terbawa dikurangi terbayar.
            table.HasCheckConstraint(
                "CK_FinReceivableInstallment_Balance",
                "\"OutstandingAmount\" = \"ScheduledAmount\" + \"CarriedOverAmount\" - \"PaidAmount\" AND \"OutstandingAmount\" >= 0");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.DeductionPeriod).HasMaxLength(7).IsRequired();
        entity.Property(x => x.ScheduledAmount).HasPrecision(18, 2);
        entity.Property(x => x.CarriedOverAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableInstallmentStatuses.Dijadwalkan);
        entity.Property(x => x.LastResultAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Plan)
            .WithMany(x => x.Installments)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.PlanId, x.InstallmentNumber }).IsUnique().HasDatabaseName("IX_FinReceivableInstallment_PlanId_InstallmentNumber");
        entity.HasIndex(x => new { x.DeductionPeriod, x.Status }).HasDatabaseName("IX_FinReceivableInstallment_DeductionPeriod_Status");
    }
}
