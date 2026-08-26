using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpCorrectionSessionConfiguration : IEntityTypeConfiguration<InpCorrectionSession>
    {
        public void Configure(EntityTypeBuilder<InpCorrectionSession> builder)
        {
            builder.ToTable("InpCorrectionSession", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.OpenReason).IsRequired().HasMaxLength(500);
            builder.Property(x => x.ChangedFieldSummary).HasMaxLength(4000);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.OpenedAt);
            builder.HasIndex(x => x.ClosedAt);
            builder.HasIndex(x => new { x.EpisodeId, x.SequenceNumber }).IsUnique();

            builder.HasIndex(x => x.EpisodeId, "IX_InpCorrectionSession_EpisodeId_Open")
                .IsUnique()
                .HasFilter("\"ClosedAt\" IS NULL");

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.CorrectionSessions)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OpenedByUser)
                .WithMany()
                .HasForeignKey(x => x.OpenedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
