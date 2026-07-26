using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class TrxRosterPublicationConfiguration : IEntityTypeConfiguration<TrxRosterPublication>
    {
        public void Configure(EntityTypeBuilder<TrxRosterPublication> entity)
        {
            entity.ToTable("TrxRosterPublication", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PublicationNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PublicationStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.PublicationChannel).HasMaxLength(30).HasDefaultValue("Application");
            entity.Property(x => x.PublishedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AudienceDefinitionJson).HasColumnType("jsonb");
            entity.Property(x => x.PublicationSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.RosterPeriod).WithMany(x => x.Publications).HasForeignKey(x => x.RosterPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PublishedByUser).WithMany().HasForeignKey(x => x.PublishedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SupersededByPublication).WithMany().HasForeignKey(x => x.SupersededByPublicationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.PublicationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.RosterPeriodId, x.VersionNumber }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.PublicationStatus, x.PublishedAt, x.IsDelete });
        }
    }
}
