using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class MstAssessmentMethodConfiguration : IEntityTypeConfiguration<MstAssessmentMethod>
    {
        public void Configure(EntityTypeBuilder<MstAssessmentMethod> builder)
        {
            builder.ToTable("MstAssessmentMethod", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.MethodCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.MethodName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.MethodType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ProviderName).HasMaxLength(200);
            builder.Property(x => x.ProviderUrl).HasMaxLength(500);
            builder.Property(x => x.MaximumScore).HasPrecision(10, 2);
            builder.Property(x => x.PassingScore).HasPrecision(10, 2);
            builder.Property(x => x.ConfigurationJson).HasColumnType("jsonb");
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.Competency).WithMany().HasForeignKey(x => x.CompetencyId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.MethodCode).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.MethodType, x.CompetencyId, x.IsActive });
        }
    }
}
