using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstWorkforceTypeConfiguration : IEntityTypeConfiguration<MstWorkforceType>
    {
        public void Configure(EntityTypeBuilder<MstWorkforceType> entity)
        {
            entity.ToTable("MstWorkforceType", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.WorkforceTypeCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.WorkforceTypeName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsInternal)
                .HasDefaultValue(true);

            entity.Property(x => x.IsClinical)
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

            entity.HasIndex(x => x.WorkforceTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.WorkforceTypeName);

            entity.HasIndex(x => new { x.IsInternal, x.IsClinical, x.IsActive, x.IsDelete });

            entity.HasIndex(x => new { x.SortOrder, x.WorkforceTypeName });

        }
    }
}
