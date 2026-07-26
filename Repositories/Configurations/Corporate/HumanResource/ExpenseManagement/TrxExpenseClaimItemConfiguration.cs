using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.ExpenseManagement
{
    public class TrxExpenseClaimItemConfiguration : IEntityTypeConfiguration<TrxExpenseClaimItem>
    {
        public void Configure(EntityTypeBuilder<TrxExpenseClaimItem> entity)
        {
            entity.ToTable("TrxExpenseClaimItem", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TransactionDate).HasColumnType("date");
            entity.Property(x => x.MerchantName).HasMaxLength(250);
            entity.Property(x => x.MerchantTaxNumber).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.UnitAmount).HasPrecision(18, 2);
            entity.Property(x => x.ClaimedAmount).HasPrecision(18, 2);
            entity.Property(x => x.EligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.NonEligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6).HasDefaultValue(1m);
            entity.Property(x => x.OriginalCurrencyAmount).HasPrecision(18, 2);
            entity.Property(x => x.BaseCurrencyAmount).HasPrecision(18, 2);
            entity.Property(x => x.MaximumAmountPerTransactionSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.MaximumAmountPerPeriodSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.BenefitLimitSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.PeriodUsedBeforeAmount).HasPrecision(18, 2);
            entity.Property(x => x.PeriodUsedAfterAmount).HasPrecision(18, 2);
            entity.Property(x => x.NonEligibleReason).HasMaxLength(1000);
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.ItemStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            ConfigureIdentity(entity);

            entity.HasOne(x => x.ExpenseClaim).WithMany(x => x.Items).HasForeignKey(x => x.ExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ExpenseCategory).WithMany().HasForeignKey(x => x.ExpenseCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReimbursementPolicy).WithMany().HasForeignKey(x => x.ReimbursementPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BenefitPlan).WithMany().HasForeignKey(x => x.BenefitPlanId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ExpenseClaimId, x.LineNumber }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.ExpenseCategoryId, x.TransactionDate, x.ItemStatus, x.IsDelete });
            entity.HasIndex(x => new { x.ReimbursementPolicyId, x.BenefitPlanId, x.IsDelete });
            entity.HasIndex(x => new { x.CostCenterId, x.TransactionDate, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxExpenseClaimItem> entity)
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
