using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    /// <summary>Template Hasil radiologi — <c>BE-RAD-26</c>.</summary>
    public class RadReportTemplateConfiguration : IEntityTypeConfiguration<RadReportTemplate>
    {
        public void Configure(EntityTypeBuilder<RadReportTemplate> builder)
        {
            builder.ToTable("RadReportTemplate", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TemplateCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TemplateName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Impression).IsRequired();

            builder.HasOne<MstRadModality>().WithMany().HasForeignKey(x => x.ModalityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<MstProcedure>().WithMany().HasForeignKey(x => x.ProcedureId).OnDelete(DeleteBehavior.Restrict);

            // Kode unik di antara template yang belum dihapus.
            builder.HasIndex(x => x.TemplateCode).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.IsActive, x.ModalityId, x.ProcedureId });
        }
    }
}
