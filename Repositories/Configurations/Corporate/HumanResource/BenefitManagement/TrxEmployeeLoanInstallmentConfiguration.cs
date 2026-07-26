using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxEmployeeLoanInstallmentConfiguration : IEntityTypeConfiguration<TrxEmployeeLoanInstallment>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeLoanInstallment> entity)
        {
            entity.ToTable("TrxEmployeeLoanInstallment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DueDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
            entity.Property(x => x.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(x => x.InterestAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.EmployeeLoan)
                .WithMany(x => x.Installments)
                .HasForeignKey(x => x.EmployeeLoanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PayrollPeriod)
                .WithMany()
                .HasForeignKey(x => x.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PayrollRunEmployee)
                .WithMany()
                .HasForeignKey(x => x.PayrollRunEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PayrollComponent)
                .WithMany()
                .HasForeignKey(x => x.PayrollComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PaidByUser)
                .WithMany()
                .HasForeignKey(x => x.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PostedByUser)
                .WithMany()
                .HasForeignKey(x => x.PostedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.EmployeeLoanId, x.InstallmentNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.DueDate, x.InstallmentStatus });

            entity.HasIndex(x => new { x.PayrollPeriodId, x.InstallmentStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeLoanInstallment> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
