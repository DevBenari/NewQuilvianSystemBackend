using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgDepartureConfiguration : IEntityTypeConfiguration<EmgDeparture>
    {
        public void Configure(EntityTypeBuilder<EmgDeparture> builder)
        {
            builder.ToTable("EmgDeparture", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DepartureNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PhysicalStatus).HasConversion<int>();
            builder.Property(x => x.HandoverStatus).HasConversion<int>();
            builder.HasIndex(x => x.DepartureNumber).IsUnique();
            builder.HasIndex(x => new { x.EmergencyVisitId, x.PhysicalStatus, x.RequestedAt });
            builder.HasIndex(x => new { x.ToServiceUnitId, x.HandoverStatus });
            builder.HasIndex(x => x.FromServiceUnitId);

            builder.HasOne(x => x.EmergencyVisit).WithMany(x => x.Departures)
                .HasForeignKey(x => x.EmergencyVisitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromServiceUnit).WithMany()
                .HasForeignKey(x => x.FromServiceUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToServiceUnit).WithMany()
                .HasForeignKey(x => x.ToServiceUnitId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
