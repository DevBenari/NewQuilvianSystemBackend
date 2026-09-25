using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabFieldChangeLogConfiguration : IEntityTypeConfiguration<LabFieldChangeLog>
    {
        public void Configure(EntityTypeBuilder<LabFieldChangeLog> builder)
        {
            builder.ToTable("LabFieldChangeLog", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.EntityId).IsRequired();
            builder.Property(x => x.FieldName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.OldValue).HasMaxLength(500);
            builder.Property(x => x.NewValue).HasMaxLength(500);
            builder.Property(x => x.ChangedAt).IsRequired();

            // NOL CASCADE DARI MANA PUN, dan itu disengaja.
            //
            // Menghapus specimen lalu ikut menghapus jejak perubahannya membuat jejak itu
            // hilang tepat pada saat ia paling dibutuhkan. Pola yang sama dipakai
            // LabTransitionHistory.
            //
            // Konsekuensinya EntityId nol ber-foreign key — itu harga yang dibayar supaya
            // tabel ini dapat melayani lebih dari satu entity sekaligus.

            builder.HasIndex(x => new { x.EntityName, x.EntityId, x.ChangedAt })
                .HasDatabaseName("IX_LabFieldChangeLog_Entity_ChangedAt");
        }
    }
}
