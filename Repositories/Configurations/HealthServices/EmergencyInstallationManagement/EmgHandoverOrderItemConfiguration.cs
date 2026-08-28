using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgHandoverOrderItemConfiguration : IEntityTypeConfiguration<EmgHandoverOrderItem>
    {
        public void Configure(EntityTypeBuilder<EmgHandoverOrderItem> builder)
        {
            builder.ToTable("EmgHandoverOrderItem", "public", table =>
            {
                table.HasCheckConstraint("CK_EmergencyOrderItem_Reference", "(\"OrderSource\" = 1 AND \"OrderReferenceId\" IS NOT NULL AND \"ExternalReference\" IS NULL) OR (\"OrderSource\" = 2 AND \"OrderReferenceId\" IS NULL AND \"ExternalReference\" IS NOT NULL)");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OrderKind).HasConversion<int>();
            builder.Property(x => x.OrderSource).HasConversion<int>();
            builder.Property(x => x.Action).HasConversion<int>();
            builder.Property(x => x.AcceptanceStatus).HasConversion<int>();
            builder.Property(x => x.OrderDescription).HasMaxLength(500).IsRequired();
            builder.HasIndex(x => new { x.EmergencyDepartureId, x.OrderKind, x.OrderReferenceId })
                .IsUnique().HasFilter("\"IsEffective\" AND \"OrderSource\" = 1 AND NOT \"IsDelete\"")
                .HasDatabaseName("UX_EmergencyHandoverOrderItem_Internal");
            builder.HasIndex(x => new { x.EmergencyDepartureId, x.OrderKind, x.ExternalReference })
                .IsUnique().HasFilter("\"IsEffective\" AND \"OrderSource\" = 2 AND NOT \"IsDelete\"")
                .HasDatabaseName("UX_EmergencyHandoverOrderItem_External");
            builder.HasOne(x => x.EmergencyDeparture).WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.EmergencyDepartureId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToServiceUnit).WithMany()
                .HasForeignKey(x => x.ToServiceUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SupersedesOrderItem).WithMany()
                .HasForeignKey(x => x.SupersedesOrderItemId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
