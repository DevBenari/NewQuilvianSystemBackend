using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpEmergencyContactConfiguration : IEntityTypeConfiguration<WfpEmergencyContact>
    {
        public void Configure(EntityTypeBuilder<WfpEmergencyContact> builder)
        {
            builder.ToTable("WfpEmergencyContact", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Relationship).HasMaxLength(100).IsRequired();
            builder.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.WhatsAppNumber).HasMaxLength(30);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.PriorityOrder });
            builder.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");
        }
    }
}
