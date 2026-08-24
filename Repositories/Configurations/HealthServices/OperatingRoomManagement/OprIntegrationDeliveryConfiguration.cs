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
        builder.Property(x => x.LastErrorCode).HasMaxLength(100);
        builder.Property(x => x.AcceptedReference).HasMaxLength(150);
        builder.HasIndex(x => new { x.Destination, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => new { x.Status, x.RetryCount });
        builder.HasOne(x => x.OprCase).WithMany().HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
