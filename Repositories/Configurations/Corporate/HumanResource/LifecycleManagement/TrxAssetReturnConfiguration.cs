using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxAssetReturnConfiguration : IEntityTypeConfiguration<TrxAssetReturn>
    {
        public void Configure(EntityTypeBuilder<TrxAssetReturn> builder)
        {
            builder.ToTable("TrxAssetReturn", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.AssetCode).HasMaxLength(100).IsRequired();
            builder.Property(x => x.AssetName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.AssetCategory).HasMaxLength(100);
            builder.Property(x => x.SerialNumber).HasMaxLength(150);
            builder.Property(x => x.AssignedDate).HasColumnType("date");
            builder.Property(x => x.ReturnedDate).HasColumnType("date");
            builder.Property(x => x.ReturnCondition).HasMaxLength(50);
            builder.Property(x => x.ReplacementCost).HasPrecision(18, 2);
            builder.Property(x => x.ReturnStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.ExitClearance).WithMany(x => x.AssetReturns).HasForeignKey(x => x.ExitClearanceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.ExitClearanceId, x.AssetCode }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.ReturnStatus });
        }
    }
}
