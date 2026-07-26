using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstReimbursementPolicyConfiguration : IEntityTypeConfiguration<MstReimbursementPolicy>
    {
        public void Configure(EntityTypeBuilder<MstReimbursementPolicy> entity)
        {
            entity.ToTable("MstReimbursementPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExpenseCategoryId).IsRequired();
            entity.Property(x => x.ReimbursementPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReimbursementPolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.LimitPeriod).HasMaxLength(50).HasDefaultValue("PerTransaction").IsRequired();
            entity.Property(x => x.MinimumClaimAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.MaximumAmountPerTransaction).HasPrecision(18, 2);
            entity.Property(x => x.MaximumAmountPerDay).HasPrecision(18, 2);
            entity.Property(x => x.MaximumAmountPerMonth).HasPrecision(18, 2);
            entity.Property(x => x.MaximumAmountPerYear).HasPrecision(18, 2);
            entity.Property(x => x.RequiresReceipt).HasDefaultValue(true);
            entity.Property(x => x.ReceiptRequiredAmount).HasPrecision(18, 2);
            entity.Property(x => x.AllowWithoutReceipt).HasDefaultValue(false);
            entity.Property(x => x.MaximumSubmissionDays).HasDefaultValue(30);
            entity.Property(x => x.AllowBackdatedSubmission).HasDefaultValue(true);
            entity.Property(x => x.RequireCostCenter).HasDefaultValue(true);
            entity.Property(x => x.RequireManagerApproval).HasDefaultValue(true);
            entity.Property(x => x.RequireHrVerification).HasDefaultValue(false);
            entity.Property(x => x.RequireFinanceVerification).HasDefaultValue(true);
            entity.Property(x => x.ApprovalWorkflowCode).HasMaxLength(100);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.ExpenseCategory)
                .WithMany(x => x.ReimbursementPolicies)
                .HasForeignKey(x => x.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeCategory)
                .WithMany()
                .HasForeignKey(x => x.EmployeeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmploymentType)
                .WithMany()
                .HasForeignKey(x => x.EmploymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ReimbursementPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ReimbursementPolicyName);
            entity.HasIndex(x => x.ExpenseCategoryId);
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.ExpenseCategoryId, x.LimitPeriod, x.IsDefault, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
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
        }
    }
}
