using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Baca, pakai, dan kembalikan plafon benefit layanan RS per orang (BE-FIN-104;
    /// FIN-INTEGRATION-1.9 R.3, INT-BIL-HR-001; FIN-VAL-255..258; FIN-STATE-1.9 R.3).
    ///
    /// HR pemilik saldo; Billing pemilik transaksi. Pakai dan Kembalikan TIDAK membuka transaksi
    /// dan TIDAK memanggil SaveChanges: keduanya wajib dipanggil di dalam transaksi pemanggil
    /// (BillingFinalizationService), sehingga pengurangan saldo dan penerbitan serah terima jatuh
    /// atau berhasil bersama (R.3.4, invariant R.10 butir 3). Tanpa transaksi pemanggil, layanan
    /// menolak bekerja (fail-closed).
    ///
    /// Konkurensi: kunci advisory per kunci serah terima lalu per orang (urutan tetap, bebas
    /// deadlock), ditambah satu pernyataan UPDATE bersyarat "RemainingAmount >= amount" sehingga
    /// saldo tidak pernah negatif walaupun dua finalisasi berjalan bersamaan. Check constraint
    /// saldo dan indeks unik (SourceHandoffKey, MovementType) menjadi lapis terakhir di database.
    /// </summary>
    public sealed class EmployeeBenefitBalanceService
    {
        private const string LogCategory = "Corporate.HumanResource.BenefitManagement.Balance";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmployeeBenefitEligibilityService _eligibilityService;

        public EmployeeBenefitBalanceService(
            ApplicationDbContext dbContext, LoggerService loggerService, EmployeeBenefitEligibilityService eligibilityService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _eligibilityService = eligibilityService;
        }

        // ------------------------------------------------------------------------------------
        // R.3.1 BacaPlafon — tidak mengubah data
        // ------------------------------------------------------------------------------------

        public async Task<BenefitBalanceReadResult> ReadAsync(BenefitBalanceReadRequest request, CancellationToken cancellationToken)
        {
            // Kelayakan dijalankan ulang: masa berlaku bisa berakhir antara pendaftaran dan finalisasi.
            var eligibility = await _eligibilityService.CheckAsync(new BenefitEligibilityRequest
            {
                EmployeeId = request.EmployeeId,
                FamilyMemberId = request.FamilyMemberId,
                ServiceDate = request.AsOfDate
            }, cancellationToken);

            if (!eligibility.IsEligible)
                return new BenefitBalanceReadResult { IsUsable = false, ReasonCode = eligibility.ReasonCode, Message = eligibility.Message };

            var result = new BenefitBalanceReadResult
            {
                IsUsable = true,
                ReasonCode = BenefitEligibilityReasonCodes.Ok,
                BenefitPersonKind = eligibility.BenefitPersonKind,
                BenefitPersonId = eligibility.BenefitPersonId,
                EnrollmentId = eligibility.EnrollmentId,
                RelationshipCode = eligibility.RelationshipCode,
                PlanCode = eligibility.PlanCode
            };

            var personId = eligibility.BenefitPersonId!.Value;
            if (eligibility.BenefitPersonKind == BenefitPersonKinds.Enrollment)
            {
                var balance = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                    .Where(x => x.Id == personId)
                    .Select(x => new { x.CoverageLimitAmount, x.UsedAmount, x.RemainingAmount })
                    .SingleAsync(cancellationToken);
                result.LimitAmount = balance.CoverageLimitAmount;
                result.UsedAmount = balance.UsedAmount;
                result.RemainingAmount = balance.RemainingAmount;
            }
            else
            {
                var balance = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .Where(x => x.Id == personId)
                    .Select(x => new { x.CoverageLimitAmount, x.UsedAmount, x.RemainingAmount })
                    .SingleAsync(cancellationToken);
                result.LimitAmount = balance.CoverageLimitAmount;
                result.UsedAmount = balance.UsedAmount;
                result.RemainingAmount = balance.RemainingAmount;
            }
            return result;
        }

        // ------------------------------------------------------------------------------------
        // R.3.2 PakaiPlafon — di dalam transaksi pemanggil
        // ------------------------------------------------------------------------------------

        public async Task<BenefitBalanceUseResult> UseAsync(BenefitBalanceUseRequest request, CancellationToken cancellationToken)
        {
            // FIN-VAL-255: nominal dan kunci yang tidak sah adalah kesalahan pemrograman pemanggil.
            if (request.Amount <= 0m)
                throw new ArgumentOutOfRangeException(nameof(request), "Nominal pemakaian plafon harus lebih dari nol.");
            if (request.SourceHandoffKey == Guid.Empty || request.InvoiceId == Guid.Empty || request.BenefitPersonId == Guid.Empty)
                throw new ArgumentException("SourceHandoffKey, InvoiceId, dan BenefitPersonId wajib diisi.", nameof(request));
            if (request.BenefitPersonKind is not (BenefitPersonKinds.Enrollment or BenefitPersonKinds.Dependent))
                throw new ArgumentException("BenefitPersonKind harus ENROLLMENT atau DEPENDENT.", nameof(request));

            EnsureCallerTransaction();
            await AcquireLockAsync(KeyLock(request.SourceHandoffKey), cancellationToken);

            // Idempotensi (FIN-STATE-1.9 R.3, FIN-VAL-257).
            var existing = await FindMovementsByKeyAsync(request.SourceHandoffKey, cancellationToken);
            var existingUse = existing.FirstOrDefault(x => x.MovementType == BenefitMovementTypes.Use);
            if (existingUse is not null)
            {
                var currentRemaining = await ReadRemainingAsync(existingUse.BenefitPersonKind, PersonIdOf(existingUse), cancellationToken);
                // Kunci yang sudah dikembalikan tidak boleh dipakai lagi; pemakaian berikutnya memakai kunci baru.
                if (existing.Any(x => x.MovementType == BenefitMovementTypes.Return))
                    return KeyConflict(currentRemaining);
                var samePerson = existingUse.BenefitPersonKind == request.BenefitPersonKind
                    && PersonIdOf(existingUse) == request.BenefitPersonId;
                if (samePerson && existingUse.Amount == request.Amount && existingUse.InvoiceId == request.InvoiceId)
                {
                    return new BenefitBalanceUseResult
                    {
                        Result = BenefitUseResults.AlreadyApplied,
                        MovementId = existingUse.Id,
                        RemainingAmount = currentRemaining
                    };
                }
                return KeyConflict(currentRemaining);
            }

            await AcquireLockAsync(PersonLock(request.BenefitPersonId), cancellationToken);

            Guid enrollmentId;
            Guid? dependentId = null;
            if (request.BenefitPersonKind == BenefitPersonKinds.Enrollment)
            {
                var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == request.BenefitPersonId, cancellationToken);
                if (enrollment is null || !EmployeeBenefitEligibilityService.IsEnrollmentUsable(enrollment))
                    return NotUsable(enrollment?.RemainingAmount ?? 0m);
                enrollmentId = enrollment.Id;
            }
            else
            {
                var dependent = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == request.BenefitPersonId, cancellationToken);
                if (dependent is null || !EmployeeBenefitEligibilityService.IsDependentUsable(dependent))
                    return NotUsable(dependent?.RemainingAmount ?? 0m);
                var parentActive = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                    .AnyAsync(x => x.Id == dependent.EmployeeBenefitEnrollmentId && !x.IsDelete
                        && x.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active, cancellationToken);
                if (!parentActive) return NotUsable(dependent.RemainingAmount);
                enrollmentId = dependent.EmployeeBenefitEnrollmentId;
                dependentId = dependent.Id;
            }

            // FIN-VAL-256: satu pernyataan UPDATE bersyarat; nol baris berarti sisa tidak cukup.
            var amount = request.Amount;
            var actor = request.ActorUserId ?? Guid.Empty;
            DateTime? now = DateTime.UtcNow;
            var rowVersion = Guid.NewGuid();
            int affected = request.BenefitPersonKind == BenefitPersonKinds.Enrollment
                ? await _dbContext.TrxEmployeeBenefitEnrollments
                    .Where(x => x.Id == request.BenefitPersonId && !x.IsDelete
                        && x.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active
                        && x.RemainingAmount >= amount)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.RemainingAmount, x => x.RemainingAmount - amount)
                        .SetProperty(x => x.UsedAmount, x => x.UsedAmount + amount)
                        .SetProperty(x => x.RowVersion, rowVersion)
                        .SetProperty(x => x.UpdateDateTime, now)
                        .SetProperty(x => x.UpdateBy, actor), cancellationToken)
                : await _dbContext.TrxEmployeeBenefitDependents
                    .Where(x => x.Id == request.BenefitPersonId && !x.IsDelete
                        && x.DependentStatus == EmployeeBenefitDependentStatuses.Verified
                        && x.RemainingAmount >= amount)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.RemainingAmount, x => x.RemainingAmount - amount)
                        .SetProperty(x => x.UsedAmount, x => x.UsedAmount + amount)
                        .SetProperty(x => x.RowVersion, rowVersion)
                        .SetProperty(x => x.UpdateDateTime, now)
                        .SetProperty(x => x.UpdateBy, actor), cancellationToken);

            var remainingAfter = await ReadRemainingAsync(request.BenefitPersonKind, request.BenefitPersonId, cancellationToken);
            if (affected == 0)
            {
                return new BenefitBalanceUseResult
                {
                    Result = BenefitUseResults.Insufficient,
                    Message = $"Sisa plafon tidak cukup. Sisa saat ini Rp {remainingAfter.ToString("N0", CultureInfo.GetCultureInfo("id-ID"))}.",
                    RemainingAmount = remainingAfter
                };
            }

            var movement = new HrdBenefitBalanceMovement
            {
                EmployeeBenefitEnrollmentId = enrollmentId,
                EmployeeBenefitDependentId = dependentId,
                BenefitPersonKind = request.BenefitPersonKind,
                MovementType = BenefitMovementTypes.Use,
                SourceHandoffKey = request.SourceHandoffKey,
                InvoiceId = request.InvoiceId,
                Amount = amount,
                BalanceBefore = remainingAfter + amount,
                BalanceAfter = remainingAfter,
                OccurredAt = DateTimeOffset.UtcNow,
                ActorUserId = request.ActorUserId,
                CreateBy = actor
            };
            _dbContext.Set<HrdBenefitBalanceMovement>().Add(movement);
            await RefreshTrackedAsync(request.BenefitPersonKind, request.BenefitPersonId, cancellationToken);

            await AuditAsync("UseStaged", movement);
            return new BenefitBalanceUseResult { Result = BenefitUseResults.Applied, MovementId = movement.Id, RemainingAmount = remainingAfter };
        }

        // ------------------------------------------------------------------------------------
        // R.3.3 KembalikanPlafon — di dalam transaksi pemanggil
        // ------------------------------------------------------------------------------------

        public async Task<BenefitBalanceReturnResult> ReturnAsync(BenefitBalanceReturnRequest request, CancellationToken cancellationToken)
        {
            if (request.SourceHandoffKey == Guid.Empty)
                throw new ArgumentException("SourceHandoffKey wajib diisi.", nameof(request));

            EnsureCallerTransaction();
            await AcquireLockAsync(KeyLock(request.SourceHandoffKey), cancellationToken);

            // FIN-VAL-258: hanya untuk USE yang ada, selalu penuh, dan hanya sekali.
            var existing = await FindMovementsByKeyAsync(request.SourceHandoffKey, cancellationToken);
            var use = existing.FirstOrDefault(x => x.MovementType == BenefitMovementTypes.Use);
            if (use is null) return new BenefitBalanceReturnResult { Result = BenefitReturnResults.NotFound };

            var personId = PersonIdOf(use);
            var previousReturn = existing.FirstOrDefault(x => x.MovementType == BenefitMovementTypes.Return);
            if (previousReturn is not null)
            {
                return new BenefitBalanceReturnResult
                {
                    Result = BenefitReturnResults.AlreadyReturned,
                    MovementId = previousReturn.Id,
                    Amount = previousReturn.Amount,
                    RemainingAmount = await ReadRemainingAsync(use.BenefitPersonKind, personId, cancellationToken)
                };
            }

            await AcquireLockAsync(PersonLock(personId), cancellationToken);

            // Pengembalian tidak bergantung status orang saat ini: yang dikembalikan adalah pemakaian
            // yang memang pernah terjadi, sehingga Used = Σ USE − Σ RETURN tetap benar (R.10 butir 2).
            var amount = use.Amount;
            var actor = request.ActorUserId ?? Guid.Empty;
            DateTime? now = DateTime.UtcNow;
            var rowVersion = Guid.NewGuid();
            int affected = use.BenefitPersonKind == BenefitPersonKinds.Enrollment
                ? await _dbContext.TrxEmployeeBenefitEnrollments
                    .Where(x => x.Id == personId && x.UsedAmount >= amount)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.RemainingAmount, x => x.RemainingAmount + amount)
                        .SetProperty(x => x.UsedAmount, x => x.UsedAmount - amount)
                        .SetProperty(x => x.RowVersion, rowVersion)
                        .SetProperty(x => x.UpdateDateTime, now)
                        .SetProperty(x => x.UpdateBy, actor), cancellationToken)
                : await _dbContext.TrxEmployeeBenefitDependents
                    .Where(x => x.Id == personId && x.UsedAmount >= amount)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.RemainingAmount, x => x.RemainingAmount + amount)
                        .SetProperty(x => x.UsedAmount, x => x.UsedAmount - amount)
                        .SetProperty(x => x.RowVersion, rowVersion)
                        .SetProperty(x => x.UpdateDateTime, now)
                        .SetProperty(x => x.UpdateBy, actor), cancellationToken);

            if (affected == 0)
            {
                // Saldo terpakai lebih kecil dari mutasi yang hendak dikembalikan: data tidak konsisten
                // dengan buku mutasi. Gagal tertutup; pemanggil membatalkan transaksinya.
                await _loggerService.WarningAsync(LogCategory, "ReturnInconsistent",
                    $"ReturnInconsistent UseMovementId={use.Id}", new { UseMovementId = use.Id, use.BenefitPersonKind });
                throw new EmployeeBenefitConflictException("Saldo plafon tidak konsisten dengan buku mutasi. Hubungi administrator.");
            }

            var remainingAfter = await ReadRemainingAsync(use.BenefitPersonKind, personId, cancellationToken);
            var movement = new HrdBenefitBalanceMovement
            {
                EmployeeBenefitEnrollmentId = use.EmployeeBenefitEnrollmentId,
                EmployeeBenefitDependentId = use.EmployeeBenefitDependentId,
                BenefitPersonKind = use.BenefitPersonKind,
                MovementType = BenefitMovementTypes.Return,
                SourceHandoffKey = use.SourceHandoffKey,
                InvoiceId = use.InvoiceId,
                Amount = amount,
                BalanceBefore = remainingAfter - amount,
                BalanceAfter = remainingAfter,
                OccurredAt = DateTimeOffset.UtcNow,
                ReversesMovementId = use.Id,
                ReasonText = Truncate(request.Reason, 500),
                ActorUserId = request.ActorUserId,
                CreateBy = actor
            };
            _dbContext.Set<HrdBenefitBalanceMovement>().Add(movement);
            await RefreshTrackedAsync(use.BenefitPersonKind, personId, cancellationToken);

            await AuditAsync("ReturnStaged", movement);
            return new BenefitBalanceReturnResult
            {
                Result = BenefitReturnResults.Returned,
                MovementId = movement.Id,
                Amount = amount,
                RemainingAmount = remainingAfter
            };
        }

        // ------------------------------------------------------------------------------------
        // Buku mutasi satu enrollment (FIN-API-1.10 R.1, GET /{id}/movements)
        // ------------------------------------------------------------------------------------

        public async Task<PagedResult<BenefitMovementResponse>> GetMovementsAsync(
            Guid enrollmentId, BenefitMovementPagedQuery query, CancellationToken cancellationToken)
        {
            var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                .Where(x => x.Id == enrollmentId && !x.IsDelete)
                .Select(x => new { x.Id, x.EmployeeId })
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Enrollment benefit tidak ditemukan.");

            if (!string.IsNullOrWhiteSpace(query.MovementType) && !BenefitMovementTypes.All.Contains(query.MovementType))
                throw new EmployeeBenefitTransitionException("Jenis mutasi harus USE atau RETURN.");

            var employeeName = enrollment.EmployeeId.HasValue
                ? await _dbContext.MstEmployees.AsNoTracking()
                    .Where(x => x.Id == enrollment.EmployeeId.Value)
                    .Select(x => x.FullName)
                    .SingleOrDefaultAsync(cancellationToken)
                : null;

            var source = from m in _dbContext.Set<HrdBenefitBalanceMovement>().AsNoTracking()
                         join d in _dbContext.TrxEmployeeBenefitDependents.AsNoTracking() on m.EmployeeBenefitDependentId equals d.Id into dJoin
                         from d in dJoin.DefaultIfEmpty()
                         join inv in _dbContext.BilInvoices.AsNoTracking() on m.InvoiceId equals inv.Id into invJoin
                         from inv in invJoin.DefaultIfEmpty()
                         where m.EmployeeBenefitEnrollmentId == enrollmentId
                         select new { m, DependentName = d != null ? d.DependentName : null, InvoiceNumber = inv != null ? inv.InvoiceNumber : null };

            if (!string.IsNullOrWhiteSpace(query.MovementType)) source = source.Where(x => x.m.MovementType == query.MovementType);
            if (query.From.HasValue)
            {
                var from = new DateTimeOffset(AppDateTimeHelper.OperationalDateToUtc(query.From.Value), TimeSpan.Zero);
                source = source.Where(x => x.m.OccurredAt >= from);
            }
            if (query.To.HasValue)
            {
                var toExclusive = new DateTimeOffset(AppDateTimeHelper.OperationalDateToUtc(query.To.Value.Date.AddDays(1)), TimeSpan.Zero);
                source = source.Where(x => x.m.OccurredAt < toExclusive);
            }

            var pageSize = Math.Clamp(query.PageSize, 1, 50);
            var pageNumber = Math.Max(query.PageNumber, 1);
            var total = await source.CountAsync(cancellationToken);
            var rows = await source
                .OrderByDescending(x => x.m.OccurredAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<BenefitMovementResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = rows.Select(x => new BenefitMovementResponse
                {
                    Id = x.m.Id,
                    MovementType = x.m.MovementType,
                    BenefitPersonKind = x.m.BenefitPersonKind,
                    BenefitPersonId = PersonIdOf(x.m),
                    PersonName = x.m.BenefitPersonKind == BenefitPersonKinds.Dependent ? x.DependentName : employeeName,
                    Amount = x.m.Amount,
                    BalanceBefore = x.m.BalanceBefore,
                    BalanceAfter = x.m.BalanceAfter,
                    InvoiceId = x.m.InvoiceId,
                    InvoiceNumber = x.InvoiceNumber,
                    OccurredAt = x.m.OccurredAt,
                    ReversesMovementId = x.m.ReversesMovementId,
                    ReasonText = x.m.ReasonText
                }).ToList()
            };
        }

        // ------------------------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------------------------

        private static Guid PersonIdOf(HrdBenefitBalanceMovement m) =>
            m.BenefitPersonKind == BenefitPersonKinds.Dependent ? m.EmployeeBenefitDependentId!.Value : m.EmployeeBenefitEnrollmentId;

        /// <summary>
        /// Mutasi untuk satu kunci: yang sudah tersimpan ditambah yang masih menunggu SaveChanges
        /// pemanggil, agar pemanggilan ulang di transaksi yang sama tetap idempoten.
        /// </summary>
        private async Task<List<HrdBenefitBalanceMovement>> FindMovementsByKeyAsync(Guid sourceHandoffKey, CancellationToken cancellationToken)
        {
            var stored = await _dbContext.Set<HrdBenefitBalanceMovement>().AsNoTracking()
                .Where(x => x.SourceHandoffKey == sourceHandoffKey)
                .ToListAsync(cancellationToken);
            var pending = _dbContext.Set<HrdBenefitBalanceMovement>().Local
                .Where(x => x.SourceHandoffKey == sourceHandoffKey && stored.All(s => s.Id != x.Id));
            return stored.Concat(pending).ToList();
        }

        private async Task<decimal> ReadRemainingAsync(string personKind, Guid personId, CancellationToken cancellationToken) =>
            personKind == BenefitPersonKinds.Enrollment
                ? await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                    .Where(x => x.Id == personId).Select(x => x.RemainingAmount).SingleOrDefaultAsync(cancellationToken)
                : await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .Where(x => x.Id == personId).Select(x => x.RemainingAmount).SingleOrDefaultAsync(cancellationToken);

        /// <summary>
        /// ExecuteUpdate melewati change tracker. Bila pemanggil kebetulan melacak baris saldo yang
        /// sama, nilainya dimuat ulang agar SaveChanges berikutnya tidak bekerja dengan saldo basi.
        /// </summary>
        private async Task RefreshTrackedAsync(string personKind, Guid personId, CancellationToken cancellationToken)
        {
            if (personKind == BenefitPersonKinds.Enrollment)
            {
                var entry = _dbContext.ChangeTracker.Entries<TrxEmployeeBenefitEnrollment>().FirstOrDefault(x => x.Entity.Id == personId);
                if (entry is not null) await entry.ReloadAsync(cancellationToken);
            }
            else
            {
                var entry = _dbContext.ChangeTracker.Entries<TrxEmployeeBenefitDependent>().FirstOrDefault(x => x.Entity.Id == personId);
                if (entry is not null) await entry.ReloadAsync(cancellationToken);
            }
        }

        private void EnsureCallerTransaction()
        {
            if (!_dbContext.Database.IsRelational() || _dbContext.Database.CurrentTransaction is null)
                throw new InvalidOperationException(
                    "Pemakaian dan pengembalian plafon wajib dijalankan di dalam transaksi pemanggil (FIN-INTEGRATION-1.9 R.3.4).");
        }

        private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
            _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

        private static string KeyLock(Guid sourceHandoffKey) => $"HRD_BENEFIT_MOVEMENT_{sourceHandoffKey:N}";

        private static string PersonLock(Guid personId) => $"HRD_BENEFIT_BALANCE_{personId:N}";

        private static BenefitBalanceUseResult KeyConflict(decimal remaining) => new()
        {
            Result = BenefitUseResults.KeyConflict,
            Message = "Kunci pemakaian plafon sudah dipakai. Hubungi administrator.",
            RemainingAmount = remaining
        };

        private static BenefitBalanceUseResult NotUsable(decimal remaining) => new()
        {
            Result = BenefitUseResults.NotUsable,
            Message = "Orang ini tidak lagi layak memakai plafon benefit menurut HR.",
            RemainingAmount = remaining
        };

        private static string? Truncate(string? value, int max) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= max ? value.Trim() : value.Trim()[..max];

        // QBE-LOG-001: hanya Id, jenis, dan nominal; tanpa nama, NIK, atau hubungan.
        // "Staged" karena mutasi baru tersimpan saat pemanggil memanggil SaveChanges dan commit.
        private Task AuditAsync(string action, HrdBenefitBalanceMovement m) =>
            _loggerService.AuditAsync(LogCategory, action, $"{action} MovementId={m.Id}", new
            {
                MovementId = m.Id,
                m.MovementType,
                m.BenefitPersonKind,
                m.EmployeeBenefitEnrollmentId,
                m.EmployeeBenefitDependentId,
                m.SourceHandoffKey,
                m.InvoiceId,
                m.Amount,
                m.ActorUserId
            });
    }
}
