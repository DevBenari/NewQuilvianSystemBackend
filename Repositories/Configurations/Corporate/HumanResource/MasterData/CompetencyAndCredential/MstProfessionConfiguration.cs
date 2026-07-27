using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    public class MstProfessionConfiguration : IEntityTypeConfiguration<MstProfession>
    {
        public void Configure(EntityTypeBuilder<MstProfession> entity)
        {
            entity.ToTable("MstProfession", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProfessionCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ProfessionName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProfessionGroup).HasMaxLength(100).HasDefaultValue("General").IsRequired();
            entity.Property(x => x.IsClinicalProfession).HasDefaultValue(false);
            entity.Property(x => x.RequiresCredentialing).HasDefaultValue(false);
            entity.Property(x => x.RequiresLicense).HasDefaultValue(false);
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

            entity.HasIndex(x => x.ProfessionCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.ProfessionName);
            entity.HasIndex(x => new { x.ProfessionGroup, x.IsClinicalProfession, x.IsActive, x.IsDelete });
        }
    }
}
