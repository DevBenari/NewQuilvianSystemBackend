using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    public class MstProcurementTypeConfiguration : IEntityTypeConfiguration<MstProcurementType>
    {
        public void Configure(EntityTypeBuilder<MstProcurementType> builder)
        {
            builder.ToTable("MstProcurementType", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TypeCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.TypeName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasIndex(x => x.TypeCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
