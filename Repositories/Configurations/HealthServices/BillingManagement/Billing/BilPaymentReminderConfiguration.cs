using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilPaymentReminderConfiguration : IEntityTypeConfiguration<BilPaymentReminder>
{
    public void Configure(EntityTypeBuilder<BilPaymentReminder> entity)
    {
        entity.ToTable("BilPaymentReminder", "public");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Channel).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.MessageTemplateCode).HasMaxLength(50);
        entity.Property(x => x.ProviderReferenceMasked).HasMaxLength(100);
        entity.Property(x => x.FailureReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.SentAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.InvoiceId);
        entity.HasIndex(x => x.PatientId);

        entity.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
