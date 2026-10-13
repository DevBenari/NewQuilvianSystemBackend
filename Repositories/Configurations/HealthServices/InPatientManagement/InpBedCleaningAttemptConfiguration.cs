using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement;

public class InpBedCleaningAttemptConfiguration : IEntityTypeConfiguration<InpBedCleaningAttempt>
{
    public void Configure(EntityTypeBuilder<InpBedCleaningAttempt> builder)
    {
        builder.ToTable("InpBedCleaningAttempt", "public", t => {
            t.HasCheckConstraint("CK_InpBedCleaningAttempt_State", "\"State\" BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_InpBedCleaningAttempt_Completion", "(\"CompletedAtUtc\" IS NULL) = (\"CompletedByUserId\" IS NULL)");
            t.HasCheckConstraint("CK_InpBedCleaningAttempt_Verification", "(\"VerifiedAtUtc\" IS NULL) = (\"VerifiedByUserId\" IS NULL)");
            t.HasCheckConstraint("CK_InpBedCleaningAttempt_Decision", "\"State\" NOT IN (3,4) OR \"VerifiedAtUtc\" IS NOT NULL");
            t.HasCheckConstraint("CK_InpBedCleaningAttempt_Rejection", "\"State\" <> 4 OR (\"RejectionReason\" IS NOT NULL AND length(trim(\"RejectionReason\")) > 0)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StartedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CompletedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.VerifiedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.ReadinessId, x.CycleId, x.StartedAtUtc });
        builder.HasIndex(x => new { x.ReadinessId, x.CycleId }, "IX_InpBedCleaningAttempt_Active").IsUnique().HasFilter("\"State\" IN (1,2)");
        builder.Property(x => x.SopReference).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.HasOne<InpBedReadiness>().WithMany().HasForeignKey(x => x.ReadinessId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.StartedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

