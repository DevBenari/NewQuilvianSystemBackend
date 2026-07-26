using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class TrxEmployeeDemotionConfiguration : IEntityTypeConfiguration<TrxEmployeeDemotion>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeDemotion> builder)
        {
            builder.ToTable("TrxEmployeeDemotion", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.DemotionNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.DemotionStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectiveDate).HasColumnType("date");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReasonText).HasMaxLength(500);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromPosition).WithMany().HasForeignKey(x => x.FromPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToPosition).WithMany().HasForeignKey(x => x.ToPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromJobLevel).WithMany().HasForeignKey(x => x.FromJobLevelId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToJobLevel).WithMany().HasForeignKey(x => x.ToJobLevelId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromEmployeeGrade).WithMany().HasForeignKey(x => x.FromEmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToEmployeeGrade).WithMany().HasForeignKey(x => x.ToEmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromSalaryGrade).WithMany().HasForeignKey(x => x.FromSalaryGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToSalaryGrade).WithMany().HasForeignKey(x => x.ToSalaryGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToSalaryStructure).WithMany().HasForeignKey(x => x.ToSalaryStructureId).OnDelete(DeleteBehavior.Restrict);
            builder.Property(x => x.NewBaseSalary).HasPrecision(18, 2);
            builder.HasIndex(x => x.DemotionNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveDate, x.DemotionStatus });
        }
    }
}
