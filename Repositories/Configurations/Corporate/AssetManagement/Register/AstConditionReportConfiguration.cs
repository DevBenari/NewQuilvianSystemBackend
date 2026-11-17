using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstConditionReport</c> — data-dictionary.md bagian 3.4 dan DDL bagian 4 (BE-AST-003).</summary>
public sealed class AstConditionReportConfiguration : IEntityTypeConfiguration<AstConditionReport>
{
    public void Configure(EntityTypeBuilder<AstConditionReport> entity)
    {
        entity.ToTable("AstConditionReport", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ReportNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Source).HasConversion<int>().IsRequired();
        entity.Property(x => x.PreviousCondition).HasConversion<int>().IsRequired();
        entity.Property(x => x.ReportedCondition).HasConversion<int>().IsRequired();
        entity.Property(x => x.IsFoundAgain).HasDefaultValue(false);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.DecidedCondition).HasConversion<int>();
        entity.Property(x => x.DecisionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.PhotoAttachment).WithMany().HasForeignKey(x => x.PhotoAttachmentId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.LossReportAttachment).WithMany().HasForeignKey(x => x.LossReportAttachmentId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.ReportNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstConditionReport_ReportNumber");

        // Satu laporan terbuka per aset: 1 = ConditionReportStatus.Dilaporkan.
        entity.HasIndex(x => x.AssetId)
            .IsUnique()
            .HasFilter("\"Status\" = 1 AND \"IsDelete\" = false")
            .HasDatabaseName("IX_AstConditionReport_Open");

        entity.HasIndex(x => x.Source);
        entity.HasIndex(x => x.Status);
    }
}
