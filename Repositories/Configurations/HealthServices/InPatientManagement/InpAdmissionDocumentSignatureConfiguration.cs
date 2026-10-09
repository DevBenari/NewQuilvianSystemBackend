using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary><c>InpAdmissionDocumentSignature</c> — kamus data 20.3 dan DDL 20.18 (<c>BE-RWI-192</c>).</summary>
    public class InpAdmissionDocumentSignatureConfiguration : IEntityTypeConfiguration<InpAdmissionDocumentSignature>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionDocumentSignature> builder)
        {
            builder.ToTable("InpAdmissionDocumentSignature", "public", table =>
            {
                // Kertas hanya untuk slot pasien/keluarga dan selalu diverifikasi petugas; atestasi
                // hanya untuk slot petugas dan selalu menunjuk akun penanda tangan.
                table.HasCheckConstraint("CK_InpAdmissionDocumentSignature_Method",
                    "(\"Method\" <> 1 OR (\"Slot\" = 1 AND \"VerifiedByUserId\" IS NOT NULL AND \"SignedByUserId\" IS NULL)) AND " +
                    "(\"Method\" <> 2 OR (\"Slot\" <> 1 AND \"SignedByUserId\" IS NOT NULL))");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Slot).HasConversion<int>().IsRequired();
            builder.Property(x => x.Method).HasConversion<int>().IsRequired();
            builder.Property(x => x.SignerName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.SignerPositionName).HasMaxLength(150);
            builder.Property(x => x.SignerRelationship).HasConversion<int?>();
            builder.Property(x => x.SignerRelationshipText).HasMaxLength(100);
            builder.Property(x => x.SignedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.RecordedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.IdempotencyKey).HasMaxLength(80);

            // Satu slot satu tanda tangan (VAL-RWA-32).
            builder.HasIndex(x => new { x.DocumentId, x.Slot }, "UX_InpAdmissionDocumentSignature_Document_Slot")
                .IsUnique()
                .HasFilter("NOT \"IsDelete\"");

            // INV-RWA-04: satu akun tidak mengisi dua slot petugas pada dokumen yang sama, juga
            // ketika dua permintaan datang bersamaan.
            builder.HasIndex(x => new { x.DocumentId, x.SignedByUserId }, "UX_InpAdmissionDocumentSignature_Document_Signer")
                .IsUnique()
                .HasFilter("\"SignedByUserId\" IS NOT NULL AND NOT \"IsDelete\"");

            builder.HasIndex(x => x.IdempotencyKey, "UX_InpAdmissionDocumentSignature_IdempotencyKey")
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL AND NOT \"IsDelete\"");

            builder.HasOne(x => x.Document).WithMany(x => x.Signatures).HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
