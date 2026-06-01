using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpPayrollConfiguration : IEntityTypeConfiguration<WfpPayroll>
    {
        public void Configure(EntityTypeBuilder<WfpPayroll> entity)
        {
            entity.ToTable("WfpPayroll", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.PayrollGroup)
                .HasMaxLength(50)
                .HasDefaultValue("Default");

            entity.Property(x => x.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("BankTransfer");

            entity.Property(x => x.BasicSalary)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.FixedAllowance)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.FixedDeduction)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.IsOvertimeEligible)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPayrollActive)
                .HasDefaultValue(true);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired();

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
                .WithOne(x => x.Payroll)
                .HasForeignKey<WfpPayroll>(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryBankAccount)
                .WithMany()
                .HasForeignKey(x => x.PrimaryBankAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PrimaryBankAccountId);

            entity.HasIndex(x => new
            {
                x.PayrollGroup,
                x.IsPayrollActive,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
