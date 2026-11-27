using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabOrderRescheduleConfiguration : IEntityTypeConfiguration<LabOrderReschedule>
    {
        public void Configure(EntityTypeBuilder<LabOrderReschedule> builder)
        {
            builder.ToTable("LabOrderReschedule", "public");
            builder.HasKey(x => x.Id);

            // Tanggal murni, bukan tengah malam UTC (LAB-DEC-263): tanggal rencana adalah hari
            // kalender WIB dan tidak boleh bergeser satu hari karena zona waktu.
            builder.Property(x => x.PreviousPlannedDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.NewPlannedDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.Reason)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.ActorUserId)
                .IsRequired();

            builder.Property(x => x.OccurredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Riwayat satu pesanan, terbaru di atas, dan jumlah penjadwalan ulangnya.
            builder.HasIndex(x => new { x.LabOrderId, x.OccurredAt });

            // Restrict: riwayat adalah jejak permanen; pesanan yang masih punya riwayat tidak
            // boleh terhapus berantai.
            builder.HasOne(x => x.LabOrder)
                .WithMany(x => x.Reschedules)
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
