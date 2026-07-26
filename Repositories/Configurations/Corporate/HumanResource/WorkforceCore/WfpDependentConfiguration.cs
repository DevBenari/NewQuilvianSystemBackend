using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpDependentConfiguration : IEntityTypeConfiguration<WfpDependent>
    {
        public void Configure(EntityTypeBuilder<WfpDependent> builder)
        {
            builder.ToTable("WfpDependent", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.DependentType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.DependentStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FamilyMember).WithMany(x => x.Dependents).HasForeignKey(x => x.FamilyMemberId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.BenefitPlan).WithMany().HasForeignKey(x => x.BenefitPlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.DependentStatus, x.IsActive });
        }
    }
}
