using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdStaffingSkillMixRuleConfiguration : IEntityTypeConfiguration<HrdStaffingSkillMixRule>
    {
        public void Configure(EntityTypeBuilder<HrdStaffingSkillMixRule> builder)
        {
            builder.ToTable("HrdStaffingSkillMixRule", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.MinimumCompetencyLevel).HasMaxLength(50);
            builder.Property(x => x.MinimumQualifiedHeadcount).HasPrecision(9, 2);
            builder.Property(x => x.IsMandatory).HasDefaultValue(true);
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasOne(x => x.StaffingStandard)
                .WithMany(x => x.SkillMixRules)
                .HasForeignKey(x => x.StaffingStandardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Shift)
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Profession)
                .WithMany()
                .HasForeignKey(x => x.ProfessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Competency)
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.StaffingStandardId, x.ShiftId, x.CompetencyId, x.ProfessionId })
                .IsUnique()
                .HasDatabaseName("IX_HrdStaffingSkillMixRule_Standard_Shift_Comp_Prof");

            builder.HasIndex(x => x.CompetencyId);
        }
    }
}
