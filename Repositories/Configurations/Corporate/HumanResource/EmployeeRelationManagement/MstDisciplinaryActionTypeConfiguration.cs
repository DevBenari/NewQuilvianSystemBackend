using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.EmployeeRelation
{
    public class MstDisciplinaryActionTypeConfiguration : IEntityTypeConfiguration<MstDisciplinaryActionType>
    {
        public void Configure(EntityTypeBuilder<MstDisciplinaryActionType> entity)
        {
            entity.ToTable("MstDisciplinaryActionType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ActionTypeName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DefaultActionLevel).HasMaxLength(40);
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

            entity.HasIndex(x => x.ActionTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ActionTypeName);
            entity.HasIndex(x => new { x.IsActive, x.IsDelete, x.SortOrder });
        }
    }
}
