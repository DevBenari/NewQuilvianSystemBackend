using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan jejak revisi nilai isian — <c>data-dictionary.md</c> bagian 3.</summary>
    public class McuExaminationResultValueRevisionConfiguration : IEntityTypeConfiguration<McuExaminationResultValueRevision>
    {
        public void Configure(EntityTypeBuilder<McuExaminationResultValueRevision> builder)
        {
            builder.ToTable("McuExaminationResultValueRevision", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.ResultValueId);

            builder.HasOne(x => x.ResultValue)
                .WithMany(x => x.Revisions)
                .HasForeignKey(x => x.ResultValueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
