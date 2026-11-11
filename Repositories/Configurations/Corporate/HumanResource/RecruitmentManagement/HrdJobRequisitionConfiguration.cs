using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdJobRequisitionConfiguration : IEntityTypeConfiguration<HrdJobRequisition>
    {
        public void Configure(EntityTypeBuilder<HrdJobRequisition> builder)
        {
            builder.ToTable("HrdJobRequisition", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequisitionNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.RequisitionNumber)
                .IsUnique();

            builder.HasIndex(x => x.HeadcountRequestId);
            builder.HasIndex(x => x.HospitalSiteId);
            builder.HasIndex(x => x.OrganizationUnitId);
            builder.HasIndex(x => x.PositionId);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.HeadcountRequest)
                .WithMany()
                .HasForeignKey(x => x.HeadcountRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Vacancies)
                .WithOne(v => v.Requisition)
                .HasForeignKey(v => v.RequisitionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
