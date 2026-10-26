using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan resume MCU berversi — <c>data-dictionary.md</c> bagian 3.</summary>
    /// <remarks>
    /// Nomor versi unik per order, dan paling banyak satu versi berlaku per order. Keduanya dijaga
    /// database supaya dua koreksi serentak tidak sama-sama menjadi versi berlaku.
    /// </remarks>
    public class McuResumeConfiguration : IEntityTypeConfiguration<McuResume>
    {
        public void Configure(EntityTypeBuilder<McuResume> builder)
        {
            builder.ToTable("McuResume", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ResumeStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.IsCurrent).HasDefaultValue(true).ValueGeneratedNever();

            builder.HasIndex(x => new { x.McuOrderId, x.VersionNumber })
                .IsUnique()
                .HasDatabaseName("IX_McuResume_Order_Version");

            builder.HasIndex(x => x.McuOrderId)
                .IsUnique()
                .HasFilter("\"IsCurrent\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_McuResume_Order_Current");

            builder.HasOne(x => x.McuOrder)
                .WithMany(x => x.Resumes)
                .HasForeignKey(x => x.McuOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FitnessCategory)
                .WithMany()
                .HasForeignKey(x => x.FitnessCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AuthorDoctor)
                .WithMany()
                .HasForeignKey(x => x.AuthorDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApprovedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReopenedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.ReopenedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
