using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan program HD — <c>data-dictionary.md</c> bagian 3.2.</summary>
    /// <remarks>
    /// <b>Unique index bersyarat <c>IX_HmdEpisode_PatientId_Active</c></b> menegakkan satu episode
    /// aktif per pasien di tingkat basis data, bukan hanya di service. Nilai <c>2</c> pada filter
    /// adalah <c>HmdEpisodeStatus.Active</c>. Ketika
    /// <c>HmdSetting.AllowMultipleActiveEpisodePerPatient</c> dinyalakan, index ini tetap ada;
    /// pelonggaran memerlukan migration yang menghapusnya (konsekuensi yang diketahui dari
    /// <c>RCG-M-05</c>).
    /// </remarks>
    public class HmdEpisodeConfiguration : IEntityTypeConfiguration<HmdEpisode>
    {
        public void Configure(EntityTypeBuilder<HmdEpisode> builder)
        {
            builder.ToTable("HmdEpisode", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EpisodeNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.EpisodeStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.ClosureReason).HasConversion<int?>();
            builder.Property(x => x.SuspendReason).HasMaxLength(500);
            builder.Property(x => x.ClosureNote).HasMaxLength(1000);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.EpisodeNumber).IsUnique();
            builder.HasIndex(x => x.PatientId)
                .IsUnique()
                .HasFilter("\"EpisodeStatus\" = 2 AND \"IsDelete\" = false")
                .HasDatabaseName("IX_HmdEpisode_PatientId_Active");
            builder.HasIndex(x => new { x.PatientId, x.EpisodeStatus });
            builder.HasIndex(x => x.ServiceUnitId);
            builder.HasIndex(x => x.DpjpDoctorId);
            builder.HasIndex(x => x.StartDate);
            builder.HasIndex(x => x.EpisodeStatus);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DpjpDoctor)
                .WithMany()
                .HasForeignKey(x => x.DpjpDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
