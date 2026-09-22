using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.CashManagement;

public sealed class FinDailyCashSnapshotConfiguration : IEntityTypeConfiguration<FinDailyCashSnapshot>
{
    public void Configure(EntityTypeBuilder<FinDailyCashSnapshot> entity)
    {
        entity.ToTable("FinDailyCashSnapshot", "public", table =>
        {
            table.HasCheckConstraint("CK_FinDailyCashSnapshot_Status",
                "\"Status\" IN ('OPEN','CLOSED')");
            // Formula kas harian FIN-DEC-020; kas kecil TIDAK ikut
            table.HasCheckConstraint("CK_FinDailyCashSnapshot_Formula",
                "\"ClosingBalance\" = \"OpeningBalance\" + \"CashReceiptAmount\" + \"OtherReceiptAmount\" - \"DisbursementAmount\" - \"BankDepositAmount\"");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.CashDate).HasColumnType("date");
        entity.Property(x => x.OpeningBalance).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.CashReceiptAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.OtherReceiptAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.DisbursementAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.BankDepositAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.ClosingBalance).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinDailyCashSnapshotStatuses.Open);
        entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.CashDate)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinDailyCashSnapshot_CashDate");
    }
}
