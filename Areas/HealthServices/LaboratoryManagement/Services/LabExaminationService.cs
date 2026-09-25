using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pengelolaan pemeriksaan terpesan (<c>LAB-DEC-024</c>, <c>LAB-DEC-026</c>, BR-20).
    ///
    /// <b>Satu batas yang membentuk seluruh berkas ini:</b> pemeriksaan dibatalkan satu per
    /// satu, sedangkan wadah diputuskan sekaligus. Membatalkan satu pemeriksaan
    /// <b>tidak menyentuh</b> pemeriksaan lain pada wadah yang sama — itu keputusan klinis atas
    /// satu jenis pemeriksaan. Sebaliknya, menolak sebuah wadah menggugurkan seluruh isinya
    /// sekaligus, dan itu pekerjaan <c>BE-LAB-12</c>, bukan di sini. Mencampur keduanya
    /// melanggar <c>VAL-13</c>.
    ///
    /// Karena itu tidak ada satu pun jalur di berkas ini yang mengubah status wadah.
    /// </summary>
    public class LabExaminationService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabExaminationService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        // =================================================================
        // Baca
        // =================================================================

        public async Task<List<LabExaminationResponse>> GetByOrderAsync(
            Guid labOrderId,
            CancellationToken cancellationToken = default)
        {
            await EnsureOrderExistsAsync(labOrderId, cancellationToken);

            return await Project(
                    _dbContext.LabExaminations
                        .AsNoTracking()
                        .Where(x => x.LabOrderId == labOrderId && !x.IsDelete))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<LabExaminationResponse>> GetBySpecimenAsync(
            Guid specimenId,
            CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.LabSpecimens
                .AsNoTracking()
                .AnyAsync(x => x.Id == specimenId && !x.IsDelete, cancellationToken);

            if (!exists)
                throw new KeyNotFoundException("Wadah sampel tidak ditemukan.");

            return await Project(
                    _dbContext.LabExaminations
                        .AsNoTracking()
                        .Where(x => x.SpecimenId == specimenId && !x.IsDelete))
                .ToListAsync(cancellationToken);
        }

        // =================================================================
        // Menambah
        // =================================================================

        /// <summary>
        /// Menambah satu pemeriksaan terpesan dan menautkannya ke wadah penopangnya.
        ///
        /// Harga disalin backend dari tarif yang berlaku saat kejadian, bukan diterima dari
        /// pemanggil. Salinan itulah yang membuat muatan fakta ke Billing dapat direproduksi
        /// persis ketika pengiriman diulang.
        /// </summary>
        public async Task<LabExaminationResponse> AddAsync(
            Guid labOrderId,
            AddLabExaminationRequest request,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.LabOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == labOrderId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pesanan laboratorium tidak ditemukan.");

            if (order.OrderStatus is LabOrderStatus.Cancelled or LabOrderStatus.Completed)
            {
                throw new LabExaminationConflictException(
                    $"Pesanan laboratorium berstatus {order.OrderStatus} tidak dapat menerima pemeriksaan baru.");
            }

            var specimen = await _dbContext.LabSpecimens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SpecimenId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Wadah sampel tidak ditemukan.");

            // Wadah milik pesanan lain tidak boleh menopang pemeriksaan pesanan ini. Tanpa
            // pemeriksaan ini, satu tabung dapat menggantung di bawah dua pesanan sekaligus dan
            // kelayakan tagihnya menjadi tidak dapat ditelusuri.
            if (specimen.LabOrderId != order.Id)
            {
                throw new LabExaminationValidationException(
                    "Wadah yang dipilih bukan milik pesanan ini.");
            }

            // VAL-18. Wadah yang sudah diputuskan tidak boleh bertambah isinya: kelayakan
            // tagihnya sudah terbit, dan menambah pemeriksaan sesudahnya berarti menagihkan
            // sesuatu yang tidak pernah ikut dinilai layak.
            if (specimen.SpecimenStatus is LabSpecimenStatus.Accepted or LabSpecimenStatus.Rejected)
            {
                throw new LabExaminationConflictException(
                    "Wadah ini sudah diputuskan, pemeriksaan baru tidak dapat ditambahkan ke wadah tersebut.");
            }

            // VAL-17.
            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ProcedureId &&
                    x.IsLaboratory &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (procedure == null)
                throw new LabExaminationValidationException("Tindakan yang dipilih bukan pemeriksaan laboratorium.");

            // INV-22 / VAL-46. Pemeriksaan yang disiplinnya tidak sesuai disiplin pesanan
            // ditolak: pesanan Mikrobiologi tidak boleh memuat Hemoglobin, karena yang
            // mengerjakannya orang lain dan daftar kerjanya pun terpisah.
            //
            // Ditegakkan hanya ketika KEDUANYA diketahui. Pesanan peninggalan sebelum kolom
            // disiplin ada, dan katalog yang belum digolongkan, keduanya bernilai kosong —
            // menolaknya akan mematikan pemesanan pada rumah sakit yang penggolongannya belum
            // diisi, padahal yang belum lengkap adalah data induknya, bukan permintaannya.
            if (order.Discipline != null &&
                procedure.LabDiscipline != null &&
                order.Discipline != procedure.LabDiscipline)
            {
                throw new LabExaminationValidationException(
                    $"Pemeriksaan ini bukan bagian dari {order.Discipline}. " +
                    "Buat pesanan terpisah untuk disiplin yang sesuai.");
            }

            // Keunikan wadah dan jenis pemeriksaan (BR-20). Diperiksa di sini supaya pemanggil
            // menerima pesan yang berarti, sementara index unik database tetap menjadi penjaga
            // terakhir bila dua permintaan datang bersamaan.
            var duplicate = await _dbContext.LabExaminations
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.SpecimenId == specimen.Id &&
                    x.ProcedureId == procedure.Id,
                    cancellationToken);

            if (duplicate)
            {
                throw new LabExaminationConflictException(
                    "Pemeriksaan yang sama tidak boleh dimasukkan dua kali dalam satu wadah.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // VAL-20. Tarif yang belum diatur dihentikan di sini, bukan dibiarkan tersimpan
            // sebagai harga kosong yang kelak menjadi tagihan nol tanpa ada yang menyadarinya.
            var tariff = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x =>
                    x.ProcedureId == procedure.Id &&
                    !x.IsDelete &&
                    (x.EffectiveStartDate == null || x.EffectiveStartDate <= now) &&
                    (x.EffectiveEndDate == null || x.EffectiveEndDate >= now))
                .OrderByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (tariff == null)
            {
                throw new LabExaminationValidationException(
                    "Tarif untuk pemeriksaan ini belum diatur. Hubungi bagian data induk.");
            }

            var entity = new LabExamination
            {
                LabOrderId = order.Id,
                SpecimenId = specimen.Id,
                ProcedureId = procedure.Id,
                ProcedureCodeSnapshot = procedure.ProcedureCode,
                ProcedureNameSnapshot = procedure.ProcedureName,
                TariffId = tariff.Id,
                TariffCodeSnapshot = tariff.TariffCode,
                UnitPriceSnapshot = tariff.NormalPrice,
                ExaminationStatus = LabExaminationStatus.Ordered,
                Urgency = LabExaminationUrgency.Routine,
                IsDuplo = false,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabExaminations.Add(entity);

            AppendHistory(
                order,
                entity,
                "Examination.Add",
                fromStatus: null,
                toStatus: LabExaminationStatus.Ordered.ToString(),
                actorUserId: actorUserId,
                occurredAt: now);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabExamination.Add",
                "Menambah pemeriksaan terpesan.",
                new
                {
                    entity.Id,
                    entity.LabOrderId,
                    entity.SpecimenId,
                    entity.ProcedureId,
                    entity.ProcedureCodeSnapshot,
                    ActorUserId = actorUserId
                });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        // =================================================================
        // Membatalkan satu pemeriksaan
        // =================================================================

        /// <summary>
        /// Membatalkan <b>satu</b> pemeriksaan terpesan.
        ///
        /// Pemeriksaan lain pada wadah yang sama tidak disentuh, dan status wadahnya sendiri
        /// tidak berubah. Menggugurkan seluruh isi wadah adalah akibat penolakan wadah, dan itu
        /// pekerjaan <c>BE-LAB-12</c>.
        /// </summary>
        public async Task<LabExaminationResponse> CancelAsync(
            Guid id,
            CancelLabExaminationRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadExaminationAsync(id, cancellationToken);
            var order = await LoadOrderAsync(entity.LabOrderId, cancellationToken);

            // VAL-19. Pemeriksaan yang sudah gugur bersama wadahnya tidak dapat dibatalkan lagi;
            // membiarkannya berarti menimpa sebab yang sebenarnya.
            if (entity.ExaminationStatus == LabExaminationStatus.Voided)
            {
                throw new LabExaminationConflictException(
                    "Pemeriksaan ini sudah gugur karena wadahnya ditolak.");
            }

            if (entity.ExaminationStatus == LabExaminationStatus.Cancelled)
                throw new LabExaminationConflictException("Pemeriksaan ini sudah dibatalkan.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var statusSebelum = entity.ExaminationStatus;
            var alasan = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

            entity.ExaminationStatus = LabExaminationStatus.Cancelled;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.Version += 1;

            AppendHistory(
                order,
                entity,
                "Examination.Cancel",
                statusSebelum.ToString(),
                LabExaminationStatus.Cancelled.ToString(),
                actorUserId,
                now,
                alasan);

            await SaveAsync(cancellationToken);

            // Pembatalan pemeriksaan yang sudah layak tagih punya akibat finansial, dan Billing
            // yang menentukan koreksinya. Yang dikerjakan di sini hanya mencatat sebabnya
            // selengkap mungkin; penerbitan fakta pembatalannya adalah pekerjaan BE-LAB-13.
            await _loggerService.InfoAsync(
                LogCategory,
                "LabExamination.Cancel",
                "Membatalkan satu pemeriksaan terpesan.",
                new
                {
                    entity.Id,
                    entity.LabOrderId,
                    entity.SpecimenId,
                    StatusSebelum = statusSebelum.ToString(),
                    SudahLayakTagih = statusSebelum == LabExaminationStatus.ChargeEligible,
                    Alasan = alasan,
                    ActorUserId = actorUserId
                });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        // =================================================================
        // Kesegeraan dan duplo — LAB-DEC-026, BE-LAB-10
        // =================================================================

        /// <summary>
        /// Menandai satu pemeriksaan sebagai cito, atau mengembalikannya menjadi biasa
        /// (<c>AC-18</c>, <c>AC-39</c>).
        ///
        /// <b>Kesegeraan melekat pada pemeriksaan, bukan pada pesanan.</b> Satu pesanan dapat
        /// memuat Kalium cito dan Kolesterol biasa sekaligus, dan hanya Kalium yang naik ke
        /// urutan atas daftar kerja. Endpoint kesegeraan pada tingkat pesanan sengaja tidak ada
        /// — <c>LAB-DEC-026</c> membatalkannya, dan <c>AC-40</c> menjaga pembatalan itu.
        ///
        /// Dua penjaga yang berlaku di sini:
        /// <c>VAL-03</c> — hanya dokter pemesan yang boleh menandai, karena kesegeraan adalah
        /// penilaian klinis atas pasiennya sendiri, bukan keputusan administratif;
        /// <c>VAL-04</c> — pesanan yang sudah selesai atau dibatalkan tidak dapat diubah
        /// kesegeraannya, sebab tidak ada lagi pekerjaan yang dapat didahulukan.
        /// </summary>
        public async Task<LabExaminationResponse> SetUrgencyAsync(
            Guid id,
            SetLabExaminationUrgencyRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadExaminationAsync(id, cancellationToken);
            var order = await LoadOrderAsync(entity.LabOrderId, cancellationToken);

            // VAL-04 lebih dulu daripada VAL-03. Pesanan yang sudah selesai tidak dapat diubah
            // oleh siapa pun, termasuk dokter pemesannya, sehingga menjawab 409 lebih tepat
            // daripada menjawab 403 kepada orang yang sebenarnya berwenang.
            if (order.OrderStatus is LabOrderStatus.Completed or LabOrderStatus.Cancelled)
            {
                throw new LabExaminationConflictException(
                    "Pesanan ini sudah selesai atau dibatalkan, kesegeraannya tidak dapat diubah lagi.");
            }

            // VAL-03.
            var actorUserId = GetCurrentUserId();

            if (actorUserId == Guid.Empty || actorUserId != order.RequestedByUserId)
            {
                throw new LabExaminationForbiddenException(
                    "Hanya dokter yang membuat pesanan ini yang boleh menandainya cito.");
            }

            EnsureExaminationMasihBerjalan(entity, "kesegeraannya");

            var tujuan = request.IsCito
                ? LabExaminationUrgency.Cito
                : LabExaminationUrgency.Routine;

            // Menyetel nilai yang sudah berlaku bukan sebuah penandaan, sehingga tidak
            // menghasilkan baris riwayat baru. Riwayat harus mencatat perpindahan, bukan
            // jumlah kali tombol ditekan.
            if (entity.Urgency == tujuan)
                return await ReadBackAsync(entity.Id, cancellationToken);

            var now = DateTime.UtcNow;
            var asal = entity.Urgency;

            entity.Urgency = tujuan;
            entity.UrgencyMarkedAt = now;
            entity.UrgencyMarkedByUserId = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version += 1;

            AppendHistory(
                order,
                entity,
                "Examination.SetUrgency",
                asal.ToString(),
                tujuan.ToString(),
                actorUserId,
                now);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabExamination.SetUrgency",
                "Mengubah kesegeraan satu pemeriksaan terpesan.",
                new
                {
                    entity.Id,
                    entity.LabOrderId,
                    entity.SpecimenId,
                    Dari = asal.ToString(),
                    Ke = tujuan.ToString(),
                    ActorUserId = actorUserId
                });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Menandai satu pemeriksaan dikerjakan ganda, atau membatalkan penandaannya
        /// (<c>AC-40</c>).
        ///
        /// Berbeda dari kesegeraan, duplo <b>bukan</b> penilaian klinis dokter pemesan
        /// melainkan keputusan pelaksanaan laboratorium, sehingga <c>VAL-03</c> tidak berlaku
        /// di sini. Yang menjaganya adalah permission <c>LabExamination : Update</c> beserta
        /// satu syarat keadaan: wadah penopangnya belum ditolak, karena bahan yang tidak layak
        /// tidak dapat dikerjakan sekali pun, apalagi dua kali.
        ///
        /// Penanda ini tidak menyentuh salinan tarif. Apakah duplo berdampak pada tarif masih
        /// <c>LAB-OPEN-013</c>.
        /// </summary>
        public async Task<LabExaminationResponse> SetDuploAsync(
            Guid id,
            SetLabExaminationDuploRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadExaminationAsync(id, cancellationToken);
            var order = await LoadOrderAsync(entity.LabOrderId, cancellationToken);

            EnsureExaminationMasihBerjalan(entity, "penanda duplonya");

            var wadahDitolak = await _dbContext.LabSpecimens
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == entity.SpecimenId && x.SpecimenStatus == LabSpecimenStatus.Rejected,
                    cancellationToken);

            if (wadahDitolak)
            {
                throw new LabExaminationConflictException(
                    "Wadah penopang pemeriksaan ini sudah ditolak, penanda duplo tidak dapat diubah lagi.");
            }

            if (entity.IsDuplo == request.IsDuplo)
                return await ReadBackAsync(entity.Id, cancellationToken);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var asal = entity.IsDuplo;

            entity.IsDuplo = request.IsDuplo;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version += 1;

            AppendHistory(
                order,
                entity,
                "Examination.SetDuplo",
                NamaPenandaDuplo(asal),
                NamaPenandaDuplo(request.IsDuplo),
                actorUserId,
                now);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabExamination.SetDuplo",
                "Mengubah penanda duplo satu pemeriksaan terpesan.",
                new
                {
                    entity.Id,
                    entity.LabOrderId,
                    entity.SpecimenId,
                    Dari = NamaPenandaDuplo(asal),
                    Ke = NamaPenandaDuplo(request.IsDuplo),
                    ActorUserId = actorUserId
                });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private static string NamaPenandaDuplo(bool isDuplo) => isDuplo ? "Duplo" : "Single";

        /// <summary>
        /// Pemeriksaan yang sudah gugur atau dibatalkan tidak menerima penandaan apa pun lagi.
        /// Mengubahnya berarti menulis keputusan baru di atas pekerjaan yang sudah berhenti.
        /// </summary>
        private static void EnsureExaminationMasihBerjalan(LabExamination entity, string apaYangDiubah)
        {
            if (entity.ExaminationStatus == LabExaminationStatus.Voided)
            {
                throw new LabExaminationConflictException(
                    $"Pemeriksaan ini sudah gugur karena wadahnya ditolak, {apaYangDiubah} tidak dapat diubah lagi.");
            }

            if (entity.ExaminationStatus == LabExaminationStatus.Cancelled)
            {
                throw new LabExaminationConflictException(
                    $"Pemeriksaan ini sudah dibatalkan, {apaYangDiubah} tidak dapat diubah lagi.");
            }
        }

        private async Task<LabExamination> LoadExaminationAsync(Guid id, CancellationToken cancellationToken) =>
            await _dbContext.LabExaminations
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pemeriksaan terpesan tidak ditemukan.");

        private async Task<LabOrder> LoadOrderAsync(Guid labOrderId, CancellationToken cancellationToken) =>
            await _dbContext.LabOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == labOrderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pesanan laboratorium tidak ditemukan.");

        /// <summary>
        /// Menulis satu baris riwayat berlingkup <c>LabExamination</c>.
        ///
        /// Barisnya menyebut pesanan dan pemeriksaannya sekaligus: pesanan supaya riwayat satu
        /// kunjungan tetap dapat dibaca berurutan, pemeriksaan supaya jelas yang mana di antara
        /// beberapa pemeriksaan pada wadah yang sama. Wadahnya tidak ikut disebut — yang
        /// berpindah bukan wadah itu.
        /// </summary>
        private void AppendHistory(
            LabOrder order,
            LabExamination examination,
            string action,
            string? fromStatus,
            string toStatus,
            Guid actorUserId,
            DateTime occurredAt,
            string? reasonNote = null)
        {
            _dbContext.LabTransitionHistories.Add(new LabTransitionHistory
            {
                Id = Guid.NewGuid(),
                LabOrderId = order.Id,
                LabExaminationId = examination.Id,
                EncounterId = order.EncounterId,
                Scope = LabTransitionScope.LabExamination,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                CorrelationId = order.Id,
                CreateDateTime = occurredAt,
                CreateBy = actorUserId
            });
        }

        private async Task EnsureOrderExistsAsync(Guid labOrderId, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.LabOrders
                .AsNoTracking()
                .AnyAsync(x => x.Id == labOrderId && !x.IsDelete, cancellationToken);

            if (!exists)
                throw new KeyNotFoundException("Pesanan laboratorium tidak ditemukan.");
        }

        private async Task<LabExaminationResponse> ReadBackAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            _dbContext.ChangeTracker.Clear();

            return await Project(
                    _dbContext.LabExaminations
                        .AsNoTracking()
                        .Where(x => x.Id == id))
                .FirstAsync(cancellationToken);
        }

        /// <summary>
        /// Proyeksi bersama. Nama penanda kesegeraan diambil lewat sub-kueri yang dibiarkan
        /// kosong bila akunnya tidak ada, supaya akun yang hilang tidak membuat baris
        /// pemeriksaannya ikut hilang dari daftar.
        /// </summary>
        private IQueryable<LabExaminationResponse> Project(IQueryable<LabExamination> source) =>
            source
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new LabExaminationResponse
                {
                    Id = x.Id,
                    LabOrderId = x.LabOrderId,
                    SpecimenId = x.SpecimenId,
                    SpecimenBarcode = x.Specimen != null ? x.Specimen.SpecimenBarcode : null,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.ProcedureCodeSnapshot,
                    ProcedureName = x.ProcedureNameSnapshot,
                    TariffId = x.TariffId,
                    TariffCode = x.TariffCodeSnapshot,
                    UnitPrice = x.UnitPriceSnapshot,
                    ExaminationStatus = x.ExaminationStatus.ToString(),
                    ChargeEligibleAt = x.ChargeEligibleAt,
                    Urgency = x.Urgency.ToString(),
                    UrgencyMarkedAt = x.UrgencyMarkedAt,
                    UrgencyMarkedByUserId = x.UrgencyMarkedByUserId,
                    UrgencyMarkedByUserName = x.UrgencyMarkedByUserId == null
                        ? null
                        : _dbContext.Set<ApplicationUser>()
                            .Where(u => u.Id == x.UrgencyMarkedByUserId.Value)
                            .Select(u => u.DisplayName)
                            .FirstOrDefault(),
                    IsDuplo = x.IsDuplo,
                    Version = x.Version
                });

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                // Penjaga terakhir keunikan wadah dan jenis pemeriksaan bila dua permintaan
                // datang bersamaan dan keduanya lolos pemeriksaan awal.
                throw new LabExaminationConflictException(
                    "Pemeriksaan yang sama tidak boleh dimasukkan dua kali dalam satu wadah.");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            return exception.InnerException?.GetType().Name == "PostgresException" &&
                   exception.InnerException.Message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase);
        }

// =================================================================
        // Pengisian hasil — slice S4a (LAB-DEC-005, LAB-DEC-076, LAB-API-v1 r21)
        //
        // BATAS YANG PALING PENTING: method ini MENGISI hasil. Ia tidak memvalidasi, tidak
        // merilis, tidak menandai nilai kritis, dan tidak mengoreksi — keempatnya tertahan
        // LAB-SIGN-001 lewat LAB-DEC-003, LAB-DEC-004, dan LAB-DEC-007.
        //
        // Nol status hasil disentuh. "Hasil sudah diisi" dibaca dari ResultEnteredAt != null —
        // sebuah fakta yang tercatat, bukan janji tentang apa yang terjadi berikutnya.
        // =================================================================

        public async Task<LabExaminationResultResponse> SetResultAsync(
            Guid id,
            LabExaminationResultRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            // VAL-77.
            var examination = await _dbContext.LabExaminations
                .Include(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            // VAL-78. Pemeriksaan yang gugur bersama wadahnya atau dibatalkan petugas tidak
            // punya hasil yang sah untuk diisi.
            if (examination.ExaminationStatus is LabExaminationStatus.Voided
                or LabExaminationStatus.Cancelled)
            {
                throw new LabExaminationConflictException(
                    "Pemeriksaan yang sudah gugur atau dibatalkan tidak dapat diisi hasilnya.");
            }

            var now = DateTime.UtcNow;

            // VAL-82. Aturan yang sama dengan LAB-DEC-064 pada penyaring, diterapkan pada waktu
            // kejadian: pemeriksaan yang mengaku dikerjakan besok adalah data yang salah, dan
            // salahnya baru ketahuan ketika laporannya dibaca.
            var examinedAt = request.ExaminedAt.HasValue
                ? AppDateTimeHelper.ToUtc(request.ExaminedAt.Value)
                : now;

            if (examinedAt > now)
            {
                throw new LabExaminationValidationException(
                    "Waktu pemeriksaan tidak boleh melewati waktu sekarang.");
            }

            var bound = await ResolveValueBoundAsync(examination, cancellationToken);

            // VAL-79.
            if (bound == null)
            {
                throw new LabExaminationValidationException(
                    "Jenis pemeriksaan ini belum memiliki batas nilai yang berlaku, sehingga hasilnya belum dapat diisi.");
            }

            LabValueOption? option = null;

            if (bound.ResultForm == LabResultForm.Numeric)
            {
                // VAL-80.
                if (!request.ResultNumeric.HasValue)
                {
                    throw new LabExaminationValidationException(
                        "Hasil pemeriksaan ini berupa angka, sehingga nilai angkanya wajib diisi.");
                }

                if (request.ResultOptionId.HasValue)
                {
                    throw new LabExaminationValidationException(
                        "Hasil pemeriksaan ini berupa angka, sehingga pilihan hasil tidak dapat dipakai.");
                }
            }
            else
            {
                // VAL-80.
                if (!request.ResultOptionId.HasValue)
                {
                    throw new LabExaminationValidationException(
                        "Hasil pemeriksaan ini berupa pilihan, sehingga salah satu pilihan wajib dipilih.");
                }

                if (request.ResultNumeric.HasValue)
                {
                    throw new LabExaminationValidationException(
                        "Hasil pemeriksaan ini berupa pilihan, sehingga nilai angka tidak dapat dipakai.");
                }

                // VAL-81. Pilihan wajib milik batas nilai yang BERLAKU — bukan pilihan sah milik
                // pemeriksaan lain. Tanpa penjagaan ini, "Negatif" milik Protein urin dapat
                // tersimpan sebagai hasil Kalium tanpa satu pun galat.
                option = await _dbContext.Set<LabValueOption>()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.ResultOptionId.Value &&
                             x.ValueBoundId == bound.Id &&
                             !x.IsDelete,
                        cancellationToken);

                if (option == null)
                {
                    throw new LabExaminationValidationException(
                        "Pilihan hasil tidak dikenal untuk jenis pemeriksaan ini.");
                }
            }

            var actorUserId = GetCurrentUserId();

            examination.ResultNumeric = bound.ResultForm == LabResultForm.Numeric
                ? request.ResultNumeric
                : null;
            examination.ResultOptionId = option?.Id;
            examination.ResultValueBoundId = bound.Id;
            examination.ResultUnitSnapshot = bound.Unit;
            examination.ExaminedAt = examinedAt;
            examination.ResultEnteredAt = now;

            // Guid.Empty adalah pelaku yang tidak pernah ada. Kolom ini nol ber-foreign key,
            // sehingga database TIDAK akan menolaknya — justru itu yang membuatnya lebih
            // berbahaya daripada kasus BE-EXT-05: ia tersimpan diam-diam. Pelajaran itu
            // diterapkan di sini dengan menulis null.
            examination.ResultEnteredByUserId = actorUserId == Guid.Empty ? null : actorUserId;

            examination.UpdateDateTime = now;
            examination.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.SetResult",
                "Hasil pemeriksaan diisi.",
                new
                {
                    examination.Id,
                    examination.LabOrderId,
                    ResultForm = bound.ResultForm.ToString(),
                    examination.ResultNumeric,
                    examination.ResultOptionId,
                    examination.ExaminedAt
                });

            return new LabExaminationResultResponse
            {
                LabExaminationId = examination.Id,
                LabOrderId = examination.LabOrderId,
                ProcedureName = examination.ProcedureNameSnapshot ?? examination.Procedure?.ProcedureName,
                ResultForm = bound.ResultForm.ToString(),
                ResultNumeric = examination.ResultNumeric,
                ResultOptionId = examination.ResultOptionId,
                ResultOptionName = option?.OptionName,
                ResultUnitSnapshot = examination.ResultUnitSnapshot,
                ResultValueBoundId = examination.ResultValueBoundId,
                ExaminedAt = examination.ExaminedAt,
                ResultEnteredAt = examination.ResultEnteredAt,
                IsOutOfNormalRange = ResolveOutOfNormalRange(bound, examination.ResultNumeric, option)
            };
        }

        /// <summary>
        /// Bentuk hasil yang berlaku bagi satu pemeriksaan (<c>r22</c>).
        ///
        /// <b>Memakai jalur pemilihan batas yang sama persis dengan <see cref="SetResultAsync"/>.</b>
        /// Dua salinan aturan yang sama pasti bercabang, dan cabangnya membuat layar menampilkan
        /// rujukan yang berbeda dari yang dipakai menilai hasilnya — selisih yang nol terlihat
        /// sampai seseorang membandingkan keduanya.
        /// </summary>
        public async Task<LabExaminationResultFormResponse> GetResultFormAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var examination = await _dbContext.LabExaminations
                .Include(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            var response = new LabExaminationResultFormResponse
            {
                LabExaminationId = examination.Id,
                ProcedureName = examination.ProcedureNameSnapshot ?? examination.Procedure?.ProcedureName,
                ResultNumeric = examination.ResultNumeric,
                ResultOptionId = examination.ResultOptionId,
                ExaminedAt = examination.ExaminedAt,
                ResultEnteredAt = examination.ResultEnteredAt
            };

            if (examination.ExaminationStatus is LabExaminationStatus.Voided
                or LabExaminationStatus.Cancelled)
            {
                response.CanEnterResult = false;
                response.BlockedReason =
                    "Pemeriksaan yang sudah gugur atau dibatalkan tidak dapat diisi hasilnya.";
                return response;
            }

            var bound = await ResolveValueBoundAsync(examination, cancellationToken);

            if (bound == null)
            {
                response.CanEnterResult = false;
                response.BlockedReason =
                    "Jenis pemeriksaan ini belum memiliki batas nilai yang berlaku, sehingga hasilnya belum dapat diisi.";
                return response;
            }

            response.CanEnterResult = true;
            response.ResultForm = bound.ResultForm.ToString();
            response.Unit = bound.Unit;
            response.NormalLow = bound.NormalLow;
            response.NormalHigh = bound.NormalHigh;

            // CriticalLow dan CriticalHigh sengaja TIDAK ikut. Batas kritis adalah LAB-DEC-004,
            // yang tertahan LAB-SIGN-001 — mengirimkannya mengundang layar menandai nilai
            // kritis, dan penandaan itu menjanjikan alur pelaporan yang belum diputuskan.

            if (bound.ResultForm == LabResultForm.Choice)
            {
                response.Options = await _dbContext.Set<LabValueOption>()
                    .AsNoTracking()
                    .Where(x => x.ValueBoundId == bound.Id && !x.IsDelete)
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new LabExaminationResultOptionResponse
                    {
                        Id = x.Id,
                        OptionName = x.OptionName,
                        IsOutOfReference = x.IsOutOfReference
                        // IsCritical sengaja tidak ikut, sebab yang sama dengan batas kritis.
                    })
                    .ToListAsync(cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// Memilih baris batas nilai yang berlaku bagi pasien pemeriksaan ini.
        ///
        /// Satu jenis pemeriksaan dapat punya beberapa baris yang dibedakan menurut jenis
        /// kelamin dan kelompok umur (<c>LAB-DEC-018</c>) — Hemoglobin pria dewasa, wanita
        /// dewasa, dan anak berdiri sebagai tiga baris terpisah.
        ///
        /// <b>Yang paling khusus menang.</b> Baris berjenis kelamin tertentu lebih diutamakan
        /// daripada <c>All</c>, dan baris berkelompok umur tertentu lebih diutamakan daripada
        /// yang tanpa kelompok umur. Tanpa urutan ini, baris umum dapat menang atas baris yang
        /// justru dibuat untuk pasien itu — dan hasilnya terbaca normal memakai batas yang salah.
        /// </summary>
        private async Task<LabValueBound?> ResolveValueBoundAsync(
            LabExamination examination,
            CancellationToken cancellationToken)
        {
            var konteks = await _dbContext.LabOrders
                .AsNoTracking()
                .Where(o => o.Id == examination.LabOrderId)
                .Select(o => new
                {
                    AgeCategoryId = o.Encounter != null ? o.Encounter.AgeCategoryId : null,
                    Gender = o.Encounter == null
                        ? null
                        : _dbContext.MstPatients
                            .Where(p => p.Id == o.Encounter.PatientId)
                            .Select(p => p.Gender)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            var genderScope = konteks?.Gender switch
            {
                Gender.Male => LabGenderScope.Male,
                Gender.Female => LabGenderScope.Female,
                _ => LabGenderScope.All
            };

            var ageCategoryId = konteks?.AgeCategoryId;

            var kandidat = await _dbContext.Set<LabValueBound>()
                .AsNoTracking()
                .Where(x =>
                    x.ProcedureId == examination.ProcedureId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    (x.GenderScope == LabGenderScope.All || x.GenderScope == genderScope) &&
                    (x.AgeCategoryId == null || x.AgeCategoryId == ageCategoryId))
                .ToListAsync(cancellationToken);

            return kandidat
                .OrderByDescending(x => x.GenderScope == genderScope && genderScope != LabGenderScope.All ? 1 : 0)
                .ThenByDescending(x => x.AgeCategoryId != null ? 1 : 0)
                .FirstOrDefault();
        }

        /// <summary>
        /// Apakah nilainya di luar rentang normal batas yang berlaku.
        ///
        /// <b>Ini keterangan, bukan penandaan nilai kritis.</b> Nilai kritis beserta kewajiban
        /// pelaporannya adalah <c>LAB-DEC-004</c>, yang tertahan <c>LAB-SIGN-001</c> — dan
        /// menandainya di sini akan menjanjikan alur pelaporan yang belum diputuskan pihak
        /// klinis.
        /// </summary>
        private static bool? ResolveOutOfNormalRange(
            LabValueBound bound,
            decimal? resultNumeric,
            LabValueOption? option)
        {
            if (option != null)
            {
                return option.IsOutOfReference;
            }

            if (!resultNumeric.HasValue) return null;
            if (!bound.NormalLow.HasValue && !bound.NormalHigh.HasValue) return null;

            if (bound.NormalLow.HasValue && resultNumeric.Value < bound.NormalLow.Value) return true;
            if (bound.NormalHigh.HasValue && resultNumeric.Value > bound.NormalHigh.Value) return true;

            return false;
        }


        // =================================================================
        // Kelengkapan dan konsultasi hasil Mikrobiologi — BE-LAB-54, slice S4b
        // (LAB-API-v1 r26 bagian 21.2; LAB-DEC-097, LAB-DEC-106; VAL-107, VAL-108)
        //
        // KETIGA METODE DI BAWAH TIDAK MERILIS APA PUN. FinalizedAt mencatat bahwa penulisnya
        // menyatakan selesai — sebuah fakta. Rilis Mikrobiologi adalah S4d, dan S4d tertahan
        // DEC-LAB-011.
        //
        // Itu sebabnya setiap respons membawa IsReleased dan DeliveryBlockedReason: supaya
        // pemanggil nol perlu MENYIMPULKAN bahwa Final sama dengan rilis.
        // =================================================================

        /// <summary>
        /// Menyatakan penulisan hasil selesai — <b>bukan</b> merilis (<c>LAB-DEC-097</c>).
        /// </summary>
        public async Task<LabExaminationCompletionResponse> FinalizeMicrobiologyResultAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var examination = await _dbContext.LabExaminations
                .Include(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            if (examination.ResultEnteredAt is null)
            {
                throw new LabExaminationValidationException(
                    "Hasil pemeriksaan ini belum diisi, jadi belum dapat dinyatakan selesai.");
            }

            if (examination.FinalizedAt is not null)
            {
                throw new LabExaminationConflictException(
                    "Hasil pemeriksaan ini sudah dinyatakan selesai.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            examination.FinalizedAt = now;

            // Guid.Empty adalah pelaku yang tidak pernah ada, dan kolom ini nol ber-foreign key
            // sehingga database TIDAK akan menolaknya. Pelajaran BE-EXT-05 diterapkan di sini.
            examination.FinalizedByUserId = actorUserId == Guid.Empty ? null : actorUserId;

            examination.UpdateDateTime = now;
            examination.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.FinalizeMicrobiologyResult",
                "Penulisan hasil Mikrobiologi dinyatakan selesai. Ini BUKAN rilis.",
                new { examination.Id, examination.LabOrderId, examination.FinalizedAt });

            return BuildCompletionResponse(examination);
        }

        /// <summary>
        /// Membuka kembali penulisan hasil sebelum rilis (<c>LAB-DEC-097</c>).
        /// </summary>
        public async Task<LabExaminationCompletionResponse> ReopenMicrobiologyResultAsync(
            Guid id,
            LabReopenRequest request,
            CancellationToken cancellationToken = default)
        {
            // LabOrder WAJIB ikut dimuat: baris riwayat transisi di bawah menyimpan EncounterId,
            // dan tanpa Include ini nilainya jatuh ke Guid.Empty. Berbeda dari kolom pengguna
            // yang nol ber-foreign key, EncounterId PUNYA foreign key — sehingga Guid.Empty
            // ditolak database dengan galat 23503, bukan tersimpan diam-diam.
            var examination = await _dbContext.LabExaminations
                .Include(x => x.Procedure)
                .Include(x => x.LabOrder)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            // VAL-107. Membuka kembali sesuatu yang belum pernah ditutup adalah permintaan yang
            // tidak punya arti, dan membiarkannya lolos akan menaikkan ReopenCount pada hasil
            // yang nol pernah difinalkan.
            if (examination.FinalizedAt is null)
            {
                throw new LabExaminationValidationException(
                    "Hasil ini belum pernah dinyatakan selesai, jadi tidak ada yang perlu dibuka kembali.");
            }

            var alasan = string.IsNullOrWhiteSpace(request?.Reason) ? null : request.Reason.Trim();

            if (alasan is null)
            {
                throw new LabExaminationValidationException(
                    "Alasan membuka kembali wajib diisi.");
            }

            if (alasan.Length > 500)
            {
                throw new LabExaminationValidationException(
                    "Alasan membuka kembali paling panjang 500 karakter.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var finalSebelumnya = examination.FinalizedAt;

            examination.FinalizedAt = null;
            examination.FinalizedByUserId = null;
            examination.ReopenCount += 1;

            examination.UpdateDateTime = now;
            examination.UpdateBy = actorUserId;

            // Jejak perpindahan dicatat pada riwayat transisi yang sudah ada, BUKAN sebagai
            // koreksi hasil terrilis: hasil ini belum pernah dirilis, sehingga ia nol menyentuh
            // S6 maupun DEC-LAB-014.
            _dbContext.LabTransitionHistories.Add(new LabTransitionHistory
            {
                LabOrderId = examination.LabOrderId,
                LabExaminationId = examination.Id,
                EncounterId = examination.LabOrder!.EncounterId,
                Scope = LabTransitionScope.LabExamination,
                Action = "LabExamination.ReopenMicrobiologyResult",
                FromStatus = "Finalized",
                ToStatus = "Draft",
                ReasonNote = alasan,
                ActorUserId = actorUserId,
                OccurredAt = now
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.ReopenMicrobiologyResult",
                "Penulisan hasil Mikrobiologi dibuka kembali sebelum rilis.",
                new
                {
                    examination.Id,
                    examination.LabOrderId,
                    FinalizedAtSebelumnya = finalSebelumnya,
                    examination.ReopenCount
                });

            return BuildCompletionResponse(examination);
        }

        /// <summary>
        /// Mencatat fakta konsultasi — penanda <c>Definitif</c> (<c>LAB-DEC-106</c>).
        ///
        /// <b>Ia nol membuka pengiriman kepada siapa pun.</b>
        /// </summary>
        public async Task<LabExaminationCompletionResponse> RecordConsultationAsync(
            Guid id,
            LabConsultationRequest request,
            CancellationToken cancellationToken = default)
        {
            var examination = await _dbContext.LabExaminations
                .Include(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            var kepada = string.IsNullOrWhiteSpace(request?.ConsultedToName)
                ? null
                : request.ConsultedToName.Trim();

            if (kepada is null)
            {
                throw new LabExaminationValidationException(
                    "Nama pihak yang dikonsultasikan wajib diisi.");
            }

            if (kepada.Length > 200)
            {
                throw new LabExaminationValidationException(
                    "Nama pihak yang dikonsultasikan paling panjang 200 karakter.");
            }

            if (request!.ConsultedAt is null)
            {
                throw new LabExaminationValidationException(
                    "Waktu konsultasi wajib diisi.");
            }

            var now = DateTime.UtcNow;

            // VAL-108. Waktu konsultasi di masa depan adalah kejadian yang belum terjadi, dan
            // mencatatnya sebagai fakta membuat jejaknya berbohong.
            if (request.ConsultedAt.Value > now)
            {
                throw new LabExaminationValidationException(
                    "Waktu konsultasi tidak boleh melewati waktu sekarang.");
            }

            var actorUserId = GetCurrentUserId();

            examination.ConsultedToName = kepada;
            examination.ConsultedAt = request.ConsultedAt;
            examination.ConsultedByUserId = actorUserId == Guid.Empty ? null : actorUserId;

            examination.UpdateDateTime = now;
            examination.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.RecordConsultation",
                "Fakta konsultasi hasil dicatat. Ini BUKAN izin pengiriman.",
                new { examination.Id, examination.LabOrderId, examination.ConsultedAt });

            return BuildCompletionResponse(examination);
        }

        /// <summary>
        /// Menyusun respons kelengkapan.
        ///
        /// <b><see cref="LabExaminationCompletionResponse.IsReleased"/> selalu bernilai salah
        /// pada rilis ini</b>, sebab rilis Mikrobiologi adalah <c>S4d</c> yang belum dibangun.
        /// Ruas itu ada supaya pemanggil nol perlu menyimpulkan sendiri.
        /// </summary>
        private static LabExaminationCompletionResponse BuildCompletionResponse(LabExamination examination)
        {
            const string belumDirilis =
                "Hasil ini belum dirilis, sehingga belum boleh dikirim kepada pasien.";

            return new LabExaminationCompletionResponse
            {
                LabExaminationId = examination.Id,
                LabOrderId = examination.LabOrderId,
                ProcedureName = examination.ProcedureNameSnapshot ?? examination.Procedure?.ProcedureName,
                IsFinalized = examination.FinalizedAt is not null,
                FinalizedAt = examination.FinalizedAt,
                FinalizedByUserId = examination.FinalizedByUserId,
                ReopenCount = examination.ReopenCount,
                IsConsulted = examination.ConsultedAt is not null,
                ConsultedToName = examination.ConsultedToName,
                ConsultedAt = examination.ConsultedAt,
                ConsultedByUserId = examination.ConsultedByUserId,
                IsReleased = false,
                DeliveryBlockedReason = belumDirilis
            };
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>Pelanggaran aturan isi pemeriksaan terpesan. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabExaminationValidationException(string message) : Exception(message);

    /// <summary>Tindakan di luar kewenangan pelakunya. Dipetakan menjadi <c>403</c>.</summary>
    public sealed class LabExaminationForbiddenException(string message) : Exception(message);

    /// <summary>Bentrokan dengan keadaan yang sudah berjalan. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabExaminationConflictException(string message) : Exception(message);
}
