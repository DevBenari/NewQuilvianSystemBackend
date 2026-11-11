using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdStaffingGapSnapshotConfiguration : IEntityTypeConfiguration<HrdStaffingGapSnapshot>
    {
        public void Configure(EntityTypeBuilder<HrdStaffingGapSnapshot> builder)
        {
            builder.ToTable("HrdStaffingGapSnapshot", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.ReferenceDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.PeriodMonth).HasColumnType("date");
            builder.Property(x => x.TakenAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReconstructedAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.HospitalSiteId, x.PeriodMonth })
                .IsUnique()
                .HasFilter("\"SnapshotType\" = 2 AND \"IsDelete\" = false")
                .HasDatabaseName("IX_HrdStaffingGapSnapshot_Site_Month");

            builder.HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentId });
        }
    }
}
