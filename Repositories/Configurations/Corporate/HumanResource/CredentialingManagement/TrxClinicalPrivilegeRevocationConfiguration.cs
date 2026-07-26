using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxClinicalPrivilegeRevocationConfiguration : IEntityTypeConfiguration<TrxClinicalPrivilegeRevocation>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalPrivilegeRevocation> entity)
        {
            entity.ToTable("TrxClinicalPrivilegeRevocation", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RevocationDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AppealDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.ClinicalPrivilege)
                .WithMany(x => x.Revocations)
                .HasForeignKey(x => x.ClinicalPrivilegeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RevokedByUser)
                .WithMany()
                .HasForeignKey(x => x.RevokedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RevocationNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.ClinicalPrivilegeId, x.RevocationStatus });

            entity.HasIndex(x => new { x.AppealDeadline, x.AppealStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxClinicalPrivilegeRevocation> entity)
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
