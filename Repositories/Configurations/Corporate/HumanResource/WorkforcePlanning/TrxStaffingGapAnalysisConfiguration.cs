using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class TrxStaffingGapAnalysisConfiguration : IEntityTypeConfiguration<TrxStaffingGapAnalysis>
    {
        public void Configure(EntityTypeBuilder<TrxStaffingGapAnalysis> builder)
        {
            builder.ToTable("TrxStaffingGapAnalysis", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.AnalysisNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PeriodStartDate).HasColumnType("date");
            builder.Property(x => x.PeriodEndDate).HasColumnType("date");
            builder.Property(x => x.RequiredHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.AvailableHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.AssignedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.OnLeaveHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.AbsentHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.QualifiedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.GapHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.GapPercentage).HasPrecision(9, 4);
            builder.Property(x => x.GapSeverity).HasMaxLength(20).IsRequired();
            builder.Property(x => x.AnalysisSource).HasMaxLength(30).IsRequired();
            builder.Property(x => x.GeneratedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ResolvedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RecommendedAction).HasMaxLength(1000);

            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.StaffingStandard).WithMany().HasForeignKey(x => x.StaffingStandardId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.StaffingRatio).WithMany().HasForeignKey(x => x.StaffingRatioId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PositionHeadcountPlan).WithMany().HasForeignKey(x => x.PositionHeadcountPlanId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AnalysisNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.PeriodStartDate, x.PeriodEndDate, x.HospitalSiteId, x.OrganizationUnitId, x.ShiftId });
            builder.HasIndex(x => new { x.GapSeverity, x.RequiresAction, x.IsResolved });

        }
    }
}
