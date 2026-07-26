using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxCandidateScreeningConfiguration : IEntityTypeConfiguration<TrxCandidateScreening>
    {
        public void Configure(EntityTypeBuilder<TrxCandidateScreening> builder)
        {
            builder.ToTable("TrxCandidateScreening", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ScreeningType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ScreeningStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Score).HasPrecision(10, 2);
            builder.Property(x => x.ScreeningResult).HasMaxLength(30);
            builder.Property(x => x.ScreenedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Notes).HasMaxLength(1500);
            builder.Property(x => x.ScreeningDataJson).HasColumnType("jsonb");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.CandidateApplication).WithMany().HasForeignKey(x => x.CandidateApplicationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RecruitmentStage).WithMany().HasForeignKey(x => x.RecruitmentStageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ScreenedByUser).WithMany().HasForeignKey(x => x.ScreenedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CandidateApplicationId, x.ScreeningType, x.ScreeningStatus });
        }
    }
}
