using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    public class TrxCompensatoryTimeOffConfiguration : IEntityTypeConfiguration<TrxCompensatoryTimeOff>
    {
        public void Configure(EntityTypeBuilder<TrxCompensatoryTimeOff> entity)
        {
            entity.ToTable("TrxCompensatoryTimeOff", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreditNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EarnedDate).HasColumnType("date");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.ExpiryDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ConversionRate).HasPrecision(10, 4);
            entity.Property(x => x.CompensatoryStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.GeneratedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ExpiredAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimeRequest).WithMany(x => x.CompensatoryTimeOffs).HasForeignKey(x => x.OvertimeRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimeRealization).WithMany(x => x.CompensatoryTimeOffs).HasForeignKey(x => x.OvertimeRealizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimeVerification).WithMany(x => x.CompensatoryTimeOffs).HasForeignKey(x => x.OvertimeVerificationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveBalanceTransaction).WithMany().HasForeignKey(x => x.LeaveBalanceTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CreditNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.CompensatoryStatus, x.ExpiryDate, x.IsDelete });
            entity.HasIndex(x => new { x.OvertimeRealizationId, x.IsDelete });
        }
    }
}
