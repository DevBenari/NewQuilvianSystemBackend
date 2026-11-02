using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan penandaan persyaratan saat check-in — <c>data-dictionary.md</c> bagian 3.</summary>
    public class McuOrderRequirementCheckConfiguration : IEntityTypeConfiguration<McuOrderRequirementCheck>
    {
        public void Configure(EntityTypeBuilder<McuOrderRequirementCheck> builder)
        {
            builder.ToTable("McuOrderRequirementCheck", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Outcome).HasConversion<int>().IsRequired();

            builder.HasIndex(x => x.McuOrderId);

            builder.HasOne(x => x.McuOrder)
                .WithMany(x => x.RequirementChecks)
                .HasForeignKey(x => x.McuOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.McuOrderComponent)
                .WithMany()
                .HasForeignKey(x => x.McuOrderComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CheckedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
