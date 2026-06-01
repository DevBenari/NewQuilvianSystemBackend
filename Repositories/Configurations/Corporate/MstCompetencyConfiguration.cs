using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstCompetencyConfiguration : IEntityTypeConfiguration<MstCompetency>
    {
        public void Configure(EntityTypeBuilder<MstCompetency> entity)
        {
            entity.ToTable("MstCompetency", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CompetencyCode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CompetencyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.CompetencyCategory)
                .HasConversion<int>()
                .HasDefaultValue(CompetencyCategory.Other)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

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

            entity.HasIndex(x => x.CompetencyCode)
                .IsUnique();

            entity.HasIndex(x => x.CompetencyName);

            entity.HasIndex(x => x.CompetencyCategory);

            entity.HasIndex(x => new
            {
                x.CompetencyCategory,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
