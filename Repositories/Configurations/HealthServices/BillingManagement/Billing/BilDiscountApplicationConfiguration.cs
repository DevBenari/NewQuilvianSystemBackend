using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilDiscountApplicationConfiguration : IEntityTypeConfiguration<BilDiscountApplication>
{
    public void Configure(EntityTypeBuilder<BilDiscountApplication> entity)
    {
        entity.ToTable("BilDiscountApplication", "public", table =>
        {
            table.HasCheckConstraint("CK_BilDiscountApplication_Amounts", "\"RequestedAmount\" >= 0 AND \"Amount\" >= 0");
            table.HasCheckConstraint("CK_BilDiscountApplication_Status", "\"ApprovalStatus\" IN ('APPROVED','PENDING_DOCTOR','PENDING_FINANCE')");
            table.HasCheckConstraint("CK_BilDiscountApplication_Target", "(\"DiscountType\" = 'PROMO_TOTAL' AND \"InvoiceItemId\" IS NULL) OR (\"DiscountType\" IN ('PROMO_ITEM','DOCTOR') AND \"InvoiceItemId\" IS NOT NULL)");
            table.HasCheckConstraint("CK_BilDiscountApplication_Approval", "(\"ApprovalStatus\" = 'APPROVED' AND (\"DiscountType\" <> 'DOCTOR' OR \"ApprovedBy\" IS NOT NULL)) OR \"ApprovalStatus\" IN ('PENDING_DOCTOR','PENDING_FINANCE')");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.DiscountType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.RequestedAmount).HasPrecision(18, 2);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.ApprovalStatus).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => new { x.InvoiceId, x.ApprovalStatus, x.IsDelete });
        entity.HasIndex(x => new { x.InvoiceId, x.DiscountPolicyId, x.InvoiceItemId, x.IsDelete });
        entity.HasOne(x => x.Invoice).WithMany(x => x.DiscountApplications)
            .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.InvoiceItem).WithMany()
            .HasForeignKey(x => x.InvoiceItemId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.DiscountPolicy).WithMany()
            .HasForeignKey(x => x.DiscountPolicyId).OnDelete(DeleteBehavior.Restrict);
    }
}
