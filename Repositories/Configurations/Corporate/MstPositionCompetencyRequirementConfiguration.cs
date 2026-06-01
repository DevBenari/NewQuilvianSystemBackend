using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstPositionCompetencyRequirementConfiguration : IEntityTypeConfiguration<MstPositionCompetencyRequirement>
    {
        public void Configure(EntityTypeBuilder<MstPositionCompetencyRequirement> entity)
        {
            entity.ToTable("MstPositionCompetencyRequirement", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PositionId)
                .IsRequired();

            entity.Property(x => x.CompetencyId)
                .IsRequired();

            entity.Property(x => x.IsRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.MinimumLevel)
                .HasConversion<int>()
                .HasDefaultValue(CompetencyLevel.Basic)
                .IsRequired();

            entity.Property(x => x.IsCertificationRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsTrainingRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.Position)
                .WithMany(x => x.CompetencyRequirements)
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Competency)
                .WithMany(x => x.PositionRequirements)
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PositionId);

            entity.HasIndex(x => x.CompetencyId);

            entity.HasIndex(x => new
            {
                x.PositionId,
                x.CompetencyId
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PositionId,
                x.IsRequired,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.CompetencyId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.MinimumLevel,
                x.IsCertificationRequired,
                x.IsTrainingRequired
            });
        }
    }
}
