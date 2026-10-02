using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinPaymentAllocationConfiguration : IEntityTypeConfiguration<FinPaymentAllocation>
{
    public void Configure(EntityTypeBuilder<FinPaymentAllocation> entity)
    {
        entity.ToTable("FinPaymentAllocation", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPaymentAllocation_PayableType", "\"PayableType\" IN ('SUPPLIER','MEDICAL_SERVICE')");
            table.HasCheckConstraint("CK_FinPaymentAllocation_Amount", "\"Amount\" > 0");
            // Tepat satu FK terisi (FIN-DES-016) — pola num_nonnulls konsisten dengan FinPayableAdjustment.
            table.HasCheckConstraint("CK_FinPaymentAllocation_ExactlyOnePayable",
                "num_nonnulls(\"SupplierPayableId\", \"MedicalServicePayableId\") = 1");
            table.HasCheckConstraint("CK_FinPaymentAllocation_PayableTypeMatch",
                "(\"PayableType\" = 'SUPPLIER' AND \"SupplierPayableId\" IS NOT NULL) OR (\"PayableType\" = 'MEDICAL_SERVICE' AND \"MedicalServicePayableId\" IS NOT NULL)");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.PayableType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.IsReversal).HasDefaultValue(false);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Payment)
            .WithMany(x => x.Allocations)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.SupplierPayable)
            .WithMany()
            .HasForeignKey(x => x.SupplierPayableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ReversalOfAllocation)
            .WithMany()
            .HasForeignKey(x => x.ReversalOfAllocationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.MedicalServicePayable)
            .WithMany(x => x.PaymentAllocations)
            .HasForeignKey(x => x.MedicalServicePayableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PaymentId).HasDatabaseName("IX_FinPaymentAllocation_PaymentId");
        entity.HasIndex(x => x.SupplierPayableId).HasDatabaseName("IX_FinPaymentAllocation_SupplierPayableId");
        entity.HasIndex(x => x.MedicalServicePayableId).HasDatabaseName("IX_FinPaymentAllocation_MedicalServicePayableId");
        entity.HasIndex(x => x.ReversalOfAllocationId).HasDatabaseName("IX_FinPaymentAllocation_ReversalOfAllocationId");
    }
}
