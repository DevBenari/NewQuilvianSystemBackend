using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary><c>InpAdmissionDocumentParty</c> — kamus data 20.4 dan DDL 20.18 (<c>BE-RWI-192</c>).</summary>
    public class InpAdmissionDocumentPartyConfiguration : IEntityTypeConfiguration<InpAdmissionDocumentParty>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionDocumentParty> builder)
        {
            builder.ToTable("InpAdmissionDocumentParty", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SourceType).HasConversion<int>().IsRequired()
                .HasDefaultValue(InpAdmissionPartySource.Manual);
            builder.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Relationship).HasConversion<int?>();
            builder.Property(x => x.RelationshipText).HasMaxLength(100);
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.BirthDate).HasColumnType("date");
            builder.Property(x => x.Gender).HasConversion<int?>();
            builder.Property(x => x.Occupation).HasMaxLength(100);
            builder.Property(x => x.IdentityType).HasConversion<int?>();
            builder.Property(x => x.IdentityNumber).HasMaxLength(50);
            builder.Property(x => x.MobilePhone).HasMaxLength(13);
            builder.Property(x => x.OfficePhone).HasMaxLength(20);

            // Satu pihak per dokumen; relasi satu-ke-satu membentuk IX_InpAdmissionDocumentParty_DocumentId unik.
            builder.HasOne(x => x.Document).WithOne(x => x.Party)
                .HasForeignKey<InpAdmissionDocumentParty>(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
