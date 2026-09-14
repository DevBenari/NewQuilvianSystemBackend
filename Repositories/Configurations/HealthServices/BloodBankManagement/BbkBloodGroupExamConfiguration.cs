using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan pemeriksaan golongan darah Bank Darah.</summary>
    /// <remarks>
    /// Empat index, seluruhnya menjawab satu pertanyaan panas yang sama: "apa golongan darah
    /// sah pasien ini sekarang, atau apakah sedang bertentangan" (<c>BD-DOM-21</c>). Pertanyaan
    /// itu diajukan setiap kali gerbang klinis dinilai, sehingga jalur bacanya tidak boleh
    /// memindai tabel.
    ///
    /// <c>AboRhesusResult</c> disimpan sebagai <c>int</c> mengikuti kebiasaan enum repository —
    /// kamus data menuliskannya <c>int?</c> (<c>BloodType</c>).
    /// </remarks>
    public class BbkBloodGroupExamConfiguration : IEntityTypeConfiguration<BbkBloodGroupExam>
    {
        public void Configure(EntityTypeBuilder<BbkBloodGroupExam> builder)
        {
            builder.ToTable("BbkBloodGroupExam", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PatientId).IsRequired();
            builder.Property(x => x.AboRhesusResult).HasConversion<int?>();
            builder.Property(x => x.ExamStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.IsValidResult).HasDefaultValue(false);
            builder.Property(x => x.IsConflictHeld).HasDefaultValue(false);
            builder.Property(x => x.Version).HasDefaultValue(0);

            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.ExamStatus);
            builder.HasIndex(x => x.IsValidResult);
            builder.HasIndex(x => x.IsConflictHeld);

            // FK sungguhan ke MstPatient, mengikuti kamus data dan pola TrxPatientEncounter.
            // Restrict: pemeriksaan golongan darah adalah rekam klinis yang wajib tetap terbaca
            // walaupun baris pasiennya kelak dirapikan.
            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Samples)
                .WithOne(x => x.BloodGroupExam!)
                .HasForeignKey(x => x.BloodGroupExamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
