using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Copy resep: surat keterangan berapa obat yang sudah diserahkan dan berapa sisanya.
/// </summary>
/// <remarks>
/// <para>
/// Dokumen ini dibawa pasien ke apotek lain, dan apotek itu bertindak atas angka yang tertera
/// padanya. Karena itu angkanya berasal dari histori penyerahan yang benar-benar mengurangi
/// stok — bukan dari status resep, yang hanya menyatakan tahap alur.
/// </para>
/// <para>
/// <b>Tidak mengubah apa pun.</b> Menerbitkan copy resep tidak menyentuh stok, tidak menyentuh
/// catatan penyerahan, dan tidak mengubah status resep. Ia menyalin keadaan ke atas kertas.
/// </para>
/// <para>
/// Identitas fasilitas dan apoteker dibaca dari master — <c>MstHospitalSite</c> dan
/// <c>WfpCredentialLicense</c> — dan <b>tidak pernah diminta sebagai isian petugas</b>. Nomor
/// izin praktik adalah fakta pada master kepegawaian; mengetiknya ulang saat mencetak dokumen
/// membuka jalan bagi nomor yang tidak pernah diverifikasi siapa pun untuk muncul di dokumen
/// yang berlaku sebagai keterangan resmi.
/// </para>
/// <para>
/// Bila kredensialnya belum terisi, kekosongan itu dilaporkan apa adanya lewat
/// <c>MissingDocumentData</c>. Tidak ada nilai yang dikarang.
/// </para>
/// </remarks>
public sealed class PrescriptionCopyService
{
    private const string LogCategory = "PharmacyManagement";

    /// <summary>Status pemakaian yang berarti obatnya benar-benar sudah diserahkan.</summary>
    private static readonly DrugUsageStatus[] DispensedStatuses =
    [
        DrugUsageStatus.NotBilled,
        DrugUsageStatus.Billed
    ];

    /// <summary>
    /// Jenis izin yang diterima sebagai izin praktik apoteker.
    /// </summary>
    /// <remarks>
    /// Dicocokkan longgar karena penamaan pada master kredensial berbeda-beda antar instalasi.
    /// Yang tidak cocok tidak dipaksakan: lebih baik kolomnya kosong daripada mencantumkan
    /// nomor izin yang bukan izin praktik.
    /// </remarks>
    private static readonly string[] PharmacistLicenseHints = ["SIPA", "APOTEKER"];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public PrescriptionCopyService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    // =================================================================== pratinjau

    /// <summary>
    /// Bahan copy resep sebagaimana keadaannya sekarang, tanpa menerbitkan apa pun.
    /// </summary>
    public async Task<PrescriptionCopyPreviewResponse?> GetPreviewAsync(Guid prescriptionId,
        Guid? pharmacistWorkforceId = null, CancellationToken cancellationToken = default)
    {
        var header = await LoadHeaderAsync(prescriptionId, cancellationToken);
        if (header == null) return null;

        var lines = await BuildLinesAsync(prescriptionId, cancellationToken);
        var issuer = await BuildIssuerAsync(pharmacistWorkforceId, cancellationToken);

        return new PrescriptionCopyPreviewResponse
        {
            PrescriptionId = header.Id,
            PrescriptionNumber = header.PrescriptionNumber,
            PrescriptionDateTime = header.PrescriptionDateTime,
            PatientName = header.PatientName,
            MedicalRecordNumber = header.MedicalRecordNumber,
            DoctorName = header.DoctorName,
            Issuer = issuer,
            IsFullyDispensed = lines.Count > 0 && lines.All(x => x.QuantityRemaining <= 0m),
            Items = lines,
            MissingDocumentData = DescribeMissing(issuer)
        };
    }

    // ================================================================== penerbitan

    /// <summary>
    /// Menerbitkan satu lembar copy resep dan membekukan angkanya.
    /// </summary>
    /// <remarks>
    /// Angka dibekukan karena lembarnya berpindah tangan. Bila dihitung ulang setiap kali
    /// dibuka, lembar yang sudah beredar dan tampilan di sistem dapat menyatakan dua hal yang
    /// berbeda tanpa ada yang tahu mana yang dipegang pasien.
    /// </remarks>
    public async Task<PrescriptionCopyDetailResponse> IssueAsync(Guid prescriptionId,
        IssuePrescriptionCopyRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var copyId = DeterministicId(request.IdempotencyKey);
        var existing = await _dbContext.PhmPrescriptionCopies.AsNoTracking()
            .AnyAsync(x => x.Id == copyId && !x.IsDelete, cancellationToken);
        if (existing) return (await GetDetailAsync(copyId, cancellationToken))!;

        var header = await LoadHeaderAsync(prescriptionId, cancellationToken)
            ?? throw new KeyNotFoundException("Resep tidak ditemukan.");

        var lines = await BuildLinesAsync(prescriptionId, cancellationToken);
        if (lines.Count == 0)
            throw new PrescriptionCopyUnprocessableException("PHM120",
                "Resep ini tidak memiliki satu pun baris obat, sehingga copy resep tidak dapat diterbitkan.");

        var issuer = await BuildIssuerAsync(request.PharmacistWorkforceId, cancellationToken);
        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var copy = new PhmPrescriptionCopy
        {
            Id = copyId,
            CopyNumber = $"CR-{now:yyyyMMdd}-{copyId.ToString("N")[..6].ToUpperInvariant()}",
            PrescriptionId = prescriptionId,
            Status = PrescriptionCopyStatus.Issued,
            IssuedAt = now,
            IssuedByUserId = actorUserId,
            PharmacistWorkforceId = issuer.PharmacistWorkforceId,
            HospitalSiteId = issuer.HospitalSiteId,
            SiteNameSnapshot = issuer.SiteName,
            SiteAddressSnapshot = issuer.SiteAddress,
            SitePhoneSnapshot = issuer.SitePhoneNumber,
            PharmacistNameSnapshot = issuer.PharmacistName,
            PharmacistLicenseNumberSnapshot = issuer.PharmacistLicenseNumber,
            PharmacistLicenseTypeSnapshot = issuer.PharmacistLicenseType,
            Notes = Normalize(request.Notes),
            ItemCount = lines.Count,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        _dbContext.PhmPrescriptionCopies.Add(copy);

        foreach (var line in lines)
        {
            _dbContext.PhmPrescriptionCopyItems.Add(new PhmPrescriptionCopyItem
            {
                PrescriptionCopyId = copy.Id,
                PrescriptionItemId = line.PrescriptionItemId,
                LineNumber = line.LineNumber,
                DrugNameSnapshot = line.DrugName,
                GenericNameSnapshot = line.GenericName,
                StrengthSnapshot = line.Strength,
                DrugFormSnapshot = line.DrugForm,
                DispenseUnitSnapshot = line.DispenseUnit,
                SignaSnapshot = line.Signa,
                AdministrationInstructionSnapshot = line.AdministrationInstruction,
                IsNarcoticSnapshot = line.IsNarcotic,
                IsPsychotropicSnapshot = line.IsPsychotropic,
                QuantityPrescribed = line.QuantityPrescribed,
                QuantityDispensed = line.QuantityDispensed,
                QuantityRemaining = line.QuantityRemaining,
                Mark = line.Mark,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "PrescriptionCopy.Issue",
            "Menerbitkan copy resep.",
            new
            {
                PrescriptionId = prescriptionId, header.PrescriptionNumber,
                PrescriptionCopyId = copy.Id, copy.CopyNumber, ActorUserId = actorUserId,
                copy.ItemCount,
                PharmacistLicenseRecorded = !string.IsNullOrWhiteSpace(issuer.PharmacistLicenseNumber)
            });

        return (await GetDetailAsync(copy.Id, cancellationToken))!;
    }

    // ==================================================================== pencabutan

    /// <summary>
    /// Mencabut sebuah lembar yang sudah terbit.
    /// </summary>
    /// <remarks>
    /// Lembarnya tidak dihapus. Ia mungkin sudah berpindah tangan, dan riwayat harus tetap
    /// menjelaskan bahwa dokumen itu pernah ada beserta alasan pencabutannya.
    /// </remarks>
    public async Task<PrescriptionCopyDetailResponse> RevokeAsync(Guid prescriptionCopyId,
        RevokePrescriptionCopyRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new PrescriptionCopyUnprocessableException("PHM121",
                "Alasan pencabutan wajib diisi.");

        var copy = await _dbContext.PhmPrescriptionCopies
            .FirstOrDefaultAsync(x => x.Id == prescriptionCopyId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Copy resep tidak ditemukan.");

        if (copy.Status == PrescriptionCopyStatus.Revoked)
            return (await GetDetailAsync(prescriptionCopyId, cancellationToken))!;

        EnsureVersion(copy.Version, request.ExpectedVersion);

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        copy.Status = PrescriptionCopyStatus.Revoked;
        copy.RevokedAt = now;
        copy.RevokedByUserId = actorUserId;
        copy.RevokeReason = request.Reason.Trim();
        copy.Version++;
        copy.UpdateDateTime = now;
        copy.UpdateBy = actorUserId;

        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "PrescriptionCopy.Revoke",
            "Mencabut copy resep.",
            new
            {
                PrescriptionCopyId = copy.Id, copy.CopyNumber, ActorUserId = actorUserId,
                Reason = copy.RevokeReason
            });

        return (await GetDetailAsync(prescriptionCopyId, cancellationToken))!;
    }

    // ======================================================================= baca

    public async Task<PrescriptionCopyDetailResponse?> GetDetailAsync(Guid prescriptionCopyId,
        CancellationToken cancellationToken = default)
    {
        var copy = await _dbContext.PhmPrescriptionCopies.AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == prescriptionCopyId && !x.IsDelete, cancellationToken);
        if (copy == null) return null;

        var header = await LoadHeaderAsync(copy.PrescriptionId, cancellationToken);

        return new PrescriptionCopyDetailResponse
        {
            Id = copy.Id,
            CopyNumber = copy.CopyNumber,
            Status = copy.Status,
            PrescriptionId = copy.PrescriptionId,
            PrescriptionNumber = header?.PrescriptionNumber ?? string.Empty,
            PrescriptionDateTime = header?.PrescriptionDateTime ?? copy.IssuedAt,
            PatientName = header?.PatientName ?? string.Empty,
            MedicalRecordNumber = header?.MedicalRecordNumber ?? string.Empty,
            DoctorName = header?.DoctorName ?? string.Empty,
            IssuedAt = copy.IssuedAt,
            Issuer = new PrescriptionCopyIssuerResponse
            {
                HospitalSiteId = copy.HospitalSiteId,
                SiteName = copy.SiteNameSnapshot,
                SiteAddress = copy.SiteAddressSnapshot,
                SitePhoneNumber = copy.SitePhoneSnapshot,
                PharmacistWorkforceId = copy.PharmacistWorkforceId,
                PharmacistName = copy.PharmacistNameSnapshot,
                PharmacistLicenseNumber = copy.PharmacistLicenseNumberSnapshot,
                PharmacistLicenseType = copy.PharmacistLicenseTypeSnapshot
            },
            Notes = copy.Notes,
            RevokedAt = copy.RevokedAt,
            RevokeReason = copy.RevokeReason,
            Version = copy.Version,
            Items = [.. copy.Items.Where(x => !x.IsDelete).OrderBy(x => x.LineNumber)
                .Select(x => new PrescriptionCopyLineResponse
                {
                    PrescriptionItemId = x.PrescriptionItemId,
                    LineNumber = x.LineNumber,
                    DrugName = x.DrugNameSnapshot,
                    GenericName = x.GenericNameSnapshot,
                    Strength = x.StrengthSnapshot,
                    DrugForm = x.DrugFormSnapshot,
                    DispenseUnit = x.DispenseUnitSnapshot,
                    Signa = x.SignaSnapshot,
                    AdministrationInstruction = x.AdministrationInstructionSnapshot,
                    IsNarcotic = x.IsNarcoticSnapshot,
                    IsPsychotropic = x.IsPsychotropicSnapshot,
                    QuantityPrescribed = x.QuantityPrescribed,
                    QuantityDispensed = x.QuantityDispensed,
                    QuantityRemaining = x.QuantityRemaining,
                    Mark = x.Mark
                })]
        };
    }

    /// <summary>Lembar apa saja yang pernah terbit untuk sebuah resep, terbaru lebih dahulu.</summary>
    public Task<List<PrescriptionCopySummaryResponse>> GetByPrescriptionAsync(Guid prescriptionId,
        CancellationToken cancellationToken = default) =>
        _dbContext.PhmPrescriptionCopies.AsNoTracking()
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
            .OrderByDescending(x => x.IssuedAt)
            .Select(x => new PrescriptionCopySummaryResponse
            {
                Id = x.Id,
                CopyNumber = x.CopyNumber,
                Status = x.Status,
                IssuedAt = x.IssuedAt,
                PharmacistName = x.PharmacistNameSnapshot,
                ItemCount = x.ItemCount
            })
            .ToListAsync(cancellationToken);

    // =================================================================== internal

    /// <summary>
    /// Menghitung tiap baris resep dari histori penyerahan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang dihitung sebagai sudah diserahkan hanyalah pemakaian berstatus
    /// <see cref="DrugUsageStatus.NotBilled"/> atau <see cref="DrugUsageStatus.Billed"/>.
    /// Draft adalah stok yang masih ditahan dan belum berpindah; Cancelled adalah penyiapan
    /// yang batal. Keduanya tidak pernah sampai ke tangan pasien, sehingga menghitungnya akan
    /// menyatakan obat sudah diserahkan padahal belum.
    /// </para>
    /// <para>
    /// Retur tidak mengembalikan sisa. Retur pada sistem ini menunjuk pemakaian pada tingkat
    /// dokumen, bukan tingkat baris resep, sehingga jumlahnya tidak dapat dikaitkan kembali ke
    /// baris tertentu. Menebak kaitannya akan menghasilkan sisa yang salah pada dokumen yang
    /// dibawa pasien. Lihat `PHA-OQ-025`.
    /// </para>
    /// </remarks>
    private async Task<List<PrescriptionCopyLineResponse>> BuildLinesAsync(Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.TrxPrescriptionItems.AsNoTracking()
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Id,
                x.SortOrder,
                x.DrugNameSnapshot,
                x.GenericNameSnapshot,
                x.StrengthSnapshot,
                x.DrugFormSnapshot,
                DispenseUnit = x.DispenseUnitSymbolSnapshot ?? x.DispenseUnitNameSnapshot,
                x.Signa,
                x.AdministrationInstruction,
                x.IsNarcoticSnapshot,
                x.IsPsychotropicSnapshot,
                x.Quantity
            })
            .ToListAsync(cancellationToken);

        if (items.Count == 0) return [];

        var dispensed = await _dbContext.PhmDrugUsageItems.AsNoTracking()
            .Where(x => x.PrescriptionItemId != null && !x.IsDelete &&
                x.DrugUsage!.PrescriptionId == prescriptionId && !x.DrugUsage.IsDelete &&
                DispensedStatuses.Contains(x.DrugUsage.Status))
            .GroupBy(x => x.PrescriptionItemId!.Value)
            .Select(g => new { PrescriptionItemId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.PrescriptionItemId, x => x.Quantity, cancellationToken);

        return [.. items.Select(item =>
        {
            var served = dispensed.TryGetValue(item.Id, out var value) ? value : 0m;

            // Tidak pernah negatif. Sisa negatif tidak punya arti bagi pasien maupun bagi
            // apotek yang membaca lembarnya.
            var remaining = Math.Max(0m, item.Quantity - served);

            return new PrescriptionCopyLineResponse
            {
                PrescriptionItemId = item.Id,
                LineNumber = item.SortOrder,
                DrugName = item.DrugNameSnapshot ?? string.Empty,
                GenericName = item.GenericNameSnapshot,
                Strength = item.StrengthSnapshot,
                DrugForm = item.DrugFormSnapshot,
                DispenseUnit = item.DispenseUnit,
                Signa = item.Signa,
                AdministrationInstruction = item.AdministrationInstruction,
                IsNarcotic = item.IsNarcoticSnapshot,
                IsPsychotropic = item.IsPsychotropicSnapshot,
                QuantityPrescribed = item.Quantity,
                QuantityDispensed = served,
                QuantityRemaining = remaining,
                Mark = remaining <= 0m ? PrescriptionCopyMark.Det : PrescriptionCopyMark.Nedet
            };
        })];
    }

    /// <summary>
    /// Menyusun identitas penerbit dari master, tanpa satu pun nilai yang dikarang.
    /// </summary>
    private async Task<PrescriptionCopyIssuerResponse> BuildIssuerAsync(
        Guid? pharmacistWorkforceId, CancellationToken cancellationToken)
    {
        // Fasilitas penerbit: lokasi utama bertipe rumah sakit. Bila penandanya belum
        // ditetapkan, dipakai lokasi pertama menurut nama supaya hasilnya tetap sama pada
        // setiap penerbitan — bukan berubah-ubah mengikuti urutan baris di database.
        var site = await _dbContext.Set<MstHospitalSite>().AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .OrderByDescending(x => x.IsMainSite)
            .ThenByDescending(x => x.SiteType == "Hospital")
            .ThenBy(x => x.SiteName)
            .Select(x => new { x.Id, x.SiteName, x.Address, x.PhoneNumber })
            .FirstOrDefaultAsync(cancellationToken);

        var issuer = new PrescriptionCopyIssuerResponse
        {
            HospitalSiteId = site?.Id,
            SiteName = site?.SiteName,
            SiteAddress = site?.Address,
            SitePhoneNumber = site?.PhoneNumber
        };

        if (!pharmacistWorkforceId.HasValue) return issuer;

        var workforce = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .Where(x => x.Id == pharmacistWorkforceId.Value && !x.IsDelete)
            .Select(x => new { x.Id, x.DisplayName })
            .FirstOrDefaultAsync(cancellationToken);
        if (workforce == null) return issuer;

        issuer.PharmacistWorkforceId = workforce.Id;
        issuer.PharmacistName = workforce.DisplayName;

        var now = DateTime.UtcNow;

        // Izin praktik yang masih berlaku, sudah terverifikasi, dan tidak dicabut. Yang utama
        // didahulukan bila seseorang memegang lebih dari satu.
        var licenses = await _dbContext.Set<WfpCredentialLicense>().AsNoTracking()
            .Where(x => x.WorkforceProfileId == workforce.Id && !x.IsDelete && x.IsActive &&
                !x.IsRevoked && x.ExpiredDate >= now)
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.ExpiredDate)
            .Select(x => new { x.LicenseType, x.LicenseNumber })
            .ToListAsync(cancellationToken);

        var license = licenses.FirstOrDefault(x => PharmacistLicenseHints.Any(hint =>
            (x.LicenseType ?? string.Empty).Contains(hint, StringComparison.OrdinalIgnoreCase)));

        if (license != null)
        {
            issuer.PharmacistLicenseNumber = license.LicenseNumber;
            issuer.PharmacistLicenseType = license.LicenseType;
        }

        return issuer;
    }

    /// <summary>
    /// Menyebut apa saja yang belum dapat dicetak karena datanya memang belum ada.
    /// </summary>
    /// <remarks>
    /// Disampaikan lebih dahulu supaya petugas tahu lembar yang akan tercetak belum lengkap,
    /// alih-alih menemukan kekosongan itu setelah dokumennya berpindah tangan.
    /// </remarks>
    private static List<string> DescribeMissing(PrescriptionCopyIssuerResponse issuer)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(issuer.SiteName))
            missing.Add("Identitas fasilitas belum ada pada master lokasi rumah sakit.");

        if (!issuer.PharmacistWorkforceId.HasValue)
            missing.Add("Apoteker penanggung jawab belum ditentukan.");
        else if (string.IsNullOrWhiteSpace(issuer.PharmacistLicenseNumber))
            missing.Add("Nomor izin praktik apoteker belum terisi pada master kredensial " +
                "kepegawaian. Lengkapi di sana; nomor izin tidak dapat diketik saat mencetak.");

        return missing;
    }

    private Task<PrescriptionHeaderView?> LoadHeaderAsync(Guid prescriptionId,
        CancellationToken cancellationToken) =>
        _dbContext.TrxPrescriptions.AsNoTracking()
            .Where(x => x.Id == prescriptionId && !x.IsDelete)
            .Select(x => new PrescriptionHeaderView
            {
                Id = x.Id,
                PrescriptionNumber = x.PrescriptionNumber,
                PrescriptionDateTime = x.PrescriptionDateTime,
                PatientName = x.Patient!.FullName,
                MedicalRecordNumber = x.Patient!.MedicalRecordNumber,
                DoctorName = x.Doctor!.FullName
            })
            .FirstOrDefaultAsync(cancellationToken);

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new PrescriptionCopyConflictException("PHM122",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
    }

    private static void EnsureIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key wajib diisi.");
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new PrescriptionCopyConflictException("PHM122",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new PrescriptionCopyForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"PrescriptionCopy:{key.Trim()}"))[..16]);

    private sealed class PrescriptionHeaderView
    {
        public Guid Id { get; init; }
        public string PrescriptionNumber { get; init; } = string.Empty;
        public DateTime PrescriptionDateTime { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string MedicalRecordNumber { get; init; } = string.Empty;
        public string DoctorName { get; init; } = string.Empty;
    }
}

public sealed class PrescriptionCopyConflictException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}

public sealed class PrescriptionCopyForbiddenException(string message) : Exception(message);

public sealed class PrescriptionCopyUnprocessableException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}
