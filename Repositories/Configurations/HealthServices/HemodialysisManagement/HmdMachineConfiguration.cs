using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan mesin HD — <c>data-dictionary.md</c> bagian 3.15.</summary>
    /// <remarks>
    /// Unique <c>(ServiceUnitId, MachineCode)</c> tanpa filter hapus: kode mesin yang sudah
    /// dinonaktifkan tetap tercatat, sehingga riwayat sesi lama tidak pernah menunjuk dua mesin
    /// berkode sama (<c>HMD-VAL-092</c>).
    /// </remarks>
    public class HmdMachineConfiguration : IEntityTypeConfiguration<HmdMachine>
    {
        public void Configure(EntityTypeBuilder<HmdMachine> builder)
        {
            builder.ToTable("HmdMachine", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MachineCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.MachineName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Manufacturer).HasMaxLength(150);
            builder.Property(x => x.SerialNumber).HasMaxLength(100);
            builder.Property(x => x.MachineStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.DedicatedFor).HasConversion<int>().IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => new { x.ServiceUnitId, x.MachineCode }).IsUnique();
            builder.HasIndex(x => x.MachineStatus);
            builder.HasIndex(x => x.DedicatedFor);
            builder.HasIndex(x => x.IsSchedulable);
            builder.HasIndex(x => x.IsActive);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
