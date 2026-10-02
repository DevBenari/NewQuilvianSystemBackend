using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>RJ-DOC-REV-BE-006 — kelompok ICD Diagnosa (DTD).</summary>
    public class MstDiagnosisGroupConfiguration : IEntityTypeConfiguration<MstDiagnosisGroup>
    {
        public void Configure(EntityTypeBuilder<MstDiagnosisGroup> builder)
        {
            builder.ToTable("MstDiagnosisGroup", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SourceCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.DtdNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.CodeRangeText).HasMaxLength(500);
            builder.Property(x => x.GroupName).HasMaxLength(300).IsRequired();
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.SourceCode).IsUnique();
            builder.HasIndex(x => x.DtdNumber);
            builder.HasIndex(x => x.GroupName);
        }
    }
}
