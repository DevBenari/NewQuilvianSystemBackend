using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan sampel pemeriksaan golongan darah.</summary>
    /// <remarks>
    /// <b>Index unik pada <c>SampleIdentifier</c> adalah penjaga terakhirnya.</b> Nilai itu
    /// ditulis petugas, sehingga pemeriksaan di service menahan kekeliruan biasa tetapi
    /// menyisakan celah balapan ketika dua petugas menyimpan identifier yang sama pada saat
    /// hampir bersamaan. Index inilah yang menutup celah itu di database.
    /// </remarks>
    public class BbkBloodGroupSampleConfiguration : IEntityTypeConfiguration<BbkBloodGroupSample>
    {
        public void Configure(EntityTypeBuilder<BbkBloodGroupSample> builder)
        {
            builder.ToTable("BbkBloodGroupSample", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BloodGroupExamId).IsRequired();
            builder.Property(x => x.SampleIdentifier).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TakenByUserId).IsRequired();
            builder.Property(x => x.TakenAt).IsRequired();

            builder.HasIndex(x => x.BloodGroupExamId);
            builder.HasIndex(x => x.SampleIdentifier).IsUnique();
        }
    }
}
