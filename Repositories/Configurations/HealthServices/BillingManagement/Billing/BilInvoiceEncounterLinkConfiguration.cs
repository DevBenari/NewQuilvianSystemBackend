using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.Billing;

public sealed class BilInvoiceEncounterLinkConfiguration : IEntityTypeConfiguration<BilInvoiceEncounterLink>
{
    public void Configure(EntityTypeBuilder<BilInvoiceEncounterLink> entity)
    {
        entity.ToTable("BilInvoiceEncounterLink", "public");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.LinkReason).HasMaxLength(40).IsRequired();
        entity.HasIndex(x => new { x.RanapInvoiceId, x.LinkedEncounterId }).IsUnique()
            .HasFilter("NOT \"IsDelete\"").HasDatabaseName("UX_BilInvoiceEncounterLink_Invoice_Encounter");
        entity.HasIndex(x => x.LinkedEncounterId);
        entity.HasOne(x => x.RanapInvoice).WithMany().HasForeignKey(x => x.RanapInvoiceId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.LinkedEncounter).WithMany().HasForeignKey(x => x.LinkedEncounterId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.SourceReferral).WithMany().HasForeignKey(x => x.SourceReferralId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Receipt).WithMany().HasForeignKey(x => x.ReceiptId).OnDelete(DeleteBehavior.Restrict);
    }
}
