using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// ------------------------------------------------------------------- permintaan

public class IssuePrescriptionCopyRequest
{
    /// <summary>
    /// Apoteker penanggung jawab, bila ada.
    /// </summary>
    /// <remarks>
    /// Yang diminta hanya identitas orangnya. Nama dan nomor izin praktiknya dibaca dari
    /// master kredensial — <b>tidak pernah diketik</b> di sini.
    /// </remarks>
    public Guid? PharmacistWorkforceId { get; set; }

    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class RevokePrescriptionCopyRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

// ---------------------------------------------------------------------- jawaban

/// <summary>
/// Bahan copy resep sebagaimana keadaannya sekarang, tanpa menerbitkan apa pun.
/// </summary>
/// <remarks>
/// Dipakai untuk melihat pratinjau sebelum memutuskan mencetak. Angkanya dihitung langsung
/// dari histori penyerahan, sehingga dapat berubah bila penyerahan berlanjut.
/// </remarks>
public class PrescriptionCopyPreviewResponse
{
    public Guid PrescriptionId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public DateTime PrescriptionDateTime { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;

    public PrescriptionCopyIssuerResponse Issuer { get; set; } = new();

    /// <summary>True bila seluruh barisnya sudah `det`.</summary>
    public bool IsFullyDispensed { get; set; }

    public List<PrescriptionCopyLineResponse> Items { get; set; } = [];

    /// <summary>
    /// Hal-hal yang tidak dapat dicetak karena datanya memang belum ada.
    /// </summary>
    /// <remarks>
    /// Ditampilkan supaya petugas tahu lembar yang akan tercetak belum lengkap, alih-alih
    /// menemukan kekosongan itu setelah dokumen berpindah tangan.
    /// </remarks>
    public List<string> MissingDocumentData { get; set; } = [];
}

public class PrescriptionCopyIssuerResponse
{
    public Guid? HospitalSiteId { get; set; }
    public string? SiteName { get; set; }
    public string? SiteAddress { get; set; }
    public string? SitePhoneNumber { get; set; }

    public Guid? PharmacistWorkforceId { get; set; }
    public string? PharmacistName { get; set; }

    /// <summary>Kosong bila master kredensial belum memuatnya.</summary>
    public string? PharmacistLicenseNumber { get; set; }

    public string? PharmacistLicenseType { get; set; }
}

public class PrescriptionCopyLineResponse
{
    public Guid PrescriptionItemId { get; set; }
    public int LineNumber { get; set; }

    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string? DrugForm { get; set; }
    public string? DispenseUnit { get; set; }
    public string? Signa { get; set; }
    public string? AdministrationInstruction { get; set; }

    public bool IsNarcotic { get; set; }
    public bool IsPsychotropic { get; set; }

    public decimal QuantityPrescribed { get; set; }
    public decimal QuantityDispensed { get; set; }
    public decimal QuantityRemaining { get; set; }

    public PrescriptionCopyMark Mark { get; set; }
}

/// <summary>Satu lembar copy resep yang sudah diterbitkan.</summary>
public class PrescriptionCopyDetailResponse
{
    public Guid Id { get; set; }
    public string CopyNumber { get; set; } = string.Empty;
    public PrescriptionCopyStatus Status { get; set; }

    public Guid PrescriptionId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public DateTime PrescriptionDateTime { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; }
    public PrescriptionCopyIssuerResponse Issuer { get; set; } = new();

    public string? Notes { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? RevokeReason { get; set; }

    public int Version { get; set; }

    /// <summary>Angka yang tercetak pada lembar ini, dibekukan saat terbit.</summary>
    public List<PrescriptionCopyLineResponse> Items { get; set; } = [];
}

public class PrescriptionCopySummaryResponse
{
    public Guid Id { get; set; }
    public string CopyNumber { get; set; } = string.Empty;
    public PrescriptionCopyStatus Status { get; set; }
    public DateTime IssuedAt { get; set; }
    public string? PharmacistName { get; set; }
    public int ItemCount { get; set; }
}
