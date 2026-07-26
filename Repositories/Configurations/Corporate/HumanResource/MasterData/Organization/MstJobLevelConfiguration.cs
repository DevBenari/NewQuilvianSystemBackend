using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstJobLevelConfiguration : IEntityTypeConfiguration<MstJobLevel>
    {
        public void Configure(EntityTypeBuilder<MstJobLevel> entity)
        {
            entity.ToTable("MstJobLevel", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JobLevelCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.JobLevelName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.LevelOrder)
                .HasDefaultValue(0);

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

            entity.HasIndex(x => x.JobLevelCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.JobLevelName);

            entity.HasIndex(x => new { x.LevelOrder, x.IsActive, x.IsDelete });
        }
    }
}
