using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadOrderConfiguration : IEntityTypeConfiguration<RadOrder>
    {
        public void Configure(EntityTypeBuilder<RadOrder> builder)
        {
            builder.ToTable("RadOrder", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderNumber).HasMaxLength(64).IsRequired();
            builder.Property(x => x.OrderStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int>();
            builder.Property(x => x.ClinicalIndication).HasMaxLength(1000);
            builder.Property(x => x.ClosureReason).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

            // Nomor pesanan tidak boleh kembar. Dijaga database, bukan hanya service: dua
            // permintaan bersamaan sama-sama lolos pemeriksaan di memori, dan yang kedua harus
            // ditolak di sini. Penyaring IsDelete mengikuti RadReport.ReportNumber supaya
            // pesanan yang sudah dihapus lunak tidak menahan nomor selamanya.
            builder.HasIndex(x => x.OrderNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.OrderStatus);
            builder.HasIndex(x => new { x.ModalityId, x.OrderStatus });

            // =========================================================================
            // BE-RAD-12 — penanda cito, RAD-DEC-013
            // =========================================================================

            // Urutan kolomnya menentukan dan tidak boleh ditukar. Daftar kerja selalu bertanya
            // "pekerjaan pada alat ini", lalu "yang cito lebih dulu", lalu "yang statusnya masih
            // berjalan". Index dengan urutan itu terpakai penuh; urutan lain memaksa database
            // memindai seluruh pesanan alat tersebut.
            builder.HasIndex(x => new { x.ModalityId, x.IsUrgent, x.OrderStatus });

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Modality)
                .WithMany()
                .HasForeignKey(x => x.ModalityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Studies)
                .WithOne(x => x.RadOrder)
                .HasForeignKey(x => x.RadOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================================
            // BE-RWI-042 - konteks perawatan rawat inap
            // =========================================================================
            builder.Property(x => x.InpEpisodeId)
                .IsRequired(false);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.InpEpisodeId,
                x.CreateDateTime
            });

            // =========================================================================
            // BE-RWI-104 / migration R8 — pemberi instruksi dan verifikasinya, RWI-DEC-153
            // =========================================================================
            builder.Property(x => x.InstructingDoctorId)
                .IsRequired(false);

            builder.Property(x => x.InstructionVerificationStatus)
                .HasConversion<int>()
                .HasDefaultValue(RadOrderInstructionVerificationStatus.NotRequired)
                .IsRequired();

            builder.Property(x => x.InstructionVerifiedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.InstructionVerifiedByUserId)
                .IsRequired(false);

            builder.HasOne<MstDoctor>()
                .WithMany()
                .HasForeignKey(x => x.InstructingDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.InstructionVerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Daftar tunggu verifikasi membaca "pesanan Pending milik dokter ini".
            builder.HasIndex(x => new { x.InstructingDoctorId, x.InstructionVerificationStatus });
            builder.HasIndex(x => x.InstructionVerifiedByUserId);
        }
    }
}
