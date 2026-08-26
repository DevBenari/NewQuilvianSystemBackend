using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpStatusHistoryConfiguration : IEntityTypeConfiguration<InpStatusHistory>
    {
        public void Configure(EntityTypeBuilder<InpStatusHistory> builder)
        {
            builder.ToTable("InpStatusHistory", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.FromStatus).HasConversion<int>();
            builder.Property(x => x.ToStatus).HasConversion<int>();
            builder.Property(x => x.ActorType).HasConversion<int>();
            builder.Property(x => x.ActionType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Reason).HasMaxLength(1000);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.ToStatus);
            builder.HasIndex(x => x.ActorType);
            builder.HasIndex(x => x.ChangedByUserId);
            builder.HasIndex(x => x.ChangedAt);
            builder.HasIndex(x => new { x.EpisodeId, x.SequenceNumber }).IsUnique();

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.StatusHistories)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
