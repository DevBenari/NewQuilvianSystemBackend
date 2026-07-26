using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.ExpenseManagement
{
    public class TrxExpensePaymentConfiguration : IEntityTypeConfiguration<TrxExpensePayment>
    {
        public void Configure(EntityTypeBuilder<TrxExpensePayment> entity)
        {
            entity.ToTable("TrxExpensePayment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PaymentNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.PaymentAmount).HasPrecision(18, 2);
            entity.Property(x => x.ReversedAmount).HasPrecision(18, 2);
            entity.Property(x => x.NetPaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaymentReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.PaymentMethodSnapshot).HasMaxLength(50);
            entity.Property(x => x.PayeeNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.BankNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.BankAccountNumberSnapshot).HasMaxLength(100);
            entity.Property(x => x.ScheduledPaymentAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PaidAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedToPayrollAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedToFinanceAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedToGlAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.ExpenseClaim).WithMany(x => x.Payments).HasForeignKey(x => x.ExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentSettlementMethod).WithMany().HasForeignKey(x => x.PaymentSettlementMethodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BankAccount).WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaidByUser).WithMany().HasForeignKey(x => x.PaidByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PaymentNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.ExpenseClaimId, x.PaymentStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPeriodId, x.IsPostedToPayroll, x.IsDelete });
            entity.HasIndex(x => new { x.FinancePaymentId, x.GlHeaderId });
            entity.HasIndex(x => new { x.PaymentStatus, x.ScheduledPaymentAt, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxExpensePayment> entity)
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
