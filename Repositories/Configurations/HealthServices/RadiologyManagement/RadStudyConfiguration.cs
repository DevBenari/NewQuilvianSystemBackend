using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadStudyConfiguration : IEntityTypeConfiguration<RadStudy>
    {
        public void Configure(EntityTypeBuilder<RadStudy> builder)
        {
            builder.ToTable("RadStudy", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StudyNumber).HasMaxLength(64).IsRequired();
            builder.Property(x => x.StudyStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int>();
            builder.Property(x => x.AbortCause).HasConversion<int>();
            builder.Property(x => x.AbortReason).HasMaxLength(1000);
            builder.Property(x => x.PerformedPortionNote).HasMaxLength(1000);
            builder.Property(x => x.RepeatCause).HasConversion<int>();
            builder.Property(x => x.RepeatReason).HasMaxLength(1000);
            builder.Property(x => x.QualityNote).HasMaxLength(1000);
            builder.Property(x => x.ClosureReason).HasMaxLength(1000);
            builder.Property(x => x.ExternalStudyUid).HasMaxLength(128);
            builder.Property(x => x.Version).IsConcurrencyToken();

            // Nomor study wajib unik. Ditegakkan database, bukan hanya di service, agar dua
            // permintaan bersamaan tidak dapat menghasilkan nomor kembar.
            builder.HasIndex(x => x.StudyNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.RadOrderId, x.StudySequence });
            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.StudyStatus);
            builder.HasIndex(x => x.RepeatOfStudyId);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Modality)
                .WithMany()
                .HasForeignKey(x => x.ModalityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rantai pengulangan. Restrict memastikan study yang gagal tidak dapat dihapus
            // selama masih menjadi asal-usul study penggantinya — tanpa itu, jumlah paparan
            // yang sebenarnya bisa hilang dari catatan.
            builder.HasOne(x => x.RepeatOfStudy)
                .WithMany()
                .HasForeignKey(x => x.RepeatOfStudyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.SafetyChecks)
                .WithOne(x => x.RadStudy)
                .HasForeignKey(x => x.RadStudyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Consumptions)
                .WithOne(x => x.RadStudy)
                .HasForeignKey(x => x.RadStudyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
