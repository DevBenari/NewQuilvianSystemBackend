using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class MstShiftSkillRequirementConfiguration : IEntityTypeConfiguration<MstShiftSkillRequirement>
    {
        public void Configure(EntityTypeBuilder<MstShiftSkillRequirement> builder)
        {
            builder.ToTable("MstShiftSkillRequirement", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.RequirementCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequirementName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.MinimumCompetencyLevel).HasMaxLength(50);
            builder.Property(x => x.MinimumQualifiedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.TargetQualifiedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ShiftGroup).WithMany().HasForeignKey(x => x.ShiftGroupId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Competency).WithMany().HasForeignKey(x => x.CompetencyId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RequirementCode).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.ShiftId, x.IsActive });
            builder.HasIndex(x => new { x.ProfessionId, x.SpecializationId, x.CompetencyId, x.IsMandatory });

        }
    }
}
