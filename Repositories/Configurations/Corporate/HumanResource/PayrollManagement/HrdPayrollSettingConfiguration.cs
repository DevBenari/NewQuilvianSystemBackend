using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class HrdPayrollSettingConfiguration : IEntityTypeConfiguration<HrdPayrollSetting>
    {
        public void Configure(EntityTypeBuilder<HrdPayrollSetting> builder)
        {
            builder.ToTable("HrdPayrollSetting", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SettingName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.BpjsKesehatanCapAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.BpjsKesehatanEmployeeRate)
                .HasPrecision(5, 2);

            builder.Property(x => x.BpjsKesehatanEmployerRate)
                .HasPrecision(5, 2);

            builder.Property(x => x.BpjsKetenagakerjaanJpCapAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.BpjsKetenagakerjaanJpEmployeeRate)
                .HasPrecision(5, 2);

            builder.Property(x => x.BpjsKetenagakerjaanJpEmployerRate)
                .HasPrecision(5, 2);

            builder.Property(x => x.BpjsKetenagakerjaanJhtEmployeeRate)
                .HasPrecision(5, 2);

            builder.Property(x => x.BpjsKetenagakerjaanJhtEmployerRate)
                .HasPrecision(5, 2);

            builder.Property(x => x.BpjsKetenagakerjaanJkkRate)
                .HasPrecision(5, 2);

            builder.Property(x => x.BpjsKetenagakerjaanJkmRate)
                .HasPrecision(5, 2);

            builder.Property(x => x.VarianceThresholdPercentage)
                .HasPrecision(5, 2);

            builder.Property(x => x.VarianceThresholdAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.DefaultNightShiftAllowanceAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.MaxLoanDeductionPercentage)
                .HasPrecision(5, 2);

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.HospitalSiteId)
                .HasDatabaseName("idx_hrd_payroll_setting_site");
        }
    }
}
