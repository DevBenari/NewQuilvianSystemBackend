using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpContractHistoryConfiguration : IEntityTypeConfiguration<WfpContractHistory>
    {
        public void Configure(EntityTypeBuilder<WfpContractHistory> builder)
        {
            builder.ToTable("WfpContractHistory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.ContractNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.HistoryType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ContractStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.SignedDate).HasColumnType("date");
            builder.Property(x => x.ProbationEndDate).HasColumnType("date");
            builder.Property(x => x.TerminatedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DocumentPath).HasMaxLength(500);
            builder.Property(x => x.Description).HasMaxLength(500);

            // S-C4-ONB amendments
            builder.Property(x => x.SourceDocumentType).HasMaxLength(50);
            builder.Property(x => x.FixedTermSeriesStartDate).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DocumentFileName).HasMaxLength(250);
            builder.Property(x => x.DocumentContentType).HasMaxLength(150);
            builder.Property(x => x.SignedRecordedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.WorkforceProfile).WithMany(x => x.ContractHistories).HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PreviousContractHistory).WithMany(x => x.Renewals).HasForeignKey(x => x.PreviousContractHistoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ContractType).WithMany().HasForeignKey(x => x.ContractTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkerSource).WithMany().HasForeignKey(x => x.WorkerSourceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TerminationReason).WithMany().HasForeignKey(x => x.TerminationReasonId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WorkforceProfileId, x.ContractNumber }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsCurrent\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentId }).HasDatabaseName("IX_WfpContractHistory_SourceDocument");
        }
    }
}
