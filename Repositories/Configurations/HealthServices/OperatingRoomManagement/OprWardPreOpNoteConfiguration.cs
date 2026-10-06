using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

/// <summary>
/// <c>OprWardPreOpNote</c> — kamus data 19.4 dan DDL 19.12 (<c>BE-RWI-176</c>, migration <c>E5</c>
/// bagian pra-operasi, sesudah <c>E4</c>).
/// </summary>
public class OprWardPreOpNoteConfiguration : IEntityTypeConfiguration<OprWardPreOpNote>
{
    public void Configure(EntityTypeBuilder<OprWardPreOpNote> builder)
    {
        builder.ToTable("OprWardPreOpNote", "public", table =>
        {
            table.HasCheckConstraint("CK_OprWardPreOpNote_TwoAccounts",
                "\"ConfirmedByUserId\" IS NULL OR \"ConfirmedByUserId\" <> \"SentByUserId\"");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VersionNumber).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.Status).IsRequired().HasDefaultValue(OprWardPreOpStatus.Draft);
        builder.Property(x => x.VitalSnapshotJson).HasColumnType("jsonb");
        builder.Property(x => x.PainSnapshotJson).HasColumnType("jsonb");
        builder.Property(x => x.MarkingLaterality).HasMaxLength(30);
        builder.Property(x => x.MarkingLocationNote).HasMaxLength(500);
        builder.Property(x => x.SiteMarkingConfirmed).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.SentAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.ConfirmedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NeedsUpdateAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Version).IsRequired().HasDefaultValue(0).IsConcurrencyToken();

        builder.HasIndex(x => new { x.OprCaseId, x.VersionNumber }, "UX_OprWardPreOpNote_Case_Version").IsUnique();
        // Paling banyak satu versi berjalan (Draft, Sent, Confirmed) per kasus.
        builder.HasIndex(x => x.OprCaseId, "UX_OprWardPreOpNote_OneOpen")
            .IsUnique()
            .HasFilter("\"Status\" IN (1, 2, 3) AND NOT \"IsDelete\"");
        builder.HasIndex(x => new { x.OprCaseId, x.Status });
        builder.HasIndex(x => x.PreviousVersionId);

        builder.HasOne(x => x.OprCase).WithMany().HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PreviousVersion).WithMany().HasForeignKey(x => x.PreviousVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
