using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement;

public class InpBedLifecycleEventConfiguration : IEntityTypeConfiguration<InpBedLifecycleEvent>
{
    public void Configure(EntityTypeBuilder<InpBedLifecycleEvent> builder)
    {
        builder.ToTable("InpBedLifecycleEvent", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OccurredAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.RecordedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.ReadinessId, x.OccurredAtUtc, x.Id });
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.EvidenceReference).HasMaxLength(200);
        builder.HasOne<InpBedReadiness>().WithMany().HasForeignKey(x => x.ReadinessId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InpBedOperationReceipt>().WithMany().HasForeignKey(x => x.OperationReceiptId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InpBedPlacement>().WithMany().HasForeignKey(x => x.PlacementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InpBedReservation>().WithMany().HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InpBedCleaningAttempt>().WithMany().HasForeignKey(x => x.CleaningAttemptId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

