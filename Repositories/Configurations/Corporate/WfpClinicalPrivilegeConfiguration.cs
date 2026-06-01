using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpClinicalPrivilegeConfiguration : IEntityTypeConfiguration<WfpClinicalPrivilege>
    {
        public void Configure(EntityTypeBuilder<WfpClinicalPrivilege> entity)
        {
            entity.ToTable("WfpClinicalPrivilege", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.CredentialLicenseId)
                .IsRequired(false);

            entity.Property(x => x.DepartmentId)
                .IsRequired(false);

            entity.Property(x => x.PositionId)
                .IsRequired(false);

            entity.Property(x => x.PrivilegeCode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.PrivilegeName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.PrivilegeType)
                .HasConversion<int>()
                .HasDefaultValue(ClinicalPrivilegeType.CorePrivilege)
                .IsRequired();

            entity.Property(x => x.ClinicalScope)
                .HasMaxLength(100);

            entity.Property(x => x.SpecialtyName)
                .HasMaxLength(150);

            entity.Property(x => x.SubSpecialtyName)
                .HasMaxLength(150);

            entity.Property(x => x.ProcedureGroup)
                .HasMaxLength(150);

            entity.Property(x => x.ProcedureName)
                .HasMaxLength(200);

            entity.Property(x => x.PracticeLocation)
                .HasMaxLength(200);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.PrivilegeStatus)
                .HasConversion<int>()
                .HasDefaultValue(ClinicalPrivilegeStatus.PendingApproval)
                .IsRequired();

            entity.Property(x => x.IsTemporary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsEmergencyPrivilege)
                .HasDefaultValue(false);

            entity.Property(x => x.IsSupervisionRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.SupervisorUserId)
                .IsRequired(false);

            entity.Property(x => x.GrantedByUserId)
                .IsRequired(false);

            entity.Property(x => x.GrantedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.GrantNote)
                .HasMaxLength(250);

            entity.Property(x => x.RejectedByUserId)
                .IsRequired(false);

            entity.Property(x => x.RejectedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RejectedReason)
                .HasMaxLength(250);

            entity.Property(x => x.SuspendedByUserId)
                .IsRequired(false);

            entity.Property(x => x.SuspendedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.SuspensionReason)
                .HasMaxLength(250);

            entity.Property(x => x.RevokedByUserId)
                .IsRequired(false);

            entity.Property(x => x.RevokedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RevokedReason)
                .HasMaxLength(250);

            entity.Property(x => x.LastReviewDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.NextReviewDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.SupportingFilePath)
                .HasMaxLength(500);

            entity.Property(x => x.SupportingFileContentType)
                .HasMaxLength(100);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

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
                .WithMany(x => x.ClinicalPrivileges)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialLicense)
                .WithMany()
                .HasForeignKey(x => x.CredentialLicenseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SupervisorUser)
                .WithMany()
                .HasForeignKey(x => x.SupervisorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.GrantedByUser)
                .WithMany()
                .HasForeignKey(x => x.GrantedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RejectedByUser)
                .WithMany()
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SuspendedByUser)
                .WithMany()
                .HasForeignKey(x => x.SuspendedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RevokedByUser)
                .WithMany()
                .HasForeignKey(x => x.RevokedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.CredentialLicenseId);

            entity.HasIndex(x => x.DepartmentId);

            entity.HasIndex(x => x.PositionId);

            entity.HasIndex(x => x.SupervisorUserId);

            entity.HasIndex(x => x.GrantedByUserId);

            entity.HasIndex(x => x.RejectedByUserId);

            entity.HasIndex(x => x.SuspendedByUserId);

            entity.HasIndex(x => x.RevokedByUserId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.PrivilegeCode
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.PrivilegeStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.PrivilegeType,
                x.PrivilegeStatus
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.DepartmentId,
                x.PositionId,
                x.PrivilegeStatus
            });

            entity.HasIndex(x => new
            {
                x.CredentialLicenseId,
                x.PrivilegeStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EffectiveStartDate,
                x.EffectiveEndDate
            });

            entity.HasIndex(x => new
            {
                x.EffectiveEndDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
