using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelAccommodationConfiguration : IEntityTypeConfiguration<TrxTravelAccommodation>
    {
        public void Configure(EntityTypeBuilder<TrxTravelAccommodation> entity)
        {
            entity.ToTable("TrxTravelAccommodation", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccommodationName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.AccommodationAddress).HasMaxLength(500);
            entity.Property(x => x.BookingReference).HasMaxLength(100);
            entity.Property(x => x.RoomType).HasMaxLength(100);
            entity.Property(x => x.CheckInAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CheckOutAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EstimatedAmount).HasPrecision(18, 2);
            entity.Property(x => x.ActualAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.AccommodationStatus).HasMaxLength(30).HasDefaultValue("Planned").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.BusinessTravelRequest).WithMany(x => x.Accommodations).HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessTravelParticipant).WithMany(x => x.Accommodations).HasForeignKey(x => x.BusinessTravelParticipantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DestinationZone).WithMany().HasForeignKey(x => x.DestinationZoneId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelClass).WithMany().HasForeignKey(x => x.TravelClassId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.CheckInAt, x.CheckOutAt, x.IsDelete });
            entity.HasIndex(x => new { x.BookingReference, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelAccommodation> entity)
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
