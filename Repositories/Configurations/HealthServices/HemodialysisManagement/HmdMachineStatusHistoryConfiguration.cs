using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan riwayat status mesin — <c>data-dictionary.md</c> bagian 3.16.</summary>
    public class HmdMachineStatusHistoryConfiguration : IEntityTypeConfiguration<HmdMachineStatusHistory>
    {
        public void Configure(EntityTypeBuilder<HmdMachineStatusHistory> builder)
        {
            builder.ToTable("HmdMachineStatusHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FromStatus).HasConversion<int?>();
            builder.Property(x => x.ToStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();

            builder.HasIndex(x => new { x.MachineId, x.ChangedAt });
            builder.HasIndex(x => x.ToStatus);
            builder.HasIndex(x => x.ChangedByUserId);

            builder.HasOne(x => x.Machine)
                .WithMany(x => x.StatusHistories)
                .HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
