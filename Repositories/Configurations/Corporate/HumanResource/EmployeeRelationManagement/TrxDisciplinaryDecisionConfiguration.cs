using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxDisciplinaryDecisionConfiguration : IEntityTypeConfiguration<TrxDisciplinaryDecision>
    {
        public void Configure(EntityTypeBuilder<TrxDisciplinaryDecision> entity)
        {
            entity.ToTable("TrxDisciplinaryDecision", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DecisionDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AppealDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsAppealAllowed).HasDefaultValue(true);
            entity.Property(x => x.IsFinalDecision).HasDefaultValue(false);
            entity.Property(x => x.IsConfidential).HasDefaultValue(true);
            entity.Property(x => x.RequiresEnhancedAudit).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.DisciplinaryCase)
                .WithMany(x => x.Decisions)
                .HasForeignKey(x => x.DisciplinaryCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RequestReason)
                .WithMany()
                .HasForeignKey(x => x.RequestReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RejectionReason)
                .WithMany()
                .HasForeignKey(x => x.RejectionReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.IssuedByUser)
                .WithMany()
                .HasForeignKey(x => x.IssuedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DecisionNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.DisciplinaryCaseId, x.DecisionDate });

            entity.HasIndex(x => new { x.DecisionStatus, x.EffectiveStartDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxDisciplinaryDecision> entity)
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
