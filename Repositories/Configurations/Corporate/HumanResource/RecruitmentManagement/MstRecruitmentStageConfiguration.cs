using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class MstRecruitmentStageConfiguration : IEntityTypeConfiguration<MstRecruitmentStage>
    {
        public void Configure(EntityTypeBuilder<MstRecruitmentStage> builder)
        {
            builder.ToTable("MstRecruitmentStage", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.StageCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.StageName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.StageType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.StageCode).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.StageOrder, x.StageType, x.IsActive });
        }
    }
}
