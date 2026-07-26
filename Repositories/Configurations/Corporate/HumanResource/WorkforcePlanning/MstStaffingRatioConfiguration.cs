using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class MstStaffingRatioConfiguration : IEntityTypeConfiguration<MstStaffingRatio>
    {
        public void Configure(EntityTypeBuilder<MstStaffingRatio> builder)
        {
            builder.ToTable("MstStaffingRatio", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.RatioBasisCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RatioBasisName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.WorkforceQuantity).HasPrecision(18, 4);
            builder.Property(x => x.WorkloadQuantity).HasPrecision(18, 4);
            builder.Property(x => x.WorkloadUnit).HasMaxLength(50).IsRequired();
            builder.Property(x => x.MinimumRatio).HasPrecision(18, 6);
            builder.Property(x => x.TargetRatio).HasPrecision(18, 6);
            builder.Property(x => x.MaximumRatio).HasPrecision(18, 6);
            builder.Property(x => x.MinimumHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.RoundingMethod).HasMaxLength(30).IsRequired();
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.StaffingStandard).WithMany(x => x.StaffingRatios).HasForeignKey(x => x.StaffingStandardId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Competency).WithMany().HasForeignKey(x => x.CompetencyId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.StaffingStandardId, x.RatioBasisCode, x.ShiftId, x.PositionId, x.ProfessionId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive });

        }
    }
}
