using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprCaseProcedureConfiguration : IEntityTypeConfiguration<OprCaseProcedure>
{
    public void Configure(EntityTypeBuilder<OprCaseProcedure> builder)
    {
        builder.ToTable("OprCaseProcedure", "public");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PatientProcedureId).IsUnique();
        builder.HasIndex(x => new { x.OprCaseId, x.Sequence }).IsUnique();
        builder.HasIndex(x => x.OprCaseId).IsUnique().HasFilter("\"IsPrimary\" = TRUE AND \"IsDelete\" = FALSE");
        builder.HasOne(x => x.OprCase).WithMany(x => x.Procedures).HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PatientProcedure).WithMany().HasForeignKey(x => x.PatientProcedureId).OnDelete(DeleteBehavior.Restrict);
    }
}
