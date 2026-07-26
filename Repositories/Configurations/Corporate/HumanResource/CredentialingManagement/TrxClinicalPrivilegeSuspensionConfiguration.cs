using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxClinicalPrivilegeSuspensionConfiguration : IEntityTypeConfiguration<TrxClinicalPrivilegeSuspension>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalPrivilegeSuspension> entity)
        {
            entity.ToTable("TrxClinicalPrivilegeSuspension", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SuspensionStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SuspensionEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SuspendedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReinstatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.ClinicalPrivilege)
                .WithMany(x => x.Suspensions)
                .HasForeignKey(x => x.ClinicalPrivilegeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SuspendedByUser)
                .WithMany()
                .HasForeignKey(x => x.SuspendedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReinstatedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReinstatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.SuspensionNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.ClinicalPrivilegeId, x.SuspensionStatus });

            entity.HasIndex(x => new { x.SuspensionEndDate, x.SuspensionStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxClinicalPrivilegeSuspension> entity)
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
