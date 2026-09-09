using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan pemeriksaan golongan darah Bank Darah.
    /// Controller tidak menyentuh <c>ApplicationDbContext</c> sendiri (<c>QBE-SVC-001</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Empat aturan melekat di sini, dan ketiga yang pertama adalah aturan keselamatan klinis —
    /// bukan preferensi arsitektur.
    /// </para>
    ///
    /// <para>
    /// <b>1. Hasil yang belum tervalidasi tidak pernah menjadi golongan darah sah.</b> Hanya
    /// pemeriksaan berstatus <c>Validated</c> yang boleh membawa <c>IsValidResult</c>
    /// (<c>BD-DOM-09</c>).
    /// </para>
    ///
    /// <para>
    /// <b>2. Sistem tidak pernah memutuskan hasil mana yang benar.</b> Ketika dua hasil
    /// tervalidasi berselisih, keduanya ditahan dan pasien kehilangan golongan darah sah
    /// (<c>BD-XINV-04</c>). Tidak ada kode di berkas ini yang memilih hasil terbaru, hasil
    /// mayoritas, atau hasil mana pun — penutupannya menuntut manusia yang menyebut satu
    /// pemeriksaan ulang (<c>DEC-BD-031</c>, <c>INV-BD-022</c>).
    /// </para>
    ///
    /// <para>
    /// <b>3. Hasil tervalidasi tidak pernah ditimpa.</b> Yang berubah saat konflik dan
    /// penyelesaiannya hanyalah penanda <c>IsValidResult</c> dan <c>IsConflictHeld</c>; nilai
    /// <c>AboRhesusResult</c> setiap pemeriksaan tetap seperti saat dicatat, sehingga seluruh
    /// riwayat tetap terbaca (<c>AC-BD-036</c>, <c>AC-BD-079</c>).
    /// </para>
    ///
    /// <para>
    /// <b>4. Kewenangan bukan urusan berkas ini.</b> Pemisahan validasi rutin dari penyelesaian
    /// konflik (<c>DEC-BD-039</c>) ditegakkan dua butir hak akses berbeda pada controller —
    /// <c>BloodGroupExam : Validate</c> dan <c>BloodGroupExam : ResolveConflict</c> — yang
    /// diberikan admin lewat layar Akses Role. Tidak ada satu pun pemeriksaan nama peran,
    /// nama jabatan, atau <c>UserType</c> di sini, dan tidak boleh ada.
    /// </para>
    /// </remarks>
    public class BbkBloodGroupExamService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private const string NotFoundMessage =
            "Pemeriksaan golongan darah tidak ditemukan atau sudah dihapus.";

        private readonly ApplicationDbContext _dbContext;

        public BbkBloodGroupExamService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =================================================================
        // Pembacaan
        // =================================================================

        public async Task<PagedResult<BloodGroupExamListDto>> GetPagedAsync(
            string? search,
            Guid? patientId,
            BbkBloodGroupExamStatus? examStatus,
            bool? isConflictHeld,
            bool? isValidResult,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BaseQuery();

            if (patientId.HasValue)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (examStatus.HasValue)
                query = query.Where(x => x.ExamStatus == examStatus.Value);

            if (isConflictHeld.HasValue)
                query = query.Where(x => x.IsConflictHeld == isConflictHeld.Value);

            if (isValidResult.HasValue)
                query = query.Where(x => x.IsValidResult == isValidResult.Value);

            // Pencarian menyasar identifier sampel: itulah satu-satunya teks yang dipegang
            // petugas di tangan ketika mencari pemeriksaan.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.Samples.Any(s => s.SampleIdentifier.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySort(query, sortBy, sortDirection);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodGroupExamListDto
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    AboRhesusResult = x.AboRhesusResult,
                    ExamStatus = x.ExamStatus,
                    IsValidResult = x.IsValidResult,
                    IsConflictHeld = x.IsConflictHeld,
                    SampleIdentifier = x.Samples
                        .OrderBy(s => s.TakenAt)
                        .Select(s => s.SampleIdentifier)
                        .FirstOrDefault(),
                    TakenAt = x.Samples
                        .OrderBy(s => s.TakenAt)
                        .Select(s => (DateTime?)s.TakenAt)
                        .FirstOrDefault(),
                    ValidatedAt = x.ValidatedAt,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.ExamStatusLabel = LabelOf(item.ExamStatus);
                item.AboRhesusResultLabel = LabelOf(item.AboRhesusResult);
            }

            return new PagedResult<BloodGroupExamListDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<BloodGroupExamSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await BaseQuery()
                .Select(x => new { x.PatientId, x.ExamStatus, x.IsConflictHeld })
                .ToListAsync(cancellationToken);

            return new BloodGroupExamSummaryResponse
            {
                TotalExam = rows.Count,
                SampleTakenExam = rows.Count(x => x.ExamStatus == BbkBloodGroupExamStatus.SampleTaken),
                ResultRecordedExam = rows.Count(x => x.ExamStatus == BbkBloodGroupExamStatus.ResultRecorded),
                ValidatedExam = rows.Count(x => x.ExamStatus == BbkBloodGroupExamStatus.Validated),
                ConflictHeldExam = rows.Count(x => x.IsConflictHeld),
                PatientWithHeldConflict = rows
                    .Where(x => x.IsConflictHeld)
                    .Select(x => x.PatientId)
                    .Distinct()
                    .Count()
            };
        }

        public async Task<BloodGroupExamDetailDto?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery()
                .Include(x => x.Samples)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            return entity == null ? null : ToDetail(entity);
        }

        /// <summary>
        /// Menjawab golongan darah sah pasien, atau menyatakan bahwa hasilnya sedang
        /// bertentangan (<c>BD-DOM-21</c>).
        /// </summary>
        /// <remarks>
        /// <b>Inilah pintu yang dipakai seluruh gerbang klinis Bank Darah.</b> Jawabannya
        /// dihitung saat ditanya, tidak pernah dibaca dari kolom yang disalin, dan tidak pernah
        /// menyentuh <c>MstPatient.BloodType</c> (<c>INV-BD-014</c>).
        ///
        /// Ketika pasien sedang menahan perbedaan, <c>BloodType</c> dipulangkan kosong dan
        /// <c>IsUsableForClinicalDecision</c> bernilai salah — dua lapis yang menyatakan hal
        /// sama, supaya pemanggil yang lalai memeriksa salah satunya tetap tidak mendapat nilai
        /// yang seolah sah (<c>VAL-BD-034</c>).
        /// </remarks>
        public async Task<ValidBloodGroupDto> GetValidBloodGroupAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var exams = await BaseQuery()
                .Where(x => x.PatientId == patientId)
                .Select(x => new
                {
                    x.Id,
                    x.AboRhesusResult,
                    x.IsValidResult,
                    x.IsConflictHeld,
                    x.ValidatedAt
                })
                .ToListAsync(cancellationToken);

            var conflicting = exams.Where(x => x.IsConflictHeld).ToList();

            if (conflicting.Count > 0)
            {
                return new ValidBloodGroupDto
                {
                    PatientId = patientId,
                    BloodType = null,
                    SourceExamId = null,
                    IsConflictHeld = true,
                    ConflictingExamIds = conflicting.Select(x => x.Id).ToList(),
                    IsUsableForClinicalDecision = false,
                    Message =
                        "Golongan darah pasien ini sedang bertentangan dan ditahan. " +
                        "Selesaikan perbedaannya lebih dulu."
                };
            }

            var valid = exams.FirstOrDefault(x => x.IsValidResult);

            if (valid == null)
            {
                return new ValidBloodGroupDto
                {
                    PatientId = patientId,
                    BloodType = null,
                    IsConflictHeld = false,
                    IsUsableForClinicalDecision = false,
                    Message = "Pasien ini belum punya hasil golongan darah yang tervalidasi."
                };
            }

            return new ValidBloodGroupDto
            {
                PatientId = patientId,
                BloodType = valid.AboRhesusResult,
                BloodTypeLabel = LabelOf(valid.AboRhesusResult),
                SourceExamId = valid.Id,
                ValidatedAt = valid.ValidatedAt,
                IsConflictHeld = false,
                IsUsableForClinicalDecision = true,
                Message = "Golongan darah sah pasien berhasil diambil."
            };
        }

        // =================================================================
        // Perubahan
        // =================================================================

        /// <summary>Mencatat pengambilan sampel dan membuka pemeriksaan berstatus <c>SampleTaken</c>.</summary>
        public async Task<BloodGroupExamResult> RecordSampleAsync(
            RecordSampleRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodGroupExamOutcome.Invalid, ActorUnknownMessage);

            if (request.PatientId == Guid.Empty)
                return Failed(BloodGroupExamOutcome.Invalid, "Pasien wajib dipilih.");

            var sampleIdentifier = NormalizeIdentifier(request.SampleIdentifier);

            if (string.IsNullOrWhiteSpace(sampleIdentifier))
                return Failed(BloodGroupExamOutcome.Invalid, "Identifier sampel wajib diisi.");

            // "Pasien sah" pada matriks perpindahan status. Pemeriksaan atas pasien yang tidak
            // ada menghasilkan hasil golongan darah yang tidak pernah dapat dibaca siapa pun.
            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete, cancellationToken);

            if (!patientExists)
                return Failed(BloodGroupExamOutcome.Invalid, "Pasien tidak ditemukan atau sudah dihapus.");

            var takenAt = request.TakenAt ?? DateTime.UtcNow;

            if (takenAt > DateTime.UtcNow)
            {
                return Failed(
                    BloodGroupExamOutcome.Invalid,
                    "Waktu pengambilan sampel tidak boleh melewati waktu sekarang.");
            }

            if (await SampleIdentifierIsUsedAsync(sampleIdentifier, cancellationToken))
            {
                return Failed(
                    BloodGroupExamOutcome.DuplicateIdentity,
                    "Identifier sampel itu sudah dipakai. Gunakan identifier lain.");
            }

            var now = DateTime.UtcNow;

            var exam = new BbkBloodGroupExam
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                ExamStatus = BbkBloodGroupExamStatus.SampleTaken,
                IsValidResult = false,
                IsConflictHeld = false,
                Version = 0,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            exam.Samples.Add(new BbkBloodGroupSample
            {
                Id = Guid.NewGuid(),
                BloodGroupExamId = exam.Id,
                SampleIdentifier = sampleIdentifier,
                TakenByUserId = actorUserId,
                TakenAt = takenAt,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            _dbContext.Set<BbkBloodGroupExam>().Add(exam);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Succeeded(exam, "Pengambilan sampel golongan darah berhasil dicatat.");
        }

        /// <summary>
        /// Mencatat hasil ABO dan Rhesus pada pemeriksaan yang sampelnya sudah diambil.
        /// </summary>
        /// <remarks>
        /// <b>Pemeriksa dan waktu pemeriksaan tidak diterima dari pemanggil</b> — keduanya
        /// diturunkan dari pengguna terautentikasi dan jam server. Itulah cara <c>VAL-BD-030</c>
        /// ditegakkan tanpa dapat dilewati: bukan dengan memeriksa apakah pemanggil mengisi
        /// keduanya, melainkan dengan tidak pernah memberi pemanggil kesempatan mengosongkannya.
        /// </remarks>
        public async Task<BloodGroupExamResult> RecordResultAsync(
            Guid id,
            RecordResultRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodGroupExamOutcome.Invalid, ActorUnknownMessage);

            if (request.AboRhesusResult == null || !IsRecordableResult(request.AboRhesusResult.Value))
            {
                return Failed(
                    BloodGroupExamOutcome.Invalid,
                    "Hasil ABO dan Rhesus wajib diisi dengan golongan darah yang sebenarnya.");
            }

            var exam = await TrackedAsync(id, cancellationToken);

            if (exam == null)
                return Failed(BloodGroupExamOutcome.NotFound, NotFoundMessage);

            if (exam.ExamStatus != BbkBloodGroupExamStatus.SampleTaken)
            {
                return Failed(
                    BloodGroupExamOutcome.NotAllowedByState,
                    exam.ExamStatus == BbkBloodGroupExamStatus.Validated
                        ? "Hasil yang sudah tervalidasi tidak pernah ditimpa. Catat pemeriksaan ulang bila hasilnya perlu dikoreksi."
                        : "Hasil pemeriksaan ini sudah tercatat. Catat pemeriksaan ulang bila hasilnya perlu dikoreksi.");
            }

            var now = DateTime.UtcNow;

            exam.AboRhesusResult = request.AboRhesusResult;
            exam.ExamStatus = BbkBloodGroupExamStatus.ResultRecorded;
            exam.ExaminedByUserId = actorUserId;
            exam.ExaminedAt = now;
            exam.Version++;
            exam.UpdateDateTime = now;
            exam.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Succeeded(exam, "Hasil golongan darah berhasil dicatat.");
        }

        /// <summary>
        /// Memvalidasi hasil rutin, lalu menilai apakah hasil itu berselisih dengan hasil sah
        /// pasien yang berlaku (<c>BD-XINV-04</c>).
        /// </summary>
        /// <remarks>
        /// <para>Empat keadaan yang mungkin, dan hanya empat:</para>
        /// <list type="number">
        /// <item>
        /// <b>Pasien sedang menahan perbedaan.</b> Hasil baru ini menjadi kandidat pemeriksaan
        /// ulang: ia tervalidasi, tetapi belum berlaku dan konfliknya <b>tetap tertahan</b>.
        /// Hanya validator klinis yang dapat menutupnya (<c>DEC-BD-031</c>).
        /// </item>
        /// <item>
        /// <b>Pasien belum punya hasil sah.</b> Hasil ini langsung berlaku.
        /// </item>
        /// <item>
        /// <b>Hasil sama dengan hasil sah sebelumnya.</b> Hasil terbaru berlaku, tanpa penahanan
        /// apa pun (<c>AC-BD-035</c>).
        /// </item>
        /// <item>
        /// <b>Hasil berbeda dari hasil sah sebelumnya.</b> Keduanya ditahan, pasien tidak punya
        /// hasil sah sama sekali, dan gerbang klinis tertutup (<c>AC-BD-034</c>). Sistem
        /// <b>tidak</b> memilih di antara keduanya.
        /// </item>
        /// </list>
        /// </remarks>
        public async Task<BloodGroupExamResult> ValidateAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodGroupExamOutcome.Invalid, ActorUnknownMessage);

            var exam = await TrackedAsync(id, cancellationToken);

            if (exam == null)
                return Failed(BloodGroupExamOutcome.NotFound, NotFoundMessage);

            if (exam.ExamStatus != BbkBloodGroupExamStatus.ResultRecorded)
            {
                return Failed(
                    BloodGroupExamOutcome.NotAllowedByState,
                    exam.ExamStatus == BbkBloodGroupExamStatus.Validated
                        ? "Hasil pemeriksaan ini sudah divalidasi."
                        : "Hasil pemeriksaan ini belum dicatat, sehingga belum ada yang dapat divalidasi.");
            }

            if (exam.AboRhesusResult == null)
            {
                return Failed(
                    BloodGroupExamOutcome.Invalid,
                    "Hasil golongan darah belum tersimpan pada pemeriksaan ini.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var now = DateTime.UtcNow;

            exam.ExamStatus = BbkBloodGroupExamStatus.Validated;
            exam.ValidatedByUserId = actorUserId;
            exam.ValidatedAt = now;
            exam.Version++;
            exam.UpdateDateTime = now;
            exam.UpdateBy = actorUserId;

            var siblings = await _dbContext.Set<BbkBloodGroupExam>()
                .Where(x => x.PatientId == exam.PatientId && x.Id != exam.Id && !x.IsDelete && !x.IsCancel)
                .ToListAsync(cancellationToken);

            var message = ApplyValidationOutcome(exam, siblings, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Succeeded(exam, message);
        }

        /// <summary>
        /// Menutup keadaan konflik dengan menunjuk satu pemeriksaan ulang tervalidasi
        /// (<c>DEC-BD-031</c> Model C). Dipanggil hanya oleh endpoint yang dijaga butir
        /// <c>BloodGroupExam : ResolveConflict</c>.
        /// </summary>
        /// <remarks>
        /// <b>Tidak ada jalan lain menutup konflik.</b> Metode ini menuntut
        /// <c>ResolvingExamId</c>, dan tidak punya cabang yang memilih hasil tanpa disebut
        /// manusia — tidak berdasarkan yang terbaru, tidak berdasarkan mayoritas
        /// (<c>AC-BD-054</c>). Bahkan validator klinis tidak dapat menutup konflik tanpa
        /// pemeriksaan ulang: wewenang tidak menggantikan prasyarat (<c>AC-BD-080</c>).
        ///
        /// <b>Hasil ulang boleh bernilai ketiga.</b> Nilai yang berbeda dari kedua hasil bentrok
        /// tetap diterima bila validator menyatakannya berlaku (<c>AC-BD-053</c>) — tidak ada
        /// pemeriksaan yang memaksanya cocok dengan salah satu hasil lama.
        /// </remarks>
        public async Task<BloodGroupConflictResolutionResult> ResolveConflictAsync(
            ResolveConflictRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return ResolutionFailed(BloodGroupExamOutcome.Invalid, ActorUnknownMessage);

            if (request.PatientId == Guid.Empty)
                return ResolutionFailed(BloodGroupExamOutcome.Invalid, "Pasien wajib dipilih.");

            // VAL-BD-051 — penyelesaian tanpa menunjuk pemeriksaan ulang ditolak sebelum apa pun
            // dibaca dari database.
            if (request.ResolvingExamId == Guid.Empty)
                return ResolutionFailed(BloodGroupExamOutcome.NotAllowedByState, MustPointToRecheckMessage);

            var patientExams = await _dbContext.Set<BbkBloodGroupExam>()
                .Where(x => x.PatientId == request.PatientId && !x.IsDelete && !x.IsCancel)
                .ToListAsync(cancellationToken);

            var conflicting = patientExams.Where(x => x.IsConflictHeld).ToList();

            if (conflicting.Count == 0)
            {
                return ResolutionFailed(
                    BloodGroupExamOutcome.NotAllowedByState,
                    "Pasien ini sedang tidak menahan perbedaan hasil golongan darah.");
            }

            var resolvingExam = patientExams.FirstOrDefault(x => x.Id == request.ResolvingExamId);

            if (resolvingExam == null)
            {
                return ResolutionFailed(
                    BloodGroupExamOutcome.NotFound,
                    "Pemeriksaan ulang yang ditunjuk tidak ditemukan pada pasien ini.");
            }

            // Pemeriksaan yang sedang bertentangan bukan "pemeriksaan ulang" — menunjuk salah
            // satu pihak konflik sama saja dengan memilih salah satu hasil lama, dan itu persis
            // yang ditolak DEC-BD-031.
            if (resolvingExam.IsConflictHeld)
                return ResolutionFailed(BloodGroupExamOutcome.NotAllowedByState, MustPointToRecheckMessage);

            if (resolvingExam.ExamStatus != BbkBloodGroupExamStatus.Validated)
                return ResolutionFailed(BloodGroupExamOutcome.NotAllowedByState, MustPointToRecheckMessage);

            var reasonCode = NormalizeIdentifier(request.ReasonCode);

            if (string.IsNullOrWhiteSpace(reasonCode))
                return ResolutionFailed(BloodGroupExamOutcome.Invalid, "Alasan penyelesaian wajib diisi.");

            var reasonExists = await _dbContext.Set<MstBloodBankReason>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.ReasonCode.ToUpper() == reasonCode && x.IsActive && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

            if (!reasonExists)
            {
                return ResolutionFailed(
                    BloodGroupExamOutcome.Invalid,
                    "Alasan penyelesaian wajib dipilih dari daftar alasan Bank Darah yang aktif.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var now = DateTime.UtcNow;

            // Seluruh pihak konflik berhenti menahan; nilai hasilnya tidak disentuh sehingga
            // riwayat keduanya tetap terbaca (AC-BD-036).
            foreach (var party in conflicting)
            {
                party.IsConflictHeld = false;
                party.IsValidResult = false;
                party.Version++;
                party.UpdateDateTime = now;
                party.UpdateBy = actorUserId;
            }

            // Penjaga INV-BD-018: tepat satu hasil sah setelah penyelesaian.
            foreach (var other in patientExams.Where(x => x.Id != resolvingExam.Id && x.IsValidResult))
            {
                other.IsValidResult = false;
                other.Version++;
                other.UpdateDateTime = now;
                other.UpdateBy = actorUserId;
            }

            resolvingExam.IsValidResult = true;
            resolvingExam.IsConflictHeld = false;
            resolvingExam.Version++;
            resolvingExam.UpdateDateTime = now;
            resolvingExam.UpdateBy = actorUserId;

            _dbContext.Set<BbkBloodGroupConflictResolution>().Add(new BbkBloodGroupConflictResolution
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                ResolvingExamId = resolvingExam.Id,
                ResolvedByUserId = actorUserId,
                ReasonCode = reasonCode,
                ResolvedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var validBloodGroup = new ValidBloodGroupDto
            {
                PatientId = request.PatientId,
                BloodType = resolvingExam.AboRhesusResult,
                BloodTypeLabel = LabelOf(resolvingExam.AboRhesusResult),
                SourceExamId = resolvingExam.Id,
                ValidatedAt = resolvingExam.ValidatedAt,
                IsConflictHeld = false,
                IsUsableForClinicalDecision = true,
                Message = "Perbedaan hasil golongan darah berhasil diselesaikan."
            };

            return new BloodGroupConflictResolutionResult(
                BloodGroupExamOutcome.Success,
                validBloodGroup,
                "Perbedaan hasil golongan darah berhasil diselesaikan. " +
                "Satu hasil sah kembali berlaku dan seluruh hasil sebelumnya tetap terbaca.");
        }

        // =================================================================
        // Aturan konflik
        // =================================================================

        /// <summary>
        /// Menentukan akibat validasi terhadap hasil sah pasien. Dipisah supaya keempat
        /// keadaannya terbaca berdampingan, bukan tersebar sebagai pemeriksaan yang berjauhan.
        /// </summary>
        private static string ApplyValidationOutcome(
            BbkBloodGroupExam exam,
            List<BbkBloodGroupExam> siblings,
            Guid actorUserId,
            DateTime now)
        {
            var heldSiblings = siblings.Where(x => x.IsConflictHeld).ToList();

            if (heldSiblings.Count > 0)
            {
                exam.IsValidResult = false;
                exam.IsConflictHeld = false;

                return
                    "Hasil pemeriksaan ulang berhasil divalidasi. Perbedaan hasil golongan darah " +
                    "pasien ini masih tertahan sampai validator klinis menyatakan hasil yang berlaku.";
            }

            var currentValid = siblings.FirstOrDefault(x => x.IsValidResult);

            if (currentValid == null)
            {
                exam.IsValidResult = true;
                exam.IsConflictHeld = false;

                return "Hasil golongan darah berhasil divalidasi dan kini berlaku sebagai hasil sah pasien.";
            }

            if (currentValid.AboRhesusResult == exam.AboRhesusResult)
            {
                currentValid.IsValidResult = false;
                currentValid.Version++;
                currentValid.UpdateDateTime = now;
                currentValid.UpdateBy = actorUserId;

                exam.IsValidResult = true;
                exam.IsConflictHeld = false;

                return "Hasil golongan darah berhasil divalidasi dan sama dengan hasil sah sebelumnya.";
            }

            // BD-XINV-04. Sistem tidak memilih: keduanya ditahan sampai validator klinis
            // menyelesaikannya lewat pemeriksaan ulang.
            currentValid.IsValidResult = false;
            currentValid.IsConflictHeld = true;
            currentValid.Version++;
            currentValid.UpdateDateTime = now;
            currentValid.UpdateBy = actorUserId;

            exam.IsValidResult = false;
            exam.IsConflictHeld = true;

            return
                "Hasil golongan darah berhasil divalidasi, tetapi berbeda dari hasil sah sebelumnya. " +
                "Pasien untuk sementara tidak punya golongan darah sah dan gerbang klinis tertahan " +
                "sampai perbedaannya diselesaikan validator klinis.";
        }

        // =================================================================
        // Pemetaan dan metadata
        // =================================================================

        public static BloodGroupExamDetailDto ToDetail(BbkBloodGroupExam entity) => new()
        {
            Id = entity.Id,
            PatientId = entity.PatientId,
            AboRhesusResult = entity.AboRhesusResult,
            AboRhesusResultLabel = LabelOf(entity.AboRhesusResult),
            ExamStatus = entity.ExamStatus,
            ExamStatusLabel = LabelOf(entity.ExamStatus),
            ExaminedByUserId = entity.ExaminedByUserId,
            ExaminedAt = entity.ExaminedAt,
            ValidatedByUserId = entity.ValidatedByUserId,
            ValidatedAt = entity.ValidatedAt,
            IsValidResult = entity.IsValidResult,
            IsConflictHeld = entity.IsConflictHeld,
            Version = entity.Version,
            Samples = entity.Samples
                .OrderBy(x => x.TakenAt)
                .Select(x => new BloodGroupSampleDto
                {
                    Id = x.Id,
                    SampleIdentifier = x.SampleIdentifier,
                    TakenByUserId = x.TakenByUserId,
                    TakenAt = x.TakenAt
                })
                .ToList(),
            AvailableActions = AvailableActionsOf(entity),
            CreateDateTime = entity.CreateDateTime,
            UpdateDateTime = entity.UpdateDateTime
        };

        /// <summary>
        /// Aksi yang layak dicoba pada keadaan sekarang. <b>Kelayakan status saja</b> — bukan
        /// kewenangan, yang tetap dijaga butir hak akses pada endpoint masing-masing.
        /// </summary>
        private static List<string> AvailableActionsOf(BbkBloodGroupExam entity)
        {
            var actions = new List<string>();

            switch (entity.ExamStatus)
            {
                case BbkBloodGroupExamStatus.SampleTaken:
                    actions.Add("RecordResult");
                    break;

                case BbkBloodGroupExamStatus.ResultRecorded:
                    actions.Add("Validate");
                    break;
            }

            // Penyelesaian konflik hidup di layar pemeriksaan, bukan daftar kerja keempat
            // (DEC-BD-033). Ia ditawarkan pada pemeriksaan yang sedang menahan perbedaan.
            if (entity.IsConflictHeld)
                actions.Add("ResolveConflict");

            return actions;
        }

        public static BloodGroupExamFilterMetadataResponse BuildFilterMetadata() => new()
        {
            DefaultFilter = new BloodGroupExamDefaultFilterResponse(),
            ExamStatusOptions = Enum.GetValues<BbkBloodGroupExamStatus>()
                .Select(x => new BloodGroupExamOptionItemResponse
                {
                    Value = (int)x,
                    Label = LabelOf(x)
                })
                .ToList(),
            BloodTypeOptions = Enum.GetValues<BloodType>()
                .Where(IsRecordableResult)
                .Select(x => new BloodGroupExamOptionItemResponse
                {
                    Value = (int)x,
                    Label = LabelOf(x) ?? x.ToString()
                })
                .ToList(),
            SortOptions = new List<BloodGroupExamSortOptionResponse>
            {
                new() { Value = "createDateTime", Label = "Waktu dibuat" },
                new() { Value = "validatedAt", Label = "Waktu validasi" },
                new() { Value = "examStatus", Label = "Status pemeriksaan" }
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        /// <summary>
        /// <c>Unknown</c> dan <c>NotDisclosed</c> menyatakan ketiadaan hasil, bukan hasil
        /// pemeriksaan. Menerimanya akan membuat pasien punya golongan darah sah yang isinya
        /// "tidak diketahui" — persis kekeliruan yang membuat <c>MstPatient.BloodType</c>
        /// ditolak sebagai sumber klinis.
        /// </summary>
        private static bool IsRecordableResult(BloodType value)
            => value != BloodType.Unknown && value != BloodType.NotDisclosed && Enum.IsDefined(value);

        private static string LabelOf(BbkBloodGroupExamStatus status) => status switch
        {
            BbkBloodGroupExamStatus.SampleTaken => "Sampel diambil",
            BbkBloodGroupExamStatus.ResultRecorded => "Hasil tercatat",
            BbkBloodGroupExamStatus.Validated => "Tervalidasi",
            _ => status.ToString()
        };

        private static string? LabelOf(BloodType? value) => value switch
        {
            null => null,
            BloodType.APositive => "A Positif",
            BloodType.ANegative => "A Negatif",
            BloodType.BPositive => "B Positif",
            BloodType.BNegative => "B Negatif",
            BloodType.ABPositive => "AB Positif",
            BloodType.ABNegative => "AB Negatif",
            BloodType.OPositive => "O Positif",
            BloodType.ONegative => "O Negatif",
            BloodType.Unknown => "Tidak diketahui",
            BloodType.NotDisclosed => "Tidak diinformasikan",
            _ => value.ToString()
        };

        // =================================================================
        // Penolong
        // =================================================================

        private const string ActorUnknownMessage =
            "Petugas pelaku tidak dikenali. Masuk kembali lalu ulangi tindakan ini.";

        private const string MustPointToRecheckMessage =
            "Perbedaan hasil hanya dapat diselesaikan setelah ada pemeriksaan ulang yang tervalidasi.";

        private IQueryable<BbkBloodGroupExam> BaseQuery()
            => _dbContext.Set<BbkBloodGroupExam>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private Task<BbkBloodGroupExam?> TrackedAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<BbkBloodGroupExam>()
                .Include(x => x.Samples)
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

        /// <remarks>
        /// Pemeriksaan ini menahan kekeliruan biasa. Penjaga mutlaknya tetap index unik pada
        /// database — lihat <c>BbkBloodGroupSampleConfiguration</c>.
        /// </remarks>
        private Task<bool> SampleIdentifierIsUsedAsync(
            string sampleIdentifier,
            CancellationToken cancellationToken)
            => _dbContext.Set<BbkBloodGroupSample>()
                .AsNoTracking()
                .AnyAsync(
                    x => !x.IsDelete && x.SampleIdentifier.ToUpper() == sampleIdentifier,
                    cancellationToken);

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static IQueryable<BbkBloodGroupExam> ApplySort(
            IQueryable<BbkBloodGroupExam> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "validatedat" => descending
                    ? query.OrderByDescending(x => x.ValidatedAt)
                    : query.OrderBy(x => x.ValidatedAt),
                "examstatus" => descending
                    ? query.OrderByDescending(x => x.ExamStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.ExamStatus).ThenBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static string NormalizeIdentifier(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

        private static BloodGroupExamResult Succeeded(BbkBloodGroupExam entity, string message)
            => new(BloodGroupExamOutcome.Success, entity, message);

        private static BloodGroupExamResult Failed(BloodGroupExamOutcome outcome, string message)
            => new(outcome, null, message);

        private static BloodGroupConflictResolutionResult ResolutionFailed(
            BloodGroupExamOutcome outcome,
            string message)
            => new(outcome, null, message);
    }

    /// <summary>
    /// Hasil satu tindakan pada pemeriksaan golongan darah. Dipetakan ke HTTP status oleh
    /// controller, bukan oleh service.
    /// </summary>
    public enum BloodGroupExamOutcome
    {
        Success = 0,

        /// <summary>Baris tidak ada atau sudah dihapus.</summary>
        NotFound = 1,

        /// <summary>Isian permintaan tidak sah.</summary>
        Invalid = 2,

        /// <summary>Identifier sampel sudah dipakai.</summary>
        DuplicateIdentity = 3,

        /// <summary>
        /// Permintaannya sah dan pelakunya berwenang, tetapi keadaan datanya tidak
        /// memungkinkan — misalnya penyelesaian konflik tanpa pemeriksaan ulang tervalidasi.
        /// </summary>
        NotAllowedByState = 4
    }

    public sealed record BloodGroupExamResult(
        BloodGroupExamOutcome Outcome,
        BbkBloodGroupExam? Entity,
        string Message);

    public sealed record BloodGroupConflictResolutionResult(
        BloodGroupExamOutcome Outcome,
        ValidBloodGroupDto? ValidBloodGroup,
        string Message);
}
