using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan catatan penyelesaian konflik golongan darah.</summary>
    /// <remarks>
    /// <c>DeleteBehavior.Restrict</c> pada pemeriksaan yang memutus bukan pilihan gaya:
    /// catatan ini append-only dan wajib tetap dapat menunjuk pemeriksaan yang mendasarinya
    /// selamanya. Cascade akan membuat jejak keputusan klinis ikut hilang bersama barisan
    /// pemeriksaannya.
    /// </remarks>
    public class BbkBloodGroupConflictResolutionConfiguration
        : IEntityTypeConfiguration<BbkBloodGroupConflictResolution>
    {
        public void Configure(EntityTypeBuilder<BbkBloodGroupConflictResolution> builder)
        {
            builder.ToTable("BbkBloodGroupConflictResolution", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PatientId).IsRequired();
            builder.Property(x => x.ResolvingExamId).IsRequired();
            builder.Property(x => x.ResolvedByUserId).IsRequired();
            builder.Property(x => x.ReasonCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ResolvedAt).IsRequired();

            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.ResolvingExamId);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ResolvingExam)
                .WithMany()
                .HasForeignKey(x => x.ResolvingExamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
