using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdClinicalPreceptorshipPreceptorConfiguration : IEntityTypeConfiguration<HrdClinicalPreceptorshipPreceptor>
    {
        public void Configure(EntityTypeBuilder<HrdClinicalPreceptorshipPreceptor> builder)
        {
            builder.ToTable("HrdClinicalPreceptorshipPreceptor", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.AssignedFrom).HasColumnType("date").IsRequired();
            builder.Property(x => x.AssignedUntil).HasColumnType("date");
            builder.Property(x => x.ChangeReason).HasMaxLength(500);

            builder.HasOne(x => x.Preceptorship).WithMany(p => p.Preceptors).HasForeignKey(x => x.PreceptorshipId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PreceptorWorkforceProfile).WithMany().HasForeignKey(x => x.PreceptorWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PreceptorshipId)
                .HasDatabaseName("IX_HrdClinicalPreceptorshipPreceptor_Open")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"AssignedUntil\" IS NULL");
        }
    }
}
