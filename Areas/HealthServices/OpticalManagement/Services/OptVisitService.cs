using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Kunjungan Optik (<c>BE-OPT-010</c>, <c>OPT-API-v1</c> grup Opt Visit, <c>INT-OPT-01</c>, <c>OPT-DEC-006</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kunjungan milik Registrasi.</b> Kelas ini tidak pernah menulis <see cref="RegPatientEncounter"/>; pendaftaran
    /// diteruskan ke <see cref="EncounterIntakeService"/> (jalur unit penunjang, jenis <c>Outpatient</c>, unit
    /// <c>ServiceUnitType.Optical</c>). Akibatnya pengguna juga wajib memegang izin Registrasi
    /// <c>PatientEncounter : Create</c> — Registrasi yang menegakkannya, bukan Optik (<c>VAL-41</c>).
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Kunci dari header <c>Idempotency-Key</c> diteruskan sebagai <c>OPT-VISIT:{kunci}</c>, sehingga
    /// Simpan dua kali menghasilkan satu kunjungan (<c>IsReplay = true</c> pada jawaban kedua) dan tidak bertabrakan
    /// dengan kunci modul lain.
    /// </para>
    /// <para>
    /// <b>Rujukan bertaut belum tersedia.</b> <c>ReferralSourceEncounterId</c> ditolak sampai <c>BE-OPT-003</c> menambah
    /// kolom tujuannya; dokter dan instansi perujuk sudah diteruskan ke Registrasi.
    /// </para>
    /// </remarks>
    public class OptVisitService
    {
        public const string IdempotencyPrefix = "OPT-VISIT:";
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;

        private static readonly OptOrderStatus[] StatusPesananTerminal =
            [OptOrderStatus.HandedOver, OptOrderStatus.Cancelled, OptOrderStatus.NotCollected];

        private static readonly EncounterStatus[] StatusKunjunganTertutup =
            [EncounterStatus.Completed, EncounterStatus.Cancelled, EncounterStatus.NoShow];

        private readonly ApplicationDbContext _dbContext;
        private readonly EncounterIntakeService _encounterIntakeService;

        public OptVisitService(ApplicationDbContext dbContext, EncounterIntakeService encounterIntakeService)
        {
            _dbContext = dbContext;
            _encounterIntakeService = encounterIntakeService;
        }

        // ---------------------------------------------------------------- pendaftaran

        public async Task<OptResult<OptVisitListItemResponse>> RegisterAsync(
            OptVisitRegisterRequest request, string? idempotencyKey, CancellationToken cancellationToken = default)
        {
            var kunci = idempotencyKey?.Trim();
            if (string.IsNullOrEmpty(kunci))
                return OptResult<OptVisitListItemResponse>.Invalid(OptErrorCodes.InvalidRequest,
                    "Header Idempotency-Key wajib diisi agar Simpan dua kali tidak membuat kunjungan ganda.");
            if (IdempotencyPrefix.Length + kunci.Length > 100)
                return OptResult<OptVisitListItemResponse>.Invalid(OptErrorCodes.InvalidRequest, "Idempotency-Key terlalu panjang.");

            if (request.PatientId is not { } patientId || patientId == Guid.Empty)
                return OptResult<OptVisitListItemResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih pasien.");

            if (request.ReferralSourceEncounterId.HasValue)
                return OptResult<OptVisitListItemResponse>.Invalid(OptErrorCodes.InvalidRequest,
                    "Tautan ke kunjungan Poli Mata asal belum tersedia. Kosongkan isian itu; dokter perujuk tetap dapat diisi.");

            var unitId = await FindOpticalUnitIdAsync(activeOnly: true, cancellationToken);
            if (unitId is null)
                return OptResult<OptVisitListItemResponse>.Conflict(OptErrorCodes.OpticalUnitMissing,
                    "Unit layanan Optik belum terdaftar atau tidak aktif. Hubungi bagian Master Data.");

            var intake = new EncounterIntakeRequest
            {
                PatientId = patientId,
                ServiceUnitId = unitId.Value,
                EncounterType = EncounterType.Outpatient,
                IdempotencyKey = IdempotencyPrefix + kunci,
                PaymentType = request.PaymentType,
                PaymentMethodId = request.PaymentMethodId,
                PatientInsuranceId = request.PatientInsuranceId,
                PatientCompanyGuarantorId = request.PatientCompanyGuarantorId,
                IsReferral = request.IsReferral,
                ReferralNumber = request.ReferralNumber,
                ReferralInstitutionId = request.ReferralInstitutionId,
                ReferralDoctorId = request.ReferralDoctorId,
                Notes = request.Notes,
            };

            EncounterIntakeResult hasil;
            try
            {
                hasil = await _encounterIntakeService.RegisterAsync(intake, cancellationToken);
            }
            catch (EncounterIntakeValidationException ex)
            {
                return OptResult<OptVisitListItemResponse>.Invalid(OptErrorCodes.RegistrationRejected, ex.Message);
            }
            catch (EncounterIntakeForbiddenException ex)
            {
                return OptResult<OptVisitListItemResponse>.Forbidden(OptErrorCodes.RegistrationForbidden, ex.Message);
            }
            catch (EncounterIntakeConflictException ex)
            {
                return OptResult<OptVisitListItemResponse>.Conflict(OptErrorCodes.RegistrationConflict, ex.Message);
            }
            catch (DbException)
            {
                return OptResult<OptVisitListItemResponse>.Unavailable(OptErrorCodes.RegistrationUnavailable,
                    "Pendaftaran gagal karena layanan registrasi sedang tidak dapat diakses. Silakan coba lagi.");
            }

            var item = await ProjectVisitsAsync(
                _dbContext.Set<RegPatientEncounter>().AsNoTracking().Where(x => x.Id == hasil.EncounterId),
                cancellationToken);
            var visit = item.Single();
            visit.IsReplay = hasil.IsReplay;
            return OptResult<OptVisitListItemResponse>.Ok(visit);
        }

        // ---------------------------------------------------------------- daftar

        public async Task<PagedResult<OptVisitListItemResponse>> GetListAsync(OptVisitListQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var visits = OpticalEncounters();

            if (query.Date is { } tanggal)
            {
                var awal = AppDateTimeHelper.OperationalDateToUtc(tanggal.ToDateTime(TimeOnly.MinValue));
                var akhir = AppDateTimeHelper.OperationalDateToUtc(tanggal.AddDays(1).ToDateTime(TimeOnly.MinValue));
                visits = visits.Where(x => x.EncounterDate >= awal && x.EncounterDate < akhir);
            }

            if (query.OpenOnly == true)
                visits = visits.Where(x => !StatusKunjunganTertutup.Contains(x.EncounterStatus));

            if (query.HasOpenOrder == true)
                visits = visits.Where(x => _dbContext.OptOrders.Any(o => o.EncounterId == x.Id && !o.IsDelete && !StatusPesananTerminal.Contains(o.Status)));

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var kata = query.Search.Trim().ToLower();
                visits = visits.Where(x => x.EncounterNumber.ToLower().Contains(kata)
                    || _dbContext.Set<MstPatient>().Any(p => p.Id == x.PatientId
                        && (p.MedicalRecordNumber.ToLower().Contains(kata) || p.FullName.ToLower().Contains(kata))));
            }

            var total = await visits.CountAsync(cancellationToken);
            var page = await ProjectVisitsAsync(
                visits.OrderByDescending(x => x.EncounterDate).ThenByDescending(x => x.EncounterNumber)
                    .Skip((pageNumber - 1) * pageSize).Take(pageSize),
                cancellationToken);

            return new PagedResult<OptVisitListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = page,
            };
        }

        // ---------------------------------------------------------------- ringkasan

        public async Task<OptResult<OptVisitDetailResponse>> GetDetailAsync(Guid encounterId, CancellationToken cancellationToken = default)
        {
            var visits = await ProjectVisitsAsync(OpticalEncounters().Where(x => x.Id == encounterId), cancellationToken);
            if (visits.Count == 0)
                return OptResult<OptVisitDetailResponse>.NotFound("Kunjungan Optik tidak ditemukan.");

            var exams = await _dbContext.OptRefractionExams.AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.ExamNumber, x.Status, x.CreateDateTime })
                .ToListAsync(cancellationToken);
            var prescriptions = await _dbContext.OptPrescriptions.AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.PrescriptionNumber, x.PrescriptionType, x.CreateDateTime })
                .ToListAsync(cancellationToken);
            var orders = await _dbContext.OptOrders.AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.OrderNumber, x.Status, x.CreateDateTime })
                .ToListAsync(cancellationToken);
            var claims = await _dbContext.OptWarrantyClaims.AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.ClaimNumber, x.Status, x.CreateDateTime })
                .ToListAsync(cancellationToken);

            return OptResult<OptVisitDetailResponse>.Ok(new OptVisitDetailResponse
            {
                Visit = visits[0],
                RefractionExams = exams.Select(x => new OptVisitDocumentItem { Id = x.Id, Number = x.ExamNumber, Status = (int)x.Status, StatusLabel = OptLabels.RefractionExamStatus(x.Status), CreatedAt = x.CreateDateTime }).ToList(),
                Prescriptions = prescriptions.Select(x => new OptVisitDocumentItem { Id = x.Id, Number = x.PrescriptionNumber, Status = (int)x.PrescriptionType, StatusLabel = OptLabels.PrescriptionType(x.PrescriptionType), CreatedAt = x.CreateDateTime }).ToList(),
                Orders = orders.Select(x => new OptVisitDocumentItem { Id = x.Id, Number = x.OrderNumber, Status = (int)x.Status, StatusLabel = OptLabels.OrderStatus(x.Status), CreatedAt = x.CreateDateTime }).ToList(),
                WarrantyClaims = claims.Select(x => new OptVisitDocumentItem { Id = x.Id, Number = x.ClaimNumber, Status = (int)x.Status, StatusLabel = OptLabels.WarrantyClaimStatus(x.Status), CreatedAt = x.CreateDateTime }).ToList(),
            });
        }

        /// <summary>
        /// Kunjungan Optik terbuka milik pasien tertentu — dipakai service Optik lain untuk memastikan dokumen menempel
        /// pada kunjungan Optik yang belum ditutup (<c>OPT-VAL-030</c>). <c>null</c> bila bukan kunjungan Optik atau sudah tertutup.
        /// </summary>
        public Task<RegPatientEncounter?> FindOpenOpticalEncounterAsync(Guid encounterId, CancellationToken cancellationToken = default) =>
            OpticalEncounters()
                .Where(x => x.Id == encounterId && !StatusKunjunganTertutup.Contains(x.EncounterStatus))
                .FirstOrDefaultAsync(cancellationToken);

        /// <summary>
        /// Kunjungan Optik apa pun statusnya; <c>null</c> bila bukan kunjungan unit Optik. Dipakai bila pemanggil perlu
        /// membedakan "tidak ditemukan" (<c>404</c>) dari "sudah ditutup" (<c>OPT-VAL-030</c>, <c>409</c>).
        /// </summary>
        public Task<RegPatientEncounter?> FindOpticalEncounterAsync(Guid encounterId, CancellationToken cancellationToken = default) =>
            OpticalEncounters()
                .Where(x => x.Id == encounterId)
                .FirstOrDefaultAsync(cancellationToken);

        /// <summary>Benar bila kunjungan sudah selesai, batal, atau tidak hadir.</summary>
        public static bool IsClosed(EncounterStatus status) => StatusKunjunganTertutup.Contains(status);

        // ---------------------------------------------------------------- pembantu

        private IQueryable<RegPatientEncounter> OpticalEncounters() =>
            from encounter in _dbContext.Set<RegPatientEncounter>().AsNoTracking()
            join unit in _dbContext.Set<MstServiceUnit>().AsNoTracking() on encounter.ServiceUnitId equals unit.Id
            where unit.ServiceUnitType == ServiceUnitType.Optical && !encounter.IsDelete
            select encounter;

        private Task<Guid?> FindOpticalUnitIdAsync(bool activeOnly, CancellationToken cancellationToken) =>
            _dbContext.Set<MstServiceUnit>().AsNoTracking()
                .Where(x => x.ServiceUnitType == ServiceUnitType.Optical && !x.IsDelete && (!activeOnly || x.IsActive))
                .OrderBy(x => x.CreateDateTime)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

        private async Task<List<OptVisitListItemResponse>> ProjectVisitsAsync(IQueryable<RegPatientEncounter> source, CancellationToken cancellationToken)
        {
            var rows = await source
                .Select(x => new OptVisitListItemResponse
                {
                    EncounterId = x.Id,
                    EncounterNumber = x.EncounterNumber,
                    EncounterDate = x.EncounterDate,
                    EncounterStatus = x.EncounterStatus,
                    PaymentType = x.PaymentType,
                    PatientId = x.PatientId,
                    MedicalRecordNumber = _dbContext.Set<MstPatient>().Where(p => p.Id == x.PatientId).Select(p => p.MedicalRecordNumber).FirstOrDefault() ?? string.Empty,
                    FullName = _dbContext.Set<MstPatient>().Where(p => p.Id == x.PatientId).Select(p => p.FullName).FirstOrDefault() ?? string.Empty,
                    IsReferral = x.IsReferral,
                    ExamCount = _dbContext.OptRefractionExams.Count(e => e.EncounterId == x.Id && !e.IsDelete && e.Status != OptRefractionExamStatus.Cancelled),
                    PrescriptionCount = _dbContext.OptPrescriptions.Count(r => r.EncounterId == x.Id && !r.IsDelete),
                    OpenOrderCount = _dbContext.OptOrders.Count(o => o.EncounterId == x.Id && !o.IsDelete && !StatusPesananTerminal.Contains(o.Status)),
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
                row.EncounterStatusLabel = EncounterStatusLabel(row.EncounterStatus);

            return rows;
        }

        /// <summary>Label status kunjungan, sama dengan <c>[Display]</c> enum Registrasi.</summary>
        public static string EncounterStatusLabel(EncounterStatus status) => status switch
        {
            EncounterStatus.Draft => "Draft",
            EncounterStatus.Registered => "Terdaftar",
            EncounterStatus.Queued => "Masuk Antrean",
            EncounterStatus.WaitingForNurse => "Menunggu Perawat",
            EncounterStatus.InNurseScreening => "Screening Perawat",
            EncounterStatus.WaitingForDoctor => "Menunggu Dokter",
            EncounterStatus.InConsultation => "Sedang Konsultasi",
            EncounterStatus.ConsultationCompleted => "Konsultasi Selesai",
            EncounterStatus.Billing => "Proses Billing",
            EncounterStatus.Completed => "Selesai",
            EncounterStatus.Cancelled => "Dibatalkan",
            EncounterStatus.NoShow => "Tidak Hadir",
            _ => "Tidak diketahui"
        };
    }
}
