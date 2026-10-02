using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// RJ-DOC-REV-BE-001. Membaca identitas klinis ringkas pasien untuk header workspace dokter:
    /// alergi, dokumen identitas utama, dan gambar kartu asuransi kunjungan. Dibaca sekali per
    /// halaman antrean (batch), bukan per baris.
    /// </summary>
    public class DoctorQueuePatientContextService
    {
        private readonly ApplicationDbContext _dbContext;

        public DoctorQueuePatientContextService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyDictionary<Guid, DoctorQueuePatientContext>> LoadAsync(
            IReadOnlyCollection<(Guid PatientId, Guid EncounterId, Guid? PatientInsuranceId)> rows,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, DoctorQueuePatientContext>();
            if (rows.Count == 0) return result;

            var patientIds = rows.Select(x => x.PatientId).Distinct().ToList();
            var encounterIds = rows.Select(x => x.EncounterId).Distinct().ToList();
            var insuranceIds = rows.Where(x => x.PatientInsuranceId.HasValue)
                .Select(x => x.PatientInsuranceId!.Value).Distinct().ToList();

            // Alergi terstruktur yang masih aktif berlaku lintas kunjungan.
            var allergyRows = await _dbContext.TrxPatientAllergies.AsNoTracking()
                .Where(x => patientIds.Contains(x.PatientId)
                    && !x.IsDelete && x.IsActive
                    && x.AllergyStatus == PatientAllergyStatus.Active)
                .Select(x => new { x.PatientId, x.AllergenName })
                .ToListAsync(cancellationToken);

            // Catatan alergi pada asesmen kunjungan ini (skrining perawat/dokter), yang terbaru.
            var assessmentAllergyRows = await _dbContext.TrxPatientAssessments.AsNoTracking()
                .Where(x => encounterIds.Contains(x.EncounterId)
                    && !x.IsDelete
                    && x.AssessmentStatus != PatientAssessmentStatus.Cancelled
                    && x.HasAllergy)
                .Select(x => new
                {
                    x.EncounterId,
                    x.AllergyType,
                    x.AllergyNote,
                    At = x.UpdateDateTime ?? x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            var identityRows = await _dbContext.MstPatientIdentityDocuments.AsNoTracking()
                .Where(x => patientIds.Contains(x.PatientId)
                    && !x.IsDelete && x.IsActive
                    && x.FilePath != null && x.FilePath != string.Empty)
                .Select(x => new { x.PatientId, x.FilePath, x.IsPrimary, At = x.UpdateDateTime ?? x.CreateDateTime })
                .ToListAsync(cancellationToken);

            var insuranceRows = insuranceIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : await _dbContext.Set<MstPatientInsurance>().AsNoTracking()
                    .Where(x => insuranceIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.CardImagePath, cancellationToken);

            var photoRows = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => patientIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.PhotoPath, cancellationToken);

            foreach (var row in rows)
            {
                var allergyParts = allergyRows
                    .Where(x => x.PatientId == row.PatientId)
                    .Select(x => x.AllergenName?.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                var latestAssessment = assessmentAllergyRows
                    .Where(x => x.EncounterId == row.EncounterId)
                    .OrderByDescending(x => x.At)
                    .FirstOrDefault();

                var assessmentText = latestAssessment == null
                    ? null
                    : string.Join(" - ", new[] { latestAssessment.AllergyType, latestAssessment.AllergyNote }
                        .Select(x => x?.Trim())
                        .Where(x => !string.IsNullOrEmpty(x)));

                if (!string.IsNullOrWhiteSpace(assessmentText)
                    && !allergyParts.Any(x => string.Equals(x, assessmentText, StringComparison.OrdinalIgnoreCase)))
                {
                    allergyParts.Add(assessmentText);
                }

                var hasAllergy = allergyParts.Count > 0 || latestAssessment != null;

                var identityPath = identityRows
                    .Where(x => x.PatientId == row.PatientId)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenByDescending(x => x.At)
                    .Select(x => x.FilePath)
                    .FirstOrDefault();

                string? cardPath = null;
                if (row.PatientInsuranceId is { } insuranceId)
                {
                    insuranceRows.TryGetValue(insuranceId, out cardPath);
                }

                photoRows.TryGetValue(row.PatientId, out var photoPath);

                result[row.EncounterId] = new DoctorQueuePatientContext(
                    hasAllergy,
                    allergyParts.Count > 0 ? string.Join("; ", allergyParts.Distinct(StringComparer.OrdinalIgnoreCase)) : null,
                    NormalizePath(photoPath),
                    NormalizePath(identityPath),
                    NormalizePath(cardPath));
            }

            return result;
        }

        private static string? NormalizePath(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public sealed record DoctorQueuePatientContext(
        bool HasAllergy,
        string? AllergySummary,
        string? PatientPhotoPath,
        string? IdentityDocumentPath,
        string? InsuranceCardImagePath)
    {
        public static readonly DoctorQueuePatientContext Empty = new(false, null, null, null, null);
    }
}
