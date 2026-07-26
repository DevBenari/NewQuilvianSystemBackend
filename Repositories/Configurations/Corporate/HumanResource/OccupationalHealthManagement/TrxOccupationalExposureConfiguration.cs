using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OccupationalHealthManagement
{
    public class TrxOccupationalExposureConfiguration : IEntityTypeConfiguration<TrxOccupationalExposure>
    {
        public void Configure(EntityTypeBuilder<TrxOccupationalExposure> entity)
        {
            entity.ToTable("TrxOccupationalExposure", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExposureDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.FollowUpDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.FollowUpRequired).HasDefaultValue(true);
            entity.Property(x => x.IsReportableIncident).HasDefaultValue(false);
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

            entity.HasOne(x => x.MedicalExamination)
                .WithMany()
                .HasForeignKey(x => x.MedicalExaminationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ExposureNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ExposureDateTime });

            entity.HasIndex(x => new { x.ExposureStatus, x.RiskLevel });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxOccupationalExposure> entity)
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
