using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgPreArrivalReservationConfiguration : IEntityTypeConfiguration<EmgPreArrivalReservation>
    {
        public void Configure(EntityTypeBuilder<EmgPreArrivalReservation> builder)
        {
            builder.ToTable("EmgPreArrivalReservation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReservationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.CallerName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.CallerPhoneNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.EstimatedArrivalDateTime)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.ChiefComplaint)
                .HasMaxLength(1000);

            builder.Property(x => x.AmbulancePickupAddress)
                .HasMaxLength(500);

            builder.Property(x => x.AssignedEscortStaffName)
                .HasMaxLength(150);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(EmergencyPreArrivalStatus.PendingArrival);

            builder.Property(x => x.ArrivedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.CancellationReason)
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.Property(x => x.RowVersion)
                .IsConcurrencyToken();

            builder.HasIndex(x => x.ReservationNumber)
                .IsUnique();

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.PatientId);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientMembership)
                .WithMany()
                .HasForeignKey(x => x.PatientMembershipId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MembershipTier)
                .WithMany()
                .HasForeignKey(x => x.MembershipTierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AmbulanceArrivalMode)
                .WithMany()
                .HasForeignKey(x => x.AmbulanceArrivalModeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ResultEmergencyVisit)
                .WithMany()
                .HasForeignKey(x => x.ResultEmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
