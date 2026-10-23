using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan order MCU — <c>data-dictionary.md</c> bagian 3.</summary>
    /// <remarks>
    /// Index unik <c>OrderNumber</c> (tanpa filter, nomor tidak pernah dipakai ulang) adalah penjaga
    /// terakhir keunikan nomor bisnis (<c>QBE-CODE-004</c>). Satu kunjungan paling banyak satu order.
    /// Seluruh FK <c>Restrict</c>: order adalah rekam layanan.
    /// </remarks>
    public class McuOrderConfiguration : IEntityTypeConfiguration<McuOrder>
    {
        public void Configure(EntityTypeBuilder<McuOrder> builder)
        {
            builder.ToTable("McuOrder", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PatientMatchMethod).HasConversion<int?>();
            builder.Property(x => x.Source).HasConversion<int>().IsRequired();
            builder.Property(x => x.OrderStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.CheckInChannel).HasConversion<int?>();
            builder.Property(x => x.ClearanceBasis).HasConversion<int?>();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.OrderNumber).IsUnique();

            builder.HasIndex(x => x.EncounterId)
                .IsUnique()
                .HasFilter("\"EncounterId\" IS NOT NULL");

            builder.HasIndex(x => new { x.ExaminationDate, x.OrderStatus });
            builder.HasIndex(x => x.ParticipantNik);
            builder.HasIndex(x => x.PackageId);
            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.SourceEncounterId);
            builder.HasIndex(x => x.CompanyGuarantorId);
            builder.HasIndex(x => x.BookingImportId);
            builder.HasIndex(x => x.DailyScheduleId);
            builder.HasIndex(x => x.McuDoctorId);

            builder.HasOne(x => x.Package)
                .WithMany()
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SourceEncounter)
                .WithMany()
                .HasForeignKey(x => x.SourceEncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RecommendingDoctor)
                .WithMany()
                .HasForeignKey(x => x.RecommendingDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.CompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BookingImport)
                .WithMany()
                .HasForeignKey(x => x.BookingImportId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DailySchedule)
                .WithMany()
                .HasForeignKey(x => x.DailyScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.McuDoctor)
                .WithMany()
                .HasForeignKey(x => x.McuDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AppliedTariff)
                .WithMany()
                .HasForeignKey(x => x.AppliedTariffId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PackageInvoiceItem)
                .WithMany()
                .HasForeignKey(x => x.PackageInvoiceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.PatientMatchConfirmedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CheckedInByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
