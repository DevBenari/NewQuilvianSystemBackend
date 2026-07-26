using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Enums.HumanResource;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstDoctorConfiguration : IEntityTypeConfiguration<MstDoctor>
    {
        public void Configure(EntityTypeBuilder<MstDoctor> entity)
        {
            entity.ToTable("MstDoctor", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId).IsRequired();
            entity.Property(x => x.DoctorCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DoctorNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NickName).HasMaxLength(100);
            entity.Property(x => x.BirthPlace).HasMaxLength(100);
            entity.Property(x => x.BirthDate).HasColumnType("date").IsRequired(false);

            entity.Property(x => x.Gender).HasConversion<int>().IsRequired(false);
            entity.Property(x => x.Religion).HasConversion<int>().HasDefaultValue(Religion.Unknown).IsRequired();
            entity.Property(x => x.MaritalStatus).HasConversion<int>().HasDefaultValue(MaritalStatus.Unknown).IsRequired();
            entity.Property(x => x.BloodType).HasConversion<int>().HasDefaultValue(BloodType.Unknown).IsRequired();

            entity.Property(x => x.IdentityType).HasMaxLength(50);
            entity.Property(x => x.IdentityNumber).HasMaxLength(50);
            entity.Property(x => x.PhoneNumber).HasMaxLength(30);
            entity.Property(x => x.WhatsAppNumber).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Address).HasMaxLength(500);

            entity.Property(x => x.WorkforceTypeId).IsRequired();
            entity.Property(x => x.EmployeeCategoryId).IsRequired();
            entity.Property(x => x.EmploymentTypeId).IsRequired();
            entity.Property(x => x.EmploymentStatusId).IsRequired();
            entity.Property(x => x.ProfessionId).IsRequired();

            entity.Property(x => x.PracticeType)
                .HasConversion<int>()
                .HasDefaultValue(DoctorPracticeType.FullTime)
                .IsRequired();

            entity.Property(x => x.CredentialingStatus)
                .HasConversion<int>()
                .HasDefaultValue(CredentialingStatus.NotStarted)
                .IsRequired();

            entity.Property(x => x.ClinicalPrivilegeStatus)
                .HasConversion<int>()
                .HasDefaultValue(ClinicalPrivilegeStatus.NotApplicable)
                .IsRequired();

            entity.Property(x => x.SpecialistName).HasMaxLength(100);
            entity.Property(x => x.SubSpecialistName).HasMaxLength(100);
            entity.Property(x => x.MedicalStaffGroup).HasMaxLength(100);
            entity.Property(x => x.GradeLevel).HasMaxLength(50);
            entity.Property(x => x.WorkLocation).HasMaxLength(50);
            entity.Property(x => x.JoinDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ProbationEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ContractStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ContractEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ResignDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ResignReason).HasMaxLength(250);
            entity.Property(x => x.CredentialingDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.IsAvailableForAppointment).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithOne(x => x.Doctor)
                .HasForeignKey<MstDoctor>(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryDepartment).WithMany().HasForeignKey(x => x.PrimaryDepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PrimaryPosition).WithMany().HasForeignKey(x => x.PrimaryPositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceType).WithMany().HasForeignKey(x => x.WorkforceTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmploymentStatus).WithMany().HasForeignKey(x => x.EmploymentStatusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ContractType).WithMany().HasForeignKey(x => x.ContractTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkerSource).WithMany().HasForeignKey(x => x.WorkerSourceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Province).WithMany().HasForeignKey(x => x.ProvinceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostalCode).WithMany().HasForeignKey(x => x.PostalCodeId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId).IsUnique();
            entity.HasIndex(x => x.DoctorCode).IsUnique();
            entity.HasIndex(x => x.DoctorNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.IdentityNumber).IsUnique().HasFilter("\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => x.Email).HasFilter("\"Email\" IS NOT NULL");
            entity.HasIndex(x => x.PhoneNumber);
            entity.HasIndex(x => x.WhatsAppNumber);
            entity.HasIndex(x => x.FullName);
            entity.HasIndex(x => new { x.PrimaryDepartmentId, x.PrimaryPositionId });
            entity.HasIndex(x => new { x.PrimaryDepartmentId, x.PrimaryPositionId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.WorkforceTypeId, x.EmployeeCategoryId, x.EmploymentTypeId, x.EmploymentStatusId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.ProfessionId, x.SpecializationId });
            entity.HasIndex(x => new { x.CredentialingStatus, x.ClinicalPrivilegeStatus, x.PracticeType });
            entity.HasIndex(x => new { x.CountryId, x.ProvinceId, x.CityId, x.DistrictId, x.PostalCodeId });
            entity.HasIndex(x => new { x.SpecialistName, x.SubSpecialistName, x.MedicalStaffGroup });
            entity.HasIndex(x => x.IsAvailableForAppointment);
            entity.HasIndex(x => x.IsActive);
        }
    }
}
