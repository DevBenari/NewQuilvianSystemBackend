using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    /// <summary><c>InpAdmissionDepositStatement</c> — kamus data 20.10 dan DDL 20.18 (<c>BE-RWI-192</c>).</summary>
    public class InpAdmissionDepositStatementConfiguration : IEntityTypeConfiguration<InpAdmissionDepositStatement>
    {
        public void Configure(EntityTypeBuilder<InpAdmissionDepositStatement> builder)
        {
            builder.ToTable("InpAdmissionDepositStatement", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DueAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.MinimumPolicyAmount).HasPrecision(18, 2);
            builder.Property(x => x.ReceivedAmount).HasPrecision(18, 2);
            builder.Property(x => x.ShortfallAmount).HasPrecision(18, 2);
            builder.Property(x => x.AmountsReadAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.Document).WithOne(x => x.DepositStatement)
                .HasForeignKey<InpAdmissionDepositStatement>(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
