using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    /// <summary>Konfigurasi Surat Tugas multi-pegawai (BE-HRD-382, migration <c>20261120093000_AddHrdAssignmentLetter</c>).</summary>
    public class HrdAssignmentLetterConfiguration : IEntityTypeConfiguration<HrdAssignmentLetter>
    {
        public void Configure(EntityTypeBuilder<HrdAssignmentLetter> builder)
        {
            builder.ToTable("HrdAssignmentLetter", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.LetterNumber).HasMaxLength(100);
            builder.Property(x => x.ActivityName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.DutyCategory).HasMaxLength(50).IsRequired();
            builder.Property(x => x.BasisDescription).HasMaxLength(1000);
            builder.Property(x => x.BasisDocumentReference).HasMaxLength(500);
            builder.Property(x => x.TaskDescription).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.Destination).HasMaxLength(255).IsRequired();
            builder.Property(x => x.StartAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.EndAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.LetterStatus).HasMaxLength(30).IsRequired().HasDefaultValue("Draft");
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.VerificationNote).HasMaxLength(1000);
            builder.Property(x => x.RevisionNote).HasMaxLength(1000);
            builder.Property(x => x.EndorsedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.EndorsementNote).HasMaxLength(1000);
            builder.Property(x => x.SignerName).HasMaxLength(200);
            builder.Property(x => x.SignerTitle).HasMaxLength(200);
            builder.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectionReason).HasMaxLength(1000);
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancellationReason).HasMaxLength(1000);
            builder.Property(x => x.ExecutionStartedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ResultReport).HasMaxLength(2000);
            builder.Property(x => x.ResultEvidenceReference).HasMaxLength(500);
            builder.Property(x => x.VerificationHash).HasMaxLength(128);
            builder.Property(x => x.DocumentSnapshotHtml).HasColumnType("text");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.LetterNumber)
                .IsUnique()
                .HasFilter("\"LetterNumber\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => x.VerificationHash)
                .IsUnique()
                .HasFilter("\"VerificationHash\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.LetterStatus, x.StartAt });
            builder.HasIndex(x => x.RequestingOrganizationUnitId);

            builder.HasMany(x => x.Participants)
                .WithOne(x => x.AssignmentLetter)
                .HasForeignKey(x => x.AssignmentLetterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Konfigurasi peserta Surat Tugas (BE-HRD-382).</summary>
    public class HrdAssignmentLetterParticipantConfiguration : IEntityTypeConfiguration<HrdAssignmentLetterParticipant>
    {
        public void Configure(EntityTypeBuilder<HrdAssignmentLetterParticipant> builder)
        {
            builder.ToTable("HrdAssignmentLetterParticipant", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ParticipantRole).HasMaxLength(100);
            builder.Property(x => x.DisplayNameSnapshot).HasMaxLength(200).IsRequired();
            builder.Property(x => x.ProfileCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.PositionNameSnapshot).HasMaxLength(200);
            builder.Property(x => x.UnitNameSnapshot).HasMaxLength(200);
            builder.Property(x => x.ScheduleConflictNote).HasMaxLength(500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AssignmentLetterId, x.WorkforceProfileId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.WorkforceProfileId);
        }
    }
}
