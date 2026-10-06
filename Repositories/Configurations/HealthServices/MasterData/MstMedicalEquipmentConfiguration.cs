using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    /// <summary>
    /// Konfigurasi master jenis alat medis — <c>keperawatan</c> kontrak <c>0.6.0</c> kamus data 12.13
    /// (<c>BE-RWI-172</c>, migration <c>K8</c>).
    /// </summary>
    public class MstMedicalEquipmentConfiguration : IEntityTypeConfiguration<MstMedicalEquipment>
    {
        public void Configure(EntityTypeBuilder<MstMedicalEquipment> builder)
        {
            builder.ToTable("MstMedicalEquipment", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EquipmentCode)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.EquipmentName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.CategoryName).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.Property(x => x.ChargeUnit).HasConversion<int>();
            builder.Property(x => x.RoundingRule)
                .HasConversion<int>()
                .HasDefaultValue(MstEquipmentRoundingRule.CeilingWholeUnit);

            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.EquipmentCode).IsUnique();
            builder.HasIndex(x => x.EquipmentName);
            builder.HasIndex(x => x.IsActive);
        }
    }
}
