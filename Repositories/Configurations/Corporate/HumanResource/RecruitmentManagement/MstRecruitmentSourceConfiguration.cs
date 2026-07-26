using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class MstRecruitmentSourceConfiguration : IEntityTypeConfiguration<MstRecruitmentSource>
    {
        public void Configure(EntityTypeBuilder<MstRecruitmentSource> builder)
        {
            builder.ToTable("MstRecruitmentSource", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.SourceCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.SourceName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.SourceType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ProviderName).HasMaxLength(200);
            builder.Property(x => x.SourceUrl).HasMaxLength(500);
            builder.Property(x => x.DefaultCostAmount).HasPrecision(18, 2);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.SourceCode).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.SourceType, x.IsActive, x.SortOrder });
        }
    }
}
