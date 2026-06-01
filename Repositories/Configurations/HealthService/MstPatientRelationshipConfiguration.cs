using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPatientRelationshipConfiguration : IEntityTypeConfiguration<MstPatientRelationship>
    {
        public void Configure(EntityTypeBuilder<MstPatientRelationship> entity)
        {
            entity.ToTable("MstPatientRelationship", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.RelatedPatientId)
                .IsRequired(false);

            entity.Property(x => x.RelationshipType)
                .HasConversion<int>()
                .HasDefaultValue(PatientRelationshipType.Unknown)
                .IsRequired();

            entity.Property(x => x.RelatedPersonName)
                .HasMaxLength(200);

            entity.Property(x => x.RelatedPersonIdentityType)
                .HasMaxLength(50);

            entity.Property(x => x.RelatedPersonIdentityNumber)
                .HasMaxLength(100);

            entity.Property(x => x.RelatedPersonPhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.RelatedPersonWhatsAppNumber)
                .HasMaxLength(30);

            entity.Property(x => x.RelatedPersonEmail)
                .HasMaxLength(200);

            entity.Property(x => x.RelatedPersonAddress)
                .HasMaxLength(500);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsEmergencyContact)
                .HasDefaultValue(false);

            entity.Property(x => x.IsResponsiblePerson)
                .HasDefaultValue(false);

            entity.Property(x => x.IsLegalGuardian)
                .HasDefaultValue(false);

            entity.Property(x => x.Notes)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RelatedPatient)
                .WithMany()
                .HasForeignKey(x => x.RelatedPatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.RelatedPatientId);

            entity.HasIndex(x => x.RelationshipType);

            entity.HasIndex(x => x.RelatedPersonIdentityNumber);

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.RelationshipType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.IsPrimary
            })
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.IsEmergencyContact,
                x.IsResponsiblePerson,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
