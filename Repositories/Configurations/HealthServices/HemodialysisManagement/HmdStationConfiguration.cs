using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan station HD — <c>data-dictionary.md</c> bagian 3.17.</summary>
    public class HmdStationConfiguration : IEntityTypeConfiguration<HmdStation>
    {
        public void Configure(EntityTypeBuilder<HmdStation> builder)
        {
            builder.ToTable("HmdStation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StationCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.StationName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.StationStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusReason).HasMaxLength(500);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => new { x.ServiceUnitId, x.StationCode }).IsUnique();
            builder.HasIndex(x => x.RoomId);
            builder.HasIndex(x => x.StationStatus);
            builder.HasIndex(x => x.IsIsolationStation);
            builder.HasIndex(x => x.IsActive);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
