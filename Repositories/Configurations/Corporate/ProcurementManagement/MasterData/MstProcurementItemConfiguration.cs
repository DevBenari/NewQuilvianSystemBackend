using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    /// <summary>Katalog Pengadaan — kamus data bagian 2. FK ke master lain memakai Restrict.</summary>
    public class MstProcurementItemConfiguration : IEntityTypeConfiguration<MstProcurementItem>
    {
        public void Configure(EntityTypeBuilder<MstProcurementItem> builder)
        {
            builder.ToTable("MstProcurementItem", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ItemCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.DefaultSpecification).HasMaxLength(1000);

            // QBE-CODE-004: kode bisnis unik dijaga database.
            builder.HasIndex(x => x.ItemCode).IsUnique();
            builder.HasIndex(x => x.ItemName);

            builder.HasOne(x => x.ProcurementType).WithMany()
                .HasForeignKey(x => x.ProcurementTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ItemCategory).WithMany()
                .HasForeignKey(x => x.ItemCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Measurement).WithMany()
                .HasForeignKey(x => x.MeasurementId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Drug).WithMany()
                .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
