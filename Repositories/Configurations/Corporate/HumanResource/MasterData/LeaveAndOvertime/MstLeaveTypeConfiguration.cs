using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstLeaveTypeConfiguration : IEntityTypeConfiguration<MstLeaveType>
    {
        public void Configure(EntityTypeBuilder<MstLeaveType> entity)
        {
            entity.ToTable("MstLeaveType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.LeaveTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LeaveTypeName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.LeaveCategory).HasMaxLength(50).HasDefaultValue("Annual").IsRequired();
            entity.Property(x => x.IsPaidLeave).HasDefaultValue(true);
            entity.Property(x => x.IsBalanceDeducted).HasDefaultValue(true);
            entity.Property(x => x.AllowHalfDay).HasDefaultValue(false);
            entity.Property(x => x.AllowHourly).HasDefaultValue(false);
            entity.Property(x => x.RequiresAttachment).HasDefaultValue(false);
            entity.Property(x => x.RequiresMedicalCertificate).HasDefaultValue(false);
            entity.Property(x => x.DefaultMinimumNoticeDays).HasDefaultValue(0);
            entity.Property(x => x.ColorCode).HasMaxLength(20);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasIndex(x => x.LeaveTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.LeaveTypeName);
            entity.HasIndex(x => new
            {
                x.LeaveCategory,
                x.IsPaidLeave,
                x.IsBalanceDeducted,
                x.IsActive,
                x.IsDelete
            });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstLeaveType> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
