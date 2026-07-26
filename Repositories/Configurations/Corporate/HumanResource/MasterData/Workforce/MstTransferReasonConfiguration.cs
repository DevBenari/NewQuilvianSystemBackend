using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstTransferReasonConfiguration : IEntityTypeConfiguration<MstTransferReason>
    {
        public void Configure(EntityTypeBuilder<MstTransferReason> entity)
        {
            entity.ToTable("MstTransferReason", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.TransferReasonCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TransferReasonName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.TransferType)
                .HasMaxLength(50)
                .HasDefaultValue("InternalTransfer")
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.RequiresApproval)
                .HasDefaultValue(true);

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

            entity.HasIndex(x => x.TransferReasonCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.TransferReasonName);

            entity.HasIndex(x => new
            {
                x.TransferType,
                x.RequiresApproval,
                x.IsActive,
                x.IsDelete
            });

        }
    }
}
