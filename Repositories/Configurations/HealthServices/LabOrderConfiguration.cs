using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
    {
        public void Configure(EntityTypeBuilder<LabOrder> entity)
        {
            entity.ToTable("LabOrder", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.ProcedureId)
                .IsRequired();

            // RJ-BIL-BE-003: siklus hidup operasional pesanan laboratorium.
            entity.Property(x => x.OrderStatus)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.StatusBeforeHold)
                .HasConversion<int>();

            // Dua petugas yang memindahkan status pesanan yang sama secara bersamaan tidak
            // boleh sama-sama berhasil.
            entity.Property(x => x.Version)
                .IsConcurrencyToken();

            entity.HasIndex(x => x.EncounterId);

            entity.HasIndex(x => x.ProcedureId);

            entity.HasIndex(x => x.OrderStatus);

            entity.HasMany(x => x.Specimens)
                .WithOne(x => x.LabOrder)
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
