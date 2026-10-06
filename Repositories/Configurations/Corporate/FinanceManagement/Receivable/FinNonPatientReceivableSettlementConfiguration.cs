using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinNonPatientReceivableSettlementConfiguration : IEntityTypeConfiguration<FinNonPatientReceivableSettlement>
{
    public void Configure(EntityTypeBuilder<FinNonPatientReceivableSettlement> entity)
    {
        entity.ToTable("FinNonPatientReceivableSettlement", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.PaymentMethod).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ReferenceNumber).HasMaxLength(100);
        entity.Property(x => x.Note).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.NonPatientReceivableId).HasDatabaseName("IX_FinNonPatientReceivableSettlement_NonPatientReceivableId");
    }
}
