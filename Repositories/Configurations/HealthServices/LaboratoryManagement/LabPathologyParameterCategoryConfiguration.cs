using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPathologyParameterCategoryConfiguration : IEntityTypeConfiguration<LabPathologyParameterCategory>
    {
        public void Configure(EntityTypeBuilder<LabPathologyParameterCategory> builder)
        {
            builder.ToTable("LabPathologyParameterCategory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabPathologyParameterId).IsRequired();
            builder.Property(x => x.LabPathologyCategoryId).IsRequired();

            // Satu parameter berlaku sekali per golongan. PARSIAL: keberlakuan yang dicabut lalu
            // dipasang kembali adalah tindakan yang wajar bagi kepala instalasi, dan tanpa
            // pembatas IsDelete ia akan ditolak database tanpa sebab yang masuk akal baginya.
            builder.HasIndex(x => new { x.LabPathologyParameterId, x.LabPathologyCategoryId })
                .IsUnique()
                .HasDatabaseName("IX_LabPathologyParameterCategory_ParameterId_CategoryId")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.LabPathologyCategoryId);

            // Restrict pada keduanya. Keberlakuan adalah bagian dari bentuk laporan yang sudah
            // terbit; parameter maupun golongan yang masih dirujuk tidak boleh hilang dari
            // bawahnya dan memutus penelusuran isi laporan lama.
            builder.HasOne(x => x.LabPathologyParameter)
                .WithMany()
                .HasForeignKey(x => x.LabPathologyParameterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LabPathologyCategory)
                .WithMany()
                .HasForeignKey(x => x.LabPathologyCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
