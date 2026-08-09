using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class SysAppVersionBuildConfiguration : IEntityTypeConfiguration<SysAppVersionBuild>
    {
        public void Configure(EntityTypeBuilder<SysAppVersionBuild> entity)
        {
            entity.ToTable("SysAppVersionBuild", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.BuildVersion)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CommitSha)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.CommitMessage)
                .HasMaxLength(500);

            entity.Property(x => x.BranchName)
                .HasMaxLength(200);

            entity.Property(x => x.BuildDateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.CreateDateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.AppVersion)
                .WithMany(x => x.Builds)
                .HasForeignKey(x => x.AppVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.AppVersionId, x.BuildVersion })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.AppVersionId, x.CommitSha })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.BuildNumber);
            entity.HasIndex(x => x.BuildDateTime);
        }
    }
}
