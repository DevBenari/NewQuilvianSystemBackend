using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan pemantauan berkala — <c>data-dictionary.md</c> bagian 3.11.</summary>
    /// <remarks>
    /// Unique <c>(SessionId, SequenceNumber)</c> mencegah dua pengamatan merebut nomor urut yang
    /// sama, sekaligus memastikan riwayat tidak saling menimpa.
    /// </remarks>
    public class HmdSessionObservationConfiguration : IEntityTypeConfiguration<HmdSessionObservation>
    {
        public void Configure(EntityTypeBuilder<HmdSessionObservation> builder)
        {
            builder.ToTable("HmdSessionObservation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TemperatureC).HasPrecision(4, 1);
            builder.Property(x => x.Note).HasMaxLength(1000);

            builder.HasIndex(x => new { x.SessionId, x.SequenceNumber }).IsUnique();
            builder.HasIndex(x => new { x.SessionId, x.ObservedAt });
            builder.HasIndex(x => x.RecordedByUserId);

            builder.HasOne(x => x.Session)
                .WithMany(x => x.Observations)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
