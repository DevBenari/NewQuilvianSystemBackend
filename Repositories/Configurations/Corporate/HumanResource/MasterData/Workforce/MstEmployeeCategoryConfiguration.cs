using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstEmployeeCategoryConfiguration : IEntityTypeConfiguration<MstEmployeeCategory>
    {
        public void Configure(EntityTypeBuilder<MstEmployeeCategory> entity)
        {
            entity.ToTable("MstEmployeeCategory", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.EmployeeCategoryCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EmployeeCategoryName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsClinical)
                .HasDefaultValue(false);

            entity.Property(x => x.RequiresCredentialing)
                .HasDefaultValue(false);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasOne(x => x.WorkforceType)
                .WithMany(x => x.EmployeeCategories)
                .HasForeignKey(x => x.WorkforceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EmployeeCategoryCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceTypeId, x.EmployeeCategoryName });

            entity.HasIndex(x => new
            {
                x.WorkforceTypeId,
                x.IsClinical,
                x.RequiresCredentialing,
                x.IsActive,
                x.IsDelete
            });

        }
    }
}
