using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdInitialFormationBatchConfiguration : IEntityTypeConfiguration<HrdInitialFormationBatch>
    {
        public void Configure(EntityTypeBuilder<HrdInitialFormationBatch> builder)
        {
            builder.ToTable("HrdInitialFormationBatch", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.SourceDocumentReference).HasMaxLength(200).IsRequired();
            builder.Property(x => x.SourceDocumentPath).HasMaxLength(500);
            builder.Property(x => x.SourceDocumentFileName).HasMaxLength(255);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.BatchNumber)
                .IsUnique()
                .HasDatabaseName("IX_HrdInitialFormationBatch_BatchNumber");

            builder.HasIndex(x => x.HospitalSiteId);
            builder.HasIndex(x => x.BatchStatus);
            builder.HasIndex(x => x.WorkflowInstanceId);
        }
    }
}
