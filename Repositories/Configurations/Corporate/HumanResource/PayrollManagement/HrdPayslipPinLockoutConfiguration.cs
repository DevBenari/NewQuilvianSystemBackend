using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class HrdPayslipPinLockoutConfiguration : IEntityTypeConfiguration<HrdPayslipPinLockout>
    {
        public void Configure(EntityTypeBuilder<HrdPayslipPinLockout> builder)
        {
            builder.ToTable("HrdPayslipPinLockout", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LockoutExpiresAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.LastFailedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            // Satu penghitung per pegawai.
            builder.HasIndex(x => x.EmployeeKey)
                .IsUnique()
                .HasDatabaseName("uq_payslip_pin_lockout_employee");
        }
    }
}
