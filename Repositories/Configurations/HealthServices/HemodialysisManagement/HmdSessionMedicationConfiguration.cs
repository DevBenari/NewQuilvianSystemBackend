using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan pemberian obat — <c>data-dictionary.md</c> bagian 3.12.</summary>
    /// <remarks>
    /// <c>DrugUsageId</c>, <c>PharmacyStorageLocationId</c>, dan <c>PharmacyMeasurementId</c> tanpa
    /// FK karena milik Farmasi dan Master Data; validasinya dijalankan layanan Farmasi saat penerusan.
    /// </remarks>
    public class HmdSessionMedicationConfiguration : IEntityTypeConfiguration<HmdSessionMedication>
    {
        public void Configure(EntityTypeBuilder<HmdSessionMedication> builder)
        {
            builder.ToTable("HmdSessionMedication", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Dose).HasPrecision(10, 3);
            builder.Property(x => x.DoseUnit).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Route).HasConversion<int>().IsRequired();
            builder.Property(x => x.HandoffStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.PharmacyQuantity).HasPrecision(18, 3);
            builder.Property(x => x.HandoffError).HasMaxLength(1000);
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => x.SessionId);
            builder.HasIndex(x => x.DrugId);
            builder.HasIndex(x => x.InstructedByDoctorId);
            builder.HasIndex(x => x.AdministeredByUserId);
            builder.HasIndex(x => x.AdministeredAt);
            builder.HasIndex(x => x.DrugUsageId);
            builder.HasIndex(x => x.HandoffStatus);

            builder.HasOne(x => x.Session)
                .WithMany(x => x.Medications)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Drug)
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InstructedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.InstructedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
