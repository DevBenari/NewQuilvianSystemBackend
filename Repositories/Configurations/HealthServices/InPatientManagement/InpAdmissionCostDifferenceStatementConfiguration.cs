using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary>
    /// <c>InpAdmissionCostDifferenceStatement</c> — kamus data 20.9 dan DDL 20.18 (<c>BE-RWI-192</c>).
    /// </summary>
    public class InpAdmissionCostDifferenceStatementConfiguration
        : IEntityTypeConfiguration<InpAdmissionCostDifferenceStatement>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionCostDifferenceStatement> builder)
        {
            builder.ToTable("InpAdmissionCostDifferenceStatement", "public", table =>
            {
                // "Saudara kandung lainnya" wajib berketerangan.
                table.HasCheckConstraint("CK_InpAdmissionCostDifferenceStatement_Other",
                    "\"Subject\" <> 5 OR \"SubjectOtherText\" IS NOT NULL");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Subject).HasConversion<int>().IsRequired();
            builder.Property(x => x.SubjectOtherText).HasMaxLength(100);

            builder.HasOne(x => x.Document).WithOne(x => x.CostDifferenceStatement)
                .HasForeignKey<InpAdmissionCostDifferenceStatement>(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
