using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.AccountingIntegration;

public sealed class FinAccountingEventOutboxConfiguration : IEntityTypeConfiguration<FinAccountingEventOutbox>
{
    public void Configure(EntityTypeBuilder<FinAccountingEventOutbox> entity)
    {
        entity.ToTable("FinAccountingEventOutbox", "public", table =>
        {
            // Kontrak Accounting hanya menerima rupiah (aturan bisnis #7).
            table.HasCheckConstraint("CK_FinAccountingEventOutbox_Currency", "\"CurrencyCode\" = 'IDR'");
            table.HasCheckConstraint("CK_FinAccountingEventOutbox_DeliveryStatus",
                "\"DeliveryStatus\" IN ('PENDING','HELD_FOR_FINALIZATION','SENT','ACKNOWLEDGED','HELD','FAILED')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.EventNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.EventTypeCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SourceModule).HasMaxLength(30).IsRequired().HasDefaultValue(FinAccountingEventSourceModules.Finance);
        entity.Property(x => x.SourceTransactionId).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SourceVersion).HasMaxLength(20).IsRequired().HasDefaultValue("1");
        entity.Property(x => x.EventOccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.AccountingDate).HasColumnType("date");
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired().HasDefaultValue("IDR");
        entity.Property(x => x.PayloadJson).HasColumnType("text").IsRequired();
        entity.Property(x => x.ComponentsJson).HasColumnType("text");
        entity.Property(x => x.DeliveryStatus).HasMaxLength(30).IsRequired().HasDefaultValue(FinAccountingEventDeliveryStatuses.Pending);
        entity.Property(x => x.HoldReason).HasMaxLength(300);
        entity.Property(x => x.AttemptCount).HasDefaultValue(0);
        entity.Property(x => x.LastAttemptAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.AccountingReceiptNumber).HasMaxLength(50);
        entity.Property(x => x.AccountingJournalNumber).HasMaxLength(50);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Lapis anti-dobel pertama (FIN-DES-019).
        entity.HasIndex(x => x.EventNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinAccountingEventOutbox_EventNumber");

        // Lapis anti-dobel kedua — kontrak ACC-XMOD-0.2 bagian 5.
        entity.HasIndex(x => new { x.SourceModule, x.SourceTransactionId, x.EventTypeCode, x.SourceVersion })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinAccountingEventOutbox_SourceIdentity");

        entity.HasIndex(x => new { x.DeliveryStatus, x.AccountingDate })
            .HasDatabaseName("IX_FinAccountingEventOutbox_DeliveryStatus");
    }
}
