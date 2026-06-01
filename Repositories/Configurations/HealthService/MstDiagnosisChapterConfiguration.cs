using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDiagnosisChapterConfiguration : IEntityTypeConfiguration<MstDiagnosisChapter>
    {
        public void Configure(EntityTypeBuilder<MstDiagnosisChapter> entity)
        {
            entity.ToTable("MstDiagnosisChapter", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ChapterCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ChapterName)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.DiagnosisCodeRangeStart)
                .HasMaxLength(50);

            entity.Property(x => x.DiagnosisCodeRangeEnd)
                .HasMaxLength(50);

            entity.Property(x => x.IcdVersion)
                .HasMaxLength(100)
                .HasDefaultValue("ICD-10")
                .IsRequired();

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasIndex(x => x.ChapterCode)
                .IsUnique();

            entity.HasIndex(x => x.ChapterName);

            entity.HasIndex(x => x.IcdVersion);

            entity.HasIndex(x => new
            {
                x.IcdVersion,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DiagnosisCodeRangeStart,
                x.DiagnosisCodeRangeEnd
            });
        }
    }
}