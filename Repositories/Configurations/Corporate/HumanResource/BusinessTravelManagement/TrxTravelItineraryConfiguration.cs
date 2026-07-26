using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelItineraryConfiguration : IEntityTypeConfiguration<TrxTravelItinerary>
    {
        public void Configure(EntityTypeBuilder<TrxTravelItinerary> entity)
        {
            entity.ToTable("TrxTravelItinerary", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ItineraryDate).HasColumnType("date");
            entity.Property(x => x.PlannedStartAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PlannedEndAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ActualStartAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ActualEndAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Origin).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Destination).HasMaxLength(250).IsRequired();
            entity.Property(x => x.ActivityDescription).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.TransportMode).HasMaxLength(100);
            entity.Property(x => x.VenueAddress).HasMaxLength(500);
            entity.Property(x => x.ContactPersonName).HasMaxLength(150);
            entity.Property(x => x.ContactPersonPhone).HasMaxLength(100);
            entity.Property(x => x.ItineraryStatus).HasMaxLength(30).HasDefaultValue("Planned").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.BusinessTravelRequest).WithMany(x => x.Itineraries).HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessTravelParticipant).WithMany(x => x.Itineraries).HasForeignKey(x => x.BusinessTravelParticipantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DestinationZone).WithMany().HasForeignKey(x => x.DestinationZoneId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.SequenceNumber, x.IsDelete });
            entity.HasIndex(x => new { x.ItineraryDate, x.ItineraryStatus, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelItinerary> entity)
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
