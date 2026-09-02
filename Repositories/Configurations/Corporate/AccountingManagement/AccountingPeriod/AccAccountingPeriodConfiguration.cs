using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.AccountingPeriod
{
    public class AccAccountingPeriodConfiguration : IEntityTypeConfiguration<AccAccountingPeriod>
    {
        public void Configure(EntityTypeBuilder<AccAccountingPeriod> entity)
        {
            entity.ToTable("AccAccountingPeriod", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.PeriodCode)
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(x => x.FiscalYear)
                .IsRequired();

            entity.Property(x => x.PeriodMonth)
                .IsRequired();

            entity.Property(x => x.StartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EndDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.PeriodStatus)
                .HasConversion<int>()
                .HasDefaultValue(AccountingPeriodStatus.Open)
                .IsRequired();

            entity.Property(x => x.ClosedBy)
                .IsRequired(false);

            entity.Property(x => x.ClosedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ReopenedBy)
                .IsRequired(false);

            entity.Property(x => x.ReopenedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastReasonNote)
                .HasMaxLength(500);

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

            // MstLegalEntity dirujuk saja dan tidak disalin. Sengaja memakai WithMany() tanpa
            // navigasi balik supaya entity milik Human Resource tidak perlu disentuh — pola yang
            // sama dipakai AccChartOfAccount pada BE-ACC-003.
            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu badan hukum hanya boleh punya satu periode per kode (ACC-DEC-037).
            entity.HasIndex(x => new { x.LegalEntityId, x.PeriodCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.FiscalYear);

            entity.HasIndex(x => x.PeriodStatus);
        }
    }
}
