using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>Catatan run impor denah (BE-RWI-237, kamus data 22.2.3, migration M-DNH-3).</summary>
    public class MstFloorPlanImportRunConfiguration : IEntityTypeConfiguration<MstFloorPlanImportRun>
    {
        public void Configure(EntityTypeBuilder<MstFloorPlanImportRun> entity)
        {
            // INV-DNH-07: penetapan siap selalu disertai referensi bukti.
            entity.ToTable("MstFloorPlanImportRun", "public", t =>
                t.HasCheckConstraint("CK_MstFloorPlanImportRun_ReadyEvidence", "NOT \"MarkedBedsReady\" OR \"ReadyEvidenceReference\" IS NOT NULL"));

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ManifestVersion).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ManifestHash).HasMaxLength(64).IsFixedLength().IsRequired();
            entity.Property(x => x.EnvironmentName).HasMaxLength(32).IsRequired();
            entity.Property(x => x.MarkedBedsReady).HasDefaultValue(false);
            entity.Property(x => x.ReadyEvidenceReference).HasMaxLength(200);
            entity.Property(x => x.SummaryJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.StartedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedAtUtc).HasColumnType("timestamp with time zone");

            entity.HasIndex(x => x.ManifestHash);
            entity.HasIndex(x => x.StartedAtUtc);

            entity.HasOne(x => x.ActorUser)
                .WithMany()
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
