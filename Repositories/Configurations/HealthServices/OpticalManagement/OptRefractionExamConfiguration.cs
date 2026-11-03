using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptRefractionExam</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptRefractionExamConfiguration : IEntityTypeConfiguration<OptRefractionExam>
    {
        public void Configure(EntityTypeBuilder<OptRefractionExam> builder)
        {
            builder.ToTable("OptRefractionExam", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.ExamNumber).IsUnique().HasDatabaseName("UX_OptRefractionExam_ExamNumber");
            builder.HasIndex(x => new { x.EncounterId, x.Status }).HasDatabaseName("IX_OptRefractionExam_EncounterId_Status");
            builder.HasIndex(x => new { x.PatientId, x.FinalizedAt }).HasDatabaseName("IX_OptRefractionExam_PatientId_FinalizedAt");
            builder.HasIndex(x => x.ExaminerWorkforceProfileId).HasDatabaseName("IX_OptRefractionExam_ExaminerWorkforceProfileId");

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstWorkforceProfile>()
                .WithMany()
                .HasForeignKey(x => x.ExaminerWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
