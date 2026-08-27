using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.MedicalRecordManagement
{
    public class TrxClinicalNoteAddendumConfiguration
        : IEntityTypeConfiguration<TrxClinicalNoteAddendum>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalNoteAddendum> builder)
        {
            builder.ToTable("TrxClinicalNoteAddendum", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AddendumText).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.CorrectionReason).HasMaxLength(500).IsRequired();
            builder.Property(x => x.SignatureDeviceInfo).HasMaxLength(250);
            builder.Property(x => x.SignatureIpAddress).HasMaxLength(64);

            // Urutan koreksi terbaca pasti dan tidak dapat kembar.
            builder.HasIndex(x => new { x.IntegrityId, x.Sequence })
                .IsUnique();

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
