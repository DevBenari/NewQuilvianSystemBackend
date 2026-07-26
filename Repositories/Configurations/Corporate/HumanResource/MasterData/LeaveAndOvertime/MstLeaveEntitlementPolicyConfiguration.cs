using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstLeaveEntitlementPolicyConfiguration : IEntityTypeConfiguration<MstLeaveEntitlementPolicy>
    {
        public void Configure(EntityTypeBuilder<MstLeaveEntitlementPolicy> entity)
        {
            entity.ToTable("MstLeaveEntitlementPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.LeavePolicyId).IsRequired();
            entity.Property(x => x.EntitlementPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntitlementPolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.EntitlementMethod).HasMaxLength(50).HasDefaultValue("AnnualGrant").IsRequired();
            entity.Property(x => x.AnnualEntitlementDays).HasPrecision(8, 2).HasDefaultValue(0m);
            entity.Property(x => x.AccrualFrequency).HasMaxLength(50).HasDefaultValue("Annual").IsRequired();
            entity.Property(x => x.AccrualAmountDays).HasPrecision(8, 2).HasDefaultValue(0m);
            entity.Property(x => x.IsProratedOnJoin).HasDefaultValue(true);
            entity.Property(x => x.IsProratedOnSeparation).HasDefaultValue(true);
            entity.Property(x => x.MinimumServiceMonths).HasDefaultValue(0);
            entity.Property(x => x.MaximumBalanceDays).HasPrecision(8, 2).IsRequired(false);
            entity.Property(x => x.RoundingMethod).HasMaxLength(50).HasDefaultValue("None").IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeavePolicy)
                .WithMany(x => x.EntitlementPolicies)
                .HasForeignKey(x => x.LeavePolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EntitlementPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.EntitlementPolicyName);
            entity.HasIndex(x => x.LeavePolicyId);
            entity.HasIndex(x => new { x.LeavePolicyId, x.EntitlementMethod, x.AccrualFrequency, x.IsDefault, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
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
        }
    }
}
