using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxCashierDisbursementVoucherConfiguration : IEntityTypeConfiguration<TrxCashierDisbursementVoucher>
    {
        public void Configure(EntityTypeBuilder<TrxCashierDisbursementVoucher> builder)
        {
            builder.ToTable("TrxCashierDisbursementVoucher", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.VoucherNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.VoucherAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.SecurityTokenHash)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.VoucherStatus)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CashierReceiptNumber)
                .HasMaxLength(100);

            builder.HasOne(x => x.Claim)
                .WithMany()
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CashedByUser)
                .WithMany()
                .HasForeignKey(x => x.CashedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.VoucherNumber)
                .IsUnique()
                .HasDatabaseName("uq_cashier_disbursement_voucher_number");

            builder.HasIndex(x => new { x.VoucherStatus, x.ExpiredAt })
                .HasDatabaseName("idx_cashier_voucher_status_exp");

            builder.HasIndex(x => x.ClaimId)
                .HasDatabaseName("idx_cashier_voucher_claim");
        }
    }
}
