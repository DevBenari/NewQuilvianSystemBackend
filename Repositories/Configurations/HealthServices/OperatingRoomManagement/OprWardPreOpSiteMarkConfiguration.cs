using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

/// <summary><c>OprWardPreOpSiteMark</c> — kamus data 19.6 dan DDL 19.12 (<c>BE-RWI-176</c>).</summary>
public class OprWardPreOpSiteMarkConfiguration : IEntityTypeConfiguration<OprWardPreOpSiteMark>
{
    public void Configure(EntityTypeBuilder<OprWardPreOpSiteMark> builder)
    {
        builder.ToTable("OprWardPreOpSiteMark", "public", table =>
        {
            table.HasCheckConstraint("CK_OprWardPreOpSiteMark_X", "\"X\" BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_OprWardPreOpSiteMark_Y", "\"Y\" BETWEEN 0 AND 100");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BodyView).IsRequired();
        builder.Property(x => x.X).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.Y).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(100);

        builder.HasIndex(x => x.NoteId);
        builder.HasOne(x => x.PreOpNote).WithMany(x => x.SiteMarks).HasForeignKey(x => x.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
