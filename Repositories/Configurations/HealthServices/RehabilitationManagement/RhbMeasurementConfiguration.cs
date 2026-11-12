using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan hasil ukur — <c>data-dictionary.md</c> bagian 2.6.</summary>
    /// <remarks>
    /// <b><c>CK_RhbMeasurement_Owner</c></b>: tepat satu dari <c>AssessmentId</c> atau
    /// <c>ProgressReviewId</c> terisi.
    /// </remarks>
    public class RhbMeasurementConfiguration : IEntityTypeConfiguration<RhbMeasurement>
    {
        public void Configure(EntityTypeBuilder<RhbMeasurement> builder)
        {
            builder.ToTable("RhbMeasurement", "public", t => t.HasCheckConstraint(
                "CK_RhbMeasurement_Owner",
                "(\"AssessmentId\" IS NULL) <> (\"ProgressReviewId\" IS NULL)"));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Source).HasConversion<int>().IsRequired();
            builder.Property(x => x.InstrumentKind).HasConversion<int?>();
            builder.Property(x => x.InstrumentName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.BodyPartOrDomain).HasMaxLength(150);
            builder.Property(x => x.AnswersJson).HasColumnType("jsonb");
            builder.Property(x => x.NumericValue).HasPrecision(10, 2);
            builder.Property(x => x.ValueText).HasMaxLength(200);
            builder.Property(x => x.Unit).HasMaxLength(30);
            builder.Property(x => x.ScoreBandCode).HasMaxLength(50);

            builder.HasIndex(x => x.AssessmentId);
            builder.HasIndex(x => x.ProgressReviewId);
            builder.HasIndex(x => x.InstrumentVersionId);

            builder.HasOne(x => x.Assessment)
                .WithMany(x => x.Measurements)
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProgressReview)
                .WithMany(x => x.Measurements)
                .HasForeignKey(x => x.ProgressReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<CliClinicalInstrumentVersion>()
                .WithMany()
                .HasForeignKey(x => x.InstrumentVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
