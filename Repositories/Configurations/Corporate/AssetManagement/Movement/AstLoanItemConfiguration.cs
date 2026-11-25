using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Movement;

public sealed class AstLoanItemConfiguration : IEntityTypeConfiguration<AstLoanItem>
{
    public void Configure(EntityTypeBuilder<AstLoanItem> entity)
    {
        entity.ToTable("AstLoanItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ConditionAtPickup).HasConversion<int>().IsRequired();
        entity.Property(x => x.ReturnCondition).HasConversion<int>();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Loan).WithMany(x => x.Items).HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.LoanId, x.AssetId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstLoanItem_Loan_Asset");

        entity.HasIndex(x => x.AssetId);
    }
}
