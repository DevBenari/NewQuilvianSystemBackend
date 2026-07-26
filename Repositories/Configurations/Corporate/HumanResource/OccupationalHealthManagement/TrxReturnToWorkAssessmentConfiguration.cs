using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OccupationalHealthManagement
{
    public class TrxReturnToWorkAssessmentConfiguration : IEntityTypeConfiguration<TrxReturnToWorkAssessment>
    {
        public void Configure(EntityTypeBuilder<TrxReturnToWorkAssessment> entity)
        {
            entity.ToTable("TrxReturnToWorkAssessment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssessmentDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpectedReturnDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ActualReturnDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PhasedReturnStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PhasedReturnEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReviewDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsPhasedReturn).HasDefaultValue(false);
            entity.Property(x => x.IsSchedulingAllowed).HasDefaultValue(false);
            entity.Property(x => x.IsClinicalDutyAllowed).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeInjury)
                .WithMany(x => x.ReturnToWorkAssessments)
                .HasForeignKey(x => x.EmployeeInjuryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveRequest)
                .WithMany()
                .HasForeignKey(x => x.LeaveRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MedicalExamination)
                .WithMany()
                .HasForeignKey(x => x.MedicalExaminationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AssessmentNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.AssessmentDate });

            entity.HasIndex(x => new { x.AssessmentStatus, x.ReviewDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxReturnToWorkAssessment> entity)
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
