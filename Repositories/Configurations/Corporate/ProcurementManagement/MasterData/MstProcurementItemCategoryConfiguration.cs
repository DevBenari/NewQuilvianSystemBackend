using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    public class MstProcurementItemCategoryConfiguration : IEntityTypeConfiguration<MstProcurementItemCategory>
    {
        public void Configure(EntityTypeBuilder<MstProcurementItemCategory> builder)
        {
            builder.ToTable("MstProcurementItemCategory", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CategoryCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasIndex(x => x.CategoryCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
