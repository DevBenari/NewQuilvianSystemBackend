using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.ExpenseManagement
{
    public class TrxExpenseReceiptConfiguration : IEntityTypeConfiguration<TrxExpenseReceipt>
    {
        public void Configure(EntityTypeBuilder<TrxExpenseReceipt> entity)
        {
            entity.ToTable("TrxExpenseReceipt", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReceiptNumber).HasMaxLength(100);
            entity.Property(x => x.ReceiptDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.MerchantName).HasMaxLength(250);
            entity.Property(x => x.ReceiptAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3);
            entity.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(150);
            entity.Property(x => x.ReceiptChecksum).HasMaxLength(128);
            entity.Property(x => x.FileChecksum).HasMaxLength(128);
            entity.Property(x => x.OcrResultJson).HasColumnType("jsonb");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.VerificationNotes).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.ExpenseClaim).WithMany(x => x.Receipts).HasForeignKey(x => x.ExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ExpenseClaimItem).WithMany(x => x.Receipts).HasForeignKey(x => x.ExpenseClaimItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DuplicateOfReceipt).WithMany().HasForeignKey(x => x.DuplicateOfReceiptId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ExpenseClaimId, x.ExpenseClaimItemId, x.IsDelete });
            entity.HasIndex(x => x.ReceiptChecksum).IsUnique().HasFilter("\"ReceiptChecksum\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => x.FileChecksum).HasFilter("\"FileChecksum\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => new { x.ReceiptDate, x.MerchantName, x.ReceiptAmount, x.IsDelete });
            entity.HasIndex(x => new { x.IsDuplicate, x.IsVerified, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxExpenseReceipt> entity)
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
