using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstPayrollComponentCategoryConfiguration : IEntityTypeConfiguration<MstPayrollComponentCategory>
    {
        public void Configure(EntityTypeBuilder<MstPayrollComponentCategory> entity)
        {
            entity.ToTable("MstPayrollComponentCategory", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ComponentCategoryCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ComponentCategoryName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ComponentGroup).HasMaxLength(50).HasDefaultValue("Earning").IsRequired();
            entity.Property(x => x.AffectsGrossPay).HasDefaultValue(true);
            entity.Property(x => x.AffectsTaxableIncome).HasDefaultValue(true);
            entity.Property(x => x.AffectsTakeHomePay).HasDefaultValue(true);
            entity.Property(x => x.IsEmployerCost).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasIndex(x => x.ComponentCategoryCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ComponentCategoryName);
            entity.HasIndex(x => new { x.ComponentGroup, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.SortOrder, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
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
        }
    }
}
