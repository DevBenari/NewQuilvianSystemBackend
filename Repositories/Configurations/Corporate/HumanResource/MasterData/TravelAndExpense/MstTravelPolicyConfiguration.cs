using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstTravelPolicyConfiguration : IEntityTypeConfiguration<MstTravelPolicy>
    {
        public void Configure(EntityTypeBuilder<MstTravelPolicy> entity)
        {
            entity.ToTable("MstTravelPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TravelTypeId).IsRequired();
            entity.Property(x => x.TravelPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TravelPolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.MinimumServiceMonths).HasDefaultValue(0);
            entity.Property(x => x.MinimumAdvanceRequestDays).HasDefaultValue(0);
            entity.Property(x => x.AllowWeekendTravel).HasDefaultValue(true);
            entity.Property(x => x.AllowHolidayTravel).HasDefaultValue(true);
            entity.Property(x => x.AllowCompanion).HasDefaultValue(false);
            entity.Property(x => x.AllowCashAdvance).HasDefaultValue(true);
            entity.Property(x => x.MaximumAdvancePercentage).HasPrecision(5, 2).HasDefaultValue(80m);
            entity.Property(x => x.RequireBudgetAvailability).HasDefaultValue(true);
            entity.Property(x => x.RequireItinerary).HasDefaultValue(true);
            entity.Property(x => x.RequireTravelOrder).HasDefaultValue(true);
            entity.Property(x => x.RequireManagerApproval).HasDefaultValue(true);
            entity.Property(x => x.RequireHrVerification).HasDefaultValue(true);
            entity.Property(x => x.RequireFinanceVerification).HasDefaultValue(true);
            entity.Property(x => x.RequireSettlement).HasDefaultValue(true);
            entity.Property(x => x.SettlementDueDays).HasDefaultValue(7);
            entity.Property(x => x.ReceiptRequiredAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovalWorkflowCode).HasMaxLength(100);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.TravelType)
                .WithMany(x => x.TravelPolicies)
                .HasForeignKey(x => x.TravelTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeCategory)
                .WithMany()
                .HasForeignKey(x => x.EmployeeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmploymentType)
                .WithMany()
                .HasForeignKey(x => x.EmploymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TravelPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.TravelPolicyName);
            entity.HasIndex(x => x.TravelTypeId);
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.TravelTypeId, x.IsDefault, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);
        }
    }
}
