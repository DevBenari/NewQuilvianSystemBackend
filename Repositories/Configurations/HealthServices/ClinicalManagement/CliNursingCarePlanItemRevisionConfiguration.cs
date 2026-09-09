using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>
    /// Bentuk tabel riwayat versi butir rencana asuhan - <c>BE-RWI-060</c>.
    /// </summary>
    public class CliNursingCarePlanItemRevisionConfiguration
        : IEntityTypeConfiguration<CliNursingCarePlanItemRevision>
    {
        public void Configure(EntityTypeBuilder<CliNursingCarePlanItemRevision> entity)
        {
            entity.ToTable("CliNursingCarePlanItemRevision", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CarePlanItemId)
                .IsRequired();

            entity.Property(x => x.VersionNumber)
                .IsRequired();

            entity.Property(x => x.ProblemStatement)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.GoalStatement)
                .HasMaxLength(500);

            entity.Property(x => x.PlannedIntervention);

            entity.Property(x => x.EvaluationNote);

            entity.Property(x => x.RevisedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.OriginalAuthorEmployeeId)
                .IsRequired();

            entity.Property(x => x.OriginalAuthoredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

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

            entity.HasOne(x => x.CarePlanItem)
                .WithMany()
                .HasForeignKey(x => x.CarePlanItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict, bukan SetNull. Versi lama tanpa penulis bukan bukti apa pun, dan justru
            // sifat itulah yang dituntut AC-CAP013-02.
            entity.HasOne(x => x.OriginalAuthorEmployee)
                .WithMany()
                .HasForeignKey(x => x.OriginalAuthorEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu butir tidak pernah memiliki dua baris untuk versi yang sama. Penjaganya di
            // database, karena dua permintaan pembaruan yang tiba bersamaan dapat membaca nomor
            // versi yang sama sebelum salah satunya sempat menyimpan.
            entity.HasIndex(x => new { x.CarePlanItemId, x.VersionNumber })
                .IsUnique();

            entity.HasIndex(x => x.CarePlanItemId);
        }
    }
}
