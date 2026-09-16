using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.Billing
{
    public class BilInvoiceItemBillingDispositionConfiguration : IEntityTypeConfiguration<BilInvoiceItemBillingDisposition>
    {
        public void Configure(EntityTypeBuilder<BilInvoiceItemBillingDisposition> entity)
        {
            entity.ToTable("BilInvoiceItemBillingDisposition", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.InvoiceItemId).IsRequired();
            entity.Property(x => x.Disposition).HasMaxLength(20).HasDefaultValue("INCLUDED").IsRequired();
            entity.Property(x => x.DecisionSource).HasMaxLength(20).HasDefaultValue("AUTO").IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

            // IdentityModel audit fields
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            // Relationships (Restrict)
            entity.HasOne(x => x.InvoiceItem)
                .WithMany()
                .HasForeignKey(x => x.InvoiceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Filtered unique index: tepat satu keputusan aktif per item
            entity.HasIndex(x => x.InvoiceItemId)
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_BilInvoiceItemBillingDisposition_ActiveItem");

            // Other indexes
            entity.HasIndex(x => x.Disposition);
            entity.HasIndex(x => x.IsActive);
        }
    }
}
