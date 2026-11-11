using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdContractCorrectionConfiguration : IEntityTypeConfiguration<HrdContractCorrection>
    {
        public void Configure(EntityTypeBuilder<HrdContractCorrection> builder)
        {
            builder.ToTable("HrdContractCorrection", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.CorrectionNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.BeforeContractNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.BeforeStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.BeforeEndDate).HasColumnType("date");
            builder.Property(x => x.BeforeDocumentPath).HasMaxLength(500);
            builder.Property(x => x.AfterContractNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.AfterStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.AfterEndDate).HasColumnType("date");
            builder.Property(x => x.ReasonText).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.EvidenceStoragePath).HasMaxLength(500).IsRequired();
            builder.Property(x => x.EvidenceFileName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.EvidenceContentType).HasMaxLength(150).IsRequired();
            builder.Property(x => x.CorrectionStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DecisionNote).HasMaxLength(1000);
            builder.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.ContractHistory).WithMany().HasForeignKey(x => x.ContractHistoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.BeforeContractType).WithMany().HasForeignKey(x => x.BeforeContractTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AfterContractType).WithMany().HasForeignKey(x => x.AfterContractTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DecidedByUser).WithMany().HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CorrectionNumber)
                .HasDatabaseName("IX_HrdContractCorrection_CorrectionNumber")
                .IsUnique();

            builder.HasIndex(x => x.ContractHistoryId)
                .HasDatabaseName("IX_HrdContractCorrection_ContractHistoryId_Open")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"CorrectionStatus\" IN (1, 2)");
        }
    }
}
