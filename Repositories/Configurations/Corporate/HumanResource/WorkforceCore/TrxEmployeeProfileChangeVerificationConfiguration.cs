using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class TrxEmployeeProfileChangeVerificationConfiguration : IEntityTypeConfiguration<TrxEmployeeProfileChangeVerification>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeProfileChangeVerification> builder)
        {
            builder.ToTable("TrxEmployeeProfileChangeVerification", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.VerificationType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.VerificationStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.VerificationNote).HasMaxLength(500);
            builder.Property(x => x.EvidenceFilePath).HasMaxLength(500);
            builder.HasOne(x => x.ProfileChangeRequest).WithMany(x => x.Verifications).HasForeignKey(x => x.ProfileChangeRequestId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.ProfileChangeDetail).WithMany().HasForeignKey(x => x.ProfileChangeDetailId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.ProfileChangeRequestId, x.VerificationStatus });
        }
    }
}
