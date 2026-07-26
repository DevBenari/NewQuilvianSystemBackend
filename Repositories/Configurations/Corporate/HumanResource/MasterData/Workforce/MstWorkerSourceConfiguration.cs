using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstWorkerSourceConfiguration : IEntityTypeConfiguration<MstWorkerSource>
    {
        public void Configure(EntityTypeBuilder<MstWorkerSource> entity)
        {
            entity.ToTable("MstWorkerSource", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.WorkerSourceCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.WorkerSourceName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.SourceType)
                .HasMaxLength(50);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsExternal)
                .HasDefaultValue(false);

            entity.Property(x => x.RequiresVendorInformation)
                .HasDefaultValue(false);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.WorkerSourceCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.WorkerSourceName);

            entity.HasIndex(x => new
            {
                x.SourceType,
                x.IsExternal,
                x.RequiresVendorInformation,
                x.IsActive,
                x.IsDelete
            });

        }
    }
}
