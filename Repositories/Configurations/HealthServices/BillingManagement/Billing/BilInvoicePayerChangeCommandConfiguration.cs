using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.Billing
{
    public class BilInvoicePayerChangeCommandConfiguration : IEntityTypeConfiguration<BilInvoicePayerChangeCommand>
    {
        public void Configure(EntityTypeBuilder<BilInvoicePayerChangeCommand> entity)
        {
            entity.ToTable("BilInvoicePayerChangeCommand", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.InvoiceId).IsRequired();
            entity.Property(x => x.EncounterId).IsRequired();
            entity.Property(x => x.PreviousPayerKind).HasMaxLength(30).IsRequired();
            entity.Property(x => x.NewPayerKind).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PreviousPayerNameSnapshot).HasMaxLength(250);
            entity.Property(x => x.NewPayerNameSnapshot).HasMaxLength(250);
            entity.Property(x => x.PreviousCalculationVersionId).IsRequired(false);
            entity.Property(x => x.NewCalculationVersionId).IsRequired();
            entity.Property(x => x.ResetAssignmentCount).HasDefaultValue(0).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
            entity.Property(x => x.CausationId).HasMaxLength(100);

            // IdentityModel audit fields
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            // Relationships (all Restrict)
            entity.HasOne(x => x.Invoice)
                .WithMany()
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PreviousCalculationVersion)
                .WithMany()
                .HasForeignKey(x => x.PreviousCalculationVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NewCalculationVersion)
                .WithMany()
                .HasForeignKey(x => x.NewCalculationVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique index: mencegah eksekusi ganda dengan IdempotencyKey yang sama
            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasDatabaseName("IX_BilInvoicePayerChangeCommand_IdempotencyKey");

            // Other indexes
            entity.HasIndex(x => x.InvoiceId);
            entity.HasIndex(x => x.EncounterId);
            entity.HasIndex(x => x.NewCalculationVersionId);
            entity.HasIndex(x => x.CorrelationId);
        }
    }
}
