using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary><c>InpAdmissionPrivacyRequest</c> — kamus data 20.6 dan DDL 20.18 (<c>BE-RWI-192</c>).</summary>
    public class InpAdmissionPrivacyRequestConfiguration : IEntityTypeConfiguration<InpAdmissionPrivacyRequest>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionPrivacyRequest> builder)
        {
            builder.ToTable("InpAdmissionPrivacyRequest", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsTransportPrivacyRequested).IsRequired().HasDefaultValue(false);

            builder.HasOne(x => x.Document).WithOne(x => x.PrivacyRequest)
                .HasForeignKey<InpAdmissionPrivacyRequest>(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
