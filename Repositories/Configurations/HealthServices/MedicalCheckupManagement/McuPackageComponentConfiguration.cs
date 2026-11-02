using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan komponen paket — <c>data-dictionary.md</c> bagian 1.</summary>
    /// <remarks>Satu prosedur dan satu jenis Pemeriksaan MCU hanya sekali per paket.</remarks>
    public class McuPackageComponentConfiguration : IEntityTypeConfiguration<McuPackageComponent>
    {
        public void Configure(EntityTypeBuilder<McuPackageComponent> builder)
        {
            builder.ToTable("McuPackageComponent", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ComponentType).HasConversion<int>().IsRequired();
            builder.Property(x => x.RefundAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(x => x.MedicalFeeAmount).HasPrecision(18, 2).HasDefaultValue(0m);

            builder.HasIndex(x => new { x.PackageId, x.ProcedureId })
                .IsUnique()
                .HasFilter("\"ProcedureId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.PackageId, x.ExaminationTypeId })
                .IsUnique()
                .HasFilter("\"ExaminationTypeId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => x.RadModalityId);

            builder.HasOne(x => x.Package)
                .WithMany(x => x.Components)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RadModality)
                .WithMany()
                .HasForeignKey(x => x.RadModalityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExaminationType)
                .WithMany()
                .HasForeignKey(x => x.ExaminationTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
