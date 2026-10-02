using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

/// <summary>
/// Kolom jejak <c>IdentityModel</c> yang sama pada seluruh tabel sliding scale.
/// </summary>
internal static class SlidingScaleAuditColumns
{
    public static void Configure<T>(EntityTypeBuilder<T> builder) where T : IdentityModel
    {
        builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.IsDelete).HasDefaultValue(false);
        builder.Property(x => x.IsCancel).HasDefaultValue(false);
    }
}

/// <summary>
/// <c>BE-RWI-102</c> / migration <c>R6</c> — kamus data 0.5 bagian 13.4.
/// </summary>
public class PhmSlidingScaleTemplateConfiguration : IEntityTypeConfiguration<PhmSlidingScaleTemplate>
{
    public void Configure(EntityTypeBuilder<PhmSlidingScaleTemplate> builder)
    {
        builder.ToTable("PhmSlidingScaleTemplate", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TemplateCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TemplateName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        SlidingScaleAuditColumns.Configure(builder);

        builder.HasIndex(x => x.TemplateCode).IsUnique();
    }
}

/// <summary>
/// <c>BE-RWI-102</c> / migration <c>R6</c> — kamus data 0.5 bagian 13.5.
/// </summary>
public class PhmSlidingScaleTemplateVersionConfiguration : IEntityTypeConfiguration<PhmSlidingScaleTemplateVersion>
{
    public void Configure(EntityTypeBuilder<PhmSlidingScaleTemplateVersion> builder)
    {
        builder.ToTable("PhmSlidingScaleTemplateVersion", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TemplateId).IsRequired();
        builder.Property(x => x.VersionNumber).IsRequired();
        builder.Property(x => x.VersionStatus)
            .HasConversion<int>()
            .HasDefaultValue(SlidingScaleVersionStatus.Draft)
            .IsRequired();
        builder.Property(x => x.GlucoseUnit).HasConversion<int>().IsRequired();
        builder.Property(x => x.DefinitionHash).HasMaxLength(64).IsFixedLength();
        builder.Property(x => x.LastModifiedByUserId).IsRequired();
        builder.Property(x => x.LastModifiedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.RetiredAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.ApprovalNote).HasMaxLength(500);
        SlidingScaleAuditColumns.Configure(builder);

        builder.HasOne(x => x.Template)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LastModifiedByUser).WithMany().HasForeignKey(x => x.LastModifiedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TemplateId, x.VersionNumber }).IsUnique();

        // FR-DOK-094 kriteria 3: tepat satu versi Approved per template, dijaga basis data.
        builder.HasIndex(x => x.TemplateId, "IX_PhmSlidingScaleTemplateVersion_Template_Approved")
            .IsUnique()
            .HasFilter("\"VersionStatus\" = 2 AND \"IsDelete\" = false");

        builder.HasIndex(x => x.LastModifiedByUserId);
        builder.HasIndex(x => x.ApprovedByUserId);
    }
}

/// <summary>
/// <c>BE-RWI-102</c> / migration <c>R6</c> — kamus data 0.5 bagian 13.6.
/// </summary>
public class PhmSlidingScaleRangeConfiguration : IEntityTypeConfiguration<PhmSlidingScaleRange>
{
    public void Configure(EntityTypeBuilder<PhmSlidingScaleRange> builder)
    {
        builder.ToTable("PhmSlidingScaleRange", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_PhmSlidingScaleRange_SingleOwner",
                "num_nonnulls(\"TemplateVersionId\", \"OrderVersionId\") = 1");
            table.HasCheckConstraint(
                "CK_PhmSlidingScaleRange_DoseUnits",
                "\"DoseUnits\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LowerBoundInclusive).HasColumnType("numeric(7,2)");
        builder.Property(x => x.UpperBoundExclusive).HasColumnType("numeric(7,2)");
        builder.Property(x => x.DoseUnits).HasColumnType("numeric(6,2)").IsRequired();
        builder.Property(x => x.InstructionText).HasMaxLength(300);
        builder.Property(x => x.RequiresPhysicianNotification).HasDefaultValue(false);
        builder.Property(x => x.SortOrder).IsRequired();
        SlidingScaleAuditColumns.Configure(builder);

        builder.HasOne(x => x.TemplateVersion)
            .WithMany(x => x.Ranges)
            .HasForeignKey(x => x.TemplateVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OrderVersion)
            .WithMany(x => x.Ranges)
            .HasForeignKey(x => x.OrderVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TemplateVersionId, x.SortOrder });
        builder.HasIndex(x => new { x.OrderVersionId, x.SortOrder });
    }
}

/// <summary>
/// <c>BE-RWI-103</c> / migration <c>R6</c> — kamus data 0.5 bagian 13.7.
/// </summary>
public class PhmSlidingScaleOrderConfiguration : IEntityTypeConfiguration<PhmSlidingScaleOrder>
{
    public void Configure(EntityTypeBuilder<PhmSlidingScaleOrder> builder)
    {
        builder.ToTable("PhmSlidingScaleOrder", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PrescriptionId).IsRequired();
        builder.Property(x => x.PrescriptionItemId).IsRequired();
        builder.Property(x => x.EncounterId).IsRequired();
        builder.Property(x => x.InpEpisodeId).IsRequired();
        builder.Property(x => x.PatientId).IsRequired();
        builder.Property(x => x.TemplateId).IsRequired();
        builder.Property(x => x.OrderStatus)
            .HasConversion<int>()
            .HasDefaultValue(SlidingScaleOrderStatus.Active)
            .IsRequired();
        builder.Property(x => x.CurrentVersionNumber).HasDefaultValue(1).IsRequired();
        builder.Property(x => x.CheckFrequencyCode).HasMaxLength(30);
        builder.Property(x => x.StoppedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.StopReason).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        SlidingScaleAuditColumns.Configure(builder);

        builder.HasOne(x => x.Prescription).WithMany().HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PrescriptionItem).WithMany().HasForeignKey(x => x.PrescriptionItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Encounter).WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.InpEpisode).WithMany().HasForeignKey(x => x.InpEpisodeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StoppedByUser).WithMany().HasForeignKey(x => x.StoppedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrderNumber).IsUnique();

        // Satu butir insulin, satu order yang berlaku.
        builder.HasIndex(x => x.PrescriptionItemId)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        builder.HasIndex(x => x.PrescriptionId);
        builder.HasIndex(x => x.EncounterId);
        builder.HasIndex(x => x.InpEpisodeId);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.TemplateId);
        builder.HasIndex(x => x.OrderStatus);
        builder.HasIndex(x => x.StoppedByUserId);
    }
}

/// <summary>
/// <c>BE-RWI-103</c> / migration <c>R6</c> — kamus data 0.5 bagian 13.8.
/// </summary>
public class PhmSlidingScaleOrderVersionConfiguration : IEntityTypeConfiguration<PhmSlidingScaleOrderVersion>
{
    public void Configure(EntityTypeBuilder<PhmSlidingScaleOrderVersion> builder)
    {
        builder.ToTable("PhmSlidingScaleOrderVersion", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.VersionNumber).IsRequired();
        builder.Property(x => x.TemplateVersionId).IsRequired();
        builder.Property(x => x.IsAdjusted).HasDefaultValue(false);
        builder.Property(x => x.AdjustmentReason).HasMaxLength(500);
        builder.Property(x => x.OrderedByDoctorId).IsRequired();
        builder.Property(x => x.OrderedByUserId).IsRequired();
        builder.Property(x => x.OrderedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        SlidingScaleAuditColumns.Configure(builder);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TemplateVersion).WithMany().HasForeignKey(x => x.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OrderedByDoctor).WithMany().HasForeignKey(x => x.OrderedByDoctorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OrderedByUser).WithMany().HasForeignKey(x => x.OrderedByUserId).OnDelete(DeleteBehavior.Restrict);

        // Penjaga benturan lapis basis data: dua penyesuaian bersamaan tidak dapat sama-sama menjadi versi N+1.
        builder.HasIndex(x => new { x.OrderId, x.VersionNumber }).IsUnique();
        builder.HasIndex(x => x.TemplateVersionId);
        builder.HasIndex(x => x.OrderedByDoctorId);
        builder.HasIndex(x => x.OrderedByUserId);
    }
}
