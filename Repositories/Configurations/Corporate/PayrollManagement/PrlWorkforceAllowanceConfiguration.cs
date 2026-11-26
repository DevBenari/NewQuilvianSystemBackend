using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.PayrollManagement
{
    public class PrlWorkforceAllowanceConfiguration : IEntityTypeConfiguration<PrlWorkforceAllowance>
    {
        public void Configure(EntityTypeBuilder<PrlWorkforceAllowance> entity)
        {
            // Nama check constraint sama persis dengan DDL (laporan PAY-BE-FIX-021).
            entity.ToTable("PrlWorkforceAllowance", "public", table =>
            {
                table.HasCheckConstraint("CK_PrlWorkforceAllowance_AssignmentSource", "\"AssignmentSource\" IN ('Direct','Policy')");
                table.HasCheckConstraint("CK_PrlWorkforceAllowance_PolicySourceHasLinks",
                    "\"AssignmentSource\" <> 'Policy' OR (\"AllowancePolicyId\" IS NOT NULL AND \"AllowancePolicyGradeRateId\" IS NOT NULL AND \"SalaryGradeId\" IS NOT NULL)");
            });
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssignmentSource).HasMaxLength(20).HasDefaultValue(WorkforceAllowanceAssignmentSources.Direct).IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Percentage).HasPrecision(7, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.MaximumAmount).HasPrecision(18, 2);
            entity.Property(x => x.IsTaxable).HasDefaultValue(true);
            entity.Property(x => x.IsProrated).HasDefaultValue(true);
            entity.Property(x => x.RequiresApproval).HasDefaultValue(false);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AllowanceStatus).HasMaxLength(30).HasDefaultValue(WorkforceAllowanceStatuses.Draft).IsRequired();
            entity.Property(x => x.StatusReason).HasMaxLength(500);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AllowanceType).WithMany().HasForeignKey(x => x.AllowanceTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AllowancePolicy).WithMany().HasForeignKey(x => x.AllowancePolicyId)
                .HasConstraintName("FK_PrlWorkforceAllowance_AllowancePolicy").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AllowancePolicyGradeRate).WithMany().HasForeignKey(x => x.AllowancePolicyGradeRateId)
                .HasConstraintName("FK_PrlWorkforceAllowance_AllowancePolicyGradeRate").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId)
                .HasConstraintName("FK_PrlWorkforceAllowance_SalaryGrade").OnDelete(DeleteBehavior.Restrict);

            // Pembacaan per pegawai (layar Tunjangan Pegawai) dan pemeriksaan tumpang-tindih per pegawai + jenis. Tumpang-tindih tanggal
            // tidak dapat diekspresikan sebagai unique index; dijaga transaksi Serializable + advisory lock di layanan.
            entity.HasIndex(x => new { x.WorkforceProfileId, x.AllowanceTypeId, x.IsDelete });
            // Mesin hitung payroll memuat penetapan Approved yang berlaku di periode untuk banyak pegawai sekaligus.
            entity.HasIndex(x => new { x.AllowanceStatus, x.IsActive, x.EffectiveStartDate, x.EffectiveEndDate, x.IsDelete });
            entity.HasIndex(x => x.AllowancePolicyId).HasDatabaseName("IX_PrlWorkforceAllowance_AllowancePolicyId");
            entity.HasIndex(x => x.AllowancePolicyGradeRateId).HasDatabaseName("IX_PrlWorkforceAllowance_AllowancePolicyGradeRateId");
            entity.HasIndex(x => x.SalaryGradeId).HasDatabaseName("IX_PrlWorkforceAllowance_SalaryGradeId");
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
