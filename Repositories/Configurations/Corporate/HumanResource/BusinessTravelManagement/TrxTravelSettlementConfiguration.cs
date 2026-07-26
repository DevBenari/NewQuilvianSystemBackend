using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelSettlementConfiguration : IEntityTypeConfiguration<TrxTravelSettlement>
    {
        public void Configure(EntityTypeBuilder<TrxTravelSettlement> entity)
        {
            entity.ToTable("TrxTravelSettlement", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SettlementNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SettlementDate).HasColumnType("date");
            entity.Property(x => x.AdvancePaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedExpenseAmount).HasPrecision(18, 2);
            entity.Property(x => x.SettlementDifferenceAmount).HasPrecision(18, 2);
            entity.Property(x => x.EmployeeRefundAmount).HasPrecision(18, 2);
            entity.Property(x => x.CompanyPayableAmount).HasPrecision(18, 2);
            entity.Property(x => x.SettledAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.SettlementStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PaidAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RefundedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.SettlementNotes).HasMaxLength(2000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.BusinessTravelRequest).WithMany(x => x.Settlements).HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelExpenseClaim).WithMany(x => x.Settlements).HasForeignKey(x => x.TravelExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelAdvanceRequest).WithMany(x => x.Settlements).HasForeignKey(x => x.TravelAdvanceRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelAdvancePayment).WithMany(x => x.Settlements).HasForeignKey(x => x.TravelAdvancePaymentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentSettlementMethod).WithMany().HasForeignKey(x => x.PaymentSettlementMethodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BankAccount).WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.SettlementNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.SettlementStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPeriodId, x.FinancePaymentId, x.GlHeaderId });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelSettlement> entity)
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
