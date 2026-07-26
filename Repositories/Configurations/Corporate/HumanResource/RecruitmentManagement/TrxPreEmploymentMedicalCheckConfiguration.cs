using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxPreEmploymentMedicalCheckConfiguration : IEntityTypeConfiguration<TrxPreEmploymentMedicalCheck>
    {
        public void Configure(EntityTypeBuilder<TrxPreEmploymentMedicalCheck> builder)
        {
            builder.ToTable("TrxPreEmploymentMedicalCheck", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.MedicalProviderName).HasMaxLength(200);
            builder.Property(x => x.ExaminationNumber).HasMaxLength(100);
            builder.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ScheduledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ExaminedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ResultIssuedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.MedicalCheckStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.FitnessResult).HasMaxLength(30);
            builder.Property(x => x.WorkRestrictions).HasMaxLength(2000);
            builder.Property(x => x.ValidUntil).HasColumnType("date");
            builder.Property(x => x.ResultDocumentPath).HasMaxLength(500);
            builder.Property(x => x.ReviewedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AdministrativeNotes).HasMaxLength(1500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.CandidateApplication).WithMany().HasForeignKey(x => x.CandidateApplicationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Candidate).WithMany().HasForeignKey(x => x.CandidateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReviewedByWorkforceProfile).WithMany().HasForeignKey(x => x.ReviewedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReviewedByUser).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CandidateApplicationId, x.MedicalCheckStatus, x.FitnessResult });
        }
    }
}
