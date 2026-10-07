using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    public class MstInpatientSettingConfiguration : IEntityTypeConfiguration<MstInpatientSetting>
    {

        public void Configure(EntityTypeBuilder<MstInpatientSetting> builder)
        {
            builder.ToTable("MstInpatientSetting", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.EpisodeNumberPrefix)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Notes).HasMaxLength(1000);

            // Baris pengaturan yang sudah ada di database dibuat jauh sebelum kolom ini
            // lahir. Tanpa nilai bawaan di sisi database, migration mengisinya 0 dan
            // pengingat kekurangan deposit menyala setiap hari tanpa ada yang memintanya.
            builder.Property(x => x.DepositFollowUpIntervalDays).HasDefaultValue(3);

            // BE-RWI-172 / E4 — dua ambang Daftar Pantau Finishing. Baris yang sudah ada
            // mendapat nilai bawaan dari database, bukan 0 yang membuat daftar langsung penuh.
            builder.Property(x => x.PendingSurgicalHandoverAlertMinutes).HasDefaultValue(60);
            builder.Property(x => x.PendingAdmissionReferralAlertMinutes).HasDefaultValue(30);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.IsActive, x.IsDefault });
        }
    }
}
