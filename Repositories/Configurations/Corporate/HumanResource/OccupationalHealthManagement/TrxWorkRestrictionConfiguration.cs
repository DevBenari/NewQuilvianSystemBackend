using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OccupationalHealthManagement
{
    public class TrxWorkRestrictionConfiguration : IEntityTypeConfiguration<TrxWorkRestriction>
    {
        public void Configure(EntityTypeBuilder<TrxWorkRestriction> entity)
        {
            entity.ToTable("TrxWorkRestriction", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReviewDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NoNightShift).HasDefaultValue(false);
            entity.Property(x => x.NoOnCall).HasDefaultValue(false);
            entity.Property(x => x.NoHeavyLifting).HasDefaultValue(false);
            entity.Property(x => x.RequiresWorkplaceAdjustment).HasDefaultValue(false);
            entity.Property(x => x.IsSchedulingBlocked).HasDefaultValue(false);
            entity.Property(x => x.IsClinicalServiceBlocked).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FitnessToWork)
                .WithMany(x => x.WorkRestrictions)
                .HasForeignKey(x => x.FitnessToWorkId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RestrictionNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate });

            entity.HasIndex(x => new { x.RestrictionStatus, x.ReviewDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxWorkRestriction> entity)
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
