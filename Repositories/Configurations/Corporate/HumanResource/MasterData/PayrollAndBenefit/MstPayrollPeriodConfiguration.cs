using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstPayrollPeriodConfiguration : IEntityTypeConfiguration<MstPayrollPeriod>
    {
        public void Configure(EntityTypeBuilder<MstPayrollPeriod> entity)
        {
            entity.ToTable("MstPayrollPeriod", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PayrollPeriodCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PayrollPeriodName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PeriodType).HasMaxLength(50).HasDefaultValue("Monthly").IsRequired();
            entity.Property(x => x.FiscalYear).IsRequired();
            entity.Property(x => x.PeriodNumber).IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.EndDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.AttendanceCutoffDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.VariableInputCutoffDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ApprovalDueDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.PaymentDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.PayrollPeriodStatus).HasMaxLength(50).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.IsLocked).HasDefaultValue(false);
            entity.Property(x => x.LockedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PayrollPeriodCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PayrollPeriodName);
            entity.HasIndex(x => new { x.FiscalYear, x.PeriodNumber, x.PeriodType });
            entity.HasIndex(x => new { x.StartDate, x.EndDate });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.FiscalYear, x.PeriodNumber });
            entity.HasIndex(x => new { x.PayrollPeriodStatus, x.IsLocked, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
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
        }
    }
}
