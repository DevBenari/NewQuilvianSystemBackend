using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
    {
        public void Configure(EntityTypeBuilder<LabOrder> entity)
        {
            entity.ToTable("LabOrder", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.ProcedureId)
                .IsRequired();

            // RJ-BIL-BE-003: siklus hidup operasional pesanan laboratorium.
            entity.Property(x => x.OrderStatus)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.StatusBeforeHold)
                .HasConversion<int>();

            // LAB-DEC-025: disiplin pesanan, boleh kosong hanya untuk pesanan yang sudah ada
            // sebelum kolom ini dibuat.
            //
            // INV-21: disiplin tidak berpindah setelah pesanan dibuat. Ketiadaan endpoint yang
            // mengubahnya belum cukup menjadi jaminan — siapa pun yang kelak menulis jalur ubah
            // baru akan ditolak di sini, bukan diam-diam berhasil.
            entity.Property(x => x.Discipline)
                .HasConversion<int>()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

            // Dua petugas yang memindahkan status pesanan yang sama secara bersamaan tidak
            // boleh sama-sama berhasil.
            entity.Property(x => x.Version)
                .IsConcurrencyToken();

            entity.HasIndex(x => x.EncounterId);

            entity.HasIndex(x => x.ProcedureId);

            entity.HasIndex(x => x.OrderStatus);

            // Daftar pantau per disiplin (S15) menyaring tepat pada kolom ini.
            entity.HasIndex(x => x.Discipline);

            entity.HasMany(x => x.Specimens)
                .WithOne(x => x.LabOrder)
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
