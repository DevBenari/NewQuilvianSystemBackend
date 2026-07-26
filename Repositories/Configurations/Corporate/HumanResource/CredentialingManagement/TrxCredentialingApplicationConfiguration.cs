using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxCredentialingApplicationConfiguration : IEntityTypeConfiguration<TrxCredentialingApplication>
    {
        public void Configure(EntityTypeBuilder<TrxCredentialingApplication> entity)
        {
            entity.ToTable("TrxCredentialingApplication", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ApplicationDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DueDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.WorkforceSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.RequirementSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ComplianceResultJson).HasColumnType("jsonb");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Profession)
                .WithMany()
                .HasForeignKey(x => x.ProfessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Specialization)
                .WithMany()
                .HasForeignKey(x => x.SpecializationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialingRequirement)
                .WithMany()
                .HasForeignKey(x => x.CredentialingRequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ApplicationNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ApplicationStatus, x.ApplicationDate });

            entity.HasIndex(x => new { x.DueDate, x.ApplicationStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxCredentialingApplication> entity)
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
