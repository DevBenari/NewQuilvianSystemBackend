using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Data.Common;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pendaftaran pasien laboratorium (<c>FR-08.1</c> .. <c>FR-08.5</c>, <c>LAB-DEC-032</c>,
    /// <c>LAB-DEC-035</c>).
    ///
    /// <b>Batas yang membentuk seluruh berkas ini, dan yang paling mudah dilanggar.</b>
    /// Laboratorium memiliki <i>layarnya</i>, Registrasi memiliki <i>kunjungannya</i>. Karena
    /// itu di sini tidak ada satu pun penulisan ke <c>RegPatientEncounter</c> maupun
    /// <c>MstPatient</c> — tidak ada <c>Add</c>, tidak ada <c>Update</c>, tidak ada
    /// <c>SaveChanges</c>. Yang ada hanya: menyusun isian, menyerahkannya ke
    /// <see cref="EncounterIntakeService"/> milik Registrasi, menunggu jawabannya, lalu
    /// membacakan hasilnya. Itulah <c>AC-45</c>, dan ada uji yang menjaganya.
    ///
    /// Godaannya nyata: menulis satu baris kunjungan di sini akan jauh lebih singkat daripada
    /// memanggil Registrasi. Yang hilang bila itu dilakukan adalah satu-satunya tempat aturan
    /// kunjungan ditegakkan — dan aturan itu akan bercabang diam-diam.
    /// </summary>
    public class LabPatientRegistrationService
    {
        private const int MaxSearchLimit = 50;

        private readonly ApplicationDbContext _dbContext;
        private readonly EncounterIntakeService _encounterIntakeService;

        public LabPatientRegistrationService(
            ApplicationDbContext dbContext,
            EncounterIntakeService encounterIntakeService)
        {
            _dbContext = dbContext;
            _encounterIntakeService = encounterIntakeService;
        }

        // =================================================================
        // Pencarian pasien — baca saja
        // =================================================================

        /// <summary>
        /// Mencari pasien yang sudah terdaftar (<c>FR-08.2</c>).
        ///
        /// Dipakai sebelum mendaftarkan kunjungan, supaya pasien lama tidak berakhir punya dua
        /// nomor rekam medis. Membacanya tidak membentuk apa pun.
        /// </summary>
        public async Task<List<LabPatientSearchResponse>> SearchPatientsAsync(
            LabPatientSearchQuery query,
            CancellationToken cancellationToken = default)
        {
            var limit = Math.Clamp(query.Limit, 1, MaxSearchLimit);

            var source = _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.MedicalRecordNumber.Contains(search) ||
                    x.PatientCode.Contains(search) ||
                    x.FullName.Contains(search) ||
                    (x.IdentityNumber != null && x.IdentityNumber.Contains(search)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(search)));
            }

            return await source
                .OrderBy(x => x.FullName)
                .Take(limit)
                .Select(x => new LabPatientSearchResponse
                {
                    PatientId = x.Id,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    PatientCode = x.PatientCode,
                    FullName = x.FullName,
                    BirthDate = x.BirthDate,
                    Gender = x.Gender != null ? x.Gender.ToString() : null,
                    IdentityNumber = x.IdentityNumber,
                    PhoneNumber = x.PhoneNumber,
                    Address = x.Address
                })
                .ToListAsync(cancellationToken);
        }

        // =================================================================
        // Pendaftaran — diteruskan ke Registrasi
        // =================================================================

        /// <summary>Mendaftarkan pasien yang datang langsung ke laboratorium (<c>AC-44</c>).</summary>
        public async Task<LabRegistrationResultResponse> RegisterWalkInAsync(
            RegisterLabWalkInRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            EnsureIdempotencyKey(request.IdempotencyKey);

            var intake = new EncounterIntakeRequest
            {
                PatientId = request.PatientId,
                ServiceUnitId = request.ServiceUnitId,
                IdempotencyKey = request.IdempotencyKey,
                PaymentType = request.PaymentType,
                PaymentMethodId = request.PaymentMethodId,
                PatientInsuranceId = request.PatientInsuranceId,
                IsReferral = false,
                Notes = request.Notes
            };

            return await ForwardAsync(intake, cancellationToken);
        }

        /// <summary>
        /// Mendaftarkan pasien rujukan luar beserta perujuknya (<c>AC-46</c>, <c>AC-50</c>).
        ///
        /// Ketiga isian rujukan diperiksa di sini lebih dulu supaya pesan yang sampai ke
        /// petugas adalah pesan <c>VAL-43</c> dan <c>VAL-44</c> yang sudah disepakati, bukan
        /// pesan bawaan model binding.
        /// </summary>
        public async Task<LabRegistrationResultResponse> RegisterExternalReferralAsync(
            RegisterLabExternalReferralRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            EnsureIdempotencyKey(request.IdempotencyKey);

            // VAL-44.
            if (string.IsNullOrWhiteSpace(request.ReferralNumber))
            {
                throw new LabPatientRegistrationValidationException(
                    "Nomor surat rujukan wajib diisi untuk pasien rujukan.");
            }

            // VAL-43. Instansi dipilih dari daftar; tidak ada jalan mengetikkan namanya.
            if (!request.ReferralInstitutionId.HasValue ||
                request.ReferralInstitutionId.Value == Guid.Empty)
            {
                throw new LabPatientRegistrationValidationException(
                    "Pilih instansi perujuk dari daftar. Bila belum ada, hubungi bagian data induk untuk menambahkannya.");
            }

            if (!request.ReferralDoctorId.HasValue || request.ReferralDoctorId.Value == Guid.Empty)
            {
                throw new LabPatientRegistrationValidationException(
                    "Pilih dokter perujuk dari daftar instansi yang bersangkutan.");
            }

            var intake = new EncounterIntakeRequest
            {
                PatientId = request.PatientId,
                ServiceUnitId = request.ServiceUnitId,
                IdempotencyKey = request.IdempotencyKey,
                PaymentType = request.PaymentType,
                PaymentMethodId = request.PaymentMethodId,
                PatientInsuranceId = request.PatientInsuranceId,
                IsReferral = true,
                ReferralNumber = request.ReferralNumber,
                ReferralInstitutionId = request.ReferralInstitutionId,
                ReferralDoctorId = request.ReferralDoctorId,
                Notes = request.Notes
            };

            return await ForwardAsync(intake, cancellationToken);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<LabRegistrationResultResponse> ForwardAsync(
            EncounterIntakeRequest intake,
            CancellationToken cancellationToken)
        {
            EncounterIntakeResult hasil;

            try
            {
                hasil = await _encounterIntakeService.RegisterAsync(intake, cancellationToken);
            }
            catch (DbException exception)
            {
                // VAL-42. Registrasi berada dalam proses yang sama, sehingga "tidak dapat
                // dihubungi" di sini berarti penyimpanannya yang tidak dapat dicapai.
                // Penolakannya tetap utuh: tidak ada satu pun baris yang tersimpan.
                throw new LabRegistrationUnavailableException(
                    "Pendaftaran gagal karena layanan registrasi sedang tidak dapat diakses. Silakan coba lagi.",
                    exception);
            }

            // Identitas pasien dibaca sendiri dari data induk — hak baca yang memang dimiliki
            // Laboratorium (CAP-09) — bukan disalin lewat jawaban Registrasi.
            var patient = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == hasil.PatientId)
                .Select(x => new { x.MedicalRecordNumber, x.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            return new LabRegistrationResultResponse
            {
                EncounterId = hasil.EncounterId,
                EncounterNumber = hasil.EncounterNumber,
                EncounterDate = hasil.EncounterDate,
                EncounterStatus = hasil.EncounterStatus.ToString(),
                PatientId = hasil.PatientId,
                MedicalRecordNumber = patient?.MedicalRecordNumber ?? string.Empty,
                FullName = patient?.FullName ?? string.Empty,
                IsReplay = hasil.IsReplay
            };
        }

        private static void EnsureIdempotencyKey(string? idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new LabPatientRegistrationValidationException(
                    "Kunci idempotensi wajib dikirim agar penekanan Simpan dua kali tidak menghasilkan dua kunjungan.");
            }
        }
    }

    /// <summary>Isian pendaftaran tidak memenuhi aturan. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabPatientRegistrationValidationException(string message) : Exception(message);

    /// <summary>Registrasi tidak dapat dicapai (<c>VAL-42</c>). Dipetakan menjadi <c>503</c>.</summary>
    public sealed class LabRegistrationUnavailableException(string message, Exception inner)
        : Exception(message, inner);
}
