using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollOvertimeInputConfiguration : IEntityTypeConfiguration<TrxPayrollOvertimeInput>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollOvertimeInput> entity)
        {

            entity.ToTable("TrxPayrollOvertimeInput", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OvertimeDate).HasColumnType("date");
            entity.Property(x => x.OvertimeStatusSnapshot).HasMaxLength(30).HasDefaultValue("Verified").IsRequired();
            entity.Property(x => x.RateMultiplier).HasPrecision(9, 4);
            entity.Property(x => x.HourlyRate).HasPrecision(18, 2);
            entity.Property(x => x.OvertimeAmount).HasPrecision(18, 2);
            entity.Property(x => x.CalculationSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ImportedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRunEmployee).WithMany(x => x.OvertimeInputs).HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimeRealization).WithMany().HasForeignKey(x => x.OvertimeRealizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimeRequest).WithMany().HasForeignKey(x => x.OvertimeRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ImportedByUser).WithMany().HasForeignKey(x => x.ImportedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PayrollRunEmployeeId, x.OvertimeRealizationId }).IsUnique().HasFilter("\"OvertimeRealizationId\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => new { x.PayrollRunEmployeeId, x.OvertimeDate, x.IsDelete });
            entity.HasIndex(x => new { x.OvertimeStatusSnapshot, x.OvertimeDate, x.IsDelete });
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
