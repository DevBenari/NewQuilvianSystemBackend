using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstWorkforceRequirementConfiguration : IEntityTypeConfiguration<MstWorkforceRequirement>
    {
        public void Configure(EntityTypeBuilder<MstWorkforceRequirement> entity)
        {
            entity.ToTable("MstWorkforceRequirement", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserType)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.RequirementCategory)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.RequirementCode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.RequirementName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.IsRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsMultipleAllowed)
                .HasDefaultValue(false);

            entity.Property(x => x.IsFileRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNumberRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsIssueDateRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsExpiredDateRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsVerificationRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsProfileRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.TargetEntityName)
                .HasMaxLength(100);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

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

            entity.HasIndex(x => new
            {
                x.UserType,
                x.RequirementCategory,
                x.RequirementCode,
                x.TargetEntityName
            })
            .IsUnique();

            entity.HasIndex(x => new
            {
                x.UserType,
                x.RequirementCategory,
                x.TargetEntityName,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
