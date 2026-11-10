using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdProbationConfiguration : IEntityTypeConfiguration<HrdProbation>
    {
        public void Configure(EntityTypeBuilder<HrdProbation> builder)
        {
            builder.ToTable("HrdProbation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.EndDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.ReminderFromDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.ProbationStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ContractHistory).WithMany().HasForeignKey(x => x.ContractHistoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ClosedByUser).WithMany().HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ContractHistoryId)
                .HasDatabaseName("IX_HrdProbation_ContractHistoryId")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.ProbationStatus, x.EndDate })
                .HasDatabaseName("IX_HrdProbation_Status_EndDate");
        }
    }
}
