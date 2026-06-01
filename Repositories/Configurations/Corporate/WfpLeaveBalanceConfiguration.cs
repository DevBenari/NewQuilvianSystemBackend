using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpLeaveBalanceConfiguration : IEntityTypeConfiguration<WfpLeaveBalance>
    {
        public void Configure(EntityTypeBuilder<WfpLeaveBalance> entity)
        {
            entity.ToTable("WfpLeaveBalance", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.LeaveYear)
                .IsRequired();

            entity.Property(x => x.LeaveType)
                .HasConversion<int>()
                .HasDefaultValue(LeaveType.AnnualLeave)
                .IsRequired();

            entity.Property(x => x.OpeningBalance)
                .HasColumnType("numeric(6,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.EntitledDays)
                .HasColumnType("numeric(6,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.UsedDays)
                .HasColumnType("numeric(6,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.PendingDays)
                .HasColumnType("numeric(6,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.RemainingDays)
                .HasColumnType("numeric(6,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

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
                .WithMany(x => x.LeaveBalances)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.LeaveYear,
                x.LeaveType
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.LeaveYear,
                x.LeaveType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.LeaveYear,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
