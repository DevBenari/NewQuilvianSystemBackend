using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabTransitionHistoryConfiguration : IEntityTypeConfiguration<LabTransitionHistory>
    {
        public void Configure(EntityTypeBuilder<LabTransitionHistory> builder)
        {
            builder.ToTable("LabTransitionHistory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Scope).HasConversion<int>().IsRequired();
            builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
            builder.Property(x => x.FromStatus).HasMaxLength(50);
            builder.Property(x => x.ToStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ReasonCode).HasMaxLength(50);
            builder.Property(x => x.ReasonNote).HasMaxLength(1000);

            builder.HasIndex(x => new { x.LabOrderId, x.OccurredAt });
            builder.HasIndex(x => x.LabSpecimenId);
            builder.HasIndex(x => x.LabExaminationId);
            builder.HasIndex(x => x.EncounterId);

            builder.HasOne(x => x.LabOrder)
                .WithMany()
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LabSpecimen)
                .WithMany()
                .HasForeignKey(x => x.LabSpecimenId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict, sama seperti kedua relasi lainnya: riwayat adalah jejak permanen, dan
            // menghapus pemeriksaan tidak boleh menghapus bukti perpindahannya.
            builder.HasOne(x => x.LabExamination)
                .WithMany()
                .HasForeignKey(x => x.LabExaminationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<TrxPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
