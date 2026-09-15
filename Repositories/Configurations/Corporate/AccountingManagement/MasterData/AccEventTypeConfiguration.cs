using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.MasterData
{
    public class AccEventTypeConfiguration : IEntityTypeConfiguration<AccEventType>
    {
        public void Configure(EntityTypeBuilder<AccEventType> entity)
        {
            entity.ToTable("AccEventType", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EventTypeCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EventTypeName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.SourceModule)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
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

            // Unik global: jenis kejadian berlaku sama untuk semua badan hukum, sama seperti
            // AccJournalType. Kode inilah yang dicocokkan dengan pesan kejadian.
            entity.HasIndex(x => x.EventTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.EventTypeName);

            entity.HasIndex(x => new { x.IsActive, x.IsDelete });
        }
    }
}
