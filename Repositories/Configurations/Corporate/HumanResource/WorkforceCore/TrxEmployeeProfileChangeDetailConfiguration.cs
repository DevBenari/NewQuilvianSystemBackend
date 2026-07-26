using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class TrxEmployeeProfileChangeDetailConfiguration : IEntityTypeConfiguration<TrxEmployeeProfileChangeDetail>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeProfileChangeDetail> builder)
        {
            builder.ToTable("TrxEmployeeProfileChangeDetail", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.FieldGroup).HasMaxLength(100).IsRequired();
            builder.Property(x => x.FieldName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.OldValue).HasMaxLength(1000);
            builder.Property(x => x.NewValue).HasMaxLength(1000);
            builder.Property(x => x.ValueType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TargetEntityName).HasMaxLength(150);
            builder.Property(x => x.DetailStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.ProfileChangeRequest).WithMany(x => x.Details).HasForeignKey(x => x.ProfileChangeRequestId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.ProfileChangeRequestId, x.SortOrder });
        }
    }
}
