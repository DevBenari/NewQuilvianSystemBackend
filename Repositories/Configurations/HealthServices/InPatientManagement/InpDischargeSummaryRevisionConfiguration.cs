using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpDischargeSummaryRevisionConfiguration : IEntityTypeConfiguration<InpDischargeSummaryRevision>
    {
        public void Configure(EntityTypeBuilder<InpDischargeSummaryRevision> builder)
        {
            builder.ToTable("InpDischargeSummaryRevision", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.PreviousDischargeType).HasConversion<int>();
            builder.Property(x => x.PrimaryDiagnosisText).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.SecondaryDiagnosisText).HasMaxLength(2000);
            builder.Property(x => x.ProcedureSummary).HasMaxLength(2000);
            builder.Property(x => x.DischargeMedicationNote).HasMaxLength(2000);
            builder.Property(x => x.FollowUpInstruction).HasMaxLength(2000);
            builder.Property(x => x.ReferralDestination).HasMaxLength(250);
            builder.Property(x => x.ClinicalSummary).HasMaxLength(4000);

            builder.HasIndex(x => x.DischargeSummaryId);
            builder.HasIndex(x => x.CorrectionSessionId);
            builder.HasIndex(x => x.PreviousSignedByDoctorId);
            builder.HasIndex(x => x.SupersededAt);
            builder.HasIndex(x => new { x.DischargeSummaryId, x.RevisionNumber }).IsUnique();

            builder.HasOne(x => x.DischargeSummary)
                .WithMany(x => x.Revisions)
                .HasForeignKey(x => x.DischargeSummaryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CorrectionSession)
                .WithMany(x => x.SummaryRevisions)
                .HasForeignKey(x => x.CorrectionSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreviousSignedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.PreviousSignedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SupersededByUser)
                .WithMany()
                .HasForeignKey(x => x.SupersededByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
