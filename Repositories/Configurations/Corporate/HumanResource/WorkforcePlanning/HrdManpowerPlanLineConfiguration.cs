using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdManpowerPlanLineConfiguration : IEntityTypeConfiguration<HrdManpowerPlanLine>
    {
        public void Configure(EntityTypeBuilder<HrdManpowerPlanLine> builder)
        {
            builder.ToTable("HrdManpowerPlanLine", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.CurrentFormationQuantity).HasPrecision(9, 2).HasDefaultValue(0m);
            builder.Property(x => x.RecommendedQuantity).HasPrecision(9, 2);
            builder.Property(x => x.RecommendationBreakdown).HasMaxLength(1000);
            builder.Property(x => x.ProposedFormationQuantity).HasPrecision(9, 2);
            builder.Property(x => x.ChangeQuantity).HasPrecision(9, 2).HasDefaultValue(0m);
            builder.Property(x => x.DeviationReason).HasMaxLength(1000);
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.MonthlyRateSnapshot).HasPrecision(18, 2);
            builder.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            builder.Property(x => x.IncrementalCost).HasPrecision(18, 2);

            builder.HasOne(x => x.Plan)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SalaryGrade)
                .WithMany()
                .HasForeignKey(x => x.SalaryGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedFormation)
                .WithMany()
                .HasForeignKey(x => x.CreatedFormationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PlanId, x.OrganizationUnitId, x.PositionId })
                .IsUnique()
                .HasDatabaseName("IX_HrdManpowerPlanLine_Plan_Unit_Position");

            builder.HasIndex(x => x.OrganizationUnitId);
        }
    }
}
