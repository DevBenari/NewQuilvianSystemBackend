using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadOrderConfiguration : IEntityTypeConfiguration<RadOrder>
    {
        public void Configure(EntityTypeBuilder<RadOrder> builder)
        {
            builder.ToTable("RadOrder", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int>();
            builder.Property(x => x.ClinicalIndication).HasMaxLength(1000);
            builder.Property(x => x.ClosureReason).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

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
        }
    }
}
