using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollEmployeeComponentConfiguration : IEntityTypeConfiguration<TrxPayrollEmployeeComponent>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollEmployeeComponent> entity)
        {

            entity.ToTable("TrxPayrollEmployeeComponent", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SourceType).HasMaxLength(50).HasDefaultValue("Master").IsRequired();
            entity.Property(x => x.ComponentCodeSnapshot).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ComponentNameSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ComponentTypeSnapshot).HasMaxLength(30).HasDefaultValue("Earning").IsRequired();
            entity.Property(x => x.CalculationMethodSnapshot).HasMaxLength(30).HasDefaultValue("Fixed").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.Rate).HasPrecision(18, 2);
            entity.Property(x => x.Percentage).HasPrecision(9, 4);
            entity.Property(x => x.BaseAmount).HasPrecision(18, 2);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.EmployerAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxableAmount).HasPrecision(18, 2);
            entity.Property(x => x.FormulaSnapshot).HasMaxLength(2000);
            entity.Property(x => x.CalculationDetailJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRunEmployee).WithMany(x => x.Components).HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PayrollRunEmployeeId, x.ComponentCodeSnapshot, x.SourceType, x.SourceId, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollComponentId, x.ComponentTypeSnapshot, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollRunEmployeeId, x.SortOrder, x.IsDelete });
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
