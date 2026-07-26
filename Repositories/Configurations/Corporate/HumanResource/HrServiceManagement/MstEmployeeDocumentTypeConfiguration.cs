using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class MstEmployeeDocumentTypeConfiguration : IEntityTypeConfiguration<MstEmployeeDocumentType>
    {
        public void Configure(EntityTypeBuilder<MstEmployeeDocumentType> entity)
        {
            entity.ToTable("MstEmployeeDocumentType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequiresApproval).HasDefaultValue(false);
            entity.Property(x => x.RequiresDigitalSignature).HasDefaultValue(false);
            entity.Property(x => x.AllowsEmployeeDownload).HasDefaultValue(true);
            entity.Property(x => x.AllowsMultipleIssuance).HasDefaultValue(true);
            entity.Property(x => x.IsConfidential).HasDefaultValue(false);
            entity.Property(x => x.RequiredDataSchemaJson).HasColumnType("jsonb");
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DocumentTypeCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.DocumentCategory, x.IsActive, x.SortOrder });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<MstEmployeeDocumentType> entity)
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
