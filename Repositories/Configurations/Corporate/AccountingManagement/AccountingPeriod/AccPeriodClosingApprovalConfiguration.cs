using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.AccountingPeriod
{
    public class AccPeriodClosingApprovalConfiguration : IEntityTypeConfiguration<AccPeriodClosingApproval>
    {
        public void Configure(EntityTypeBuilder<AccPeriodClosingApproval> entity)
        {
            entity.ToTable("AccPeriodClosingApproval", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AccountingPeriodId)
                .IsRequired();

            entity.Property(x => x.ActionSequence)
                .IsRequired();

            entity.Property(x => x.Action)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.ActionBy)
                .IsRequired();

            entity.Property(x => x.ActionAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.ActionNote)
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

            // Restrict, bukan Cascade: riwayat penutupan adalah bukti audit dan tidak boleh ikut
            // terhapus bersama periodenya. Pola yang sama dipakai AccJournalApproval.
            entity.HasOne(x => x.AccountingPeriod)
                .WithMany(x => x.ClosingApprovals)
                .HasForeignKey(x => x.AccountingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu periode tidak boleh punya dua tindakan bernomor urut sama. Inilah yang
            // mencegah dua pengajuan bersamaan menghasilkan riwayat yang saling menimpa.
            entity.HasIndex(x => new { x.AccountingPeriodId, x.ActionSequence })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ActionBy);
        }
    }
}
