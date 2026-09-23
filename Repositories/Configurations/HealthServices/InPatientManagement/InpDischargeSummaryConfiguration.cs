using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpDischargeSummaryConfiguration : IEntityTypeConfiguration<InpDischargeSummary>
    {
        public void Configure(EntityTypeBuilder<InpDischargeSummary> builder)
        {
            builder.ToTable("InpDischargeSummary", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.PrimaryDiagnosisText).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.SecondaryDiagnosisText).HasMaxLength(2000);
            builder.Property(x => x.ProcedureSummary).HasMaxLength(2000);
            builder.Property(x => x.DischargeMedicationNote).HasMaxLength(2000);
            builder.Property(x => x.FollowUpInstruction).HasMaxLength(2000);
            builder.Property(x => x.ReferralDestination).HasMaxLength(250);
            builder.Property(x => x.ClinicalSummary).HasMaxLength(4000);

            // BE-RWI-085 / RWI-DEC-112 — tiga bagian resume yang selama ini tidak ada.
            // Seluruhnya nullable: resume lama tetap terbaca dan tetap dapat ditandatangani,
            // dan isi minimal resume masih di bawah gerbang pemilik klinis (RWI-RULE-032).
            builder.Property(x => x.ImportantFindingsSummary).HasMaxLength(4000);
            builder.Property(x => x.DischargeConditionNote).HasMaxLength(2000);
            builder.Property(x => x.EducationSummary).HasMaxLength(2000);

            // INV-INP-05 — satu episode paling banyak satu resume pulang.
            builder.HasIndex(x => x.EpisodeId).IsUnique();
            builder.HasIndex(x => x.SignedAt);
            builder.HasIndex(x => x.SignedByDoctorId);

            builder.HasOne(x => x.Episode)
                .WithMany()
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SignedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.SignedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
