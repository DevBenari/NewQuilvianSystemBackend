using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelTransportationConfiguration : IEntityTypeConfiguration<TrxTravelTransportation>
    {
        public void Configure(EntityTypeBuilder<TrxTravelTransportation> entity)
        {
            entity.ToTable("TrxTravelTransportation", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TransportationType).HasMaxLength(50).HasDefaultValue("Air").IsRequired();
            entity.Property(x => x.ProviderName).HasMaxLength(200);
            entity.Property(x => x.BookingReference).HasMaxLength(100);
            entity.Property(x => x.TicketNumber).HasMaxLength(100);
            entity.Property(x => x.Origin).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Destination).HasMaxLength(250).IsRequired();
            entity.Property(x => x.DepartureAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ArrivalAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EstimatedAmount).HasPrecision(18, 2);
            entity.Property(x => x.ActualAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.TicketFilePath).HasMaxLength(500);
            entity.Property(x => x.TicketFileName).HasMaxLength(255);
            entity.Property(x => x.TransportationStatus).HasMaxLength(30).HasDefaultValue("Planned").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.BusinessTravelRequest).WithMany(x => x.Transportations).HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessTravelParticipant).WithMany(x => x.Transportations).HasForeignKey(x => x.BusinessTravelParticipantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelItinerary).WithMany(x => x.Transportations).HasForeignKey(x => x.TravelItineraryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelClass).WithMany().HasForeignKey(x => x.TravelClassId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.DepartureAt, x.TransportationStatus, x.IsDelete });
            entity.HasIndex(x => new { x.BookingReference, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelTransportation> entity)
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
