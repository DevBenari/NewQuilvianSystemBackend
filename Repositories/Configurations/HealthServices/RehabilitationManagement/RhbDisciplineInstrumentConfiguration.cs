using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan instrumen per disiplin — <c>data-dictionary.md</c> bagian 2.20.</summary>
    /// <remarks>
    /// Unique <c>(DisciplineId, InstrumentKind)</c> memakai <c>NULLS NOT DISTINCT</c>
    /// (<c>AreNullsDistinct(false)</c>, PostgreSQL 15+) agar baris asesmen medis
    /// (<c>DisciplineId</c> kosong) juga tidak dobel.
    /// </remarks>
    public class RhbDisciplineInstrumentConfiguration : IEntityTypeConfiguration<RhbDisciplineInstrument>
    {
        public void Configure(EntityTypeBuilder<RhbDisciplineInstrument> builder)
        {
            builder.ToTable("RhbDisciplineInstrument", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.InstrumentKind).HasConversion<int>().IsRequired();
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => new { x.DisciplineId, x.InstrumentKind })
                .IsUnique()
                .AreNullsDistinct(false)
                .HasFilter("\"IsDelete\" = false");

            builder.HasOne(x => x.Discipline)
                .WithMany(x => x.Instruments)
                .HasForeignKey(x => x.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
