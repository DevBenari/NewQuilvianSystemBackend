using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>
    /// Pemetaan berkas surat rujukan pendaftaran online — kiosk <c>data/data-dictionary.md</c> PO-D.3.
    /// </summary>
    public class RegOnlineRegistrationReferralDocumentConfiguration
        : IEntityTypeConfiguration<RegOnlineRegistrationReferralDocument>
    {
        public void Configure(EntityTypeBuilder<RegOnlineRegistrationReferralDocument> builder)
        {
            builder.ToTable("RegOnlineRegistrationReferralDocument", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OnlineRegistrationId).IsRequired();
            builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.SizeBytes).IsRequired();
            builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            builder.Property(x => x.PageOrder).IsRequired();

            builder.HasIndex(x => x.OnlineRegistrationId);

            builder.HasOne(x => x.OnlineRegistration)
                .WithMany(x => x.ReferralDocuments)
                .HasForeignKey(x => x.OnlineRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EncounterReferralDocument)
                .WithMany()
                .HasForeignKey(x => x.EncounterReferralDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
