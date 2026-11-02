using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Penyimpanan dan pembacaan General Consent Rawat Jalan per kunjungan
    /// (<c>RJ-REG-REV-BE-007</c>, <c>RJ-REG-DEC-012</c>).
    /// </summary>
    /// <remarks>
    /// Dipanggil sesudah kunjungan terbentuk, baik lewat pendaftaran poliklinik maupun registrasi
    /// rujukan Laboratorium. Consent yang sudah tersimpan tidak ditimpa: penyimpanan ulang —
    /// misalnya tombol <i>Coba lagi</i> setelah respons pertama hilang di jaringan — mengembalikan
    /// isian yang sudah ada, sehingga tidak pernah ada dua consent pada satu kunjungan.
    /// </remarks>
    public class OutpatientGeneralConsentService
    {
        /// <summary>Jumlah butir klausul General Consent RS MMC; semuanya wajib disetujui.</summary>
        public const int ClauseCount = 12;

        public const string NotFoundMessage = "General Consent kunjungan ini belum tersimpan.";

        private const string EncounterNotFoundMessage = "Kunjungan tidak ditemukan.";
        private const string SignaturePrefix = "data:image/png;base64,";

        /// <summary>Batas panjang data URL tanda tangan (± 1,5 MB gambar PNG).</summary>
        private const int SignatureMaxLength = 2_000_000;

        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        private readonly ApplicationDbContext _dbContext;

        public OutpatientGeneralConsentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Consent yang hidup pada kunjungan; <c>null</c> bila belum ada.</summary>
        public async Task<OutpatientGeneralConsentResponse?> GetAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var consent = await _dbContext.RegPatientGeneralConsents
                .AsNoTracking()
                .Include(x => x.PatientEncounter)
                .FirstOrDefaultAsync(
                    x => x.PatientEncounterId == encounterId && !x.IsDelete,
                    cancellationToken);

            return consent == null ? null : await ToResponseAsync(consent, cancellationToken);
        }

        /// <summary>Kunjungan ada dan belum dihapus.</summary>
        public Task<bool> EncounterExistsAsync(Guid encounterId, CancellationToken cancellationToken = default) =>
            _dbContext.RegPatientEncounters
                .AsNoTracking()
                .AnyAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);

        /// <summary>
        /// Menyimpan consent pertama kunjungan. Urutan penolakan: kunjungan tidak ada (404) →
        /// bukan Rawat Jalan atau batal (400) → isian tidak sah (400).
        /// </summary>
        public async Task<OutpatientGeneralConsentResult> SaveAsync(
            Guid encounterId,
            OutpatientGeneralConsentSaveRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var encounter = await _dbContext.RegPatientEncounters
                .AsNoTracking()
                .Where(x => x.Id == encounterId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.PatientId,
                    x.EncounterType,
                    x.EncounterStatus,
                    x.IsCancel
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (encounter == null)
                return OutpatientGeneralConsentResult.NotFound(EncounterNotFoundMessage);

            if (encounter.EncounterType != EncounterType.Outpatient)
                return OutpatientGeneralConsentResult.Invalid("General Consent ini hanya untuk kunjungan Rawat Jalan.");

            if (encounter.IsCancel || encounter.EncounterStatus == EncounterStatus.Cancelled)
                return OutpatientGeneralConsentResult.Invalid("Kunjungan sudah dibatalkan, General Consent tidak dapat disimpan.");

            var existing = await GetAsync(encounterId, cancellationToken);

            if (existing != null)
            {
                return OutpatientGeneralConsentResult.Ok(
                    existing,
                    "General Consent kunjungan ini sudah tersimpan; isian tidak diubah.");
            }

            var validationError = Validate(request, out var signerType, out var clauses);

            if (validationError != null)
                return OutpatientGeneralConsentResult.Invalid(validationError);

            var now = DateTime.UtcNow;

            _dbContext.RegPatientGeneralConsents.Add(new RegPatientGeneralConsent
            {
                PatientId = encounter.PatientId,
                PatientEncounterId = encounter.Id,
                SignerType = signerType,
                SignerName = request.SignerName.Trim(),
                SignerRelationshipText = CleanText(request.SignerRelationshipText),
                SignerIdentityNumber = CleanText(request.SignerIdentityNumber),
                SignerPhoneNumber = CleanText(request.SignerPhoneNumber),
                SignerAddress = CleanText(request.SignerAddress),
                AgreedClauses = clauses,
                SignatureImageBase64 = request.SignatureImageBase64.Trim(),
                SignedAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now
            });

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                // Dua penyimpanan bersamaan: unique index memutuskan pemenangnya, yang kalah
                // menerima consent pemenang alih-alih galat.
                _dbContext.ChangeTracker.Clear();
            }

            var saved = await GetAsync(encounterId, cancellationToken);

            return OutpatientGeneralConsentResult.Ok(saved!, "General Consent berhasil disimpan.");
        }

        private static string? Validate(
            OutpatientGeneralConsentSaveRequest request,
            out GeneralConsentSignerType signerType,
            out int[] clauses)
        {
            signerType = default;
            clauses = Array.Empty<int>();

            var rawSignerType = request.SignerType?.Trim() ?? string.Empty;

            // Hanya nama enum yang diterima; angka seperti "1" ditolak supaya kontraknya satu bentuk.
            if (rawSignerType.Length == 0 ||
                int.TryParse(rawSignerType, out _) ||
                !Enum.TryParse(rawSignerType, ignoreCase: true, out signerType) ||
                !Enum.IsDefined(signerType))
            {
                return "Jenis penanda tangan tidak dikenal.";
            }

            if (string.IsNullOrWhiteSpace(request.SignerName) || request.SignerName.Trim().Length < 3)
                return "Nama penanda tangan minimal 3 karakter.";

            if (request.SignerName.Trim().Length > 200)
                return "Nama penanda tangan maksimal 200 karakter.";

            var phone = request.SignerPhoneNumber?.Trim();

            if (!string.IsNullOrEmpty(phone) && (phone.Length > 13 || !phone.All(char.IsAsciiDigit)))
                return "Nomor telepon penanda tangan hanya angka, maksimal 13 digit.";

            var agreed = (request.AgreedClauses ?? new List<int>()).Distinct().OrderBy(x => x).ToArray();

            if (agreed.Length != ClauseCount || !agreed.SequenceEqual(Enumerable.Range(1, ClauseCount)))
                return $"Seluruh {ClauseCount} butir klausul General Consent wajib disetujui.";

            if (!IsPngDataUrl(request.SignatureImageBase64))
                return "Tanda tangan digital wajib dibubuhkan dalam format gambar PNG.";

            clauses = agreed;
            return null;
        }

        private static bool IsPngDataUrl(string? value)
        {
            var dataUrl = value?.Trim();

            if (string.IsNullOrEmpty(dataUrl) ||
                dataUrl.Length > SignatureMaxLength ||
                !dataUrl.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                var bytes = Convert.FromBase64String(dataUrl[SignaturePrefix.Length..]);
                return bytes.Length > PngSignature.Length && bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private async Task<OutpatientGeneralConsentResponse> ToResponseAsync(
            RegPatientGeneralConsent consent,
            CancellationToken cancellationToken)
        {
            var officer = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == consent.CreateBy)
                .Select(u => new { u.DisplayName, u.UserName })
                .FirstOrDefaultAsync(cancellationToken);

            return new OutpatientGeneralConsentResponse
            {
                ConsentId = consent.Id,
                EncounterId = consent.PatientEncounterId,
                EncounterNumber = consent.PatientEncounter?.EncounterNumber ?? string.Empty,
                PatientId = consent.PatientId,
                SignerType = consent.SignerType.ToString(),
                SignerTypeName = GetDisplayName(consent.SignerType),
                SignerName = consent.SignerName,
                SignerRelationshipText = consent.SignerRelationshipText,
                SignerIdentityNumber = consent.SignerIdentityNumber,
                SignerPhoneNumber = consent.SignerPhoneNumber,
                SignerAddress = consent.SignerAddress,
                AgreedClauses = consent.AgreedClauses.ToList(),
                SignatureImageBase64 = consent.SignatureImageBase64,
                SignedAt = consent.SignedAt,
                OfficerName = officer == null
                    ? null
                    : string.IsNullOrWhiteSpace(officer.DisplayName) ? officer.UserName : officer.DisplayName
            };
        }

        private static string? CleanText(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        private static bool IsUniqueViolation(DbUpdateException exception) =>
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

        private static string GetDisplayName<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }

    public enum OutpatientGeneralConsentResultStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2
    }

    public sealed record OutpatientGeneralConsentResult(
        OutpatientGeneralConsentResultStatus Status,
        string Message,
        OutpatientGeneralConsentResponse? Consent)
    {
        public static OutpatientGeneralConsentResult Ok(OutpatientGeneralConsentResponse consent, string message) =>
            new(OutpatientGeneralConsentResultStatus.Success, message, consent);

        public static OutpatientGeneralConsentResult NotFound(string message) =>
            new(OutpatientGeneralConsentResultStatus.NotFound, message, null);

        public static OutpatientGeneralConsentResult Invalid(string message) =>
            new(OutpatientGeneralConsentResultStatus.Invalid, message, null);
    }
}
