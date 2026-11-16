using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstJobDescriptionConfiguration : IEntityTypeConfiguration<MstJobDescription>
    {
        public void Configure(EntityTypeBuilder<MstJobDescription> entity)
        {
            entity.ToTable("MstJobDescription", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PositionId)
                .IsRequired();

            entity.Property(x => x.JobDescriptionCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.VersionNumber)
                .IsRequired();

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Draft")
                .IsRequired();

            entity.Property(x => x.Purpose)
                .HasMaxLength(2000)
                .IsRequired();

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
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.JobDescription)
                .HasForeignKey(x => x.JobDescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.PositionId, x.VersionNumber })
                .IsUnique();

            entity.HasIndex(x => x.JobDescriptionCode)
                .IsUnique();

            entity.HasIndex(x => new { x.PositionId, x.Status });
        }
    }
}
