using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary><c>InpAdmissionPrivacyEntry</c> — kamus data 20.7 dan DDL 20.18 (<c>BE-RWI-192</c>).</summary>
    public class InpAdmissionPrivacyEntryConfiguration : IEntityTypeConfiguration<InpAdmissionPrivacyEntry>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionPrivacyEntry> builder)
        {
            builder.ToTable("InpAdmissionPrivacyEntry", "public", table =>
            {
                // Tiga baris per jenis, mengikuti formulir V1 (G-40).
                table.HasCheckConstraint("CK_InpAdmissionPrivacyEntry_LineNo", "\"LineNo\" BETWEEN 1 AND 3");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EntryType).HasConversion<int>().IsRequired();
            builder.Property(x => x.Text).IsRequired().HasMaxLength(200);

            builder.HasIndex(x => new { x.DocumentId, x.EntryType, x.LineNo }).IsUnique();

            builder.HasOne(x => x.Document).WithMany(x => x.PrivacyEntries).HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
