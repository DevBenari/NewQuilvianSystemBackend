using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    public class MstProcurementFundSourceConfiguration : IEntityTypeConfiguration<MstProcurementFundSource>
    {
        public void Configure(EntityTypeBuilder<MstProcurementFundSource> builder)
        {
            builder.ToTable("MstProcurementFundSource", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FundSourceCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.FundSourceName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasIndex(x => x.FundSourceCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
