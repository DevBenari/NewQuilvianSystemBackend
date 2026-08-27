using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgDepartureEventConfiguration : IEntityTypeConfiguration<EmgDepartureEvent>
    {
        public void Configure(EntityTypeBuilder<EmgDepartureEvent> builder)
        {
            builder.ToTable("EmgDepartureEvent", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EventType).HasConversion<int>();
            builder.HasIndex(x => new { x.EmergencyDepartureId, x.OccurredAt });
            builder.HasIndex(x => new { x.EmergencyDepartureId, x.IsEffective });
            builder.HasOne(x => x.EmergencyDeparture).WithMany(x => x.Events)
                .HasForeignKey(x => x.EmergencyDepartureId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SupersedesEvent).WithMany()
                .HasForeignKey(x => x.SupersedesEventId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
