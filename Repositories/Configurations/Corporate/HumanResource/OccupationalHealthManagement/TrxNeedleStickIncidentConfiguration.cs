using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OccupationalHealthManagement
{
    public class TrxNeedleStickIncidentConfiguration : IEntityTypeConfiguration<TrxNeedleStickIncident>
    {
        public void Configure(EntityTypeBuilder<TrxNeedleStickIncident> entity)
        {
            entity.ToTable("TrxNeedleStickIncident", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IncidentDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.FollowUpDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsHollowBoreNeedle).HasDefaultValue(false);
            entity.Property(x => x.WasVisibleBloodPresent).HasDefaultValue(false);
            entity.Property(x => x.IsSourceKnown).HasDefaultValue(false);
            entity.Property(x => x.PostExposureProphylaxisRecommended).HasDefaultValue(false);
            entity.Property(x => x.PostExposureProphylaxisStarted).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.OccupationalExposure)
                .WithMany(x => x.NeedleStickIncidents)
                .HasForeignKey(x => x.OccupationalExposureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.IncidentNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.IncidentDateTime });

            entity.HasIndex(x => new { x.IncidentStatus, x.FollowUpDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxNeedleStickIncident> entity)
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
