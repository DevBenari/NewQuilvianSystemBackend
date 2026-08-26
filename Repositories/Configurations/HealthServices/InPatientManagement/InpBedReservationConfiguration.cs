using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpBedReservationConfiguration : IEntityTypeConfiguration<InpBedReservation>
    {
        public void Configure(EntityTypeBuilder<InpBedReservation> builder)
        {
            builder.ToTable("InpBedReservation", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.ReservationStatus).HasConversion<int>();

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.BedId);
            builder.HasIndex(x => x.ExpiresAt);
            builder.HasIndex(x => x.ReservationStatus);

            // INV-INP-02 — satu tempat tidur paling banyak satu pemesanan aktif. 1 = Active.
            builder.HasIndex(x => x.BedId, "IX_InpBedReservation_BedId_Active")
                .IsUnique()
                .HasFilter("\"ReservationStatus\" = 1");

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.BedReservations)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Bed)
                .WithMany()
                .HasForeignKey(x => x.BedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReservedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReservedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
