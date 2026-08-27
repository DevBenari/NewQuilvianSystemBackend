using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.MedicalRecordManagement
{
    public class TrxClinicalNoteAuthorDelegationConfiguration
        : IEntityTypeConfiguration<TrxClinicalNoteAuthorDelegation>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalNoteAuthorDelegation> builder)
        {
            builder.ToTable("TrxClinicalNoteAuthorDelegation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Trigger).HasConversion<int>();
            builder.Property(x => x.GrantReason).HasMaxLength(500);

            // Dipakai memeriksa apakah jalur pengganti sedang terbuka untuk seorang penulis.
            builder.HasIndex(x => new { x.OriginalAuthorUserId, x.IsActive, x.ValidUntil });
            builder.HasIndex(x => x.Trigger);
            builder.HasIndex(x => x.GrantedByUserId);
        }
    }
}
