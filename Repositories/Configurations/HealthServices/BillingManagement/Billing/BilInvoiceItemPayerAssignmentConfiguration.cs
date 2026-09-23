using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.Billing
{
    public class BilInvoiceItemPayerAssignmentConfiguration : IEntityTypeConfiguration<BilInvoiceItemPayerAssignment>
    {
        public void Configure(EntityTypeBuilder<BilInvoiceItemPayerAssignment> entity)
        {
            entity.ToTable("BilInvoiceItemPayerAssignment", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.InvoiceItemId).IsRequired();
            entity.Property(x => x.EncounterGuarantorId).IsRequired(false);
            entity.Property(x => x.PayerKind).HasMaxLength(30).HasDefaultValue("CASH").IsRequired();
            entity.Property(x => x.AssignmentSource).HasMaxLength(20).HasDefaultValue("AUTO").IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

            // IdentityModel audit fields
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            // Relationships (all Restrict)
            entity.HasOne(x => x.InvoiceItem)
                .WithMany()
                .HasForeignKey(x => x.InvoiceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EncounterGuarantor)
                .WithMany()
                .HasForeignKey(x => x.EncounterGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Filtered unique index: tepat satu penanggung aktif per baris biaya
            entity.HasIndex(x => x.InvoiceItemId)
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_BilInvoiceItemPayerAssignment_ActiveItem");

            // Other indexes
            entity.HasIndex(x => x.EncounterGuarantorId);
            entity.HasIndex(x => x.PayerKind);
            entity.HasIndex(x => x.IsActive);
        }
    }
}
