using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>
    /// Pemetaan otorisasi darurat pemberian kantong darah (BD-DOM-08 / BE-BD-008).
    /// </summary>
    public class BbkEmergencyAuthorizationConfiguration
        : IEntityTypeConfiguration<BbkEmergencyAuthorization>
    {
        public void Configure(EntityTypeBuilder<BbkEmergencyAuthorization> builder)
        {
            builder.ToTable("BbkEmergencyAuthorization", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BloodUnitId)
                .IsRequired();

            builder.Property(x => x.PatientId)
                .IsRequired();

            builder.Property(x => x.AuthorizedByUserId)
                .IsRequired();

            builder.Property(x => x.AuthorizedAt)
                .IsRequired();

            builder.Property(x => x.ReasonCode)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ReasonNote)
                .HasMaxLength(500);

            builder.Property(x => x.BypassScope)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.AuthorizerRole)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.EmergencyConditionNote)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(
                x => x.BloodUnitId,
                "IX_BbkEmergencyAuthorization_BloodUnitId");

            builder.HasIndex(
                x => x.PatientId,
                "IX_BbkEmergencyAuthorization_PatientId");

            // Kamus data menandai AuthorizerRole ber-index: audit membaca otorisasi darurat
            // per peran penerbit.
            builder.HasIndex(
                x => x.AuthorizerRole,
                "IX_BbkEmergencyAuthorization_AuthorizerRole");

            builder.HasOne(x => x.BloodUnit)
                .WithMany()
                .HasForeignKey(x => x.BloodUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
