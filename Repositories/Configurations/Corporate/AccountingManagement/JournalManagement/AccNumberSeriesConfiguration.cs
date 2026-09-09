using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.JournalManagement
{
    public class AccNumberSeriesConfiguration : IEntityTypeConfiguration<AccNumberSeries>
    {
        public void Configure(EntityTypeBuilder<AccNumberSeries> entity)
        {
            entity.ToTable("AccNumberSeries", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SequenceKey)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ScopeKey)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ResetPolicy)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.CurrentValue)
                .IsRequired();

            entity.Property(x => x.LastAllocatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            // Kunci alokasi. Satu deret per (SequenceKey, ScopeKey) — inilah yang membuat reset
            // per bulan mungkin tanpa lock global.
            entity.HasIndex(x => new { x.SequenceKey, x.ScopeKey })
                .IsUnique();
        }
    }
}
