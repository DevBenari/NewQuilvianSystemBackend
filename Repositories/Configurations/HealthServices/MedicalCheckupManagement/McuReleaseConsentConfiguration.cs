using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan persetujuan pelepasan hasil — <c>data-dictionary.md</c> bagian 3.</summary>
    /// <remarks>Satu persetujuan berlaku per order: index unik ber-filter <c>RevokedAt</c> kosong.</remarks>
    public class McuReleaseConsentConfiguration : IEntityTypeConfiguration<McuReleaseConsent>
    {
        public void Configure(EntityTypeBuilder<McuReleaseConsent> builder)
        {
            builder.ToTable("McuReleaseConsent", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EvidenceType).HasConversion<int>().IsRequired();

            builder.HasIndex(x => x.McuOrderId)
                .IsUnique()
                .HasFilter("\"RevokedAt\" IS NULL AND \"IsDelete\" = false")
                .HasDatabaseName("IX_McuReleaseConsent_Active");

            builder.HasOne(x => x.McuOrder)
                .WithMany()
                .HasForeignKey(x => x.McuOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.CompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RevokedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
