using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.JournalManagement
{
    public class AccJournalApprovalConfiguration : IEntityTypeConfiguration<AccJournalApproval>
    {
        public void Configure(EntityTypeBuilder<AccJournalApproval> entity)
        {
            entity.ToTable("AccJournalApproval", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JournalId)
                .IsRequired();

            entity.Property(x => x.ApprovalAction)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.ActionBy)
                .IsRequired();

            entity.Property(x => x.ActionAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.Reason)
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

            // Restrict, bukan Cascade: riwayat persetujuan adalah bukti audit dan tidak boleh
            // ikut terhapus bersama jurnalnya.
            entity.HasOne(x => x.Journal)
                .WithMany(x => x.Approvals)
                .HasForeignKey(x => x.JournalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.JournalId);

            entity.HasIndex(x => x.ActionBy);
        }
    }
}
