using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstEmployeeGradeConfiguration : IEntityTypeConfiguration<MstEmployeeGrade>
    {
        public void Configure(EntityTypeBuilder<MstEmployeeGrade> entity)
        {
            entity.ToTable("MstEmployeeGrade", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.GradeCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.GradeName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.GradeOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

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

            entity.HasOne(x => x.JobLevel)
                .WithMany(x => x.EmployeeGrades)
                .HasForeignKey(x => x.JobLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.GradeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.GradeName);

            entity.HasIndex(x => new { x.JobLevelId, x.GradeOrder, x.IsActive, x.IsDelete });
        }
    }
}
