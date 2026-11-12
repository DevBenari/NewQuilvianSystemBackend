using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptPrescription</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptPrescriptionConfiguration : IEntityTypeConfiguration<OptPrescription>
    {
        public void Configure(EntityTypeBuilder<OptPrescription> builder)
        {
            builder.ToTable("OptPrescription", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PrescriptionType).HasConversion<int>().IsRequired();
            builder.Property(x => x.Origin).HasConversion<int>().IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.PrescriptionNumber).IsUnique().HasDatabaseName("UX_OptPrescription_PrescriptionNumber");
            builder.HasIndex(x => new { x.PatientId, x.ValidUntil }).HasDatabaseName("IX_OptPrescription_PatientId_ValidUntil");
            builder.HasIndex(x => x.EncounterId).HasDatabaseName("IX_OptPrescription_EncounterId");
            builder.HasIndex(x => x.RefractionExamId).HasDatabaseName("IX_OptPrescription_RefractionExamId");
            builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL").HasDatabaseName("UX_OptPrescription_IdempotencyKey");

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<OptRefractionExam>()
                .WithMany()
                .HasForeignKey(x => x.RefractionExamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
