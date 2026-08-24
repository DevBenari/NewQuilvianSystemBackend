using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpBedPlacementConfiguration : IEntityTypeConfiguration<InpBedPlacement>
    {
        public void Configure(EntityTypeBuilder<InpBedPlacement> builder)
        {
            builder.ToTable("InpBedPlacement", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.EndReason).HasConversion<int>();
            builder.Property(x => x.TransferReason).HasMaxLength(500);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.BedId);
            builder.HasIndex(x => x.RoomId);
            builder.HasIndex(x => x.ServiceUnitId);
            builder.HasIndex(x => x.PatientClassId);
            builder.HasIndex(x => x.StartDateTime);
            builder.HasIndex(x => x.EndDateTime);
            builder.HasIndex(x => new { x.EpisodeId, x.SequenceNumber }).IsUnique();

            // INV-INP-02 — satu tempat tidur paling banyak satu penempatan aktif.
            builder.HasIndex(x => x.BedId, "IX_InpBedPlacement_BedId_Active")
                .IsUnique()
                .HasFilter("\"EndDateTime\" IS NULL");

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.BedPlacements)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Bed)
                .WithMany()
                .HasForeignKey(x => x.BedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientClass)
                .WithMany()
                .HasForeignKey(x => x.PatientClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PlacedByUser)
                .WithMany()
                .HasForeignKey(x => x.PlacedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EndedByUser)
                .WithMany()
                .HasForeignKey(x => x.EndedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
