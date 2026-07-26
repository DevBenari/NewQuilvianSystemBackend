using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class WfpTransportAllowanceConfiguration : IEntityTypeConfiguration<WfpTransportAllowance>
    {
        public void Configure(EntityTypeBuilder<WfpTransportAllowance> entity)
        {

            entity.ToTable("WfpTransportAllowance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AllowanceStatus).HasMaxLength(30).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.MonthlyAmount).HasPrecision(18, 2);
            entity.Property(x => x.PerAttendanceAmount).HasPrecision(18, 2);
            entity.Property(x => x.MaximumMonthlyAmount).HasPrecision(18, 2);
            entity.Property(x => x.AccruedAmount).HasPrecision(18, 2);
            entity.Property(x => x.UsedAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.RemainingAmount).HasPrecision(18, 2);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TransportAllowancePolicy).WithMany(x => x.TransportAllowances).HasForeignKey(x => x.TransportAllowancePolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.TransportAllowancePolicyId, x.AllowanceStatus, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsDelete });
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
