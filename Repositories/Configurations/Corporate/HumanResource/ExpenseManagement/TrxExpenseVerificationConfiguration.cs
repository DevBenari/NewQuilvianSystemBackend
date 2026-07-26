using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.ExpenseManagement
{
    public class TrxExpenseVerificationConfiguration : IEntityTypeConfiguration<TrxExpenseVerification>
    {
        public void Configure(EntityTypeBuilder<TrxExpenseVerification> entity)
        {
            entity.ToTable("TrxExpenseVerification", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.VerificationType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.VerificationStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.ClaimedAmountSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.EligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.NonEligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.VerifiedAmount).HasPrecision(18, 2);
            entity.Property(x => x.ChecklistResultJson).HasColumnType("jsonb");
            entity.Property(x => x.VerificationNotes).HasMaxLength(2000);
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.ExpenseClaim).WithMany(x => x.Verifications).HasForeignKey(x => x.ExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ExpenseClaimItem).WithMany(x => x.Verifications).HasForeignKey(x => x.ExpenseClaimItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ExpenseClaimId, x.VerificationType, x.IsFinalVerification, x.IsDelete });
            entity.HasIndex(x => new { x.ExpenseClaimItemId, x.VerificationStatus, x.IsDelete });
            entity.HasIndex(x => new { x.VerifiedByUserId, x.VerificationStatus, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxExpenseVerification> entity)
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
