using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.Billing;

public sealed class BilSupersededItemReviewConfiguration : IEntityTypeConfiguration<BilSupersededItemReview>
{
    public void Configure(EntityTypeBuilder<BilSupersededItemReview> entity)
    {
        entity.ToTable("BilSupersededItemReview", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilSupersededItemReview_Decision",
                "\"Decision\" IN ('REINSTATE','KEEP','NEEDS_POST_CLOSE_CORRECTION')");

            table.HasCheckConstraint(
                "CK_BilSupersededItemReview_ReinstatedItem",
                "(\"Decision\" = 'REINSTATE' AND \"ReinstatedInvoiceItemId\" IS NOT NULL) OR (\"Decision\" <> 'REINSTATE' AND \"ReinstatedInvoiceItemId\" IS NULL)");

            table.HasCheckConstraint(
                "CK_BilSupersededItemReview_Reason",
                "length(trim(\"Reason\")) > 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Decision).HasMaxLength(40).IsRequired();
        entity.Property(x => x.InvoiceStatusAtDecision).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.InvoiceItemId)
            .IsUnique()
            .HasFilter("NOT \"IsDelete\"")
            .HasDatabaseName("UX_BilSupersededItemReview_InvoiceItem");

        entity.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("UX_BilSupersededItemReview_IdempotencyKey");

        entity.HasIndex(x => x.InvoiceId)
            .HasDatabaseName("IX_BilSupersededItemReview_InvoiceId");

        entity.HasIndex(x => x.Decision)
            .HasDatabaseName("IX_BilSupersededItemReview_Decision");

        entity.HasOne(x => x.InvoiceItem)
            .WithMany()
            .HasForeignKey(x => x.InvoiceItemId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ReinstatedInvoiceItem)
            .WithMany()
            .HasForeignKey(x => x.ReinstatedInvoiceItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

