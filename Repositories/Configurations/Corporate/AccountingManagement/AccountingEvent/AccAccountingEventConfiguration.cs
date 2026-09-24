using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.AccountingEvent
{
    public class AccAccountingEventConfiguration : IEntityTypeConfiguration<AccAccountingEvent>
    {
        public void Configure(EntityTypeBuilder<AccAccountingEvent> entity)
        {
            entity.ToTable("AccAccountingEvent", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.EventNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EventTypeCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.SourceModule)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.SourceTransactionId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.SourceVersion)
                .HasMaxLength(20)
                .HasDefaultValue("1")
                .IsRequired();

            entity.Property(x => x.EventOccurredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.AccountingDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.DocumentDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.CurrencyCode)
                .HasMaxLength(3)
                .HasDefaultValue(AccAccountingEvent.MataUangRupiah)
                .IsRequired();

            entity.Property(x => x.EventStatus)
                .HasConversion<int>()
                .HasDefaultValue(AccountingEventStatus.Diterima)
                .IsRequired();

            entity.Property(x => x.HoldReasonCode)
                .HasMaxLength(50);

            entity.Property(x => x.RawPayload)
                .HasColumnType("text")
                .IsRequired();

            entity.Property(x => x.AttemptCount)
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.CorrelationId)
                .IsRequired();

            entity.Property(x => x.CausationId)
                .IsRequired();

            entity.Property(x => x.IgnoreReason)
                .HasMaxLength(500);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EventType)
                .WithMany()
                .HasForeignKey(x => x.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Journal)
                .WithMany()
                .HasForeignKey(x => x.JournalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EventNumber)
                .IsUnique();

            entity.HasIndex(x => new { x.SourceModule, x.SourceTransactionId, x.EventTypeCode, x.SourceVersion })
                .IsUnique();

            entity.HasIndex(x => new { x.LegalEntityId, x.EventStatus });

            entity.HasIndex(x => x.EventTypeId);

            entity.HasIndex(x => x.AccountingDate);

            entity.HasIndex(x => x.JournalId);

            entity.HasIndex(x => x.CorrelationId);
        }
    }
}
