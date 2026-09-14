using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Administrator.MasterData
{
    public class MstCompanyGuarantorReimbursementRouteConfiguration : IEntityTypeConfiguration<MstCompanyGuarantorReimbursementRoute>
    {
        public void Configure(EntityTypeBuilder<MstCompanyGuarantorReimbursementRoute> entity)
        {
            entity.ToTable("MstCompanyGuarantorReimbursementRoute", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CompanyGuarantorId)
                .IsRequired();

            entity.Property(x => x.RouteType)
                .HasMaxLength(30)
                .HasDefaultValue("SELF")
                .IsRequired();

            entity.Property(x => x.InsuranceProviderId)
                .IsRequired(false);

            entity.Property(x => x.Priority)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            // IdentityModel audit fields
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);
            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            // Relationships
            entity.HasOne(x => x.CompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.CompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InsuranceProvider)
                .WithMany()
                .HasForeignKey(x => x.InsuranceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(x => x.CompanyGuarantorId)
                .HasDatabaseName("IX_MstCompanyGuarantorReimbursementRoute_CompanyGuarantorId");
            entity.HasIndex(x => x.RouteType)
                .HasDatabaseName("IX_MstCompanyGuarantorReimbursementRoute_RouteType");
            entity.HasIndex(x => x.InsuranceProviderId)
                .HasDatabaseName("IX_MstCompanyGuarantorReimbursementRoute_InsuranceProviderId");

            // Filtered unique index: maksimal satu rute bawaan aktif per perusahaan
            entity.HasIndex(x => x.CompanyGuarantorId)
                .IsUnique()
                .HasFilter("\"IsDefault\" = true AND \"IsActive\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_MstCompanyGuarantorReimbursementRoute_Default");
        }
    }
}
