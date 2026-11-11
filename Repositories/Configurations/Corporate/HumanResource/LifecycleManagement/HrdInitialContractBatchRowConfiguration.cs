using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdInitialContractBatchRowConfiguration : IEntityTypeConfiguration<HrdInitialContractBatchRow>
    {
        public void Configure(EntityTypeBuilder<HrdInitialContractBatchRow> builder)
        {
            builder.ToTable("HrdInitialContractBatchRow", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ContractNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.FixedTermSeriesStartDate).HasColumnType("date");
            builder.Property(x => x.IsValid).HasDefaultValue(false);
            builder.Property(x => x.ValidationMessage).HasMaxLength(500);

            builder.HasOne(x => x.Batch).WithMany(b => b.Rows).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ContractType).WithMany().HasForeignKey(x => x.ContractTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CreatedContractHistory).WithMany().HasForeignKey(x => x.CreatedContractHistoryId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.BatchId, x.WorkforceProfileId })
                .HasDatabaseName("IX_HrdInitialContractBatchRow_BatchId_WorkforceProfileId")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
