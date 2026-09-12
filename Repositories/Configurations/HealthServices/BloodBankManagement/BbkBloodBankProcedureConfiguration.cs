using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan tindakan Bank Darah, aggregate root <c>BD-AGG-05</c>.</summary>
    /// <remarks>
    /// <para>
    /// Index mengikuti kamus data kontrak <c>v4</c>: <c>ProcedureNumber</c> unik, lalu
    /// <c>BloodOrderId</c>, <c>ServiceUnitId</c>, <c>BdrsDoctorId</c>, <c>PatientClassId</c>,
    /// <c>ProcedureRefId</c>, dan <c>ProcedureStatus</c>.
    /// </para>
    /// <para>
    /// <b><c>ProcedureStatus</c> adalah concurrency token.</b> Kamus data tidak memberi kolom
    /// <c>Version</c> pada tabel ini, sehingga penjaga tulis-bersamaan memakai status itu sendiri:
    /// <c>UPDATE</c> penyelesaian hanya berhasil bila status di database masih <c>Recorded</c>.
    /// Tanpa perubahan schema, dua penyelesaian serentak tidak sama-sama tersimpan.
    /// </para>
    /// <para>
    /// Seluruh FK memakai <c>Restrict</c>. FK <c>TariffId</c> ke <c>MstTariff</c> mengikuti kamus
    /// data ("FK tarif"); EF membentuk index untuknya menurut konvensi.
    /// </para>
    /// </remarks>
    public class BbkBloodBankProcedureConfiguration : IEntityTypeConfiguration<BbkBloodBankProcedure>
    {
        public void Configure(EntityTypeBuilder<BbkBloodBankProcedure> builder)
        {
            builder.ToTable("BbkBloodBankProcedure", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProcedureNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.BloodOrderId).IsRequired();
            builder.Property(x => x.ServiceUnitId).IsRequired();
            builder.Property(x => x.BdrsDoctorId).IsRequired();
            builder.Property(x => x.PerformedByUserId).IsRequired();
            builder.Property(x => x.PatientClassId).IsRequired();
            builder.Property(x => x.ProcedureRefId).IsRequired();
            builder.Property(x => x.TariffId).IsRequired();
            builder.Property(x => x.ProcedureCodeSnapshot).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ProcedureNameSnapshot).HasMaxLength(200).IsRequired();
            builder.Property(x => x.TariffAmountSnapshot).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.ProcedureStatus).HasConversion<int>().IsRequired().IsConcurrencyToken();

            builder.HasIndex(x => x.ProcedureNumber).IsUnique();
            builder.HasIndex(x => x.BloodOrderId);
            builder.HasIndex(x => x.ServiceUnitId);
            builder.HasIndex(x => x.BdrsDoctorId);
            builder.HasIndex(x => x.PatientClassId);
            builder.HasIndex(x => x.ProcedureRefId);
            builder.HasIndex(x => x.ProcedureStatus);

            builder.HasOne(x => x.BloodOrder)
                .WithMany()
                .HasForeignKey(x => x.BloodOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BdrsDoctor)
                .WithMany()
                .HasForeignKey(x => x.BdrsDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientClass)
                .WithMany()
                .HasForeignKey(x => x.PatientClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProcedureRef)
                .WithMany()
                .HasForeignKey(x => x.ProcedureRefId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Tariff)
                .WithMany()
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
