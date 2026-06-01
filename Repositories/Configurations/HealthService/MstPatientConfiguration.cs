using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPatientConfiguration : IEntityTypeConfiguration<MstPatient>
    {
        public void Configure(EntityTypeBuilder<MstPatient> entity)
        {
            entity.ToTable("MstPatient", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PatientCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.MedicalRecordNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PatientType)
                .HasConversion<int>()
                .HasDefaultValue(PatientType.General)
                .IsRequired();

            entity.Property(x => x.PatientStatus)
                .HasConversion<int>()
                .HasDefaultValue(PatientStatus.Active)
                .IsRequired();

            entity.Property(x => x.RegistrationSource)
                .HasConversion<int>()
                .HasDefaultValue(PatientRegistrationSource.Unknown)
                .IsRequired();

            entity.Property(x => x.FullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.NickName)
                .HasMaxLength(100);

            entity.Property(x => x.BirthPlace)
                .HasMaxLength(100);

            entity.Property(x => x.BirthDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.Gender)
                .HasConversion<int>()
                .IsRequired(false);

            entity.Property(x => x.Religion)
                .HasConversion<int>()
                .HasDefaultValue(Religion.Unknown)
                .IsRequired();

            entity.Property(x => x.MaritalStatus)
                .HasConversion<int>()
                .HasDefaultValue(MaritalStatus.Unknown)
                .IsRequired();

            entity.Property(x => x.BloodType)
                .HasConversion<int>()
                .HasDefaultValue(BloodType.Unknown)
                .IsRequired();

            entity.Property(x => x.IdentityType)
                .HasMaxLength(50);

            entity.Property(x => x.IdentityNumber)
                .HasMaxLength(50);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.WhatsAppNumber)
                .HasMaxLength(30);

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.PhotoPath)
                .HasMaxLength(500);

            entity.Property(x => x.IsMember)
                .HasDefaultValue(true);

            entity.Property(x => x.DefaultMembershipTierId)
                .IsRequired(false);

            entity.Property(x => x.ActivePatientMembershipId)
                .IsRequired(false);

            entity.HasOne(x => x.DefaultMembershipTier)
                .WithMany()
                .HasForeignKey(x => x.DefaultMembershipTierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DefaultMembershipTierId);

            entity.HasIndex(x => x.ActivePatientMembershipId);

            entity.Property(x => x.IsNewborn)
                .HasDefaultValue(false);

            entity.Property(x => x.BirthWeightGram)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.BirthLengthCm)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.BirthTime)
                .IsRequired(false);

            entity.Property(x => x.DeliveryMethod)
                .HasMaxLength(100);

            entity.Property(x => x.IsDeceased)
                .HasDefaultValue(false);

            entity.Property(x => x.DeceasedDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.MergeReason)
                .HasMaxLength(250);

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

            entity.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Province)
                .WithMany()
                .HasForeignKey(x => x.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.District)
                .WithMany()
                .HasForeignKey(x => x.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PostalCode)
                .WithMany()
                .HasForeignKey(x => x.PostalCodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DefaultMembershipTier)
                .WithMany()
                .HasForeignKey(x => x.DefaultMembershipTierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MotherPatient)
                .WithMany()
                .HasForeignKey(x => x.MotherPatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MergedToPatient)
                .WithMany()
                .HasForeignKey(x => x.MergedToPatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PatientCode)
                .IsUnique();

            entity.HasIndex(x => x.MedicalRecordNumber)
                .IsUnique();

            entity.HasIndex(x => x.IdentityNumber)
                .IsUnique()
                .HasFilter("\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => x.FullName);

            entity.HasIndex(x => x.PhoneNumber);

            entity.HasIndex(x => x.WhatsAppNumber);

            entity.HasIndex(x => x.Email);

            entity.HasIndex(x => x.MotherPatientId);

            entity.HasIndex(x => x.ActivePatientMembershipId);

            entity.HasIndex(x => new
            {
                x.PatientType,
                x.PatientStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.FullName,
                x.BirthDate,
                x.Gender
            });

            entity.HasIndex(x => new
            {
                x.CountryId,
                x.ProvinceId,
                x.CityId,
                x.DistrictId,
                x.PostalCodeId
            });

            entity.HasIndex(x => new
            {
                x.IsMember,
                x.IsNewborn,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
