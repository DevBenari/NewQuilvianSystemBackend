using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary><c>InpAdmissionPrintLog</c> — kamus data 20.13 dan DDL 20.18 (<c>BE-RWI-192</c>).</summary>
    public class InpAdmissionPrintLogConfiguration : IEntityTypeConfiguration<InpAdmissionPrintLog>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionPrintLog> builder)
        {
            builder.ToTable("InpAdmissionPrintLog", "public", table =>
            {
                // INV-RWA-09: cetak ulang selalu beralasan; alasan "lainnya" berketerangan; cetak
                // dokumen selalu menunjuk dokumennya; salinan 1–10.
                table.HasCheckConstraint("CK_InpAdmissionPrintLog_Reprint",
                    "(\"IsReprint\" = false OR \"ReprintReason\" IS NOT NULL) AND " +
                    "(\"ReprintReason\" IS DISTINCT FROM 4 OR \"ReprintNote\" IS NOT NULL) AND " +
                    "(\"PrintKind\" <> 5 OR \"DocumentId\" IS NOT NULL) AND " +
                    "(\"Copies\" BETWEEN 1 AND 10)");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PrintKind).HasConversion<int>().IsRequired();
            builder.Property(x => x.DocumentStatusAtPrint).HasConversion<int?>();
            builder.Property(x => x.Copies).IsRequired().HasDefaultValue(1);
            builder.Property(x => x.IsReprint).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.ReprintReason).HasConversion<int?>();
            builder.Property(x => x.ReprintNote).HasMaxLength(200);
            builder.Property(x => x.PrintedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.IdempotencyKey).HasMaxLength(80);

            builder.HasIndex(x => new { x.EpisodeId, x.PrintKind, x.PrintedAt });
            builder.HasIndex(x => new { x.DocumentId, x.PrintedAt });
            builder.HasIndex(x => x.IdempotencyKey, "UX_InpAdmissionPrintLog_IdempotencyKey")
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL AND NOT \"IsDelete\"");

            builder.HasOne(x => x.Episode).WithMany().HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Document).WithMany().HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
