using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan penugasan dokter MCU — <c>data-dictionary.md</c> bagian 3.</summary>
    /// <remarks>Satu penugasan berlaku per order: dijaga index unik ber-filter <c>EndedAt</c> kosong.</remarks>
    public class McuOrderDoctorAssignmentConfiguration : IEntityTypeConfiguration<McuOrderDoctorAssignment>
    {
        public void Configure(EntityTypeBuilder<McuOrderDoctorAssignment> builder)
        {
            builder.ToTable("McuOrderDoctorAssignment", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.McuOrderId)
                .IsUnique()
                .HasFilter("\"EndedAt\" IS NULL AND \"IsDelete\" = false")
                .HasDatabaseName("IX_McuOrderDoctorAssignment_Active");

            builder.HasIndex(x => x.DoctorId);

            builder.HasOne(x => x.McuOrder)
                .WithMany(x => x.DoctorAssignments)
                .HasForeignKey(x => x.McuOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
