using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Dugaan reaksi obat dicatat sebagai alergi tertaut dosis MAR — <c>BE-RWI-117</c>, <c>FR-KEP-070</c>,
    /// <c>INT-KEP-13</c>, <c>VAL-KEP-35</c>, <c>AC-MVP-029</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Contoh.</b> Ceftriaxone 1 g IV Budi diberikan 08.05. Pukul 08.25 Ns. Siti melihat ruam gatal di dada dan
    /// mencatat dugaan reaksi dari dosis itu. Lahir satu <c>TrxPatientAllergy</c> kategori <c>Drug</c>, kepastian
    /// <c>Suspected</c>, belum diverifikasi, tertaut <c>SourceMedicationAdministrationId</c> dan episode. Saat
    /// ceftriaxone diresepkan lagi, peringatan alergi aktif muncul. Yang mengkonfirmasi adalah dokter lewat
    /// <c>PATCH patient-allergies/{id}/verify</c> yang sudah ada.
    /// </para>
    /// <para>
    /// <b>Baris MAR tidak disentuh.</b> Dosis dibaca dan dikunci selama transaksi supaya dua kiriman bersamaan tidak
    /// melahirkan dua dugaan, tetapi status maupun isinya tidak diubah — dosis 08.05 tetap <c>Administered</c>.
    /// </para>
    /// <para>
    /// <b>Kiriman ulang.</b> <c>TrxPatientAllergy</c> tidak punya kolom kunci permintaan (migration <c>K6</c> hanya
    /// menambah dua kolom). Kiriman ulang dikenali dari kunci alami: dugaan yang sudah tertaut dosis yang sama dikembalikan
    /// apa adanya, bukan digandakan.
    /// </para>
    /// </remarks>
    public class AdverseDrugReactionService
    {
        private const string LogCategory = "HealthServices.Clinical.AdverseDrugReaction";
        private const string SequenceKey = "CLI_PATIENT_ALLERGY_ADR";
        private const string NumberPrefix = "ADR";

        private readonly ApplicationDbContext _dbContext;
        private readonly NursingEpisodeWriteGuard _writeGuard;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly LoggerService _loggerService;

        public AdverseDrugReactionService(
            ApplicationDbContext dbContext,
            NursingEpisodeWriteGuard writeGuard,
            NumberSeriesAllocator numberSeriesAllocator,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _numberSeriesAllocator = numberSeriesAllocator;
            _loggerService = loggerService;
        }

        public async Task<NursingResult<PatientAllergyResponse>> CreateFromMedicationAdministrationAsync(
            CreateSuspectedAdverseDrugReactionRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // VAL-KEP-35b
            if (string.IsNullOrWhiteSpace(request.ReactionDescription))
                return NursingResult<PatientAllergyResponse>.Fail(StatusCodes.Status400BadRequest, "Jelaskan reaksi yang terlihat.", "REACTION_DESCRIPTION_REQUIRED");

            if (!Enum.IsDefined(request.Severity))
                return NursingResult<PatientAllergyResponse>.Fail(StatusCodes.Status400BadRequest, "Tingkat keparahan tidak dikenal.");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var dosis = await _dbContext.Set<PhmMedicationAdministration>()
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM public.""PhmMedicationAdministration""
                    WHERE ""Id"" = {request.MedicationAdministrationId}
                      AND ""IsDelete"" = false
                    FOR UPDATE")
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (dosis == null)
                return NursingResult<PatientAllergyResponse>.Fail(StatusCodes.Status404NotFound, "Dosis MAR tidak ditemukan.");

            // 422 perawatan ditutup, 403 unit lain — penjaga tulis keperawatan yang sama.
            var penjaga = await _writeGuard.EnsureCanWriteAsync(dosis.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<PatientAllergyResponse>();

            // VAL-KEP-35a
            if (dosis.DoseStatus != MedicationDoseStatus.Administered)
                return NursingResult<PatientAllergyResponse>.Fail(StatusCodes.Status409Conflict, "Dugaan reaksi hanya dicatat untuk obat yang sudah diberikan.", "DOSE_NOT_ADMINISTERED");

            var sudahAda = await _dbContext.Set<TrxPatientAllergy>().AsNoTracking()
                .Where(x => x.SourceMedicationAdministrationId == dosis.Id && !x.IsDelete && x.AllergyStatus != PatientAllergyStatus.Cancelled)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (sudahAda.HasValue)
                return NursingResult<PatientAllergyResponse>.Ok(await ToResponseAsync(sudahAda.Value, cancellationToken), "Dugaan reaksi obat dari dosis ini sudah tercatat.", isReplay: true);

            // Aturan duplikat yang sama dengan pencatatan alergi manual: satu obat satu catatan alergi hidup.
            var alergiObatAda = await _dbContext.Set<TrxPatientAllergy>().AsNoTracking()
                .AnyAsync(x => x.PatientId == dosis.PatientId && x.DrugId == dosis.DrugId && !x.IsDelete && x.AllergyStatus != PatientAllergyStatus.Cancelled, cancellationToken);

            if (alergiObatAda)
                return NursingResult<PatientAllergyResponse>.Fail(StatusCodes.Status409Conflict,
                    "Alergi obat yang sama sudah tercatat untuk pasien ini. Perbarui catatan alergi itu.", "ALLERGY_ALREADY_RECORDED");

            var butir = await _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .Where(x => x.Id == dosis.PrescriptionItemId)
                .Select(x => new { x.DrugCodeSnapshot, x.DrugNameSnapshot, x.GenericNameSnapshot })
                .FirstAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var konteks = penjaga.Value!;
            var onset = request.ReactionOnsetAt.HasValue ? AsUtc(request.ReactionOnsetAt.Value) : now;

            if (onset > now.AddMinutes(5))
                return NursingResult<PatientAllergyResponse>.Fail(StatusCodes.Status400BadRequest, "Waktu reaksi tidak boleh di masa depan.");

            string nomor;

            try
            {
                nomor = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(SequenceKey, NumberPrefix, NumberSeriesResetPolicies.Daily, 4, actorUserId, DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return NursingResult<PatientAllergyResponse>.Fail(StatusCodes.Status422UnprocessableEntity, "Nomor catatan alergi gagal diterbitkan. Hubungi administrator sistem.");
            }

            var alergi = new TrxPatientAllergy
            {
                Id = Guid.NewGuid(),
                AllergyRecordNumber = nomor,
                PatientId = dosis.PatientId,
                EncounterId = dosis.EncounterId,
                ServiceUnitId = konteks.ServiceUnitId,
                DrugId = dosis.DrugId,
                AllergyCategory = PatientAllergyCategory.Drug,
                AllergenCode = Truncate(butir.DrugCodeSnapshot, 100),
                AllergenName = Truncate(butir.DrugNameSnapshot, 250) ?? "Obat",
                AllergenGroupName = Truncate(butir.GenericNameSnapshot, 250),
                ReactionDescription = Truncate(request.ReactionDescription, 1000),
                Severity = request.Severity,
                Certainty = PatientAllergyCertainty.Suspected,
                AllergyStatus = PatientAllergyStatus.Active,
                FirstReactionDate = onset,
                LastReactionDate = onset,
                ReportedDateTime = now,
                SourceOfInformation = "Nurse",
                IsHighRisk = request.Severity is PatientAllergySeverity.Severe or PatientAllergySeverity.LifeThreatening,
                IsLifeThreatening = request.Severity == PatientAllergySeverity.LifeThreatening,
                IsAlertEnabled = true,
                IsVerified = false,
                ClinicalNote = Truncate(request.ClinicalNote, 1000),
                IsActive = true,
                InpEpisodeId = dosis.InpEpisodeId,
                SourceMedicationAdministrationId = dosis.Id,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<TrxPatientAllergy>().Add(alergi);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            // ReactionDescription dan ClinicalNote klinis — tidak masuk payload logger.
            await _loggerService.InfoAsync(LogCategory, "AdverseDrugReaction.CreateFromMedicationAdministration",
                "Perawat mencatat dugaan reaksi obat dari dosis MAR.",
                new { alergi.Id, alergi.AllergyRecordNumber, MedicationAdministrationId = dosis.Id, alergi.InpEpisodeId, RecordedBy = actorUserId });

            return NursingResult<PatientAllergyResponse>.Ok(await ToResponseAsync(alergi.Id, cancellationToken),
                "Dugaan reaksi obat berhasil dicatat sebagai alergi yang belum diverifikasi.", StatusCodes.Status201Created);
        }

        private async Task<PatientAllergyResponse> ToResponseAsync(Guid id, CancellationToken cancellationToken)
        {
            var x = await _dbContext.Set<TrxPatientAllergy>().AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Encounter)
                .Include(a => a.ServiceUnit)
                .Include(a => a.VerifiedByUser)
                .FirstAsync(a => a.Id == id, cancellationToken);

            return new PatientAllergyResponse
            {
                Id = x.Id,
                AllergyRecordNumber = x.AllergyRecordNumber,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter?.EncounterNumber,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit?.ServiceUnitName,
                DrugId = x.DrugId,
                AllergyCategory = x.AllergyCategory,
                AllergenCode = x.AllergenCode,
                AllergenName = x.AllergenName,
                AllergenGroupName = x.AllergenGroupName,
                ReactionType = x.ReactionType,
                Severity = x.Severity,
                Certainty = x.Certainty,
                AllergyStatus = x.AllergyStatus,
                FirstReactionDate = x.FirstReactionDate,
                LastReactionDate = x.LastReactionDate,
                ReportedDateTime = x.ReportedDateTime,
                SourceOfInformation = x.SourceOfInformation,
                IsHighRisk = x.IsHighRisk,
                IsLifeThreatening = x.IsLifeThreatening,
                IsAlertEnabled = x.IsAlertEnabled,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser?.DisplayName,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static string? Truncate(string? value, int max) =>
            string.IsNullOrWhiteSpace(value) ? null : (value.Trim().Length > max ? value.Trim()[..max] : value.Trim());

        private static DateTime AsUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
