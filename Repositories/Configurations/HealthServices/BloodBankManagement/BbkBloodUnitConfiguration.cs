using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan kantong darah operasional, aggregate root <c>BD-AGG-03</c>.</summary>
    /// <remarks>
    /// <para>
    /// <b>Index unik <c>PmiBagNumber</c></b> mengikuti kamus data: satu nomor kantong PMI tidak
    /// dapat tercatat dua kali, di permintaan mana pun.
    /// </para>
    /// <para>
    /// <b><c>IX_BbkBloodUnit_CurrentPlacementId</c> belum ada</b> karena kolomnya lahir bersama
    /// <c>BbkBloodUnitPlacement</c> pada <c>BE-BD-015</c>.
    /// </para>
    /// <para>
    /// Seluruh FK memakai <c>Restrict</c>: kantong adalah rekam klinis yang asalnya tidak boleh
    /// putus.
    /// </para>
    /// </remarks>
    public class BbkBloodUnitConfiguration : IEntityTypeConfiguration<BbkBloodUnit>
    {
        public void Configure(EntityTypeBuilder<BbkBloodUnit> builder)
        {
            builder.ToTable("BbkBloodUnit", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PmiBagNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ProviderRequestId).IsRequired();
            builder.Property(x => x.ReceiptId).IsRequired();
            builder.Property(x => x.BloodComponentId).IsRequired();
            builder.Property(x => x.IsExcess).IsRequired();
            builder.Property(x => x.UnitStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.IssuedViaEmergency).IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.PmiBagNumber).IsUnique();
            builder.HasIndex(x => x.ProviderRequestId);
            builder.HasIndex(x => x.ReceiptId);
            builder.HasIndex(x => x.BloodComponentId);
            builder.HasIndex(x => x.UnitStatus);
            builder.HasIndex(x => x.IssuedToPatientId);

            builder.HasOne(x => x.BloodComponent)
                .WithMany()
                .HasForeignKey(x => x.BloodComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.IssuedToPatient)
                .WithMany()
                .HasForeignKey(x => x.IssuedToPatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
