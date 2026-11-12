using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class HrdPayslipPinSessionConfiguration : IEntityTypeConfiguration<HrdPayslipPinSession>
    {
        public void Configure(EntityTypeBuilder<HrdPayslipPinSession> builder)
        {
            builder.ToTable("HrdPayslipPinSession", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            builder.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.HasIndex(x => x.TokenHash)
                .IsUnique()
                .HasDatabaseName("uq_payslip_pin_session_token");
            builder.HasIndex(x => new { x.EmployeeKey, x.ExpiresAt });
        }
    }
}
