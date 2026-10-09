using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary>
    /// <c>InpAdmissionDocument</c> — kamus data 20.2 dan DDL 20.18 (<c>BE-RWI-192</c>, migration
    /// <c>E10</c>, sesudah <c>E9</c>).
    /// </summary>
    public class InpAdmissionDocumentConfiguration : IEntityTypeConfiguration<InpAdmissionDocument>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionDocument> builder)
        {
            builder.ToTable("InpAdmissionDocument", "public", table =>
            {
                // Keadaan wajib per status (INV-RWA-10): kunci dan selesai selalu punya salinan
                // beku; batal selalu beralasan; versi koreksi selalu menunjuk versi lama.
                table.HasCheckConstraint("CK_InpAdmissionDocument_State",
                    "(\"Status\" <> 2 OR (\"LockedAt\" IS NOT NULL AND \"SnapshotJson\" IS NOT NULL)) AND " +
                    "(\"Status\" <> 3 OR (\"CompletedAt\" IS NOT NULL AND \"SnapshotJson\" IS NOT NULL)) AND " +
                    "(\"Status\" <> 4 OR \"SupersededAt\" IS NOT NULL) AND " +
                    "(\"Status\" <> 5 OR (\"CancelledAt\" IS NOT NULL AND \"CancelledReason\" IS NOT NULL)) AND " +
                    "(\"VersionNo\" = 1 OR (\"PreviousVersionId\" IS NOT NULL AND \"CorrectionReason\" IS NOT NULL))");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DocumentType).HasConversion<int>().IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired()
                .HasDefaultValue(InpAdmissionDocumentStatus.Draft);
            builder.Property(x => x.VersionNo).IsRequired().HasDefaultValue(1);
            builder.Property(x => x.CorrectionReason).HasMaxLength(500);
            builder.Property(x => x.SigningCity).HasMaxLength(100);
            builder.Property(x => x.StatementDate).HasColumnType("date");
            builder.Property(x => x.Note).HasMaxLength(1000);
            builder.Property(x => x.SnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.LockedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.SupersededAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelledReason).HasMaxLength(500);
            builder.Property(x => x.IdempotencyKey).HasMaxLength(80);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // INV-RWA-01: paling banyak satu dokumen aktif per jenis per episode.
            builder.HasIndex(x => new { x.EpisodeId, x.DocumentType }, "UX_InpAdmissionDocument_Episode_Type_Active")
                .IsUnique()
                .HasFilter("\"Status\" IN (1, 2, 3) AND NOT \"IsDelete\"");

            // Satu versi hanya punya satu pengganti.
            builder.HasIndex(x => x.PreviousVersionId, "UX_InpAdmissionDocument_PreviousVersionId")
                .IsUnique()
                .HasFilter("\"PreviousVersionId\" IS NOT NULL");

            builder.HasIndex(x => x.IdempotencyKey, "UX_InpAdmissionDocument_IdempotencyKey")
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL AND NOT \"IsDelete\"");

            builder.HasIndex(x => new { x.EpisodeId, x.DocumentType, x.CreateDateTime });
            builder.HasIndex(x => new { x.PatientId, x.DocumentType, x.Status });

            builder.HasOne(x => x.Episode).WithMany().HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PreviousVersion).WithMany().HasForeignKey(x => x.PreviousVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
