using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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
            var entity = await _dbContext.LabExaminations
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan terpesan tidak ditemukan.");

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

            entity.ExaminationStatus = LabExaminationStatus.Cancelled;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.Version += 1;

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
                    Alasan = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                    ActorUserId = actorUserId
                });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        // =================================================================
        // Pembantu
        // =================================================================

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

    /// <summary>Bentrokan dengan keadaan yang sudah berjalan. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabExaminationConflictException(string message) : Exception(message);
}
