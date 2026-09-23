using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabOrganismConfiguration : IEntityTypeConfiguration<LabOrganism>
    {
        public void Configure(EntityTypeBuilder<LabOrganism> builder)
        {
            builder.ToTable("LabOrganism", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrganismCode).HasMaxLength(32).IsRequired();
            builder.Property(x => x.OrganismName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(256);

            // VAL-91. PARSIAL: penghapusan di sistem ini bersifat penandaan, sehingga tanpa
            // pembatas ini kode yang pernah dipakai lalu dihapus nol akan pernah dapat dipakai
            // lagi. Kelas cacat yang sama dengan LAB-CONFLICT-005.
            builder.HasIndex(x => x.OrganismCode)
                .IsUnique()
                .HasDatabaseName("IX_LabOrganism_OrganismCode")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.OrganismName);

            builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        }
    }
}
