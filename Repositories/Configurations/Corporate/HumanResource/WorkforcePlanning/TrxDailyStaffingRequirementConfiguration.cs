using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class TrxDailyStaffingRequirementConfiguration : IEntityTypeConfiguration<TrxDailyStaffingRequirement>
    {
        public void Configure(EntityTypeBuilder<TrxDailyStaffingRequirement> builder)
        {
            builder.ToTable("TrxDailyStaffingRequirement", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.StaffingDate).HasColumnType("date");
            builder.Property(x => x.MinimumRequiredHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.TargetRequiredHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.MaximumRequiredHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.AvailableHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.AllocatedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.GapHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.RequirementStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.GenerationSource).HasMaxLength(30).IsRequired();
            builder.Property(x => x.GeneratedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Competency).WithMany().HasForeignKey(x => x.CompetencyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.StaffingStandard).WithMany().HasForeignKey(x => x.StaffingStandardId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ShiftSkillRequirement).WithMany().HasForeignKey(x => x.ShiftSkillRequirementId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.StaffingGapAnalysis).WithMany().HasForeignKey(x => x.StaffingGapAnalysisId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.StaffingDate, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.ShiftId, x.PositionId, x.ProfessionId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.RequirementStatus, x.GapHeadcount, x.IsLocked });

        }
    }
}
