using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class HrdInitialPlacementBatchConfiguration : IEntityTypeConfiguration<HrdInitialPlacementBatch>
    {
        public void Configure(EntityTypeBuilder<HrdInitialPlacementBatch> builder)
        {
            builder.ToTable("HrdInitialPlacementBatch", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.BatchName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.BatchStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ExecutionSummary).HasMaxLength(1000);

            builder.HasIndex(x => x.BatchNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.BatchStatus);
        }
    }

    public class HrdInitialPlacementBatchRowConfiguration : IEntityTypeConfiguration<HrdInitialPlacementBatchRow>
    {
        public void Configure(EntityTypeBuilder<HrdInitialPlacementBatchRow> builder)
        {
            builder.ToTable("HrdInitialPlacementBatchRow", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.BaseSalary).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ValidationMessage).HasMaxLength(1000);

            builder.HasOne(x => x.Batch)
                .WithMany(x => x.Rows)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ManagerWorkforceProfile).WithMany().HasForeignKey(x => x.ManagerWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.BatchId, x.WorkforceProfileId }).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
