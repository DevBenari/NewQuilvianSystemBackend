using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstEmployeeConfiguration : IEntityTypeConfiguration<MstEmployee>
    {
        public void Configure(EntityTypeBuilder<MstEmployee> entity)
        {
            entity.ToTable("MstEmployee", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.EmployeeCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EmployeeNumber)
                .HasMaxLength(50)
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
                .IsRequired();

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
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.IdentityNumber)
                .HasMaxLength(16)
                .IsRequired();

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(13);

            entity.Property(x => x.WhatsAppNumber)
                .HasMaxLength(13);

            entity.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.PrimaryDepartmentId)
                .IsRequired();

            entity.Property(x => x.PrimaryPositionId)
                .IsRequired();

            entity.Property(x => x.EmployeeStatus)
                .HasConversion<int>()
                .HasDefaultValue(EmployeeStatus.Active)
                .IsRequired();

            entity.Property(x => x.ProfessionType)
                .HasConversion<int>()
                .HasDefaultValue(EmployeeProfessionType.GeneralStaff)
                .IsRequired();

            entity.Property(x => x.EmploymentType)
                .HasConversion<int>()
                .HasDefaultValue(EmploymentType.Contract)
                .IsRequired();

            entity.Property(x => x.GradeLevel)
                .HasMaxLength(50);

            entity.Property(x => x.WorkLocation)
                .HasMaxLength(50);

            entity.Property(x => x.JoinDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.ProbationEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ContractStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ContractEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ResignDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ResignReason)
                .HasMaxLength(250);

            entity.Property(x => x.EmergencyContactName)
                .HasMaxLength(200);

            entity.Property(x => x.EmergencyContactRelation)
                .HasMaxLength(50);

            entity.Property(x => x.EmergencyContactPhone)
                .HasMaxLength(13);

            entity.Property(x => x.EmergencyContactAddress)
                .HasMaxLength(500);

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

            entity.HasOne(x => x.WorkforceProfile)
                .WithOne(x => x.Employee)
                .HasForeignKey<MstEmployee>(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryDepartment)
                .WithMany()
                .HasForeignKey(x => x.PrimaryDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryPosition)
                .WithMany()
                .HasForeignKey(x => x.PrimaryPositionId)
                .OnDelete(DeleteBehavior.Restrict);

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

            entity.HasIndex(x => x.WorkforceProfileId)
                .IsUnique();

            entity.HasIndex(x => x.EmployeeCode)
                .IsUnique();

            entity.HasIndex(x => x.EmployeeNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IdentityNumber)
                .IsUnique()
                .HasFilter("\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => x.Email)
                .IsUnique()
                .HasFilter("\"Email\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => x.PhoneNumber);

            entity.HasIndex(x => x.WhatsAppNumber);

            entity.HasIndex(x => x.FullName);

            entity.HasIndex(x => new
            {
                x.PrimaryDepartmentId,
                x.PrimaryPositionId
            });

            entity.HasIndex(x => new
            {
                x.PrimaryDepartmentId,
                x.PrimaryPositionId,
                x.IsActive,
                x.IsDelete
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
                x.EmployeeStatus,
                x.ProfessionType,
                x.EmploymentType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => x.Religion);

            entity.HasIndex(x => x.MaritalStatus);

            entity.HasIndex(x => x.BloodType);

            entity.HasIndex(x => x.IsActive);
        }
    }
}