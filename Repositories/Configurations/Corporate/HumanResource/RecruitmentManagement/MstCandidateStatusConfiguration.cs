using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class MstCandidateStatusConfiguration : IEntityTypeConfiguration<MstCandidateStatus>
    {
        public void Configure(EntityTypeBuilder<MstCandidateStatus> builder)
        {
            builder.ToTable("MstCandidateStatus", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.StatusName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.StatusCategory).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.StatusCode).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.StatusCategory, x.IsFinalStatus, x.IsActive, x.SortOrder });
        }
    }
}
