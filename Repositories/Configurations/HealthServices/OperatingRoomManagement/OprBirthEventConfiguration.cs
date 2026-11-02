using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprBirthEventConfiguration : IEntityTypeConfiguration<OprBirthEvent>
{
    public void Configure(EntityTypeBuilder<OprBirthEvent> builder)
    {
        builder.ToTable("OprBirthEvent", "public");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BirthDateTime)
            .IsRequired();

        builder.Property(x => x.Gender)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.WeightGrams)
            .HasColumnType("decimal(8,2)")
            .IsRequired();

        builder.Property(x => x.LengthCentimeters)
            .HasColumnType("decimal(8,2)")
            .IsRequired();

        builder.Property(x => x.HeadCircumferenceCm)
            .HasColumnType("decimal(8,2)");

        builder.Property(x => x.ApgarScore1Min)
            .IsRequired();

        builder.Property(x => x.ApgarScore5Min)
            .IsRequired();

        builder.Property(x => x.ResuscitationNote)
            .HasMaxLength(1000);

        builder.Property(x => x.ConditionNote)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.OprCaseId);

        builder.HasOne(x => x.OprCase)
            .WithMany()
            .HasForeignKey(x => x.OprCaseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
