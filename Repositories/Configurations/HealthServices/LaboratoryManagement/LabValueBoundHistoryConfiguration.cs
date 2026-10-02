using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabValueBoundHistoryConfiguration : IEntityTypeConfiguration<LabValueBoundHistory>
    {
        public void Configure(EntityTypeBuilder<LabValueBoundHistory> builder)
        {
            builder.ToTable("LabValueBoundHistory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ValueBoundId).IsRequired();
            builder.Property(x => x.ChangedField).HasMaxLength(100).IsRequired();
            builder.Property(x => x.OldValue).HasMaxLength(200);
            builder.Property(x => x.NewValue).HasMaxLength(200);
            builder.Property(x => x.ChangeReason).HasMaxLength(1000);
            builder.Property(x => x.ActorUserId).IsRequired();

            // AC-34 menuntut riwayat yang permanen. Ketiadaan endpoint yang mengubah belum cukup
            // menjadi jaminan: siapa pun yang kelak menulis jalur ubah baru akan ditolak di sini,
            // bukan diam-diam berhasil. Riwayat yang dapat diubah bukan riwayat.
            //
            // Yang dikunci hanya kolom faktanya. Kolom audit bawaan tidak disentuh, sehingga
            // perilaku soft delete dan jejak audit repository tetap berjalan seperti biasa.
            foreach (var nama in new[]
            {
                nameof(LabValueBoundHistory.ValueBoundId),
                nameof(LabValueBoundHistory.ChangedField),
                nameof(LabValueBoundHistory.OldValue),
                nameof(LabValueBoundHistory.NewValue),
                nameof(LabValueBoundHistory.ActorUserId),
                nameof(LabValueBoundHistory.ApprovedByUserId),
                nameof(LabValueBoundHistory.ChangeReason),
                nameof(LabValueBoundHistory.OccurredAt)
            })
            {
                builder.Property(nama).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            }

            // Riwayat satu batas nilai hampir selalu dibaca berurutan waktu.
            builder.HasIndex(x => new { x.ValueBoundId, x.OccurredAt });

            builder.HasOne(x => x.ValueBound)
                .WithMany()
                .HasForeignKey(x => x.ValueBoundId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
