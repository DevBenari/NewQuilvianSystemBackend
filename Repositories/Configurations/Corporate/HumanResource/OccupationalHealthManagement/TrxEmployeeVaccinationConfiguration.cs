using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OccupationalHealthManagement
{
    public class TrxEmployeeVaccinationConfiguration : IEntityTypeConfiguration<TrxEmployeeVaccination>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeVaccination> entity)
        {
            entity.ToTable("TrxEmployeeVaccination", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScheduledDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AdministrationDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NextDoseDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiryDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DoseNumber).HasDefaultValue(1);
            entity.Property(x => x.IsRelevantForRole).HasDefaultValue(true);
            entity.Property(x => x.IsMandatory).HasDefaultValue(false);
            entity.Property(x => x.IsCompliant).HasDefaultValue(false);
            entity.Property(x => x.IsVerified).HasDefaultValue(false);
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

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.VaccinationNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.VaccineType, x.DoseNumber });

            entity.HasIndex(x => new { x.VaccinationStatus, x.NextDoseDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeVaccination> entity)
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
