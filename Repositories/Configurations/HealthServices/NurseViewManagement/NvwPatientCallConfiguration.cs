using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.NurseViewManagement
{
    public class NvwPatientCallConfiguration : IEntityTypeConfiguration<NvwPatientCall>
    {
        public void Configure(EntityTypeBuilder<NvwPatientCall> builder)
        {
            builder.ToTable("NvwPatientCall", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FloorCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Priority).HasConversion<int>().IsRequired();
            builder.Property(x => x.CallStatus)
                .HasConversion<int>()
                .HasDefaultValue(NvwPatientCallStatus.Waiting)
                .IsRequired();
            builder.Property(x => x.FirstPressedAt).IsRequired();
            builder.Property(x => x.LastPressedAt).IsRequired();
            builder.Property(x => x.PressCount).HasDefaultValue(1).IsRequired();
            builder.Property(x => x.CloseReason).HasConversion<int?>();
            builder.Property(x => x.CloseNote).HasMaxLength(250);
            builder.Property(x => x.IsBedsideConfirmed).HasDefaultValue(false).IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasOne(x => x.Bed)
                .WithMany()
                .HasForeignKey(x => x.BedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BedTablet)
                .WithMany()
                .HasForeignKey(x => x.BedTabletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BedPlacement)
                .WithMany()
                .HasForeignKey(x => x.InpBedPlacementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.BedId, "IX_NvwPatientCall_BedId_Active")
                .IsUnique()
                .HasFilter("\"CallStatus\" IN (1, 2) AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.FloorCode, x.CallStatus });
            builder.HasIndex(x => x.BedTabletId);
            builder.HasIndex(x => x.InpBedPlacementId);
            builder.HasIndex(x => x.FirstPressedAt);
        }
    }
}
