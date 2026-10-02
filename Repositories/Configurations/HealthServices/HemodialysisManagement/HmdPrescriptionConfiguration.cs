using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan resep HD — <c>data-dictionary.md</c> bagian 3.7.</summary>
    /// <remarks>
    /// <b>Unique index bersyarat <c>IX_HmdPrescription_EpisodeId_Active</c></b> menjaga tepat satu
    /// resep aktif per episode. Nilai <c>2</c> adalah <c>HmdPrescriptionStatus.Active</c>. FK ke
    /// resep pengganti diberi nama eksplisit karena merujuk tabelnya sendiri.
    /// </remarks>
    public class HmdPrescriptionConfiguration : IEntityTypeConfiguration<HmdPrescription>
    {
        public void Configure(EntityTypeBuilder<HmdPrescription> builder)
        {
            builder.ToTable("HmdPrescription", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PrescriptionStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.DialyzerType).HasMaxLength(100);
            builder.Property(x => x.DialysateComposition).HasMaxLength(200);
            builder.Property(x => x.SodiumBicarbonateProfile).HasMaxLength(200);
            builder.Property(x => x.DialysateTemperatureC).HasPrecision(4, 1);
            builder.Property(x => x.AnticoagulantPlan).HasMaxLength(500);
            builder.Property(x => x.ClinicalNote).HasMaxLength(1000);
            builder.Property(x => x.CancelReason).HasMaxLength(500);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.EpisodeId)
                .IsUnique()
                .HasFilter("\"PrescriptionStatus\" = 2 AND \"IsDelete\" = false")
                .HasDatabaseName("IX_HmdPrescription_EpisodeId_Active");
            builder.HasIndex(x => new { x.EpisodeId, x.PrescriptionStatus });
            builder.HasIndex(x => x.PrescribingDoctorId);
            builder.HasIndex(x => x.EffectiveDate);
            builder.HasIndex(x => x.VascularAccessId);
            builder.HasIndex(x => x.SupersededByPrescriptionId);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.Prescriptions)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PrescribingDoctor)
                .WithMany()
                .HasForeignKey(x => x.PrescribingDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VascularAccess)
                .WithMany()
                .HasForeignKey(x => x.VascularAccessId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SupersededByPrescription)
                .WithMany()
                .HasForeignKey(x => x.SupersededByPrescriptionId)
                .HasConstraintName("FK_HmdPrescription_SupersededByPrescription")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
