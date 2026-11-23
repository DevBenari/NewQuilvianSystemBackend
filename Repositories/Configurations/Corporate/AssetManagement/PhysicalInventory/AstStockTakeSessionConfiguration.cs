using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.PhysicalInventory;

public sealed class AstStockTakeSessionConfiguration : IEntityTypeConfiguration<AstStockTakeSession>
{
    public void Configure(EntityTypeBuilder<AstStockTakeSession> entity)
    {
        entity.ToTable("AstStockTakeSession", "public", table =>
        {
            table.HasCheckConstraint("CK_AstStockTakeSession_Dates", "\"PlannedEndDate\" >= \"PlannedStartDate\"");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.SessionNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.PlannedStartDate).HasColumnType("date");
        entity.Property(x => x.PlannedEndDate).HasColumnType("date");
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.ExpectedCount).HasDefaultValue(0);
        entity.Property(x => x.ScannedCount).HasDefaultValue(0);
        entity.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CountingClosedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DecisionReason).HasMaxLength(500);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.SessionNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstStockTakeSession_SessionNumber");

        entity.HasIndex(x => x.OrganizationUnitId)
            .IsUnique()
            .HasFilter("\"Status\" IN (2, 3, 4) AND \"IsDelete\" = false")
            .HasDatabaseName("IX_AstStockTakeSession_ActivePerUnit");

        entity.HasIndex(x => x.PlannedEndDate);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.LocationId);
    }
}
