using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpClearanceMarkConfiguration : IEntityTypeConfiguration<InpClearanceMark>
    {
        public void Configure(EntityTypeBuilder<InpClearanceMark> builder)
        {
            builder.ToTable("InpClearanceMark", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => x.ClearanceItemId);
            builder.HasIndex(x => new { x.EpisodeId, x.ClearanceItemId }).IsUnique();

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.ClearanceMarks)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ClearanceItem)
                .WithMany()
                .HasForeignKey(x => x.ClearanceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MarkedByUser)
                .WithMany()
                .HasForeignKey(x => x.MarkedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
