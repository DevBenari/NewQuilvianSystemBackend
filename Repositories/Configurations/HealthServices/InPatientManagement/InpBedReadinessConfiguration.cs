using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement;

public class InpBedReadinessConfiguration : IEntityTypeConfiguration<InpBedReadiness>
{
    public void Configure(EntityTypeBuilder<InpBedReadiness> builder)
    {
        builder.ToTable("InpBedReadiness", "public", t => {
            t.HasCheckConstraint("CK_InpBedReadiness_State", "\"State\" BETWEEN 0 AND 4");
            t.HasCheckConstraint("CK_InpBedReadiness_Verification", "(\"VerifiedAtUtc\" IS NULL) = (\"VerifiedByUserId\" IS NULL)");
            t.HasCheckConstraint("CK_InpBedReadiness_Ready", "\"State\" <> 4 OR (\"VerifiedAtUtc\" IS NOT NULL AND \"VerificationReference\" IS NOT NULL)");
            t.HasCheckConstraint("CK_InpBedReadiness_Version", "\"Version\" > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VerifiedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => x.BedId).IsUnique();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property(x => x.VerificationReference).HasMaxLength(200);
        builder.HasOne<MstBed>().WithMany().HasForeignKey(x => x.BedId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InpBedPlacement>().WithMany().HasForeignKey(x => x.LastReleasedPlacementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

