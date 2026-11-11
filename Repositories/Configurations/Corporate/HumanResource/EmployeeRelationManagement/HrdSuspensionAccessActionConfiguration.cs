using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class HrdSuspensionAccessActionConfiguration : IEntityTypeConfiguration<HrdSuspensionAccessAction>
    {
        public void Configure(EntityTypeBuilder<HrdSuspensionAccessAction> entity)
        {
            entity.ToTable("HrdSuspensionAccessAction", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.ActionAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.LockoutUntil).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            entity.HasOne(x => x.Suspension)
                .WithMany()
                .HasForeignKey(x => x.SuspensionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.SuspensionId, x.UserId, x.ActionType })
                .HasDatabaseName("IX_HrdSuspensionAccessAction_Suspension_User_Action");
            entity.HasIndex(x => x.UserId);
        }
    }
}
