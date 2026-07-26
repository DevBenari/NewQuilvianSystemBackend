using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    public class MstClinicalPrivilegeCatalogConfiguration : IEntityTypeConfiguration<MstClinicalPrivilegeCatalog>
    {
        public void Configure(EntityTypeBuilder<MstClinicalPrivilegeCatalog> entity)
        {
            entity.ToTable("MstClinicalPrivilegeCatalog", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PrivilegeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PrivilegeName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.PrivilegeCategory).HasMaxLength(100).HasDefaultValue("ClinicalProcedure").IsRequired();
            entity.Property(x => x.ReferenceProcedureCode).HasMaxLength(100);
            entity.Property(x => x.MinimumCompetencyLevel).HasConversion<int>().IsRequired(false);
            entity.Property(x => x.MinimumExperienceMonths).HasDefaultValue(0);
            entity.Property(x => x.RequiresSupervision).HasDefaultValue(false);
            entity.Property(x => x.AllowsIndependentPractice).HasDefaultValue(true);
            entity.Property(x => x.IsHighRisk).HasDefaultValue(false);
            entity.Property(x => x.DefaultValidityMonths).IsRequired(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(1000);
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

            entity.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequiredCompetency).WithMany().HasForeignKey(x => x.RequiredCompetencyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.PrivilegeCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.PrivilegeName);
            entity.HasIndex(x => new { x.ProfessionId, x.SpecializationId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.PrivilegeCategory, x.IsHighRisk, x.IsActive, x.IsDelete });
        }
    }
}
