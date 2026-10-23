using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary>Riwayat append-only High/Low Dependency (BE-RWI-234, kamus data 22.2.5, M-DNH-2).</summary>
    public class InpCareDependencyHistoryConfiguration : IEntityTypeConfiguration<InpCareDependencyHistory>
    {
        public void Configure(EntityTypeBuilder<InpCareDependencyHistory> builder)
        {
            builder.ToTable("InpCareDependencyHistory", "public", t =>
            {
                t.HasCheckConstraint("CK_InpCareDependencyHistory_PreviousLevel", "\"PreviousLevel\" IS NULL OR \"PreviousLevel\" BETWEEN 1 AND 2");
                t.HasCheckConstraint("CK_InpCareDependencyHistory_NewLevel", "\"NewLevel\" BETWEEN 1 AND 2");
                t.HasCheckConstraint("CK_InpCareDependencyHistory_Source", "\"Source\" BETWEEN 1 AND 3");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PreviousLevel).HasConversion<int>();
            builder.Property(x => x.NewLevel).HasConversion<int>();
            builder.Property(x => x.Source).HasConversion<int>();
            builder.Property(x => x.Note).HasMaxLength(500);
            builder.Property(x => x.RecordedAtUtc).HasColumnType("timestamp with time zone");

            // Urutan stabil riwayat: terbaru dulu per episode.
            builder.HasIndex(x => new { x.EpisodeId, x.RecordedAtUtc, x.Id });

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.CareDependencyHistories)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ActorUser)
                .WithMany()
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
