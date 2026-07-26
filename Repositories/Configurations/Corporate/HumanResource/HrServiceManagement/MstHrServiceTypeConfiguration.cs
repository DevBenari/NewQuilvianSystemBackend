using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class MstHrServiceTypeConfiguration : IEntityTypeConfiguration<MstHrServiceType>
    {
        public void Configure(EntityTypeBuilder<MstHrServiceType> entity)
        {
            entity.ToTable("MstHrServiceType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DefaultSlaHours).HasDefaultValue(24);
            entity.Property(x => x.RequiresWorkflow).HasDefaultValue(false);
            entity.Property(x => x.RequiresAttachment).HasDefaultValue(false);
            entity.Property(x => x.AllowsEmployeeComment).HasDefaultValue(true);
            entity.Property(x => x.AllowsInternalComment).HasDefaultValue(true);
            entity.Property(x => x.AutoCreateDocumentRequest).HasDefaultValue(false);
            entity.Property(x => x.IsEmployeeSelectable).HasDefaultValue(true);
            entity.Property(x => x.IsConfidential).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.FormSchemaJson).HasColumnType("jsonb");
            entity.Property(x => x.ValidationRuleJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.HrServiceCategory).WithMany(x => x.ServiceTypes).HasForeignKey(x => x.HrServiceCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeDocumentType).WithMany(x => x.HrServiceTypes).HasForeignKey(x => x.EmployeeDocumentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DefaultAssignedUser).WithMany().HasForeignKey(x => x.DefaultAssignedUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ServiceTypeCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.HrServiceCategoryId, x.IsEmployeeSelectable, x.IsActive, x.SortOrder });
            entity.HasIndex(x => new { x.WorkflowDefinitionId, x.AssignmentSource });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<MstHrServiceType> entity)
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
