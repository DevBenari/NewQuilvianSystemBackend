using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.PhysicalInventory;

public sealed class AstStockTakeFollowUpConfiguration : IEntityTypeConfiguration<AstStockTakeFollowUp>
{
    public void Configure(EntityTypeBuilder<AstStockTakeFollowUp> entity)
    {
        entity.ToTable("AstStockTakeFollowUp", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Type).HasConversion<int>().IsRequired();
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.TargetDocumentType).HasMaxLength(40);
        entity.Property(x => x.TargetDocumentNumber).HasMaxLength(30);
        entity.Property(x => x.ProcessedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DismissedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DismissReason).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Session).WithMany(x => x.FollowUps).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Result).WithMany(x => x.FollowUps).HasForeignKey(x => x.ResultId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.ResultId, x.Type })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstStockTakeFollowUp_Result_Type");

        entity.HasIndex(x => x.SessionId);
        entity.HasIndex(x => x.Type);
        entity.HasIndex(x => x.Status);
    }
}
