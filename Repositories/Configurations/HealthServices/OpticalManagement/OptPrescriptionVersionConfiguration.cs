using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptPrescriptionVersion</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptPrescriptionVersionConfiguration : IEntityTypeConfiguration<OptPrescriptionVersion>
    {
        public void Configure(EntityTypeBuilder<OptPrescriptionVersion> builder)
        {
            builder.ToTable("OptPrescriptionVersion", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OdSph).HasPrecision(5, 2);
            builder.Property(x => x.OdCyl).HasPrecision(5, 2);
            builder.Property(x => x.OdAdd).HasPrecision(4, 2);
            builder.Property(x => x.OsSph).HasPrecision(5, 2);
            builder.Property(x => x.OsCyl).HasPrecision(5, 2);
            builder.Property(x => x.OsAdd).HasPrecision(4, 2);
            builder.Property(x => x.PdDistance).HasPrecision(4, 1);
            builder.Property(x => x.PdNear).HasPrecision(4, 1);
            builder.Property(x => x.OdClPower).HasPrecision(5, 2);
            builder.Property(x => x.OdClCyl).HasPrecision(5, 2);
            builder.Property(x => x.OdClBaseCurve).HasPrecision(4, 2);
            builder.Property(x => x.OdClDiameter).HasPrecision(4, 2);
            builder.Property(x => x.OsClPower).HasPrecision(5, 2);
            builder.Property(x => x.OsClCyl).HasPrecision(5, 2);
            builder.Property(x => x.OsClBaseCurve).HasPrecision(4, 2);
            builder.Property(x => x.OsClDiameter).HasPrecision(4, 2);

            builder.HasIndex(x => new { x.PrescriptionId, x.VersionNo }).IsUnique().HasDatabaseName("UX_OptPrescriptionVersion_PrescriptionId_VersionNo");
            builder.HasIndex(x => x.AuthorWorkforceProfileId).HasDatabaseName("IX_OptPrescriptionVersion_AuthorWorkforceProfileId");

            builder.HasOne(x => x.Prescription)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstWorkforceProfile>()
                .WithMany()
                .HasForeignKey(x => x.AuthorWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
