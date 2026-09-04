using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabValueBoundConfiguration : IEntityTypeConfiguration<LabValueBound>
    {
        public void Configure(EntityTypeBuilder<LabValueBound> builder)
        {
            builder.ToTable("LabValueBound", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProcedureId).IsRequired();

            builder.Property(x => x.ResultForm).HasConversion<int>().IsRequired();
            builder.Property(x => x.GenderScope).HasConversion<int>().IsRequired();

            builder.Property(x => x.Unit).HasMaxLength(20);

            builder.Property(x => x.NormalLow).HasPrecision(18, 4);
            builder.Property(x => x.NormalHigh).HasPrecision(18, 4);
            builder.Property(x => x.CriticalLow).HasPrecision(18, 4);
            builder.Property(x => x.CriticalHigh).HasPrecision(18, 4);

            // BR-14 dan VAL-21: satu jenis pemeriksaan boleh punya beberapa baris batas, tetapi
            // tidak boleh dua baris untuk kelompok pasien yang sama. Keunikan ditegakkan
            // database supaya dua permintaan bersamaan tidak dapat sama-sama berhasil; jalur
            // 409 beserta pesannya adalah pekerjaan endpoint pengelolaan pada BE-LAB-04.
            //
            // Filter IsDelete mengikuti soft delete base model audit: baris yang sudah dihapus
            // tidak boleh menghalangi pembuatan baris baru untuk kelompok yang sama.
            //
            // AreNullsDistinct(false) menutup celah yang ditemukan saat BE-LAB-02 diverifikasi.
            // PostgreSQL biasanya menganggap NULL selalu berbeda dari NULL lain, sehingga dua
            // baris "semua umur" untuk pemeriksaan dan jenis kelamin yang sama lolos dari index
            // unik — persis contoh Kalium pada BR-14. Padahal AgeCategoryId yang kosong justru
            // punya arti: berlaku untuk semua umur. Di sini NULL diperlakukan sebagai satu nilai
            // yang sama, sehingga VAL-21 tegak penuh untuk kasus itu juga.
            builder.HasIndex(x => new { x.ProcedureId, x.GenderScope, x.AgeCategoryId })
                .IsUnique()
                .HasDatabaseName("IX_LabValueBound_Procedure_Gender_AgeCategory")
                .HasFilter("\"IsDelete\" = false")
                .AreNullsDistinct(false);

            // MstProcedure dan MstAgeCategory milik master-data. Restrict menjaga data induk
            // yang masih dirujuk batas nilai tidak dapat dihapus dari sisi sana.
            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AgeCategory)
                .WithMany()
                .HasForeignKey(x => x.AgeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cascade dipakai di sini — dan hanya di sini pada modul Laboratorium — karena
            // sebuah pilihan tidak punya makna tanpa batas nilai induknya, dan keduanya bukan
            // data klinis transaksional (erd/data-dictionary.md bagian 6).
            builder.HasMany(x => x.Options)
                .WithOne(x => x.ValueBound)
                .HasForeignKey(x => x.ValueBoundId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
