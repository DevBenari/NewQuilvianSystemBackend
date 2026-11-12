using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstUtilization</c> — data-dictionary.md bagian 3.8 (BE-AST-003).</summary>
public sealed class AstUtilizationConfiguration : IEntityTypeConfiguration<AstUtilization>
{
    public void Configure(EntityTypeBuilder<AstUtilization> entity)
    {
        entity.ToTable("AstUtilization", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.UtilizationNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.UtilizationType).HasConversion<int>().IsRequired();
        entity.Property(x => x.PartnerName).HasMaxLength(200).IsRequired();
        entity.Property(x => x.AgreementNumber).HasMaxLength(100).IsRequired();
        entity.Property(x => x.AgreementValue).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.StartDate).HasColumnType("date");
        entity.Property(x => x.EndDate).HasColumnType("date");
        entity.Property(x => x.Purpose).HasMaxLength(1000);
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.RequiredApprovalLevel).HasConversion<int>();
        entity.Property(x => x.ThresholdUsed).HasPrecision(18, 2);
        entity.Property(x => x.DecisionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.AgreementAttachment).WithMany().HasForeignKey(x => x.AgreementAttachmentId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.UtilizationNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstUtilization_UtilizationNumber");

        entity.HasIndex(x => x.UtilizationType);
        entity.HasIndex(x => x.StartDate);
        entity.HasIndex(x => x.EndDate);
        entity.HasIndex(x => x.Status);
    }
}
