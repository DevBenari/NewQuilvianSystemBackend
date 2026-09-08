using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.MasterData
{
    public class AccJournalTypeConfiguration : IEntityTypeConfiguration<AccJournalType>
    {
        public void Configure(EntityTypeBuilder<AccJournalType> entity)
        {
            entity.ToTable("AccJournalType", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JournalTypeCode)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(x => x.JournalTypeName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.NumberPrefix)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(x => x.RequiresApproval)
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(x => x.IsSystemType)
                .HasDefaultValue(false)
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

            // Unik global: jenis jurnal berlaku sama untuk semua badan hukum, jadi tidak
            // digabung dengan LegalEntityId.
            entity.HasIndex(x => x.JournalTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.IsActive, x.IsDelete });
        }
    }
}
