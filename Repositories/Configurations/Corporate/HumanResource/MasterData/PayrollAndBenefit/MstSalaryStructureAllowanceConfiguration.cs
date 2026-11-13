using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstSalaryStructureAllowanceConfiguration : IEntityTypeConfiguration<MstSalaryStructureAllowance>
    {
        public void Configure(EntityTypeBuilder<MstSalaryStructureAllowance> entity)
        {
            entity.ToTable("MstSalaryStructureAllowance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SalaryStructureId).IsRequired();
            entity.Property(x => x.AllowanceTypeId).IsRequired();
            entity.Property(x => x.CalculationMethod)
                .HasMaxLength(30)
                .HasDefaultValue(SalaryStructureAllowanceCalculationMethods.FixedAmount)
                .IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.Percentage).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.SalaryStructure)
                .WithMany()
                .HasForeignKey(x => x.SalaryStructureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AllowanceType)
                .WithMany()
                .HasForeignKey(x => x.AllowanceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu jenis tunjangan paling banyak satu baris aktif per struktur (validasi SA-04).
            entity.HasIndex(x => new { x.SalaryStructureId, x.AllowanceTypeId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.AllowanceTypeId);
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
