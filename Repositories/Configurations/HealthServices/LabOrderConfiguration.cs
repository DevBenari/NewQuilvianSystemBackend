using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
    {
        public void Configure(EntityTypeBuilder<LabOrder> entity)
        {
            entity.ToTable("LabOrder", "public");

            entity.HasKey(x => x.Id);

            // LAB-DEC-072: nomor pesanan yang dapat disebut manusia.
            //
            // Wajib, dan nilainya tidak berpindah sesudah pesanan tersimpan. Nomor ini dicetak
            // pada amplop hasil pasien; mengubahnya berarti dokumen fisik yang beredar tidak lagi
            // menunjuk pesanan yang benar. Seperti Discipline, larangannya ditegakkan di sini —
            // bukan hanya lewat ketiadaan endpoint yang mengubahnya.
            entity.Property(x => x.OrderNumber)
                .HasMaxLength(32)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

            // Jaring pengaman terakhir alokasi nomor. Kunci advisory mengurangi tabrakan; index
            // inilah yang membuat dua pesanan bernomor sama menjadi mustahil.
            entity.HasIndex(x => x.OrderNumber)
                .IsUnique();

            entity.Property(x => x.LabReportNumber)
                .HasMaxLength(32);

            // Unik atas (Discipline, LabReportNumber) — dan tahunnya ikut terjaga karena ia
            // TERKANDUNG di dalam nomornya sendiri: 26-1129 nol dapat bertabrakan dengan
            // 27-1129. Menambah kolom tahun tersendiri hanya menyimpan hal yang sama dua kali.
            //
            // PARSIAL atas dua hal sekaligus, dan keduanya perlu:
            //   IsDelete = false            — penghapusan di sini berupa penandaan
            //   LabReportNumber IS NOT NULL — seluruh pesanan lama nol bernomor cetak, dan
            //                                 index unik penuh akan menolak yang kedua.
            //
            // PostgreSQL sebenarnya memperlakukan NULL sebagai saling berbeda, sehingga syarat
            // kedua tidak wajib secara teknis. Ia ditulis supaya index-nya menyatakan maksudnya
            // sendiri, dan supaya ia tetap benar bila penyedia lain dipakai.
            entity.HasIndex(x => new { x.Discipline, x.LabReportNumber })
                .IsUnique()
                .HasDatabaseName("IX_LabOrder_Discipline_LabReportNumber")
                .HasFilter("\"IsDelete\" = false AND \"LabReportNumber\" IS NOT NULL");

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

            // =========================================================================
            // BE-RWI-042 - konteks perawatan rawat inap
            // =========================================================================
            entity.Property(x => x.InpEpisodeId)
                .IsRequired(false);

            entity.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.InpEpisodeId,
                x.CreateDateTime
            });

            // =========================================================================
            // BE-LAB-30 / LAB-DEC-061 - konfirmasi pesanan
            // =========================================================================

            // Ketiganya nullable tanpa nilai bawaan. Seluruh pesanan yang sudah ada memang
            // tidak pernah dikonfirmasi; nilai bawaan apa pun akan mengarang riwayat yang
            // tidak pernah terjadi.
            entity.Property(x => x.ConfirmedByUserId)
                .IsRequired(false);

            entity.Property(x => x.ConfirmedAt)
                .IsRequired(false);

            entity.Property(x => x.ExaminerDoctorId)
                .IsRequired(false);

            // Dokter pemeriksa menunjuk data induk global. Restrict, bukan Cascade: menghapus
            // seorang dokter tidak boleh ikut menghapus pesanan yang pernah ditanganinya.
            entity.HasOne<MstDoctor>()
                .WithMany()
                .HasForeignKey(x => x.ExaminerDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Daftar pesanan per dokter pemeriksa menyaring tepat pada kolom ini.
            entity.HasIndex(x => x.ExaminerDoctorId);
        }
    }
}
