using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinPaymentDeductionConfiguration : IEntityTypeConfiguration<FinPaymentDeduction>
{
    public void Configure(EntityTypeBuilder<FinPaymentDeduction> entity)
    {
        entity.ToTable("FinPaymentDeduction", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPaymentDeduction_Type",
                "\"DeductionType\" IN ('PPH21','KASBON','PATIENT_DEBT','SITTING_FEE','KSO','IURAN','OTHER')");
            table.HasCheckConstraint("CK_FinPaymentDeduction_Direction",
                "\"Direction\" IN ('DEDUCTION','ADDITION')");
            table.HasCheckConstraint("CK_FinPaymentDeduction_Amount", "\"Amount\" > 0");
            // Pos lain-lain (OTHER) wajib menyebut alasannya (data-dictionary.md §A.6).
            table.HasCheckConstraint("CK_FinPaymentDeduction_OtherReason",
                "\"DeductionType\" <> 'OTHER' OR \"Reason\" IS NOT NULL");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.DeductionType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Direction).HasMaxLength(10).IsRequired().HasDefaultValue(FinPaymentDeductionDirections.Deduction);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(100);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Payment)
            .WithMany(x => x.Deductions)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.PaymentId, x.DeductionType }).HasDatabaseName("IX_FinPaymentDeduction_Payment_Type");
    }
}
