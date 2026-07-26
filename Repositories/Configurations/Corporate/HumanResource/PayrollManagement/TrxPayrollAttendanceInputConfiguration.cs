using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollAttendanceInputConfiguration : IEntityTypeConfiguration<TrxPayrollAttendanceInput>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollAttendanceInput> entity)
        {

            entity.ToTable("TrxPayrollAttendanceInput", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AttendanceDate).HasColumnType("date");
            entity.Property(x => x.AttendanceStatusSnapshot).HasMaxLength(30).HasDefaultValue("Present").IsRequired();
            entity.Property(x => x.PaidLeaveDays).HasPrecision(9, 2);
            entity.Property(x => x.UnpaidLeaveDays).HasPrecision(9, 2);
            entity.Property(x => x.AbsentDays).HasPrecision(9, 2);
            entity.Property(x => x.AttendanceAllowanceAmount).HasPrecision(18, 2);
            entity.Property(x => x.AttendanceDeductionAmount).HasPrecision(18, 2);
            entity.Property(x => x.AttendanceSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ImportedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRunEmployee).WithMany(x => x.AttendanceInputs).HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ImportedByUser).WithMany().HasForeignKey(x => x.ImportedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PayrollRunEmployeeId, x.AttendanceDate }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.AttendanceDailyId, x.IsCorrectionApplied, x.IsDelete });
            entity.HasIndex(x => new { x.AttendanceStatusSnapshot, x.AttendanceDate, x.IsDelete });
        }

        private static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
            where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
