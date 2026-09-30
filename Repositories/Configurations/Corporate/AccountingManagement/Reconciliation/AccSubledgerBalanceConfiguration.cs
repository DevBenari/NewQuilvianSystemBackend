using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.Reconciliation
{
    public class AccSubledgerBalanceConfiguration : IEntityTypeConfiguration<AccSubledgerBalance>
    {
        public void Configure(EntityTypeBuilder<AccSubledgerBalance> entity)
        {
            entity.ToTable("AccSubledgerBalance", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.AccountingPeriodId)
                .IsRequired();

            entity.Property(x => x.ChartOfAccountId)
                .IsRequired();

            entity.Property(x => x.Balance)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.AsOfDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.SourceVersionNumber)
                .IsRequired();

            entity.Property(x => x.AccountingEventId)
                .IsRequired();

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

            entity.HasOne(x => x.AccountingPeriod)
                .WithMany()
                .HasForeignKey(x => x.AccountingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ChartOfAccount)
                .WithMany()
                .HasForeignKey(x => x.ChartOfAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AccountingEvent)
                .WithMany()
                .HasForeignKey(x => x.AccountingEventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.LegalEntityId, x.AccountingPeriodId, x.ChartOfAccountId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.AccountingEventId);
        }
    }
}
