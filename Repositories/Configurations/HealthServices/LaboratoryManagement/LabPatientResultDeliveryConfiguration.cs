using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPatientResultDeliveryConfiguration : IEntityTypeConfiguration<LabPatientResultDelivery>
    {
        public void Configure(EntityTypeBuilder<LabPatientResultDelivery> builder)
        {
            builder.ToTable("LabPatientResultDelivery", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Channel).HasConversion<int>().IsRequired();
            builder.Property(x => x.Outcome).HasConversion<int>().IsRequired();
            builder.Property(x => x.FailureReason).HasConversion<int?>();
            builder.Property(x => x.FailureNote).HasMaxLength(500);
            builder.Property(x => x.DestinationSnapshot).HasMaxLength(20).IsRequired();
            builder.Property(x => x.RecordedByUserId).IsRequired();
            builder.Property(x => x.RecordedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.ClientRequestId).IsRequired();

            // Angka Terkirim ke Pasien per baris daftar (02-backend-architecture.md 13.1).
            builder.HasIndex(x => new { x.DocumentId, x.Outcome });

            // VAL-172: klik ganda tidak melahirkan dua catatan.
            builder.HasIndex(x => x.ClientRequestId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasOne(x => x.Document)
                .WithMany(x => x.Deliveries)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ResultVersion)
                .WithMany()
                .HasForeignKey(x => x.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
