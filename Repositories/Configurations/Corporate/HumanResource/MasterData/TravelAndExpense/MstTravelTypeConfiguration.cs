using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstTravelTypeConfiguration : IEntityTypeConfiguration<MstTravelType>
    {
        public void Configure(EntityTypeBuilder<MstTravelType> entity)
        {
            entity.ToTable("MstTravelType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TravelTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TravelTypeName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.TravelScope).HasMaxLength(50).HasDefaultValue("Domestic").IsRequired();
            entity.Property(x => x.RequiresInvitationLetter).HasDefaultValue(false);
            entity.Property(x => x.RequiresTravelOrder).HasDefaultValue(true);
            entity.Property(x => x.RequiresPassport).HasDefaultValue(false);
            entity.Property(x => x.RequiresVisa).HasDefaultValue(false);
            entity.Property(x => x.AllowCashAdvance).HasDefaultValue(true);
            entity.Property(x => x.AllowPersonalVehicle).HasDefaultValue(false);
            entity.Property(x => x.RequireExpenseSettlement).HasDefaultValue(true);
            entity.Property(x => x.DefaultSettlementDueDays).HasDefaultValue(7);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasIndex(x => x.TravelTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.TravelTypeName);
            entity.HasIndex(x => new { x.TravelScope, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.SortOrder, x.IsActive, x.IsDelete });
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
