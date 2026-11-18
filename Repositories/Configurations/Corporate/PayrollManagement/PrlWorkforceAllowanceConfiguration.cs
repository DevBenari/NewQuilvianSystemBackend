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
            entity.ToTable("PrlWorkforceAllowance", "public");
            entity.HasKey(x => x.Id);

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

            // Pembacaan per pegawai (layar Tunjangan Pegawai) dan pemeriksaan tumpang-tindih per pegawai + jenis. Tumpang-tindih tanggal
            // tidak dapat diekspresikan sebagai unique index; dijaga transaksi Serializable + advisory lock di layanan.
            entity.HasIndex(x => new { x.WorkforceProfileId, x.AllowanceTypeId, x.IsDelete });
            // Mesin hitung payroll memuat penetapan Approved yang berlaku di periode untuk banyak pegawai sekaligus.
            entity.HasIndex(x => new { x.AllowanceStatus, x.IsActive, x.EffectiveStartDate, x.EffectiveEndDate, x.IsDelete });
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
