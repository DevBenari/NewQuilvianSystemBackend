using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan penilaian sesi — <c>data-dictionary.md</c> bagian 3.10.</summary>
    /// <remarks>
    /// Unique <c>(SessionId, Phase)</c>: satu sesi punya satu penilaian pra dan satu pasca.
    /// <c>PatientVitalSignId</c> tanpa FK karena milik Clinical Management.
    /// </remarks>
    public class HmdSessionAssessmentConfiguration : IEntityTypeConfiguration<HmdSessionAssessment>
    {
        public void Configure(EntityTypeBuilder<HmdSessionAssessment> builder)
        {
            builder.ToTable("HmdSessionAssessment", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Phase).HasConversion<int>().IsRequired();
            builder.Property(x => x.BodyWeightKg).HasPrecision(5, 2);
            builder.Property(x => x.Complaint).HasMaxLength(1000);
            builder.Property(x => x.AccessConditionNote).HasMaxLength(500);
            builder.Property(x => x.PatientCondition).HasMaxLength(1000);

            builder.HasIndex(x => new { x.SessionId, x.Phase }).IsUnique();
            builder.HasIndex(x => x.AssessedByUserId);
            builder.HasIndex(x => x.PatientVitalSignId);

            builder.HasOne(x => x.Session)
                .WithMany(x => x.Assessments)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
