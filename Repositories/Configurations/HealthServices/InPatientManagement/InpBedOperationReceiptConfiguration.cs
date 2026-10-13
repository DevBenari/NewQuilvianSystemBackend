using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement;

public class InpBedOperationReceiptConfiguration : IEntityTypeConfiguration<InpBedOperationReceipt>
{
    public void Configure(EntityTypeBuilder<InpBedOperationReceipt> builder)
    {
        builder.ToTable("InpBedOperationReceipt", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CommittedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.ActorUserId, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => x.CommittedAtUtc);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OperationName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ResultKind).HasMaxLength(50).IsRequired();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

