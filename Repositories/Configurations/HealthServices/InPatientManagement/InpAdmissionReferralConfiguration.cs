using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary>
    /// <c>InpAdmissionReferral</c> — kamus data 19.8 dan DDL 19.12 (<c>BE-RWI-181</c>, migration
    /// <c>E6</c>, sesudah <c>E5</c> dan sebelum <c>I6</c>).
    /// </summary>
    public class InpAdmissionReferralConfiguration : IEntityTypeConfiguration<InpAdmissionReferral>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionReferral> builder)
        {
            builder.ToTable("InpAdmissionReferral", "public", table =>
            {
                // Completed wajib menunjuk episodenya; Cancelled wajib beralasan.
                table.HasCheckConstraint("CK_InpAdmissionReferral_State",
                    "(\"Status\" <> 2 OR \"CompletedEpisodeId\" IS NOT NULL) AND (\"Status\" <> 3 OR \"CancelledReason\" IS NOT NULL)");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestedCareLevel).HasConversion<int>().IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired()
                .HasDefaultValue(InpAdmissionReferralStatus.Pending);
            builder.Property(x => x.RecoveryDecisionNote).HasMaxLength(2000);
            builder.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelledReason).HasMaxLength(500);
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // INV-RWF-32: satu permintaan Pending per pasien dan per kasus OK.
            builder.HasIndex(x => x.PatientId, "UX_InpAdmissionReferral_Patient_Pending")
                .IsUnique()
                .HasFilter("\"Status\" = 1 AND NOT \"IsDelete\"");
            builder.HasIndex(x => x.OprCaseId, "UX_InpAdmissionReferral_Case_Pending")
                .IsUnique()
                .HasFilter("\"Status\" = 1 AND NOT \"IsDelete\"");
            builder.HasIndex(x => x.CompletedEpisodeId, "UX_InpAdmissionReferral_CompletedEpisode")
                .IsUnique()
                .HasFilter("\"CompletedEpisodeId\" IS NOT NULL");
            builder.HasIndex(x => new { x.Status, x.RequestedAt }, "IX_InpAdmissionReferral_Status_RequestedAt");
            builder.HasIndex(x => x.SourceEncounterId);

            builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SourceEncounter).WithMany().HasForeignKey(x => x.SourceEncounterId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OprCase).WithMany().HasForeignKey(x => x.OprCaseId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PrimarySurgeon).WithMany().HasForeignKey(x => x.PrimarySurgeonId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CompletedEpisode).WithMany().HasForeignKey(x => x.CompletedEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
