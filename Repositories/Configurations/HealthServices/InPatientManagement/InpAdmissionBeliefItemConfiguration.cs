using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary><c>InpAdmissionBeliefItem</c> — kamus data 20.8 dan DDL 20.18 (<c>BE-RWI-192</c>).</summary>
    public class InpAdmissionBeliefItemConfiguration : IEntityTypeConfiguration<InpAdmissionBeliefItem>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionBeliefItem> builder)
        {
            builder.ToTable("InpAdmissionBeliefItem", "public", table =>
            {
                // Paling banyak lima butir (RWI-DEC-242).
                table.HasCheckConstraint("CK_InpAdmissionBeliefItem_ItemNo", "\"ItemNo\" BETWEEN 1 AND 5");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text).IsRequired().HasMaxLength(500);

            builder.HasIndex(x => new { x.DocumentId, x.ItemNo }).IsUnique();

            builder.HasOne(x => x.Document).WithMany(x => x.BeliefItems).HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
