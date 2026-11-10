using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdRecredentialingTierOverrideConfiguration : IEntityTypeConfiguration<HrdRecredentialingTierOverride>
    {
        public void Configure(EntityTypeBuilder<HrdRecredentialingTierOverride> entity)
        {
            entity.ToTable("HrdRecredentialingTierOverride", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CalculatedTier).HasMaxLength(40).IsRequired();
            entity.Property(x => x.NewTier).HasMaxLength(40).IsRequired();
            entity.Property(x => x.JustificationReason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.ChairmanNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.OverriddenAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasIndex(x => new { x.WorkforceProfileId, x.IsActive });
            entity.HasIndex(x => new { x.WorkforceProfileId, x.OverriddenAt });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdRecredentialingTierOverride> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
