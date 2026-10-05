using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinNonPatientReceivableConfiguration : IEntityTypeConfiguration<FinNonPatientReceivable>
{
    public void Configure(EntityTypeBuilder<FinNonPatientReceivable> entity)
    {
        entity.ToTable("FinNonPatientReceivable", "public", table =>
        {
            table.HasCheckConstraint("CK_FinNonPatientReceivable_Category", "\"Category\" IN ('PARKING','TENANT')");
            table.HasCheckConstraint("CK_FinNonPatientReceivable_Status", "\"Status\" IN ('OUTSTANDING','PARTIALLY_SETTLED','SETTLED','WRITTEN_OFF','CANCELLED')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ReceivableNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Category).HasMaxLength(20).IsRequired();
        entity.Property(x => x.CounterpartyName).HasMaxLength(200).IsRequired();
        entity.Property(x => x.RentedObject).HasMaxLength(200).IsRequired();
        entity.Property(x => x.BilledAmount).HasPrecision(18, 2);
        entity.Property(x => x.LateFeeAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinNonPatientReceivableStatuses.Outstanding);
        entity.Property(x => x.Note).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.ReceivableNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinNonPatientReceivable_ReceivableNumber");
        entity.HasIndex(x => x.Category).HasDatabaseName("IX_FinNonPatientReceivable_Category");
        entity.HasIndex(x => x.Status).HasDatabaseName("IX_FinNonPatientReceivable_Status");
        entity.HasIndex(x => x.DueDate).HasDatabaseName("IX_FinNonPatientReceivable_DueDate");

        entity.HasMany(x => x.Settlements)
            .WithOne(x => x.NonPatientReceivable)
            .HasForeignKey(x => x.NonPatientReceivableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
