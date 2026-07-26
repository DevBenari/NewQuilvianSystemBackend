using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxMedicalServiceFeeCalculationConfiguration : IEntityTypeConfiguration<TrxMedicalServiceFeeCalculation>
    {
        public void Configure(EntityTypeBuilder<TrxMedicalServiceFeeCalculation> entity)
        {

            entity.ToTable("TrxMedicalServiceFeeCalculation", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CalculationNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ServicePeriodStartDate).HasColumnType("date");
            entity.Property(x => x.ServicePeriodEndDate).HasColumnType("date");
            entity.Property(x => x.CalculationStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.GrossServiceAmount).HasPrecision(18, 2);
            entity.Property(x => x.FeePercentage).HasPrecision(9, 4);
            entity.Property(x => x.GrossFeeAmount).HasPrecision(18, 2);
            entity.Property(x => x.DeductionAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.NetFeeAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.SourceSummaryJson).HasColumnType("jsonb");
            entity.Property(x => x.CalculationDetailJson).HasColumnType("jsonb");
            entity.Property(x => x.CalculatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedToPayrollAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollRun).WithMany().HasForeignKey(x => x.PayrollRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollRunEmployee).WithMany().HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CalculatedByUser).WithMany().HasForeignKey(x => x.CalculatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CalculationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.ServicePeriodStartDate, x.ServicePeriodEndDate, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPeriodId, x.CalculationStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollRunEmployeeId, x.CalculationStatus, x.IsDelete });
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
