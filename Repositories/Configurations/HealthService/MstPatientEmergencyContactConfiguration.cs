using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPatientEmergencyContactConfiguration : IEntityTypeConfiguration<MstPatientEmergencyContact>
    {
        public void Configure(EntityTypeBuilder<MstPatientEmergencyContact> entity)
        {
            entity.ToTable("MstPatientEmergencyContact", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.ContactName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Relationship)
                .HasMaxLength(50);

            entity.Property(x => x.IdentityType)
                .HasMaxLength(50);

            entity.Property(x => x.IdentityNumber)
                .HasMaxLength(100);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.WhatsAppNumber)
                .HasMaxLength(30);

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsResponsiblePerson)
                .HasDefaultValue(false);

            entity.Property(x => x.IsSameAddressAsPatient)
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

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.ContactName);

            entity.HasIndex(x => x.PhoneNumber);

            entity.HasIndex(x => x.WhatsAppNumber);

            entity.HasIndex(x => x.IdentityNumber);

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
                x.IsResponsiblePerson,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
