using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxInvestigationEvidenceConfiguration : IEntityTypeConfiguration<TrxInvestigationEvidence>
    {
        public void Configure(EntityTypeBuilder<TrxInvestigationEvidence> entity)
        {
            entity.ToTable("TrxInvestigationEvidence", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CollectedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ChainOfCustodyJson).HasColumnType("jsonb");
            entity.Property(x => x.IsConfidential).HasDefaultValue(true);
            entity.Property(x => x.RequiresEnhancedAudit).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkplaceInvestigation)
                .WithMany(x => x.EvidenceItems)
                .HasForeignKey(x => x.WorkplaceInvestigationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CollectedByUser)
                .WithMany()
                .HasForeignKey(x => x.CollectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EvidenceNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkplaceInvestigationId, x.EvidenceType });

            entity.HasIndex(x => x.FileChecksum);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxInvestigationEvidence> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
