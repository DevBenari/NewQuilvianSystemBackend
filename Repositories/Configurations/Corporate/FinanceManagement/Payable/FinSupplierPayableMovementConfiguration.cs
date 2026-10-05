using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinSupplierPayableMovementConfiguration : IEntityTypeConfiguration<FinSupplierPayableMovement>
{
    public void Configure(EntityTypeBuilder<FinSupplierPayableMovement> entity)
    {
        entity.ToTable("FinSupplierPayableMovement", "public", table =>
        {
            table.HasCheckConstraint("CK_FinSupplierPayableMovement_Balance", "\"BalanceAfter\" = \"BalanceBefore\" + \"Amount\"");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.MovementType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
        entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        entity.Property(x => x.BusinessDate).HasColumnType("date").IsRequired();
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone").IsRequired();
        entity.Property(x => x.PaymentMethodCode).HasMaxLength(30);
        entity.Property(x => x.FundingSourceType).HasMaxLength(30);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(100);
        entity.Property(x => x.Notes).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.SupplierPayable)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.SupplierPayableId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FinSupplierPayableMovement_FinSupplierPayable_SupplierPayableId");

        entity.HasIndex(x => new { x.SupplierPayableId, x.BusinessDate }).HasDatabaseName("IX_FinSupplierPayableMovement_SupplierPayableId_BusinessDate");
        entity.HasIndex(x => x.BusinessDate).HasDatabaseName("IX_FinSupplierPayableMovement_BusinessDate");
        entity.HasIndex(x => x.PaymentId).HasDatabaseName("IX_FinSupplierPayableMovement_PaymentId");
        entity.HasIndex(x => x.PaymentAllocationId).HasDatabaseName("IX_FinSupplierPayableMovement_PaymentAllocationId");
        entity.HasIndex(x => x.PaymentMethodCode).HasDatabaseName("IX_FinSupplierPayableMovement_PaymentMethodCode");
        entity.HasIndex(x => x.OpeningItemBatchId).HasDatabaseName("IX_FinSupplierPayableMovement_OpeningItemBatchId");
        entity.HasIndex(x => x.CorrelationId).HasDatabaseName("IX_FinSupplierPayableMovement_CorrelationId");

        entity.HasIndex(x => x.ProofId)
            .IsUnique()
            .HasFilter("\"ProofId\" IS NOT NULL AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinSupplierPayableMovement_ProofId");
    }
}
