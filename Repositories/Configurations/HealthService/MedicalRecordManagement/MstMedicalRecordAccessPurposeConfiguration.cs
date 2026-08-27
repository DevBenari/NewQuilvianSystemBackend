using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.MedicalRecordManagement
{
    public class MstMedicalRecordAccessPurposeConfiguration
        : IEntityTypeConfiguration<MstMedicalRecordAccessPurpose>
    {
        public void Configure(EntityTypeBuilder<MstMedicalRecordAccessPurpose> builder)
        {
            builder.ToTable("MstMedicalRecordAccessPurpose", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PurposeCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PurposeName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.HasIndex(x => x.PurposeCode).IsUnique();
            builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        }
    }
}
