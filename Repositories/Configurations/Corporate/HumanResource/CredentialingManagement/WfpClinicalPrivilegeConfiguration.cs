using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class WfpClinicalPrivilegeConfiguration : IEntityTypeConfiguration<WfpClinicalPrivilege>
    {
        public void Configure(EntityTypeBuilder<WfpClinicalPrivilege> entity)
        {
            entity.ToTable("WfpClinicalPrivilege", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.GrantedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SuspendedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialLicense)
                .WithMany(x => x.ClinicalPrivileges)
                .HasForeignKey(x => x.CredentialLicenseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ClinicalPrivilegeCatalog)
                .WithMany()
                .HasForeignKey(x => x.ClinicalPrivilegeCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialingDecision)
                .WithMany()
                .HasForeignKey(x => x.CredentialingDecisionId)
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

            entity.HasIndex(x => new { x.WorkforceProfileId, x.PrivilegeCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.EffectiveEndDate, x.PrivilegeStatus, x.IsActive });

            entity.HasIndex(x => new { x.ClinicalPrivilegeCatalogId, x.PrivilegeStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<WfpClinicalPrivilege> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
