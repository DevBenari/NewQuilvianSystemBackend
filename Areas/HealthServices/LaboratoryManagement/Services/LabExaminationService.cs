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
using System.Globalization;
using System.Linq.Expressions;
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

            // VAL-143 — arah sementara ARCH-GAP-LAB-09 sampai DEC-LAB-019 dijawab. Hasil yang
            // sudah dirilis sudah menjadi dokumen klinis pasien dan mungkin sudah dibaca dokter;
            // membatalkan pemeriksaannya menghilangkan hasil itu dari pandangan tanpa jejak
            // koreksi. Perbaikannya lewat koreksi (S6), bukan pembatalan.
            if (entity.ReleasedAt is not null)
            {
                throw new LabExaminationValidationException(
                    "Pemeriksaan yang hasilnya sudah dirilis tidak dapat dibatalkan. Hasilnya hanya dapat diperbaiki lewat koreksi.");
            }

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

            EnsureResultNotFinalized(examination);

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
            examination.Version += 1;

            await SaveResultWriteAsync(cancellationToken);

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
                IsOutOfNormalRange = ResolveOutOfNormalRange(bound, examination.ResultNumeric, option),
                ReferenceFlag = ResolveReferenceFlag(
                    bound.NormalLow,
                    bound.NormalHigh,
                    examination.ResultNumeric,
                    option?.IsOutOfReference)?.ToString()
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
            var row = await ProjectResultFormRows(
                    _dbContext.LabExaminations.Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            var forms = await BuildResultFormsAsync(row.LabOrderId, [row], cancellationToken);

            return forms[0];
        }

        /// <summary>
        /// Seluruh pemeriksaan Patologi Klinik yang tidak batal pada satu order, dalam bentuk
        /// yang sama persis dengan <see cref="GetResultFormAsync"/> (<c>LAB-DEC-149</c>,
        /// <c>r33</c> 28.2). Satu panggilan untuk satu halaman.
        ///
        /// <b>Jumlah kueri tetap, berapa pun barisnya.</b> Pemeriksaan dibaca dalam satu kueri,
        /// lalu batas nilai, pilihan, dan riwayat batas dimuat berkelompok. Halaman 19 baris
        /// tidak boleh menjadi 19 kali <see cref="GetResultFormAsync"/>.
        /// </summary>
        public async Task<List<LabExaminationResultFormResponse>> GetResultSheetByOrderAsync(
            Guid labOrderId,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => x.Id == labOrderId && !x.IsDelete)
                .Select(x => new { x.Discipline })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Pesanan laboratorium tidak ditemukan.");

            if (order.Discipline is not null && order.Discipline != LabDiscipline.ClinicalPathology)
            {
                throw NotClinicalPathologyOrder();
            }

            // "Tidak batal" berarti Cancelled saja yang keluar (AC-234). Pemeriksaan yang GUGUR
            // bersama wadahnya tetap tampil, dengan isian terkunci beserta alasannya — petugas
            // perlu tahu wadahnya ditolak, bukan melihat baris itu lenyap.
            var rows = await ProjectResultFormRows(
                    _dbContext.LabExaminations.Where(x =>
                        x.LabOrderId == labOrderId &&
                        !x.IsDelete &&
                        x.ExaminationStatus != LabExaminationStatus.Cancelled))
                .OrderBy(x => x.CreateDateTime)
                .ThenBy(x => x.ProcedureName)
                .ToListAsync(cancellationToken);

            // VAL-123 bagi order lama yang disiplinnya kosong: disiplin dibaca dari katalog
            // tiap pemeriksaan, sumber yang juga menurunkan disiplin order (LAB-DEC-048).
            if (rows.Any(x => x.Discipline is not null && x.Discipline != LabDiscipline.ClinicalPathology))
            {
                throw NotClinicalPathologyOrder();
            }

            return await BuildResultFormsAsync(labOrderId, rows, cancellationToken);
        }

        /// <summary><c>VAL-123</c>.</summary>
        private static LabExaminationValidationException NotClinicalPathologyOrder() =>
            new("Order ini bukan Patologi Klinik. Buka halaman hasil sesuai disiplinnya.");

        /// <summary>Isi satu pemeriksaan yang dibutuhkan respons bentuk hasil.</summary>
        private sealed class ResultFormRow
        {
            public Guid Id { get; init; }
            public Guid LabOrderId { get; init; }
            public Guid ProcedureId { get; init; }
            public string? ProcedureName { get; init; }
            /// <summary>Disiplin order; order lama yang kosong jatuh ke disiplin katalog pemeriksaan.</summary>
            public LabDiscipline? Discipline { get; init; }
            public LabExaminationStatus ExaminationStatus { get; init; }
            public LabExaminationUrgency Urgency { get; init; }
            public decimal? ResultNumeric { get; init; }
            public Guid? ResultOptionId { get; init; }
            public Guid? ResultValueBoundId { get; init; }
            public DateTime? ExaminedAt { get; init; }
            public DateTime? ResultEnteredAt { get; init; }
            public DateTime? FinalizedAt { get; init; }
            public Guid? FinalizedByUserId { get; init; }
            public int ReopenCount { get; init; }
            public string? ConsultedToName { get; init; }
            public DateTime? ConsultedAt { get; init; }
            public DateTime CreateDateTime { get; init; }
            // BE-LAB-76 — pengisi dan pengesah.
            public Guid? ResultEnteredByUserId { get; init; }
            public DateTime? ValidatedAt { get; init; }
            public Guid? ValidatedByUserId { get; init; }
            public string? ValidatedByPositionNameSnapshot { get; init; }
            public Guid? ValidationExceptionReasonId { get; init; }
            public string? ValidationExceptionReasonNameSnapshot { get; init; }
            public DateTime? ReleasedAt { get; init; }
            public Guid? ReleasedByUserId { get; init; }
            public string? ReleasedByPositionNameSnapshot { get; init; }
            public Guid? ReleaseExceptionReasonId { get; init; }
            public string? ReleaseExceptionReasonNameSnapshot { get; init; }
        }

        private static IQueryable<ResultFormRow> ProjectResultFormRows(IQueryable<LabExamination> source) =>
            source
                .AsNoTracking()
                .Select(x => new ResultFormRow
                {
                    Id = x.Id,
                    LabOrderId = x.LabOrderId,
                    ProcedureId = x.ProcedureId,
                    ProcedureName = x.ProcedureNameSnapshot ?? (x.Procedure != null ? x.Procedure.ProcedureName : null),
                    Discipline = x.LabOrder != null && x.LabOrder.Discipline != null
                        ? x.LabOrder.Discipline
                        : x.Procedure != null ? x.Procedure.LabDiscipline : null,
                    ExaminationStatus = x.ExaminationStatus,
                    Urgency = x.Urgency,
                    ResultNumeric = x.ResultNumeric,
                    ResultOptionId = x.ResultOptionId,
                    ResultValueBoundId = x.ResultValueBoundId,
                    ExaminedAt = x.ExaminedAt,
                    ResultEnteredAt = x.ResultEnteredAt,
                    FinalizedAt = x.FinalizedAt,
                    FinalizedByUserId = x.FinalizedByUserId,
                    ReopenCount = x.ReopenCount,
                    ConsultedToName = x.ConsultedToName,
                    ConsultedAt = x.ConsultedAt,
                    CreateDateTime = x.CreateDateTime,
                    ResultEnteredByUserId = x.ResultEnteredByUserId,
                    ValidatedAt = x.ValidatedAt,
                    ValidatedByUserId = x.ValidatedByUserId,
                    ValidatedByPositionNameSnapshot = x.ValidatedByPositionNameSnapshot,
                    ValidationExceptionReasonId = x.ValidationExceptionReasonId,
                    ValidationExceptionReasonNameSnapshot = x.ValidationExceptionReasonNameSnapshot,
                    ReleasedAt = x.ReleasedAt,
                    ReleasedByUserId = x.ReleasedByUserId,
                    ReleasedByPositionNameSnapshot = x.ReleasedByPositionNameSnapshot,
                    ReleaseExceptionReasonId = x.ReleaseExceptionReasonId,
                    ReleaseExceptionReasonNameSnapshot = x.ReleaseExceptionReasonNameSnapshot
                });

        /// <summary>
        /// Menyusun respons bentuk hasil bagi banyak pemeriksaan <b>satu order</b> sekaligus.
        ///
        /// Dua batas yang berbeda dipakai dengan sengaja. <b>Bentuk isian</b> — rentang, satuan,
        /// pilihan — memakai batas yang berlaku <b>hari ini</b> bagi pasien itu, sebab itulah
        /// yang dipakai bila hasilnya disimpan sekarang. <b>Penanda</b> memakai batas yang
        /// berlaku <b>saat hasil disimpan</b>, supaya perubahan batas kemudian hari tidak
        /// mengubah arti hasil lama.
        /// </summary>
        private async Task<List<LabExaminationResultFormResponse>> BuildResultFormsAsync(
            Guid labOrderId,
            IReadOnlyList<ResultFormRow> rows,
            CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return [];
            }

            var berjalan = rows
                .Where(x => x.ExaminationStatus is not (LabExaminationStatus.Voided or LabExaminationStatus.Cancelled))
                .ToList();

            var boundPerProcedure = new Dictionary<Guid, LabValueBound>();

            if (berjalan.Count > 0)
            {
                // Satu pesanan, satu pasien — konteks batas cukup dibaca sekali.
                var patient = await LoadBoundPatientContextAsync(labOrderId, cancellationToken);
                var procedureIds = berjalan.Select(x => x.ProcedureId).Distinct().ToList();

                var kandidat = await ApplicableBounds(patient)
                    .Where(x => procedureIds.Contains(x.ProcedureId))
                    .ToListAsync(cancellationToken);

                boundPerProcedure = kandidat
                    .GroupBy(x => x.ProcedureId)
                    .ToDictionary(g => g.Key, g => PickBound(g, patient.GenderScope)!);
            }

            var choiceBoundIds = boundPerProcedure.Values
                .Where(x => x.ResultForm == LabResultForm.Choice)
                .Select(x => x.Id)
                .ToList();

            var optionsPerBound = new Dictionary<Guid, List<LabExaminationResultOptionResponse>>();

            if (choiceBoundIds.Count > 0)
            {
                optionsPerBound = (await _dbContext.Set<LabValueOption>()
                        .AsNoTracking()
                        .Where(x => choiceBoundIds.Contains(x.ValueBoundId) && !x.IsDelete)
                        .OrderBy(x => x.SortOrder)
                        .Select(x => new
                        {
                            x.ValueBoundId,
                            Option = new LabExaminationResultOptionResponse
                            {
                                Id = x.Id,
                                OptionName = x.OptionName,
                                IsOutOfReference = x.IsOutOfReference
                                // IsCritical sengaja tidak ikut, sebab yang sama dengan batas kritis.
                            }
                        })
                        .ToListAsync(cancellationToken))
                    .GroupBy(x => x.ValueBoundId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Option).ToList());
            }

            var flags = await ResolveReferenceFlagsAsync(rows, cancellationToken);

            // BE-LAB-76. Nama pemvalidasi dan perilis dibaca BERKELOMPOK — satu kueri untuk seluruh
            // baris, dilewati bila tidak satu pun baris tervalidasi. Halaman 18 baris tidak boleh
            // menjadi 18 kueri nama.
            var pelakuIds = rows
                .SelectMany(x => new[] { x.ValidatedByUserId, x.ReleasedByUserId })
                .Where(x => x.HasValue && x.Value != Guid.Empty)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var namaPelaku = pelakuIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => pelakuIds.Contains(u.Id))
                    .Select(u => new { u.Id, Name = u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode })
                    .ToDictionaryAsync(x => x.Id, x => (string?)x.Name, cancellationToken);

            string? Nama(Guid? userId) =>
                userId is Guid id && namaPelaku.TryGetValue(id, out var nama) ? nama : null;

            return rows.Select(row =>
            {
                var namaPemvalidasi = row.ValidatedAt is not null ? Nama(row.ValidatedByUserId) : null;
                var namaPerilis = row.ReleasedAt is not null ? Nama(row.ReleasedByUserId) : null;

                var response = new LabExaminationResultFormResponse
                {
                    ResultEnteredByUserId = row.ResultEnteredByUserId,
                    ResultStatus = DeriveResultStatus(row.ReleasedAt, row.ValidatedAt, row.FinalizedAt, row.ResultEnteredAt).ToString(),
                    IsValidated = row.ValidatedAt is not null,
                    ValidatedAt = row.ValidatedAt,
                    ValidatedByUserId = row.ValidatedByUserId,
                    ValidatedByName = namaPemvalidasi,
                    ValidatedByPositionName = row.ValidatedByPositionNameSnapshot,
                    ValidationExceptionMarker = ValidationExceptionMarker(
                        row.ValidationExceptionReasonId, namaPemvalidasi, row.ValidationExceptionReasonNameSnapshot),
                    IsReleased = row.ReleasedAt is not null,
                    ReleasedAt = row.ReleasedAt,
                    ReleasedByUserId = row.ReleasedByUserId,
                    ReleasedByName = namaPerilis,
                    ReleasedByPositionName = row.ReleasedByPositionNameSnapshot,
                    ReleaseExceptionMarker = ReleaseExceptionMarker(
                        row.ReleaseExceptionReasonId, namaPerilis, row.ReleaseExceptionReasonNameSnapshot),
                    DeliveryBlockedReason = row.ReleasedAt is not null ? null : NotReleasedMessage,

                    LabExaminationId = row.Id,
                    ProcedureName = row.ProcedureName,
                    ResultNumeric = row.ResultNumeric,
                    ResultOptionId = row.ResultOptionId,
                    ExaminedAt = row.ExaminedAt,
                    ResultEnteredAt = row.ResultEnteredAt,
                    Urgency = row.Urgency.ToString(),
                    ReferenceFlag = flags.TryGetValue(row.Id, out var flag) ? flag.ToString() : null,
                    IsFinalized = row.FinalizedAt is not null,
                    FinalizedAt = row.FinalizedAt,
                    FinalizedByUserId = row.FinalizedByUserId,
                    ReopenCount = row.ReopenCount,
                    IsConsulted = row.ConsultedAt is not null,
                    ConsultedToName = row.ConsultedToName,
                    ConsultedAt = row.ConsultedAt
                };

                if (row.ExaminationStatus is LabExaminationStatus.Voided or LabExaminationStatus.Cancelled)
                {
                    response.CanEnterResult = false;
                    response.BlockedReason =
                        "Pemeriksaan yang sudah gugur atau dibatalkan tidak dapat diisi hasilnya.";
                    return response;
                }

                if (!boundPerProcedure.TryGetValue(row.ProcedureId, out var bound))
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
                    response.Options = optionsPerBound.TryGetValue(bound.Id, out var options)
                        ? options
                        : new List<LabExaminationResultOptionResponse>();
                }

                return response;
            }).ToList();
        }

        /// <summary>
        /// Penanda rujukan setiap hasil yang sudah diisi, dihitung dari batas yang berlaku
        /// <b>saat hasil disimpan</b>.
        ///
        /// Hasil pilihan membaca <c>IsOutOfReference</c> pilihan yang tersimpan. Pilihan yang
        /// sudah dipakai hasil tidak dapat dihapus (relasi <c>Restrict</c>), sehingga nilainya
        /// adalah nilai saat hasil disimpan.
        /// </summary>
        private async Task<Dictionary<Guid, LabReferenceFlag>> ResolveReferenceFlagsAsync(
            IReadOnlyList<ResultFormRow> rows,
            CancellationToken cancellationToken)
        {
            var hasil = new Dictionary<Guid, LabReferenceFlag>();

            var terisi = rows
                .Where(x => x.ResultEnteredAt.HasValue && x.ResultValueBoundId.HasValue)
                .ToList();

            if (terisi.Count == 0)
            {
                return hasil;
            }

            var optionIds = terisi
                .Where(x => x.ResultOptionId.HasValue)
                .Select(x => x.ResultOptionId!.Value)
                .Distinct()
                .ToList();

            var optionOutOfReference = optionIds.Count == 0
                ? new Dictionary<Guid, bool>()
                : await _dbContext.Set<LabValueOption>()
                    .AsNoTracking()
                    .Where(x => optionIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.IsOutOfReference, cancellationToken);

            var ranges = await LoadNormalRangesAtResultAsync(
                terisi.Where(x => !x.ResultOptionId.HasValue && x.ResultNumeric.HasValue).ToList(),
                cancellationToken);

            foreach (var row in terisi)
            {
                LabReferenceFlag? flag = null;

                if (row.ResultOptionId is Guid optionId)
                {
                    if (optionOutOfReference.TryGetValue(optionId, out var isOut))
                    {
                        flag = ResolveReferenceFlag(null, null, null, isOut);
                    }
                }
                else if (ranges.TryGetValue(row.Id, out var range))
                {
                    flag = ResolveReferenceFlag(range.Low, range.High, row.ResultNumeric, null);
                }

                if (flag.HasValue)
                {
                    hasil[row.Id] = flag.Value;
                }
            }

            return hasil;
        }

        /// <summary>
        /// Rentang normal <b>saat hasil disimpan</b>, per pemeriksaan.
        ///
        /// Batas nilai disunting di baris yang sama (<c>LabValueBoundService.UpdateAsync</c>),
        /// sehingga <c>ResultValueBoundId</c> saja belum membekukan rentangnya. Yang
        /// membekukannya adalah riwayat perubahan per kolom (<c>AC-34</c>): bila rentang
        /// berubah sesudah hasil disimpan, nilai lama pada perubahan <b>pertama</b> sesudah
        /// waktu itu adalah nilai yang berlaku bagi hasil tersebut.
        ///
        /// Baris batas dibaca tanpa menyaring <c>IsActive</c> dan <c>IsDelete</c>: batas yang
        /// kemudian dinonaktifkan tetap batas yang dipakai hasil itu.
        /// </summary>
        private async Task<Dictionary<Guid, (decimal? Low, decimal? High)>> LoadNormalRangesAtResultAsync(
            IReadOnlyList<ResultFormRow> rows,
            CancellationToken cancellationToken)
        {
            var hasil = new Dictionary<Guid, (decimal? Low, decimal? High)>();

            if (rows.Count == 0)
            {
                return hasil;
            }

            var boundIds = rows.Select(x => x.ResultValueBoundId!.Value).Distinct().ToList();

            var bounds = await _dbContext.Set<LabValueBound>()
                .AsNoTracking()
                .Where(x => boundIds.Contains(x.Id))
                .Select(x => new { x.Id, x.NormalLow, x.NormalHigh })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var sejak = rows.Min(x => x.ResultEnteredAt!.Value);

            var perubahan = await _dbContext.LabValueBoundHistories
                .AsNoTracking()
                .Where(x =>
                    boundIds.Contains(x.ValueBoundId) &&
                    x.OccurredAt > sejak &&
                    (x.ChangedField == nameof(LabValueBound.NormalLow) ||
                     x.ChangedField == nameof(LabValueBound.NormalHigh)))
                .Select(x => new BoundChange(x.ValueBoundId, x.ChangedField, x.OldValue, x.OccurredAt))
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                if (!bounds.TryGetValue(row.ResultValueBoundId!.Value, out var bound))
                {
                    continue;
                }

                var saat = row.ResultEnteredAt!.Value;

                if (TryValueAtResult(perubahan, bound.Id, nameof(LabValueBound.NormalLow), saat, bound.NormalLow, out var low) &&
                    TryValueAtResult(perubahan, bound.Id, nameof(LabValueBound.NormalHigh), saat, bound.NormalHigh, out var high))
                {
                    hasil[row.Id] = (low, high);
                }
            }

            return hasil;
        }

        private sealed record BoundChange(Guid ValueBoundId, string ChangedField, string? OldValue, DateTime OccurredAt);

        /// <summary>
        /// Nilai satu kolom batas pada waktu <paramref name="saat"/>. <c>false</c> bila riwayat
        /// menyimpan nilai lama yang tidak terbaca sebagai angka — penanda lebih baik kosong
        /// daripada dihitung dari rentang yang ditebak.
        /// </summary>
        private static bool TryValueAtResult(
            IEnumerable<BoundChange> perubahan,
            Guid boundId,
            string field,
            DateTime saat,
            decimal? nilaiSekarang,
            out decimal? nilai)
        {
            var pertama = perubahan
                .Where(x => x.ValueBoundId == boundId && x.ChangedField == field && x.OccurredAt > saat)
                .OrderBy(x => x.OccurredAt)
                .FirstOrDefault();

            if (pertama is null)
            {
                nilai = nilaiSekarang;
                return true;
            }

            if (pertama.OldValue is null)
            {
                nilai = null;
                return true;
            }

            // LabValueBoundService menulis nilai lama dengan InvariantCulture.
            if (decimal.TryParse(pertama.OldValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                nilai = parsed;
                return true;
            }

            nilai = null;
            return false;
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
            var patient = await LoadBoundPatientContextAsync(examination.LabOrderId, cancellationToken);

            var kandidat = await ApplicableBounds(patient)
                .Where(x => x.ProcedureId == examination.ProcedureId)
                .ToListAsync(cancellationToken);

            return PickBound(kandidat, patient.GenderScope);
        }

        /// <summary>Jenis kelamin dan kelompok umur pasien yang menentukan batas nilai.</summary>
        private readonly record struct BoundPatientContext(LabGenderScope GenderScope, Guid? AgeCategoryId);

        private async Task<BoundPatientContext> LoadBoundPatientContextAsync(
            Guid labOrderId,
            CancellationToken cancellationToken)
        {
            var konteks = await _dbContext.LabOrders
                .AsNoTracking()
                .Where(o => o.Id == labOrderId)
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

            return new BoundPatientContext(genderScope, konteks?.AgeCategoryId);
        }

        /// <summary>Baris batas nilai aktif yang cocok dengan pasien — belum disaring per pemeriksaan.</summary>
        private IQueryable<LabValueBound> ApplicableBounds(BoundPatientContext patient)
        {
            var genderScope = patient.GenderScope;
            var ageCategoryId = patient.AgeCategoryId;

            return _dbContext.Set<LabValueBound>()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    (x.GenderScope == LabGenderScope.All || x.GenderScope == genderScope) &&
                    (x.AgeCategoryId == null || x.AgeCategoryId == ageCategoryId));
        }

        /// <summary>Yang paling khusus menang — lihat <see cref="ResolveValueBoundAsync"/>.</summary>
        private static LabValueBound? PickBound(IEnumerable<LabValueBound> kandidat, LabGenderScope genderScope) =>
            kandidat
                .OrderByDescending(x => x.GenderScope == genderScope && genderScope != LabGenderScope.All ? 1 : 0)
                .ThenByDescending(x => x.AgeCategoryId != null ? 1 : 0)
                .FirstOrDefault();

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

        /// <summary>
        /// Penanda rujukan (<c>r33</c> 28.3, <c>LAB-FE-015</c>). Batas yang diberikan wajib batas
        /// yang berlaku <b>saat hasil disimpan</b>.
        ///
        /// Contoh: Kalium 6,4 pada 3,5–5,1 → <c>High</c>; Hemoglobin 9,4 pada 13,0–17,0 →
        /// <c>Low</c>; Protein urin <c>+2</c> yang ditandai di luar rujukan →
        /// <c>OutOfReference</c>. Nilai kritis tidak dihitung — itu <c>S5</c>.
        /// </summary>
        private static LabReferenceFlag? ResolveReferenceFlag(
            decimal? normalLow,
            decimal? normalHigh,
            decimal? resultNumeric,
            bool? optionIsOutOfReference)
        {
            if (optionIsOutOfReference.HasValue)
            {
                return optionIsOutOfReference.Value ? LabReferenceFlag.OutOfReference : LabReferenceFlag.Normal;
            }

            if (!resultNumeric.HasValue) return null;
            if (!normalLow.HasValue && !normalHigh.HasValue) return null;

            if (normalLow.HasValue && resultNumeric.Value < normalLow.Value) return LabReferenceFlag.Low;
            if (normalHigh.HasValue && resultNumeric.Value > normalHigh.Value) return LabReferenceFlag.High;

            return LabReferenceFlag.Normal;
        }


        // =================================================================
        // Kelengkapan dan konsultasi hasil Patologi Klinik dan Mikrobiologi
        // BE-LAB-54 (slice S4b), dijadikan netral disiplin oleh BE-LAB-67
        // (LAB-API-v1 r33 bagian 28; LAB-DEC-097, LAB-DEC-106, LAB-DEC-135; VAL-107, VAL-108,
        // VAL-122)
        //
        // KETIGA METODE DI BAWAH TIDAK MERILIS APA PUN. FinalizedAt mencatat bahwa penulisnya
        // menyatakan selesai — sebuah fakta. Rilis adalah S4/S4d.
        //
        // Itu sebabnya setiap respons membawa IsReleased dan DeliveryBlockedReason: supaya
        // pemanggil nol perlu MENYIMPULKAN bahwa Final sama dengan rilis.
        // =================================================================

        /// <summary>
        /// Menyatakan penulisan hasil selesai — <b>bukan</b> merilis (<c>LAB-DEC-097</c>).
        /// Berlaku bagi Patologi Klinik dan Mikrobiologi.
        /// </summary>
        public async Task<LabExaminationCompletionResponse> FinalizeResultAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var examination = await _dbContext.LabExaminations
                .Include(x => x.Procedure)
                .Include(x => x.LabOrder)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            EnsureNotAnatomicalPathology(examination);

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

            // Dua Final bersamaan sama-sama membaca FinalizedAt kosong dan sama-sama lolos
            // penjaga di atas. Kenaikan Version inilah yang membuat yang kedua bentrok.
            examination.Version += 1;

            await SaveResultWriteAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.FinalizeResult",
                "Penulisan hasil dinyatakan selesai. Ini BUKAN rilis.",
                new { examination.Id, examination.LabOrderId, examination.FinalizedAt });

            return BuildCompletionResponse(examination);
        }

        /// <summary>
        /// Membuka kembali penulisan hasil sebelum rilis (<c>LAB-DEC-097</c>). Berlaku bagi
        /// Patologi Klinik dan Mikrobiologi.
        /// </summary>
        public async Task<LabExaminationCompletionResponse> ReopenResultAsync(
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

            EnsureNotAnatomicalPathology(examination);

            // VAL-107. Membuka kembali sesuatu yang belum pernah ditutup adalah permintaan yang
            // tidak punya arti, dan membiarkannya lolos akan menaikkan ReopenCount pada hasil
            // yang nol pernah difinalkan.
            if (examination.FinalizedAt is null)
            {
                throw new LabExaminationValidationException(
                    "Hasil ini belum pernah dinyatakan selesai, jadi tidak ada yang perlu dibuka kembali.");
            }

            // VAL-136. Hasil yang sudah divalidasi tidak dibuka analis sendiri — yang
            // mengembalikannya adalah pemvalidasi atau perilis lewat "Kembalikan ke analis",
            // yang mencatat alasan dari daftar koreksi (LAB-DEC-135 butir 3, INV-48).
            if (examination.ValidatedAt is not null)
            {
                throw new LabExaminationConflictException(
                    "Hasil ini sudah divalidasi. Minta pemvalidasi atau perilis mengembalikannya bila perlu diubah.");
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
                // LAB-PERM-v1 rev 10 bagian 12.6: nama netral untuk baris BARU saja. Baris lama
                // bernama LabExamination.ReopenMicrobiologyResult sengaja tidak diubah.
                Action = "LabExamination.ReopenResult",
                FromStatus = "Finalized",
                ToStatus = "Draft",
                ReasonNote = alasan,
                ActorUserId = actorUserId,
                OccurredAt = now
            });

            // Reopen dan Validasi pada detik yang sama sama-sama membaca FinalizedAt terisi dan
            // ValidatedAt kosong. Kenaikan Version inilah yang membuat yang kedua bentrok —
            // tanpanya dapat tersimpan hasil tervalidasi yang FinalizedAt-nya kosong (20.1).
            examination.Version += 1;

            await SaveResultWriteAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.ReopenResult",
                "Penulisan hasil dibuka kembali sebelum rilis.",
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
                .Include(x => x.LabOrder)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

            EnsureNotAnatomicalPathology(examination);

            // VAL-121.
            if (examination.FinalizedAt is not null)
            {
                throw new LabExaminationConflictException(
                    "Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu sebelum mencatat konsultasi.");
            }

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
            examination.Version += 1;

            await SaveResultWriteAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.RecordConsultation",
                "Fakta konsultasi hasil dicatat. Ini BUKAN izin pengiriman.",
                new { examination.Id, examination.LabOrderId, examination.ConsultedAt });

            return BuildCompletionResponse(examination);
        }

        /// <summary>
        /// <c>VAL-122</c>. Final, Reopen, dan konsultasi ditolak pada pemeriksaan Patologi
        /// Anatomi: order PA punya baris <see cref="LabExamination"/>, tetapi hasilnya tinggal
        /// di laporan per pesanan (<c>LAB-DEC-085</c>). Tanpa penjaga ini Final atas baris itu
        /// menjawab "hasil belum diisi" — benar secara teknis, menyesatkan bagi petugas.
        ///
        /// Disiplin dibaca dari order; order lama yang berdisiplin kosong jatuh ke disiplin
        /// katalog pemeriksaannya, sumber yang sama yang menurunkan disiplin order
        /// (<c>LAB-DEC-048</c>). Pemanggil wajib memuat <c>LabOrder</c> dan <c>Procedure</c>.
        /// </summary>
        private static void EnsureNotAnatomicalPathology(LabExamination examination)
        {
            if (ResolveDiscipline(examination) == LabDiscipline.AnatomicalPathology)
            {
                throw new LabExaminationValidationException(
                    "Hasil Patologi Anatomi diselesaikan lewat laporan Patologi Anatomi.");
            }
        }

        /// <summary>
        /// Disiplin sebuah pemeriksaan: dari order, lalu jatuh ke disiplin katalog pemeriksaannya
        /// bagi order lama yang berdisiplin kosong (<c>LAB-DEC-048</c>). Satu tempat, dipakai
        /// penjaga Patologi Anatomi dan tindakan validasi, supaya keduanya tidak pernah berbeda
        /// pendapat tentang disiplin baris yang sama. Pemanggil wajib memuat <c>LabOrder</c> dan
        /// <c>Procedure</c>.
        /// </summary>
        internal static LabDiscipline? ResolveDiscipline(LabExamination examination) =>
            examination.LabOrder?.Discipline ?? examination.Procedure?.LabDiscipline;

        /// <summary>
        /// Keadaan hasil <b>turunan</b> (<c>LAB-DEC-080</c>) — bukan kolom tersimpan, dan bukan
        /// <c>LabExaminationStatus</c>, yang bersumbu kelayakan tagih.
        /// </summary>
        internal static LabResultStatus DeriveResultStatus(LabExamination examination) =>
            DeriveResultStatus(
                examination.ReleasedAt,
                examination.ValidatedAt,
                examination.FinalizedAt,
                examination.ResultEnteredAt);

        /// <summary>
        /// Penyaring kueri yang memilih pemeriksaan ber-<paramref name="status"/> tertentu —
        /// <b>cermin persis</b> <see cref="DeriveResultStatus(DateTime?, DateTime?, DateTime?, DateTime?)"/>
        /// dalam bentuk yang dapat diterjemahkan ke SQL. Keduanya berdampingan supaya antrean
        /// validasi (<c>BE-LAB-77</c>) tidak pernah menurunkan keadaan dengan rumus lain: bila
        /// salah satu berubah, yang lain wajib ikut.
        /// </summary>
        internal static Expression<Func<LabExamination, bool>> HasResultStatus(LabResultStatus status) => status switch
        {
            LabResultStatus.Released => x => x.ReleasedAt != null,
            LabResultStatus.Validated => x => x.ReleasedAt == null && x.ValidatedAt != null,
            LabResultStatus.Final => x => x.ReleasedAt == null && x.ValidatedAt == null && x.FinalizedAt != null,
            LabResultStatus.Draft => x => x.ReleasedAt == null && x.ValidatedAt == null && x.FinalizedAt == null && x.ResultEnteredAt != null,
            _ => x => x.ReleasedAt == null && x.ValidatedAt == null && x.FinalizedAt == null && x.ResultEnteredAt == null
        };

        /// <summary>
        /// Penanda rujukan bagi sekumpulan pemeriksaan — <b>penghitung yang sama</b> dengan lembar
        /// hasil (batas yang berlaku saat hasil disimpan). Dipakai antrean validasi supaya penanda
        /// di antrean dan di halaman hasil tidak pernah berbeda.
        /// </summary>
        internal async Task<Dictionary<Guid, LabReferenceFlag>> ResolveReferenceFlagsByIdAsync(
            IReadOnlyList<Guid> examinationIds,
            CancellationToken cancellationToken)
        {
            if (examinationIds.Count == 0)
            {
                return new Dictionary<Guid, LabReferenceFlag>();
            }

            var rows = await ProjectResultFormRows(
                    _dbContext.LabExaminations.Where(x => examinationIds.Contains(x.Id)))
                .ToListAsync(cancellationToken);

            return await ResolveReferenceFlagsAsync(rows, cancellationToken);
        }

        /// <summary>Penurun yang sama bagi baris hasil proyeksi (jalur baca).</summary>
        internal static LabResultStatus DeriveResultStatus(
            DateTime? releasedAt,
            DateTime? validatedAt,
            DateTime? finalizedAt,
            DateTime? resultEnteredAt)
        {
            if (releasedAt is not null) return LabResultStatus.Released;
            if (validatedAt is not null) return LabResultStatus.Validated;
            if (finalizedAt is not null) return LabResultStatus.Final;
            if (resultEnteredAt is not null) return LabResultStatus.Draft;

            return LabResultStatus.NotEntered;
        }

        /// <summary>
        /// Pesan <c>409</c> bila baris pemeriksaan berubah di antara dibaca dan disimpan
        /// (<c>LAB-API-v1</c> <c>r33</c> 28.2, <c>02-backend-architecture.md</c> 20.1).
        /// </summary>
        internal const string ResultConcurrencyMessage =
            "Hasil ini baru saja diubah orang lain. Muat ulang lalu ulangi.";

        /// <summary>
        /// <c>VAL-120</c>. Hasil yang sudah Final tidak diisi ulang tanpa Reopen
        /// (<c>LAB-DEC-147</c>). Pemanggil wajib memeriksanya <b>sebelum</b> satu pun perubahan
        /// dibuat — pada Mikrobiologi itu berarti sebelum isolat lama ditandai hapus.
        ///
        /// Satu salinan untuk Patologi Klinik dan Mikrobiologi: dua salinan aturan yang sama
        /// pasti bercabang.
        /// </summary>
        internal static void EnsureResultNotFinalized(LabExamination examination)
        {
            if (examination.FinalizedAt is not null)
            {
                throw new LabExaminationConflictException(
                    "Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu bila perlu diubah.");
            }
        }

        /// <summary>
        /// Simpan bagi penulisan hasil yang menaikkan <see cref="LabExamination.Version"/>.
        /// Token yang dinaikkan hanya berguna bila bentrokannya dijawab <c>409</c>, bukan
        /// dibiarkan menjadi <c>500</c>.
        /// </summary>
        private async Task SaveResultWriteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new LabExaminationConflictException(ResultConcurrencyMessage);
            }
        }

        /// <summary>
        /// Menyusun respons kelengkapan. <b>Satu penyusun</b> bagi Final, Reopen, konsultasi,
        /// Validasi, dan Rilis (<c>LabResultValidationService</c>), supaya ruasnya tidak pernah
        /// bercabang.
        ///
        /// <see cref="LabExaminationCompletionResponse.IsReleased"/> dibaca dari
        /// <c>ReleasedAt</c>. Hanya Patologi Klinik yang dapat dirilis hari ini, sehingga bagi
        /// Mikrobiologi ruas itu tetap salah sampai <c>S4d</c>.
        /// </summary>
        /// <param name="examination">Pemeriksaan yang baru saja ditulis.</param>
        /// <param name="validatedByName">
        /// Nama pemvalidasi. Hanya jalur Validasi dan Rilis yang mengirimnya: pada Final, Reopen,
        /// dan konsultasi hasilnya belum pernah tervalidasi — ketiganya ditolak bila sudah.
        /// </param>
        /// <param name="releasedByName">Nama perilis — hanya jalur Rilis.</param>
        internal static LabExaminationCompletionResponse BuildCompletionResponse(
            LabExamination examination,
            string? validatedByName = null,
            string? releasedByName = null)
        {
            var penandaValidasi = ValidationExceptionMarker(
                examination.ValidationExceptionReasonId, validatedByName, examination.ValidationExceptionReasonNameSnapshot);

            var penandaRilis = ReleaseExceptionMarker(
                examination.ReleaseExceptionReasonId, releasedByName, examination.ReleaseExceptionReasonNameSnapshot);

            var dirilis = examination.ReleasedAt is not null;

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
                ResultStatus = DeriveResultStatus(examination).ToString(),
                IsValidated = examination.ValidatedAt is not null,
                ValidatedAt = examination.ValidatedAt,
                ValidatedByUserId = examination.ValidatedByUserId,
                ValidatedByName = examination.ValidatedAt is not null ? validatedByName : null,
                ValidatedByPositionName = examination.ValidatedByPositionNameSnapshot,
                ValidationExceptionMarker = penandaValidasi,
                IsReleased = dirilis,
                ReleasedAt = examination.ReleasedAt,
                ReleasedByUserId = examination.ReleasedByUserId,
                ReleasedByName = dirilis ? releasedByName : null,
                ReleasedByPositionName = examination.ReleasedByPositionNameSnapshot,
                ReleaseExceptionMarker = penandaRilis,
                DeliveryBlockedReason = dirilis ? null : NotReleasedMessage
            };
        }

        /// <summary>Kenapa hasil yang belum dirilis tidak boleh dikirim — satu teks bagi setiap respons.</summary>
        internal const string NotReleasedMessage =
            "Hasil ini belum dirilis, sehingga belum boleh dikirim kepada pasien.";

        /// <summary>
        /// Bunyi penanda pengecualian validasi, disetujui kata per kata (20.10 butir 5,
        /// <c>LAB-DEC-003</c>). <b>Satu penyusun</b> bagi respons tindakan dan jalur baca. Alasan
        /// dibaca dari snapshot, sehingga mengganti nama alasan kelak tidak mengubah penanda lama.
        /// </summary>
        internal static string? ValidationExceptionMarker(Guid? reasonId, string? validatorName, string? reasonNameSnapshot) =>
            reasonId is not null && validatorName is not null
                ? $"Divalidasi oleh pengisi sendiri — {validatorName} — {reasonNameSnapshot}"
                : null;

        /// <summary>Bunyi penanda pengecualian rilis (20.10 butir 5). Lihat <see cref="ValidationExceptionMarker"/>.</summary>
        internal static string? ReleaseExceptionMarker(Guid? reasonId, string? releaserName, string? reasonNameSnapshot) =>
            reasonId is not null && releaserName is not null
                ? $"Dirilis oleh pemvalidasi sendiri — {releaserName} — {reasonNameSnapshot}"
                : null;

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
