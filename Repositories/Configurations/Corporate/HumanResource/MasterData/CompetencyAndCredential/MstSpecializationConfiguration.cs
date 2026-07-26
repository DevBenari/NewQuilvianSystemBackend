using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    public class MstSpecializationConfiguration : IEntityTypeConfiguration<MstSpecialization>
    {
        public void Configure(EntityTypeBuilder<MstSpecialization> entity)
        {
            entity.ToTable("MstSpecialization", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProfessionId).IsRequired();
            entity.Property(x => x.SpecializationCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SpecializationName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SpecializationType).HasMaxLength(50).HasDefaultValue("Specialization").IsRequired();
            entity.Property(x => x.IsClinicalSpecialization).HasDefaultValue(true);
            entity.Property(x => x.RequiresCredentialing).HasDefaultValue(true);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.Profession)
                .WithMany(x => x.Specializations)
                .HasForeignKey(x => x.ProfessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ParentSpecialization)
                .WithMany(x => x.ChildSpecializations)
                .HasForeignKey(x => x.ParentSpecializationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ProfessionId, x.SpecializationCode })
                .IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.ProfessionId, x.SpecializationName });
            entity.HasIndex(x => new { x.ParentSpecializationId, x.IsActive, x.IsDelete });
        }
    }
}
