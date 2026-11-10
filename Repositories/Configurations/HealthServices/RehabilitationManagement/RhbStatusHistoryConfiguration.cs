using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan riwayat status — <c>data-dictionary.md</c> bagian 2.17. Polimorfik, tanpa FK.</summary>
    public class RhbStatusHistoryConfiguration : IEntityTypeConfiguration<RhbStatusHistory>
    {
        public void Configure(EntityTypeBuilder<RhbStatusHistory> builder)
        {
            builder.ToTable("RhbStatusHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AggregateType).HasMaxLength(40).IsRequired();
            builder.Property(x => x.FromStatus).HasMaxLength(40);
            builder.Property(x => x.ToStatus).HasMaxLength(40).IsRequired();
            builder.Property(x => x.Action).HasMaxLength(40).IsRequired();
            builder.Property(x => x.ReasonText).HasMaxLength(1000);

            builder.HasIndex(x => new { x.AggregateType, x.AggregateId, x.OccurredAt });
        }
    }
}
