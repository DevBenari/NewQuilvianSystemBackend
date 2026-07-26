using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OccupationalHealthManagement
{
    public class TrxEmployeeMedicalExaminationConfiguration : IEntityTypeConfiguration<TrxEmployeeMedicalExamination>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeMedicalExamination> entity)
        {
            entity.ToTable("TrxEmployeeMedicalExamination", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScheduledAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NextExaminationDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReminderSentAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsMandatory).HasDefaultValue(false);
            entity.Property(x => x.IsClinicalDataRestricted).HasDefaultValue(true);
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

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkLocation)
                .WithMany()
                .HasForeignKey(x => x.WorkLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HealthRecord)
                .WithMany(x => x.MedicalExaminations)
                .HasForeignKey(x => x.HealthRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ExaminationNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ScheduledAt });

            entity.HasIndex(x => new { x.AdministrativeStatus, x.NextExaminationDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeMedicalExamination> entity)
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
