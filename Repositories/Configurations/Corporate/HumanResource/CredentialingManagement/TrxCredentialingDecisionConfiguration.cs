using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxCredentialingDecisionConfiguration : IEntityTypeConfiguration<TrxCredentialingDecision>
    {
        public void Configure(EntityTypeBuilder<TrxCredentialingDecision> entity)
        {
            entity.ToTable("TrxCredentialingDecision", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DecisionDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.CredentialingApplication)
                .WithMany(x => x.Decisions)
                .HasForeignKey(x => x.CredentialingApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DecisionByUser)
                .WithMany()
                .HasForeignKey(x => x.DecisionByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CommitteeChairUser)
                .WithMany()
                .HasForeignKey(x => x.CommitteeChairUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DecisionNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.CredentialingApplicationId, x.DecisionStatus, x.DecisionDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxCredentialingDecision> entity)
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
