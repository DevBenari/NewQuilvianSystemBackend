using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstNurseStationClusterConfiguration : IEntityTypeConfiguration<MstNurseStationCluster>
    {
        public void Configure(EntityTypeBuilder<MstNurseStationCluster> entity)
        {
            entity.ToTable("MstNurseStationCluster", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceUnitId)
                .IsRequired();

            entity.Property(x => x.ClusterCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ClusterName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ShortName)
                .HasMaxLength(50);

            entity.Property(x => x.LocationName)
                .HasMaxLength(100);

            entity.Property(x => x.FloorName)
                .HasMaxLength(50);

            entity.Property(x => x.RoomName)
                .HasMaxLength(50);

            entity.Property(x => x.IsAvailableForRegistrationQueue)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForScreening)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForDisplay)
                .HasDefaultValue(true);

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.CreateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.UpdateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ClusterCode)
                .IsUnique();

            entity.HasIndex(x => x.ClusterName);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClusterName
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAvailableForRegistrationQueue,
                x.IsAvailableForScreening,
                x.IsAvailableForDisplay,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
