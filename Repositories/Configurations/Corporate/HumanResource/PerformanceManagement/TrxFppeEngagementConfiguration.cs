using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    /// <summary>
    /// Konfigurasi tambahan entity legacy <c>TrxFppeEngagement</c> (pemetaan tabel tetap dari atribut model).
    /// Ditambahkan 10 November 2026 bersama peralihan nomor penugasan ke provider bersama: kode bisnis yang
    /// unik wajib dijaga index unik di database (<c>QBE-CODE-004</c>).
    /// </summary>
    public class TrxFppeEngagementConfiguration : IEntityTypeConfiguration<TrxFppeEngagement>
    {
        public void Configure(EntityTypeBuilder<TrxFppeEngagement> entity)
        {
            entity.HasIndex(x => x.EngagementNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
