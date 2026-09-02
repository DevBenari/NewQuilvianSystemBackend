using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadStudySafetyCheckConfiguration : IEntityTypeConfiguration<RadStudySafetyCheck>
    {
        public void Configure(EntityTypeBuilder<RadStudySafetyCheck> builder)
        {
            builder.ToTable("RadStudySafetyCheck", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequirementCodeSnapshot).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequirementNameSnapshot).HasMaxLength(200).IsRequired();
            builder.Property(x => x.CheckState).HasConversion<int>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

            // Satu butir hanya boleh muncul sekali pada satu study. Butir yang sama tercatat
            // dua kali membuat "sudah dijawab" bergantung pada baris mana yang dibaca.
            builder.HasIndex(x => new { x.RadStudyId, x.SafetyRequirementId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.RadStudyId, x.CheckState });

            builder.HasOne(x => x.SafetyRequirement)
                .WithMany()
                .HasForeignKey(x => x.SafetyRequirementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
