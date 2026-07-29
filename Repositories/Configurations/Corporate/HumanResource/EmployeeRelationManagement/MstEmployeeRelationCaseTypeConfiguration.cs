using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.EmployeeRelation
{
    public class MstEmployeeRelationCaseTypeConfiguration : IEntityTypeConfiguration<MstEmployeeRelationCaseType>
    {
        public void Configure(EntityTypeBuilder<MstEmployeeRelationCaseType> entity)
        {
            entity.ToTable("MstEmployeeRelationCaseType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CaseTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CaseTypeName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CaseCategory).HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasIndex(x => x.CaseTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.CaseTypeName);
            entity.HasIndex(x => new { x.IsActive, x.IsDelete, x.SortOrder });
        }
    }
}
