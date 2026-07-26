using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class WfpTransportAllowanceTransactionConfiguration : IEntityTypeConfiguration<WfpTransportAllowanceTransaction>
    {
        public void Configure(EntityTypeBuilder<WfpTransportAllowanceTransaction> entity)
        {

            entity.ToTable("WfpTransportAllowanceTransaction", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TransactionNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TransactionDate).HasColumnType("date");
            entity.Property(x => x.TransactionType).HasMaxLength(30).HasDefaultValue("Accrual").IsRequired();
            entity.Property(x => x.TransactionStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.Rate).HasPrecision(18, 2);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.BalanceAfterTransaction).HasPrecision(18, 2);
            entity.Property(x => x.SourceType).HasMaxLength(50);
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.TransportAllowance).WithMany(x => x.Transactions).HasForeignKey(x => x.TransportAllowanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollRunEmployee).WithMany().HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TransactionNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.TransportAllowanceId, x.TransactionDate, x.TransactionType, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPeriodId, x.TransactionStatus, x.IsDelete });
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
