using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>
    /// <c>CliTransferHandover</c> — kamus data 19.9 dan DDL 19.12 (<c>BE-RWI-183</c>, migration
    /// <c>E7</c>, <c>P2</c>).
    /// </summary>
    public class CliTransferHandoverConfiguration : IEntityTypeConfiguration<CliTransferHandover>
    {
        public void Configure(EntityTypeBuilder<CliTransferHandover> builder)
        {
            builder.ToTable("CliTransferHandover", "public", table =>
            {
                table.HasCheckConstraint("CK_CliTransferHandover_TwoAccounts",
                    "\"ReceivedByUserId\" IS NULL OR \"ReceivedByUserId\" <> \"SentByUserId\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<int>().IsRequired()
                .HasDefaultValue(CliTransferHandoverStatus.NotSent);
            builder.Property(x => x.SoapSummary).HasMaxLength(4000);
            builder.Property(x => x.HandedItems).HasMaxLength(2000);
            builder.Property(x => x.SpecialInstructions).HasMaxLength(2000);
            builder.Property(x => x.SnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.SentAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReceivedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectionReason).HasMaxLength(1000);
            builder.Property(x => x.Version).IsRequired().HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.ToPlacementId, "UX_CliTransferHandover_ToPlacement").IsUnique();
            builder.HasIndex(x => new { x.InpEpisodeId, x.Status }, "IX_CliTransferHandover_Episode_Status");
            builder.HasIndex(x => new { x.ToServiceUnitId, x.Status }, "IX_CliTransferHandover_ToUnit_Status");
            builder.HasIndex(x => x.FromPlacementId);
            builder.HasIndex(x => x.FromServiceUnitId);

            builder.HasOne(x => x.Episode).WithMany().HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromPlacement).WithMany().HasForeignKey(x => x.FromPlacementId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToPlacement).WithMany().HasForeignKey(x => x.ToPlacementId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromServiceUnit).WithMany().HasForeignKey(x => x.FromServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToServiceUnit).WithMany().HasForeignKey(x => x.ToServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
