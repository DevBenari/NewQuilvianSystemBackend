using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Configurations;

public sealed class BilPettyCashVoucherConfiguration : IEntityTypeConfiguration<BilPettyCashVoucher>
{
    public void Configure(EntityTypeBuilder<BilPettyCashVoucher> entity)
    {
        entity.ToTable("BilPettyCashVoucher", "public", table =>
        {
            table.HasCheckConstraint("CK_BilPettyCashVoucher_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_BilPettyCashVoucher_Status", "\"Status\" IN ('WAITING_APPROVAL','APPROVED','CASH_RECEIVED','COMPLETED','REJECTED')");
            // RejectionReason wajib terisi tepat ketika Status = REJECTED, dan kosong pada status lain.
            table.HasCheckConstraint("CK_BilPettyCashVoucher_RejectionReason", "(\"Status\" = 'REJECTED' AND \"RejectionReason\" IS NOT NULL) OR (\"Status\" <> 'REJECTED' AND \"RejectionReason\" IS NULL)");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.VoucherNumber).HasMaxLength(40).IsRequired();
        entity.Property(x => x.RecipientName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Purpose).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(PettyCashVoucherStatuses.WaitingApproval);
        entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.DisbursedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ProofReferenceNumber).HasMaxLength(60);
        entity.Property(x => x.ProofSubmittedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.VoucherNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_BilPettyCashVoucher_VoucherNumber");

        entity.HasIndex(x => new { x.Status, x.SubmittedAt })
            .HasDatabaseName("IX_BilPettyCashVoucher_Status_SubmittedAt");

        entity.HasIndex(x => x.CategoryId)
            .HasDatabaseName("IX_BilPettyCashVoucher_CategoryId");

        entity.HasIndex(x => x.RecipientName)
            .HasDatabaseName("IX_BilPettyCashVoucher_RecipientName");

        entity.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL")
            .HasDatabaseName("IX_BilPettyCashVoucher_IdempotencyKey");

        entity.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
