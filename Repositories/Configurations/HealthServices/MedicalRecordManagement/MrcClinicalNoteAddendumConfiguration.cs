using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalRecordManagement
{
    public class MrcClinicalNoteAddendumConfiguration
        : IEntityTypeConfiguration<MrcClinicalNoteAddendum>
    {
        public void Configure(EntityTypeBuilder<MrcClinicalNoteAddendum> builder)
        {
            builder.ToTable("MrcClinicalNoteAddendum", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AddendumText).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.CorrectionReason).HasMaxLength(500).IsRequired();
            builder.Property(x => x.SignatureDeviceInfo).HasMaxLength(250);
            builder.Property(x => x.SignatureIpAddress).HasMaxLength(64);

            // Urutan koreksi terbaca pasti dan tidak dapat kembar.
            builder.HasIndex(x => new { x.IntegrityId, x.Sequence })
                .IsUnique();

            // BE-38: bawaan Correction supaya addendum lama terbaca koreksi; paling banyak satu
            // pengesahan per catatan (VAL-RM-A02).
            builder.Property(x => x.AddendumPurpose).HasDefaultValue(ClinicalNoteAddendumPurpose.Correction);
            builder.HasIndex(x => x.IntegrityId, "IX_MrcClinicalNoteAddendum_IntegrityId_Attestation")
                .IsUnique()
                .HasFilter("\"AddendumPurpose\" = 2 AND \"IsDelete\" = false");

            builder.HasIndex(x => x.AuthorUserId);
            builder.HasIndex(x => x.DelegationId);

            builder.HasOne(x => x.Integrity)
                .WithMany(x => x.Addendums)
                .HasForeignKey(x => x.IntegrityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Delegation)
                .WithMany()
                .HasForeignKey(x => x.DelegationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
