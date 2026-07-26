using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.ExpenseManagement
{
    public class TrxExpenseReversalConfiguration : IEntityTypeConfiguration<TrxExpenseReversal>
    {
        public void Configure(EntityTypeBuilder<TrxExpenseReversal> entity)
        {
            entity.ToTable("TrxExpenseReversal", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReversalNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReversalStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.ReversalAmount).HasPrecision(18, 2);
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ReversalReason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.ExpenseClaim).WithMany(x => x.Reversals).HasForeignKey(x => x.ExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ExpensePayment).WithMany(x => x.Reversals).HasForeignKey(x => x.ExpensePaymentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ReversalNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.ExpensePaymentId, x.ReversalStatus, x.IsDelete });
            entity.HasIndex(x => new { x.ExpenseClaimId, x.ReversalStatus, x.IsDelete });
            entity.HasIndex(x => new { x.FinanceReversalId, x.GlReversalHeaderId });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxExpenseReversal> entity)
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
