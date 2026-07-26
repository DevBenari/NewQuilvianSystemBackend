using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OccupationalHealthManagement
{
    public class TrxEmployeeHealthSurveillanceConfiguration : IEntityTypeConfiguration<TrxEmployeeHealthSurveillance>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeHealthSurveillance> entity)
        {
            entity.ToTable("TrxEmployeeHealthSurveillance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScheduledDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NextSurveillanceDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReminderSentAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsCompliant).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OccupationalExposure)
                .WithMany(x => x.HealthSurveillanceRecords)
                .HasForeignKey(x => x.OccupationalExposureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MedicalExamination)
                .WithMany()
                .HasForeignKey(x => x.MedicalExaminationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.SurveillanceNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ScheduledDate });

            entity.HasIndex(x => new { x.SurveillanceStatus, x.NextSurveillanceDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeHealthSurveillance> entity)
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
