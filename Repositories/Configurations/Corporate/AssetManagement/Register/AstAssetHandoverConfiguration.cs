using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstAssetHandover</c> — data-dictionary.md bagian 3.3 (BE-AST-003).</summary>
public sealed class AstAssetHandoverConfiguration : IEntityTypeConfiguration<AstAssetHandover>
{
    public void Configure(EntityTypeBuilder<AstAssetHandover> entity)
    {
        entity.ToTable("AstAssetHandover", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.HandoverNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Purpose).HasConversion<int>().IsRequired();
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.Property(x => x.DecisionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.TargetOrganizationUnit).WithMany().HasForeignKey(x => x.TargetOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.TargetLocation).WithMany().HasForeignKey(x => x.TargetLocationId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.TargetCustodianEmployee).WithMany().HasForeignKey(x => x.TargetCustodianEmployeeId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.HandoverNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstAssetHandover_HandoverNumber");

        entity.HasIndex(x => x.Purpose);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.TargetOrganizationUnitId);
        entity.HasIndex(x => x.TargetCustodianEmployeeId);
    }
}
