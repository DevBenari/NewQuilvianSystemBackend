using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class MstInterviewTemplateConfiguration : IEntityTypeConfiguration<MstInterviewTemplate>
    {
        public void Configure(EntityTypeBuilder<MstInterviewTemplate> builder)
        {
            builder.ToTable("MstInterviewTemplate", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.TemplateCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TemplateName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.InterviewType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.PassingScore).HasPrecision(10, 2);
            builder.Property(x => x.QuestionDefinitionJson).HasColumnType("jsonb");
            builder.Property(x => x.EvaluationCriteriaJson).HasColumnType("jsonb");
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.JobFamily).WithMany().HasForeignKey(x => x.JobFamilyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.JobLevel).WithMany().HasForeignKey(x => x.JobLevelId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RatingScale).WithMany().HasForeignKey(x => x.RatingScaleId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TemplateCode).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.PositionId, x.ProfessionId, x.SpecializationId, x.InterviewType, x.IsActive });
        }
    }
}
