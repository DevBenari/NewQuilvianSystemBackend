using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprIntegrationDeliveryConfiguration : IEntityTypeConfiguration<OprIntegrationDelivery>
{
    public void Configure(EntityTypeBuilder<OprIntegrationDelivery> builder)
    {
        builder.ToTable("OprIntegrationDelivery", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Destination).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MessageType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PayloadReference).HasMaxLength(250).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.EventVersion).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.LastErrorCode).HasMaxLength(100);
        builder.Property(x => x.AcceptedReference).HasMaxLength(150);
        builder.HasIndex(x => new { x.Destination, x.IdempotencyKey }).IsUnique();

        // Satu kejadian bisnis hanya boleh menghasilkan satu pesan, dan itu dijaga basis data,
        // bukan hanya oleh pemeriksaan di service. Dua permintaan yang benar-benar bersamaan
        // membuat pemeriksaan di memori sama-sama lolos; indeks inilah yang menggagalkan yang
        // kedua.
        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.RetryCount });
        builder.HasOne(x => x.OprCase).WithMany().HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
