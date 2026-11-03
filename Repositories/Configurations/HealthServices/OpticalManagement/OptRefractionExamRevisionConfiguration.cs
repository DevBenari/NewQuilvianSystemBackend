using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptRefractionExamRevision</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptRefractionExamRevisionConfiguration : IEntityTypeConfiguration<OptRefractionExamRevision>
    {
        public void Configure(EntityTypeBuilder<OptRefractionExamRevision> builder)
        {
            builder.ToTable("OptRefractionExamRevision", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AutorefOdSph).HasPrecision(5, 2);
            builder.Property(x => x.AutorefOdCyl).HasPrecision(5, 2);
            builder.Property(x => x.AutorefOsSph).HasPrecision(5, 2);
            builder.Property(x => x.AutorefOsCyl).HasPrecision(5, 2);
            builder.Property(x => x.SubjectiveOdSph).HasPrecision(5, 2);
            builder.Property(x => x.SubjectiveOdCyl).HasPrecision(5, 2);
            builder.Property(x => x.SubjectiveOdAdd).HasPrecision(4, 2);
            builder.Property(x => x.SubjectiveOsSph).HasPrecision(5, 2);
            builder.Property(x => x.SubjectiveOsCyl).HasPrecision(5, 2);
            builder.Property(x => x.SubjectiveOsAdd).HasPrecision(4, 2);
            builder.Property(x => x.PdDistance).HasPrecision(4, 1);
            builder.Property(x => x.PdNear).HasPrecision(4, 1);

            builder.HasIndex(x => new { x.RefractionExamId, x.RevisionNo }).IsUnique().HasDatabaseName("UX_OptRefractionExamRevision_RefractionExamId_RevisionNo");

            builder.HasOne(x => x.RefractionExam)
                .WithMany(x => x.Revisions)
                .HasForeignKey(x => x.RefractionExamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstWorkforceProfile>()
                .WithMany()
                .HasForeignKey(x => x.RevisedByWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
