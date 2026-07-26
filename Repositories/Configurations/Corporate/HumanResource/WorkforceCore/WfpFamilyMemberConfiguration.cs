using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpFamilyMemberConfiguration : IEntityTypeConfiguration<WfpFamilyMember>
    {
        public void Configure(EntityTypeBuilder<WfpFamilyMember> builder)
        {
            builder.ToTable("WfpFamilyMember", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.Relationship).HasMaxLength(100).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Gender).HasConversion<int?>();
            builder.Property(x => x.BirthDate).HasColumnType("date");
            builder.Property(x => x.IdentityType).HasMaxLength(50);
            builder.Property(x => x.IdentityNumber).HasMaxLength(100);
            builder.Property(x => x.MaritalStatusText).HasMaxLength(100);
            builder.Property(x => x.Occupation).HasMaxLength(200);
            builder.Property(x => x.PhoneNumber).HasMaxLength(30);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.Relationship, x.FullName });
            builder.HasIndex(x => new { x.WorkforceProfileId, x.IdentityNumber }).HasFilter("\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");
        }
    }
}
