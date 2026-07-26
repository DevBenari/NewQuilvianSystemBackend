using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxJobVacancyConfiguration : IEntityTypeConfiguration<TrxJobVacancy>
    {
        public void Configure(EntityTypeBuilder<TrxJobVacancy> builder)
        {
            builder.ToTable("TrxJobVacancy", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.VacancyNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.VacancyTitle).HasMaxLength(200).IsRequired();
            builder.Property(x => x.VacancyDescription).HasMaxLength(3000);
            builder.Property(x => x.Responsibilities).HasMaxLength(3000);
            builder.Property(x => x.Requirements).HasMaxLength(3000);
            builder.Property(x => x.OpenDate).HasColumnType("date");
            builder.Property(x => x.CloseDate).HasColumnType("date");
            builder.Property(x => x.PublicationStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.EmploymentLocationType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.PublishedSalaryMinimum).HasPrecision(18, 2);
            builder.Property(x => x.PublishedSalaryMaximum).HasPrecision(18, 2);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.PublishedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.JobRequisition).WithMany().HasForeignKey(x => x.JobRequisitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RecruitmentSource).WithMany().HasForeignKey(x => x.RecruitmentSourceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PublishedByUser).WithMany().HasForeignKey(x => x.PublishedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.VacancyNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.PublicationStatus, x.OpenDate, x.CloseDate, x.IsActive });
            builder.HasIndex(x => x.JobRequisitionId);
        }
    }
}
