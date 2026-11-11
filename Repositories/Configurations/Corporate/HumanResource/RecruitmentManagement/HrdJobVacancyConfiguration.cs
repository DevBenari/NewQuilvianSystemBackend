using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdJobVacancyConfiguration : IEntityTypeConfiguration<HrdJobVacancy>
    {
        public void Configure(EntityTypeBuilder<HrdJobVacancy> builder)
        {
            builder.ToTable("HrdJobVacancy", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.VacancyNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.VacancyNumber)
                .IsUnique();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(x => x.RequisitionId);
            builder.HasIndex(x => x.IsClinical);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.Requisition)
                .WithMany(r => r.Vacancies)
                .HasForeignKey(x => x.RequisitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Applications)
                .WithOne(a => a.Vacancy)
                .HasForeignKey(a => a.VacancyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
