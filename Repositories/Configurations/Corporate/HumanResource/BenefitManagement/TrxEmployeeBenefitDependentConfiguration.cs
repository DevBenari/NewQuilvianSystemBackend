using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxEmployeeBenefitDependentConfiguration : IEntityTypeConfiguration<TrxEmployeeBenefitDependent>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeBenefitDependent> entity)
        {
            entity.ToTable("TrxEmployeeBenefitDependent", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BirthDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CoverageLimitAmount).HasPrecision(18, 2);
            entity.Property(x => x.UsedAmount).HasPrecision(18, 2);
            entity.Property(x => x.RemainingAmount).HasPrecision(18, 2);
            entity.Property(x => x.SupportingDocumentJson).HasColumnType("jsonb");
            entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.EmployeeBenefitEnrollment)
                .WithMany(x => x.Dependents)
                .HasForeignKey(x => x.EmployeeBenefitEnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FamilyMember)
                .WithMany()
                .HasForeignKey(x => x.FamilyMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Dependent)
                .WithMany()
                .HasForeignKey(x => x.DependentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RequestedByUser)
                .WithMany()
                .HasForeignKey(x => x.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.EmployeeBenefitEnrollmentId, x.DependentName, x.RelationshipType })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.DependentStatus, x.IsActive });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeBenefitDependent> entity)
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
