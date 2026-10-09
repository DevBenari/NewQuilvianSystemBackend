using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary>
    /// <c>InpAdmissionHandoverItem</c> — kamus data 20.5 dan DDL 20.18 (<c>BE-RWI-192</c>). FK ke
    /// <c>MstInpatientClearanceItem</c>, maka migration <c>E10</c> wajib sesudah <c>E9</c>.
    /// </summary>
    public class InpAdmissionHandoverItemConfiguration : IEntityTypeConfiguration<InpAdmissionHandoverItem>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionHandoverItem> builder)
        {
            builder.ToTable("InpAdmissionHandoverItem", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemCodeSnapshot).IsRequired().HasMaxLength(50);
            builder.Property(x => x.ItemNameSnapshot).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Choice).HasConversion<int?>();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => new { x.DocumentId, x.ClearanceItemId }).IsUnique();
            builder.HasIndex(x => new { x.DocumentId, x.LineNo }).IsUnique();

            builder.HasOne(x => x.Document).WithMany(x => x.HandoverItems).HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ClearanceItem).WithMany().HasForeignKey(x => x.ClearanceItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
