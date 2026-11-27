using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPatientResultVersionConfiguration : IEntityTypeConfiguration<LabPatientResultVersion>
    {
        public void Configure(EntityTypeBuilder<LabPatientResultVersion> builder)
        {
            builder.ToTable("LabPatientResultVersion", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.VersionNumber).IsRequired();
            builder.Property(x => x.DocumentNumber).HasMaxLength(60).IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.SupersededAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.VerificationToken).HasMaxLength(64).IsRequired();
            builder.Property(x => x.TokenRevokedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            builder.Property(x => x.FileSha256).HasMaxLength(64).IsFixedLength().IsRequired();
            builder.Property(x => x.FileSizeBytes).IsRequired();

            // INV-70: nomor versi unik per dokumen.
            builder.HasIndex(x => new { x.DocumentId, x.VersionNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Halaman verifikasi kelak (S17-2) mencari versi lewat token.
            builder.HasIndex(x => x.VerificationToken)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasOne(x => x.Document)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
