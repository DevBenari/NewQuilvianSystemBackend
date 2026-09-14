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
    /// <b><c>CurrentPlacementId</c> dan FK melingkarnya</b> lahir pada <c>BE-BD-015</c>. Kantong
    /// menunjuk penempatannya, dan penempatan menunjuk balik kantongnya. Simpul itu dibereskan
    /// dengan kolom yang boleh kosong: kantong lahir tanpa penempatan, lalu terisi saat
    /// penempatan pertama (<c>02-backend-architecture.md</c>).
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
            // Index biasa, sesuai kamus data. Dinyatakan eksplisit karena FK melingkar kantong ⇄
            // penempatan membuat konvensi EF menandainya unik.
            builder.HasIndex(x => x.CurrentPlacementId).IsUnique(false);

            builder.HasOne(x => x.CurrentPlacement)
                .WithMany()
                .HasForeignKey(x => x.CurrentPlacementId)
                .OnDelete(DeleteBehavior.Restrict);

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
