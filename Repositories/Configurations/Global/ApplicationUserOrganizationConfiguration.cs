using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class ApplicationUserOrganizationConfiguration : IEntityTypeConfiguration<ApplicationUserOrganization>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserOrganization> entity)
        {
            entity.ToTable("AspNetUserOrganization", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.HasOne(x => x.User)
                .WithMany(x => x.DepartmentPositions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.UserId);

            // Uniqueness ber-effective-date: satu orang boleh menempati Departemen + Posisi yang
            // sama lebih dari sekali sepanjang periodenya berbeda. Inilah yang memungkinkan
            // riwayat, rehire, dan kembali ke penempatan lama.
            //
            // AreNullsDistinct(false) memakai NULLS NOT DISTINCT (PostgreSQL 15+). Tanpa itu,
            // PostgreSQL menganggap setiap NULL berbeda, sehingga beberapa baris ber-
            // EffectiveStartDate null untuk orang dan penempatan yang sama akan lolos diam-diam.
            entity.HasIndex(x => new
            {
                x.UserId,
                x.DepartmentId,
                x.PositionId,
                x.EffectiveStartDate
            })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.DepartmentId,
                x.PositionId
            });

            // Satu penempatan otoritatif menghasilkan paling banyak satu proyeksi yang belum
            // ditutup. Baris warisan tanpa sumber yang dapat dibuktikan bernilai null dan sengaja
            // dikecualikan dari index ini.
            entity.HasIndex(x => x.SourceAssignmentId)
                .IsUnique()
                .HasFilter("\"SourceAssignmentId\" IS NOT NULL AND \"IsDelete\" = false");

            // Index unik mutlak (UserId, DepartmentId, PositionId) sengaja DIHAPUS pada Phase A0.
            // Ia melarang pengulangan penempatan apa pun, termasuk baris historis yang sudah
            // ditutup, sehingga rehire dan kembali ke penempatan lama menjadi mustahil dan
            // proyeksi satu-baris-per-assignment tidak dapat dibuat. Uniqueness yang benar sudah
            // dijaga dua index di atas.
        }
    }
}
