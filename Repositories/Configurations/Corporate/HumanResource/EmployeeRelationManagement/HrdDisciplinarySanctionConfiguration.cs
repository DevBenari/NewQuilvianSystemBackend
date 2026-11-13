using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class HrdDisciplinarySanctionConfiguration : IEntityTypeConfiguration<HrdDisciplinarySanction>
    {
        public void Configure(EntityTypeBuilder<HrdDisciplinarySanction> entity)
        {
            entity.ToTable("HrdDisciplinarySanction", "public");
            entity.HasKey(x => x.SanctionId);

            entity.Property(x => x.SanctionId).HasColumnName("sanction_id");
            entity.Property(x => x.InvestigationId).HasColumnName("investigation_id");
            entity.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(x => x.SanctionNumber).HasColumnName("sanction_number").HasMaxLength(64).IsRequired();
            entity.Property(x => x.SanctionLevel).HasColumnName("sanction_level").HasMaxLength(32).IsRequired();
            entity.Property(x => x.IssuedDate).HasColumnName("issued_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.ValidUntil).HasColumnName("valid_until").HasColumnType("timestamp with time zone");
            entity.Property(x => x.ViolationDescription).HasColumnName("violation_description").IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.IsAppealed).HasColumnName("is_appealed").HasDefaultValue(false);
            entity.Property(x => x.AppealStatement).HasColumnName("appeal_statement");
            entity.Property(x => x.AppealEvidenceUrls).HasColumnName("appeal_evidence_urls");
            entity.Property(x => x.AppealResolution).HasColumnName("appeal_resolution");
            entity.Property(x => x.AppealedAt).HasColumnName("appealed_at").HasColumnType("timestamp with time zone");

            entity.HasIndex(x => x.SanctionNumber).IsUnique();
            entity.HasIndex(x => x.EmployeeId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ValidUntil);

            entity.HasOne(x => x.Investigation)
                .WithMany(x => x.Sanctions)
                .HasForeignKey(x => x.InvestigationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
