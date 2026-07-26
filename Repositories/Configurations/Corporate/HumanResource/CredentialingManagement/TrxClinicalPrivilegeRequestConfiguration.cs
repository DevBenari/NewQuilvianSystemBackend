using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxClinicalPrivilegeRequestConfiguration : IEntityTypeConfiguration<TrxClinicalPrivilegeRequest>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalPrivilegeRequest> entity)
        {
            entity.ToTable("TrxClinicalPrivilegeRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestedEffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RequestedEffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SupportingEvidenceJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ExistingClinicalPrivilege)
                .WithMany(x => x.Requests)
                .HasForeignKey(x => x.ExistingClinicalPrivilegeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ClinicalPrivilegeCatalog)
                .WithMany()
                .HasForeignKey(x => x.ClinicalPrivilegeCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialingApplication)
                .WithMany()
                .HasForeignKey(x => x.CredentialingApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PrivilegeRequestNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.RequestStatus });

            entity.HasIndex(x => new { x.ClinicalPrivilegeCatalogId, x.RequestStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxClinicalPrivilegeRequest> entity)
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
