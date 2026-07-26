using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OccupationalHealthManagement
{
    public class TrxEmployeeInjuryConfiguration : IEntityTypeConfiguration<TrxEmployeeInjury>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeInjury> entity)
        {
            entity.ToTable("TrxEmployeeInjury", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.InjuryDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReturnToWorkDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsLostTimeInjury).HasDefaultValue(false);
            entity.Property(x => x.LostWorkDays).HasDefaultValue(0);
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

            entity.HasOne(x => x.FitnessToWork)
                .WithMany(x => x.RelatedInjuries)
                .HasForeignKey(x => x.FitnessToWorkId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.InjuryNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.InjuryDateTime });

            entity.HasIndex(x => new { x.InjuryStatus, x.SeverityLevel });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeInjury> entity)
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
