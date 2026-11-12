using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdStaffingGapSnapshotLineConfiguration : IEntityTypeConfiguration<HrdStaffingGapSnapshotLine>
    {
        public void Configure(EntityTypeBuilder<HrdStaffingGapSnapshotLine> builder)
        {
            builder.ToTable("HrdStaffingGapSnapshotLine", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.FormationQuantity).HasPrecision(9, 2);
            builder.Property(x => x.FilledFte).HasPrecision(9, 2);
            builder.Property(x => x.ToBeVacatedFte).HasPrecision(9, 2);
            builder.Property(x => x.TemporarilyUnavailableFte).HasPrecision(9, 2);
            builder.Property(x => x.NotYetCompetentFte).HasPrecision(9, 2);
            builder.Property(x => x.OutsourcedFte).HasPrecision(9, 2);
            builder.Property(x => x.PermanentGap).HasPrecision(9, 2);
            builder.Property(x => x.TemporaryGap).HasPrecision(9, 2);
            builder.Property(x => x.CompetencyGap).HasPrecision(9, 2);

            builder.Property(x => x.PlacementDataComplete).HasDefaultValue(false);
            builder.Property(x => x.LeaveDataComplete).HasDefaultValue(false);
            builder.Property(x => x.CompetencyDataComplete).HasDefaultValue(false);
            builder.Property(x => x.WorkforceTypeDataComplete).HasDefaultValue(false);

            builder.HasOne(x => x.Snapshot)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.SnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.SnapshotId, x.OrganizationUnitId, x.PositionId })
                .IsUnique()
                .HasDatabaseName("IX_HrdStaffingGapSnapshotLine_Snapshot_Unit_Position");
        }
    }
}
