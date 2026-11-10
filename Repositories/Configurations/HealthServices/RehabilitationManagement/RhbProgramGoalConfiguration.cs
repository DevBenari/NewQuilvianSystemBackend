using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan tujuan yang dikejar program — <c>data-dictionary.md</c> bagian 2.10.</summary>
    public class RhbProgramGoalConfiguration : IEntityTypeConfiguration<RhbProgramGoal>
    {
        public void Configure(EntityTypeBuilder<RhbProgramGoal> builder)
        {
            builder.ToTable("RhbProgramGoal", "public");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ProgramId, x.GoalId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.GoalId);

            builder.HasOne(x => x.Program)
                .WithMany(x => x.Goals)
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Goal)
                .WithMany()
                .HasForeignKey(x => x.GoalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
