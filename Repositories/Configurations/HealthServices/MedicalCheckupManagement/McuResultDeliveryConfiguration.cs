using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan catatan penyerahan hasil — <c>data-dictionary.md</c> bagian 3.</summary>
    public class McuResultDeliveryConfiguration : IEntityTypeConfiguration<McuResultDelivery>
    {
        public void Configure(EntityTypeBuilder<McuResultDelivery> builder)
        {
            builder.ToTable("McuResultDelivery", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RecipientType).HasConversion<int>().IsRequired();
            builder.Property(x => x.ContentType).HasConversion<int>().IsRequired();
            builder.Property(x => x.Channel).HasConversion<int>().IsRequired();
            builder.Property(x => x.RequiresCorrectionNotice).HasDefaultValue(false);

            builder.HasIndex(x => x.McuOrderId).HasDatabaseName("IX_McuResultDelivery_Order");
            builder.HasIndex(x => x.McuResumeId);
            builder.HasIndex(x => x.RequiresCorrectionNotice);

            builder.HasOne(x => x.McuOrder)
                .WithMany()
                .HasForeignKey(x => x.McuOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.McuResume)
                .WithMany()
                .HasForeignKey(x => x.McuResumeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.DeliveredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
