using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpTransportAllowanceConfiguration : IEntityTypeConfiguration<WfpTransportAllowance>
    {
        public void Configure(EntityTypeBuilder<WfpTransportAllowance> entity)
        {
            entity.ToTable("WfpTransportAllowance", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.AllowanceMode)
                .HasMaxLength(50)
                .HasDefaultValue("None")
                .IsRequired();

            entity.Property(x => x.IsEligible)
                .HasDefaultValue(false);

            entity.Property(x => x.IsRegularTransportEligible)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNightTransportEligible)
                .HasDefaultValue(false);

            entity.Property(x => x.MonthlyAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.DailyAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.NightAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.IsProrated)
                .HasDefaultValue(true);

            entity.Property(x => x.IsTaxable)
                .HasDefaultValue(true);

            entity.Property(x => x.IsPayrollComponent)
                .HasDefaultValue(true);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

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

            entity.HasOne(x => x.WorkforceProfile)
                .WithOne(x => x.TransportAllowance)
                .HasForeignKey<WfpTransportAllowance>(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TransportAllowancePolicy)
                .WithMany()
                .HasForeignKey(x => x.TransportAllowancePolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.TransportAllowancePolicyId);

            entity.HasIndex(x => new
            {
                x.AllowanceMode,
                x.IsEligible,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => x.IsNightTransportEligible);
        }
    }
}
