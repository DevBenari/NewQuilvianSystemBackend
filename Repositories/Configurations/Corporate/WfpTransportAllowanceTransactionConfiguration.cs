using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpTransportAllowanceTransactionConfiguration : IEntityTypeConfiguration<WfpTransportAllowanceTransaction>
    {
        public void Configure(EntityTypeBuilder<WfpTransportAllowanceTransaction> entity)
        {
            entity.ToTable("WfpTransportAllowanceTransaction", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.TransactionDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.PeriodYearMonth)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.AllowanceType)
                .HasMaxLength(50)
                .HasDefaultValue("Regular")
                .IsRequired();

            entity.Property(x => x.Amount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.IsGeneratedFromAttendance)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNightShift)
                .HasDefaultValue(false);

            entity.Property(x => x.TransactionStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Draft")
                .IsRequired();

            entity.Property(x => x.Notes)
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
                .WithMany(x => x.TransportAllowanceTransactions)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TransportAllowance)
                .WithMany()
                .HasForeignKey(x => x.TransportAllowanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TransportAllowancePolicy)
                .WithMany()
                .HasForeignKey(x => x.TransportAllowancePolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Attendance)
                .WithMany()
                .HasForeignKey(x => x.AttendanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.TransportAllowanceId);

            entity.HasIndex(x => x.TransportAllowancePolicyId);

            entity.HasIndex(x => x.AttendanceId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.PeriodYearMonth,
                x.AllowanceType,
                x.TransactionStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.TransactionDate,
                x.TransactionStatus,
                x.IsDelete
            });

            entity.HasIndex(x => x.IsNightShift);
        }
    }
}
