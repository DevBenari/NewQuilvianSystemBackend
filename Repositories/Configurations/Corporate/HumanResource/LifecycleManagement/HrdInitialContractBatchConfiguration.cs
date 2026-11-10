using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdInitialContractBatchConfiguration : IEntityTypeConfiguration<HrdInitialContractBatch>
    {
        public void Configure(EntityTypeBuilder<HrdInitialContractBatch> builder)
        {
            builder.ToTable("HrdInitialContractBatch", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.BatchNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.BatchStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.TotalRows).HasDefaultValue(0);
            builder.Property(x => x.ValidRows).HasDefaultValue(0);
            builder.Property(x => x.InvalidRows).HasDefaultValue(0);
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DecidedByUser).WithMany().HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.BatchNumber).IsUnique();
            builder.HasIndex(x => x.HospitalSiteId);
        }
    }
}
