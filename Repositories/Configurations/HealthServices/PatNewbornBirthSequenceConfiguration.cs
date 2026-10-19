using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices;

public sealed class PatNewbornBirthSequenceConfiguration : IEntityTypeConfiguration<PatNewbornBirthSequence>
{
    public void Configure(EntityTypeBuilder<PatNewbornBirthSequence> entity)
    {
        entity.ToTable("PatNewbornBirthSequence", "public");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DeliveryCaseId).IsRequired();
        entity.Property(x => x.LastAllocatedBirthOrder).IsRequired();
        entity.HasIndex(x => x.DeliveryCaseId)
            .IsUnique()
            .HasDatabaseName("UX_PatNewbornBirthSequence_DeliveryCaseId");
        entity.ToTable(table => table.HasCheckConstraint(
            "CK_PatNewbornBirthSequence_LastAllocatedBirthOrder_Positive",
            "\"LastAllocatedBirthOrder\" > 0"));
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
    }
}
