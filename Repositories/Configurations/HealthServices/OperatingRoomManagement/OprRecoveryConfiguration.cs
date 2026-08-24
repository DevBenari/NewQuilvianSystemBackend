using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprRecoveryConfiguration : IEntityTypeConfiguration<OprRecovery>
{
    public void Configure(EntityTypeBuilder<OprRecovery> builder)
    {
        builder.ToTable("OprRecovery", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ScoreSystem).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ScoreValue).HasPrecision(18, 4);
        builder.Property(x => x.ObservationJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.DecisionNote).HasMaxLength(2000);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.OprCaseId).IsUnique();
        builder.HasOne(x => x.OprCase).WithOne().HasForeignKey<OprRecovery>(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
