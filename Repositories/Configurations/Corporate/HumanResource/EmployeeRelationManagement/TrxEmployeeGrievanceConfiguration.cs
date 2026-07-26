using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxEmployeeGrievanceConfiguration : IEntityTypeConfiguration<TrxEmployeeGrievance>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeGrievance> entity)
        {
            entity.ToTable("TrxEmployeeGrievance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.GrievanceDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ResolvedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.WithdrawnAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AttachmentMetadataJson).HasColumnType("jsonb");
            entity.Property(x => x.IsIdentityProtected).HasDefaultValue(true);
            entity.Property(x => x.IsConfidential).HasDefaultValue(true);
            entity.Property(x => x.RequiresEnhancedAudit).HasDefaultValue(true);
            entity.Property(x => x.CanComplainantViewStatus).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.ComplainantWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.ComplainantWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ComplainantEmployee)
                .WithMany()
                .HasForeignKey(x => x.ComplainantEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ComplainantUser)
                .WithMany()
                .HasForeignKey(x => x.ComplainantUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AgainstWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.AgainstWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedHrUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedHrUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.GrievanceNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.ComplainantWorkforceProfileId, x.GrievanceDate });

            entity.HasIndex(x => new { x.GrievanceStatus, x.AssignedHrUserId });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeGrievance> entity)
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
