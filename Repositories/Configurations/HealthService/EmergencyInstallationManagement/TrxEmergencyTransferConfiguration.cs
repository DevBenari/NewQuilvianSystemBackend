using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.EmergencyInstallationManagement
{
    public class TrxEmergencyTransferConfiguration : IEntityTypeConfiguration<TrxEmergencyTransfer>
    {
        public void Configure(EntityTypeBuilder<TrxEmergencyTransfer> builder)
        {
            builder.ToTable("TrxEmergencyTransfer", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransferNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.TransferStatus).HasConversion<int>();
            builder.Property(x => x.TransferReason).HasMaxLength(1000);
            builder.Property(x => x.HandoverSummary).HasMaxLength(2000);
            builder.Property(x => x.RejectionReason).HasMaxLength(1000);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => x.TransferNumber).IsUnique();
            builder.HasIndex(x => new { x.EmergencyVisitId, x.TransferStatus, x.RequestedAt });
            builder.HasIndex(x => new { x.ToServiceUnitId, x.TransferStatus });
            builder.HasIndex(x => x.FromServiceUnitId);
            builder.HasIndex(x => x.FromRoomId);
            builder.HasIndex(x => x.ToRoomId);
            builder.HasIndex(x => x.FromBedId);
            builder.HasIndex(x => x.ToBedId);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany(x => x.Transfers)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FromServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.FromServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ToServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ToServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestedByUser)
                .WithMany()
                .HasForeignKey(x => x.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AcceptedByUser)
                .WithMany()
                .HasForeignKey(x => x.AcceptedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SendingNurseUser)
                .WithMany()
                .HasForeignKey(x => x.SendingNurseUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReceivingNurseUser)
                .WithMany()
                .HasForeignKey(x => x.ReceivingNurseUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
