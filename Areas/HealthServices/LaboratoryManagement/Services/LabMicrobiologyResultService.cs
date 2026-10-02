using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pengisian dan pembacaan hasil Mikrobiologi berstruktur (<c>BE-LAB-48</c>, slice
    /// <c>S4b</c>).
    ///
    /// <b>Mengisi hasil saja.</b> Nol memvalidasi, nol merilis, nol menandai nilai kritis, dan
    /// nol mengoreksi hasil terrilis — keempatnya milik <c>S4d</c>, <c>S5</c>, dan <c>S6</c>.
    /// </summary>
    public class LabMicrobiologyResultService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;
        private readonly LabSusceptibilityInterpreter _interpreter;
        private readonly LabProcedureMicrobiologyProfileService _profileService;
        private readonly LabMicrobiologyCriticalRuleService _criticalRuleService;

        public LabMicrobiologyResultService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService,
            LabSusceptibilityInterpreter interpreter,
            LabProcedureMicrobiologyProfileService profileService,
            LabMicrobiologyCriticalRuleService criticalRuleService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
            _interpreter = interpreter;
            _profileService = profileService;
            _criticalRuleService = criticalRuleService;
        }

        /// <summary>
        /// Mengganti <b>seluruh</b> isi hasil Mikrobiologi dalam satu tindakan
        /// (<c>AC-114</c>).
        ///
        /// <b>Utuh atau nol.</b> Seluruh pekerjaan berjalan di dalam satu transaksi: satu
        /// baris yang ditolak membatalkan seluruhnya, dan nol baris tersimpan sebagian
        /// (<c>AC-112</c>).
        /// </summary>
        public async Task<LabMicrobiologyResultResponse> SetResultAsync(
            Guid examinationId,
            LabMicrobiologyResultRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var examination = await _dbContext.LabExaminations
                .Include(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == examinationId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            // VAL-83. Pemeriksaan yang sudah gugur atau dibatalkan tidak boleh menerima hasil.
            if (examination.ExaminationStatus is LabExaminationStatus.Voided or LabExaminationStatus.Cancelled)
            {
                throw new LabMicrobiologyResultValidationException(
                    "Pemeriksaan ini sudah tidak berjalan, hasilnya tidak dapat diisi.");
            }

            // VAL-120. WAJIB di sini, sebelum ReplaceIsolatesAsync menandai isolat lama untuk
            // dihapus (AC-225 jalur setengah jalan): hasil Final dengan satu isolat dan dua
            // belas baris antibiogram harus tetap persis begitu sesudah permintaannya ditolak.
            LabExaminationService.EnsureResultNotFinalized(examination);

            // VAL-103. Kultur steril adalah hasil yang sah dan nol isolat — tetapi status
            // temuannya tetap wajib, sebab itulah yang menyatakan hasilnya.
            if (request.MicrobiologyFinding is null)
            {
                throw new LabMicrobiologyResultValidationException(
                    "Status temuan wajib dipilih sebelum hasil disimpan.");
            }

            // VAL-118. Hanya pemeriksaan yang SUDAH DIPROFILKAN TEGAS sebagai tidak memakai
            // set bakteri yang ditolak. Pemeriksaan yang belum diprofilkan tetap diterima —
            // tabel pemetaan dimulai kosong, dan menolak seluruhnya sampai kepala instalasi
            // selesai memetakan berarti halaman hasil lahir dalam keadaan tidak dapat dipakai.
            // Alasan yang sama dipakai jalur jatuh LAB-DEC-111.
            if (request.Isolates.Count > 0 &&
                !await _profileService.UsesSusceptibilitySetAsync(examination.ProcedureId, cancellationToken))
            {
                throw new LabMicrobiologyResultValidationException(
                    "Pemeriksaan ini tidak memakai set bakteri.");
            }

            var now = DateTime.UtcNow;
            var examinedAt = request.ExaminedAt ?? now;

            // VAL-89.
            if (examinedAt > now)
            {
                throw new LabMicrobiologyResultValidationException(
                    "Waktu pemeriksaan tidak boleh melewati waktu sekarang.");
            }

            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await ReplaceIsolatesAsync(examination, request, now, actorUserId, cancellationToken);

                examination.MicrobiologyFinding = request.MicrobiologyFinding;
                examination.ResultQualifier = request.ResultQualifier;
                examination.CultureType = request.CultureType;
                examination.SusceptibilityMethod = request.SusceptibilityMethod;
                examination.ExaminedAt = examinedAt;
                examination.ResultEnteredAt = now;

                // Guid.Empty adalah pelaku yang tidak pernah ada, dan kolom ini nol ber-foreign
                // key sehingga database TIDAK akan menolaknya.
                examination.ResultEnteredByUserId = actorUserId == Guid.Empty ? null : actorUserId;

                examination.UpdateDateTime = now;
                examination.UpdateBy = actorUserId;
                examination.Version += 1;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new LabExaminationConflictException(LabExaminationService.ResultConcurrencyMessage);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.SetMicrobiologyResult",
                "Hasil Mikrobiologi diisi.",
                new
                {
                    examination.Id,
                    examination.LabOrderId,
                    Finding = examination.MicrobiologyFinding?.ToString(),
                    JumlahIsolat = request.Isolates.Count
                });

            return await GetResultAsync(examinationId, cancellationToken);
        }

        private async Task ReplaceIsolatesAsync(
            LabExamination examination,
            LabMicrobiologyResultRequest request,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            // AC-114. PUT MENGGANTI, bukan menambah. Isi lama dibuang lebih dulu supaya
            // pengiriman berulang menghasilkan keadaan yang sama — pola idempoten yang sama
            // dengan PUT /result pada S4a.
            var isolatLama = await _dbContext.LabMicrobiologyIsolates
                .Include(x => x.Susceptibilities)
                .Where(x => x.LabExaminationId == examination.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var lama in isolatLama)
            {
                _dbContext.LabIsolateSusceptibilities.RemoveRange(lama.Susceptibilities);
                _dbContext.LabMicrobiologyIsolates.Remove(lama);
            }

            if (request.Isolates.Count == 0)
            {
                // AC-111. Nol pertumbuhan adalah hasil yang SAH — biakan yang tidak
                // menumbuhkan apa pun menyingkirkan dugaan infeksi bakteri.
                return;
            }

            var organismIds = request.Isolates.Select(x => x.LabOrganismId).Distinct().ToList();

            var organismAktif = await _dbContext.LabOrganisms
                .AsNoTracking()
                .Where(x => organismIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .ToDictionaryAsync(x => x.Id, x => x.OrganismName, cancellationToken);

            var antibioticIds = request.Isolates
                .SelectMany(x => x.Susceptibilities)
                .Select(x => x.LabAntibioticId)
                .Distinct()
                .ToList();

            var antibiotikAktif = await _dbContext.LabAntibiotics
                .AsNoTracking()
                .Where(x => antibioticIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .ToDictionaryAsync(x => x.Id, x => new { x.AntibioticName, x.DiscContentUg }, cancellationToken);

            var satuanSah = await _dbContext.MstMeasurements
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsForLaboratory)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var isolatRequest in request.Isolates)
            {
                // VAL-85. AC-113: penolakan berlaku pada baris BARU. Isolat lama yang
                // menunjuk organisme nonaktif tetap terbaca utuh, sebab pembacaan nol
                // menyaring IsActive dan nama sudah tersimpan sebagai snapshot (INV-31).
                if (!organismAktif.TryGetValue(isolatRequest.LabOrganismId, out var organismName))
                {
                    throw new LabMicrobiologyResultValidationException(
                        "Organisme ini sudah tidak dipakai lagi. Pilih dari daftar yang tersedia.");
                }

                // VAL-117.
                if (!isolatRequest.IsSusceptibilityTested && isolatRequest.Susceptibilities.Count > 0)
                {
                    throw new LabMicrobiologyResultValidationException(
                        $"Kuman {organismName} ditandai tidak diuji, jadi tidak boleh punya baris antibiotik.");
                }

                var isolat = new LabMicrobiologyIsolate
                {
                    LabExaminationId = examination.Id,
                    LabOrganismId = isolatRequest.LabOrganismId,
                    OrganismNameSnapshot = organismName,
                    IsSusceptibilityTested = isolatRequest.IsSusceptibilityTested,
                    Note = Normalize(isolatRequest.Note),
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.LabMicrobiologyIsolates.Add(isolat);

                var breakpoints = await _interpreter.LoadBreakpointsAsync(
                    isolatRequest.LabOrganismId, cancellationToken);

                var antibiotikTerpakai = new HashSet<Guid>();

                foreach (var kepekaanRequest in isolatRequest.Susceptibilities)
                {
                    // VAL-87. AC-112.
                    if (!antibiotikTerpakai.Add(kepekaanRequest.LabAntibioticId))
                    {
                        throw new LabMicrobiologyResultValidationException(
                            "Antibiotik ini sudah diuji pada kuman tersebut. Satu antibiotik cukup sekali.");
                    }

                    // VAL-86.
                    if (!antibiotikAktif.TryGetValue(kepekaanRequest.LabAntibioticId, out var antibiotik))
                    {
                        throw new LabMicrobiologyResultValidationException(
                            "Antibiotik ini sudah tidak ada pada panel uji. Pilih dari daftar yang tersedia.");
                    }

                    // VAL-116. Nol boleh negatif; nilai 0 DITERIMA.
                    if (kepekaanRequest.ZoneDiameterMm is < 0)
                    {
                        throw new LabMicrobiologyResultValidationException(
                            "Lebar zona tidak boleh kurang dari nol.");
                    }

                    // VAL-112.
                    if (kepekaanRequest.Concentration.HasValue && kepekaanRequest.ConcentrationUnitId is null)
                    {
                        throw new LabMicrobiologyResultValidationException(
                            $"Pilih satuan untuk nilai kadar {antibiotik.AntibioticName}.");
                    }

                    if (kepekaanRequest.ConcentrationUnitId.HasValue &&
                        !satuanSah.Contains(kepekaanRequest.ConcentrationUnitId.Value))
                    {
                        throw new LabMicrobiologyResultValidationException(
                            "Satuan itu tidak berlaku untuk pemeriksaan laboratorium.");
                    }

                    breakpoints.TryGetValue(kepekaanRequest.LabAntibioticId, out var breakpointValue);

                    LabBreakpointSnapshot? breakpoint =
                        breakpoints.ContainsKey(kepekaanRequest.LabAntibioticId) ? breakpointValue : null;

                    // VAL-113 dan VAL-114 ditegakkan penghitung (BE-LAB-61).
                    var verdict = LabSusceptibilityInterpreter.Resolve(
                        kepekaanRequest.ZoneDiameterMm,
                        breakpoint,
                        kepekaanRequest.Result,
                        kepekaanRequest.ResultOverrideReason,
                        antibiotik.AntibioticName);

                    _dbContext.LabIsolateSusceptibilities.Add(new LabIsolateSusceptibility
                    {
                        LabMicrobiologyIsolateId = isolat.Id,
                        LabAntibioticId = kepekaanRequest.LabAntibioticId,
                        AntibioticNameSnapshot = antibiotik.AntibioticName,
                        Concentration = kepekaanRequest.Concentration,
                        ConcentrationUnitId = kepekaanRequest.ConcentrationUnitId,
                        DiscContentUgSnapshot = breakpoint?.DiscContentUg ?? antibiotik.DiscContentUg,
                        BreakpointLowerMmSnapshot = breakpoint?.LowerMm,
                        BreakpointUpperMmSnapshot = breakpoint?.UpperMm,
                        ZoneDiameterMm = kepekaanRequest.ZoneDiameterMm,
                        ComputedResult = verdict.ComputedResult,
                        Result = verdict.Result,
                        IsResultOverridden = verdict.IsResultOverridden,
                        ResultOverrideReason = verdict.IsResultOverridden
                            ? Normalize(kepekaanRequest.ResultOverrideReason)
                            : null,
                        Note = Normalize(kepekaanRequest.Note),
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                }
            }
        }

        /// <summary>
        /// Membaca hasil Mikrobiologi utuh.
        ///
        /// <b>Pembacaan nol menyaring <c>IsActive</c></b> pada organisme maupun antibiotik —
        /// itulah yang membuat hasil lama tetap terbaca utuh sesudah data induknya
        /// dinonaktifkan (<c>AC-113</c>, <c>INV-31</c>). Namanya pun sudah tersimpan sebagai
        /// snapshot, sehingga ia nol bergantung pada baris induknya.
        /// </summary>
        public async Task<LabMicrobiologyResultResponse> GetResultAsync(
            Guid examinationId,
            CancellationToken cancellationToken = default)
        {
            // LabOrder dan Specimen ikut dimuat karena ruas turunan r27 22.3 bersumber pada
            // keduanya — nomor cetak dan disiplin dari pesanan, waktu pengambilan serta
            // penerimaan fisik dari wadah. BE-LAB-54 sudah sekali gagal karena penunjuk
            // induknya tidak dimuat lalu jatuh ke nilai kosong tanpa galat.
            var examination = await _dbContext.LabExaminations
                .AsNoTracking()
                .Include(x => x.Procedure)
                .Include(x => x.LabOrder)
                .Include(x => x.Specimen)
                .FirstOrDefaultAsync(x => x.Id == examinationId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            var isolates = await _dbContext.LabMicrobiologyIsolates
                .AsNoTracking()
                .Include(x => x.Susceptibilities.Where(s => !s.IsDelete))
                .ThenInclude(s => s.ConcentrationUnit)
                .Where(x => x.LabExaminationId == examinationId && !x.IsDelete)
                .OrderBy(x => x.OrganismNameSnapshot)
                .ToListAsync(cancellationToken);

            var response = new LabMicrobiologyResultResponse
            {
                LabExaminationId = examination.Id,
                LabOrderId = examination.LabOrderId,
                ProcedureName = examination.ProcedureNameSnapshot ?? examination.Procedure?.ProcedureName,
                MicrobiologyFinding = examination.MicrobiologyFinding,
                ResultQualifier = examination.ResultQualifier,
                CultureType = examination.CultureType,
                SusceptibilityMethod = examination.SusceptibilityMethod,
                ExaminedAt = examination.ExaminedAt,
                ResultEnteredAt = examination.ResultEnteredAt,
                ResultEnteredByUserId = examination.ResultEnteredByUserId,
                IsFinalized = examination.FinalizedAt is not null,

                // LAB-DEC-096. Waktu efektif menjawab kapan bahan meninggalkan TUBUH PASIEN,
                // bukan kapan ia sampai di laboratorium — lihat PrintReceivedAt di bawah.
                EffectiveAt = examination.Specimen?.CollectedAt,
                IssuedAt = examination.FinalizedAt,

                ReopenCount = examination.ReopenCount,
                IsConsulted = examination.ConsultedAt is not null,
                ConsultedToName = examination.ConsultedToName,
                ConsultedAt = examination.ConsultedAt,

                // LAB-DEC-117 dan LAB-DEC-118 — ruas cetak.
                LabReportNumber = examination.LabOrder?.LabReportNumber,
                PrintReceivedAt = examination.Specimen?.PhysicallyReceivedAt,
                PrintCompletedAt = examination.FinalizedAt,

                // r35 30.3 — ruas pengesah dan keadaan, bentuk dan arti sama dengan halaman hasil
                // Patologi Klinik (r34 29.3). Keadaan dari turunan yang sama (BE-LAB-76), bukan
                // rumus kedua. Rilis dinyatakan, bukan disimpulkan pemanggil (LAB-DEC-097).
                ResultStatus = LabExaminationService.DeriveResultStatus(examination).ToString(),
                ValidatedAt = examination.ValidatedAt,
                ValidatedByUserId = examination.ValidatedByUserId,
                ValidatedByPositionName = examination.ValidatedByPositionNameSnapshot,
                ReleasedAt = examination.ReleasedAt,
                ReleasedByUserId = examination.ReleasedByUserId,
                ReleasedByPositionName = examination.ReleasedByPositionNameSnapshot,
                IsReleased = examination.ReleasedAt is not null
            };

            // LAB-DEC-120. Pengesah adalah pemvalidasi dan perilis yang TERCATAT pada pemeriksaan
            // ini — tidak pernah pencetak atau penulis hasil. Kosong sampai tindakannya terjadi
            // (AC-183). Ketiga nama dibaca dalam SATU kueri (BE-LAB-79).
            var nama = await ReadActorNamesAsync(examination, cancellationToken);

            response.AnalystName = nama.Analyst;
            response.ValidatedByName = examination.ValidatedAt is not null ? nama.Validator : null;
            response.AuthorizingOfficerName = examination.ReleasedAt is not null ? nama.Releaser : null;
            response.ValidationExceptionMarker = LabExaminationService.ValidationExceptionMarker(
                examination.ValidationExceptionReasonId, response.ValidatedByName, examination.ValidationExceptionReasonNameSnapshot);
            response.ReleaseExceptionMarker = LabExaminationService.ReleaseExceptionMarker(
                examination.ReleaseExceptionReasonId, response.AuthorizingOfficerName, examination.ReleaseExceptionReasonNameSnapshot);

            response.UsesSusceptibilitySet = await _profileService.UsesSusceptibilitySetAsync(
                examination.ProcedureId, cancellationToken);

            await ApplyDisciplineSettingAsync(response, examination.LabOrder?.Discipline, cancellationToken);

            // Seluruh aturan kritis ditarik SEKALI, lalu dicocokkan di memori terhadap setiap
            // baris antibiogram — satu antibiogram lazim memuat dua puluh baris lebih.
            var ruleSet = await _criticalRuleService.LoadRuleSetAsync(cancellationToken);

            response.CriticalRuleAvailable = ruleSet.Available;

            foreach (var isolat in isolates)
            {
                response.Isolates.Add(new LabMicrobiologyIsolateResponse
                {
                    Id = isolat.Id,
                    LabOrganismId = isolat.LabOrganismId,
                    OrganismName = isolat.OrganismNameSnapshot,
                    IsSusceptibilityTested = isolat.IsSusceptibilityTested,
                    BreakpointAvailable = await _interpreter.HasAnyBreakpointAsync(
                        isolat.LabOrganismId, cancellationToken),
                    Note = isolat.Note,
                    Susceptibilities = [.. isolat.Susceptibilities
                        .OrderBy(s => s.AntibioticNameSnapshot)
                        .Select(s => new LabIsolateSusceptibilityResponse
                        {
                            Id = s.Id,
                            LabAntibioticId = s.LabAntibioticId,
                            AntibioticName = s.AntibioticNameSnapshot,
                            DiscContentUg = s.DiscContentUgSnapshot,
                            BreakpointLowerMm = s.BreakpointLowerMmSnapshot,
                            BreakpointUpperMm = s.BreakpointUpperMmSnapshot,
                            Concentration = s.Concentration,
                            ConcentrationUnitId = s.ConcentrationUnitId,
                            ConcentrationUnitSymbol = s.ConcentrationUnit?.MeasurementSymbol,
                            ZoneDiameterMm = s.ZoneDiameterMm,
                            ComputedResult = s.ComputedResult,
                            Result = s.Result,
                            IsResultOverridden = s.IsResultOverridden,

                            // Dihitung saat dibaca, nol disimpan: aturan kritis dapat berubah,
                            // dan nilai tersimpan akan membekukan penilaian lama sebagai
                            // kalau-kalau fakta.
                            IsCritical = ruleSet.IsCritical(isolat.LabOrganismId, s.LabAntibioticId, s.Result),

                            ResultOverrideReason = s.ResultOverrideReason,
                            Note = s.Note
                        })]
                });
            }

            return response;
        }

        /// <summary>
        /// Nama analis (<c>LAB-DEC-105</c>, <c>AC-168</c>), pemvalidasi, dan perilis — dalam
        /// <b>satu</b> kueri, dilewati bila tidak ada satu pun pelaku yang tercatat.
        ///
        /// <para>
        /// Nama analis tetap <c>DisplayName</c>, seperti sebelum <c>BE-LAB-79</c>. Nama pemvalidasi
        /// dan perilis memakai rumus yang sama dengan lembar hasil Patologi Klinik
        /// (<c>BE-LAB-76</c>) — satu orang tidak terbaca dengan dua nama di dua halaman hasil.
        /// </para>
        ///
        /// <para>
        /// <b>Mengembalikan null bagi pengguna yang sudah tidak ada</b> — bukan melempar galat.
        /// Akun yang dihapus tidak boleh membuat hasil lama gagal dibaca; yang hilang hanya namanya,
        /// sedangkan id pelakunya tetap tersimpan.
        /// </para>
        /// </summary>
        private async Task<(string? Analyst, string? Validator, string? Releaser)> ReadActorNamesAsync(
            LabExamination examination,
            CancellationToken cancellationToken)
        {
            static Guid? Sah(Guid? id) => id is Guid g && g != Guid.Empty ? g : null;

            var analis = Sah(examination.ResultEnteredByUserId);
            var pemvalidasi = examination.ValidatedAt is not null ? Sah(examination.ValidatedByUserId) : null;
            var perilis = examination.ReleasedAt is not null ? Sah(examination.ReleasedByUserId) : null;

            var ids = new[] { analis, pemvalidasi, perilis }
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return (null, null, null);
            }

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            return (
                analis is Guid a && users.TryGetValue(a, out var ua) ? ua.DisplayName : null,
                pemvalidasi is Guid v && users.TryGetValue(v, out var uv) ? uv.Name : null,
                perilis is Guid r && users.TryGetValue(r, out var ur) ? ur.Name : null);
        }

        /// <summary>
        /// Menempelkan label konsultan, nama konsultan, dan kalimat baku milik disiplin
        /// pesanan (<c>LAB-DEC-119</c>, <c>LAB-DEC-127</c>).
        ///
        /// <b>Pesanan tanpa disiplin dibiarkan tanpa ketiganya.</b> Pesanan berdisiplin kosong
        /// memang sah (<c>AC-85</c>), dan menebak disiplinnya di sini berarti mencetak footer
        /// milik disiplin lain.
        /// </summary>
        private async Task ApplyDisciplineSettingAsync(
            LabMicrobiologyResultResponse response,
            LabDiscipline? discipline,
            CancellationToken cancellationToken)
        {
            if (discipline is null) return;

            var setting = await _dbContext.LabDisciplineSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Discipline == discipline.Value && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (setting is null) return;

            response.ConsultantLabel = setting.ConsultantLabel;
            response.ConsultantName = setting.ConsultantName;
            response.StandingNote = setting.StandingNote;
        }

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>Pelanggaran aturan isi hasil Mikrobiologi. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabMicrobiologyResultValidationException(string message) : Exception(message);
}
