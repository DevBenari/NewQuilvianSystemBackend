using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgAdmissionRequestEventConfiguration : IEntityTypeConfiguration<EmgAdmissionRequestEvent>
    {
        public void Configure(EntityTypeBuilder<EmgAdmissionRequestEvent> builder)
        {
            builder.ToTable("EmgAdmissionRequestEvent", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventType).HasConversion<int>().IsRequired();
            builder.Property(x => x.FromStatus).HasConversion<int?>();
            builder.Property(x => x.ToStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.OccurredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(
                x => new { x.AdmissionRequestId, x.OccurredAt },
                "IX_EmgAdmissionRequestEvent_Request_OccurredAt");
        }
    }
}
