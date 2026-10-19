using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Enrollment benefit pegawai (BE-FIN-103; FIN-API-1.10 R.1; FIN-STATE-1.9 R.1).
    /// Aggregate ber-lifecycle: Draft -> Submitted -> Approved -> Active, dibatalkan lewat aksi
    /// cancel. Saldo plafon pegawai sendiri ditetapkan saat Activate dari batas plan
    /// (FIN-DEC-207, FIN-VAL-250). Pengurangan dan pengembalian saldo BUKAN milik service ini
    /// (BE-FIN-104). Status Expired tidak dipasang oleh service ini; masa berlaku dibaca dari
    /// tanggal (IsWithinEffectivePeriod) sampai ada task penjadwal tersendiri.
    /// </summary>
    public sealed class EmployeeBenefitEnrollmentService
    {
        private const string LogCategory = "Corporate.HumanResource.BenefitManagement.Enrollment";
        private const string SequenceKey = "HRD_BENEFIT_ENROLLMENT";
        private const string NumberPrefix = "BEN";
        private static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly NumberSeriesAllocator _numberAllocator;
        private readonly WorkforceBenefitOwnerResolver _ownerResolver;

        public EmployeeBenefitEnrollmentService(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            NumberSeriesAllocator numberAllocator,
            WorkforceBenefitOwnerResolver? ownerResolver = null)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _numberAllocator = numberAllocator;
            _ownerResolver = ownerResolver ?? new WorkforceBenefitOwnerResolver(dbContext);
        }

        // ------------------------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------------------------

        public EmployeeBenefitEnrollmentFilterMetadataResponse GetFilterMetadata() => new()
        {
            EnrollmentStatuses = EmployeeBenefitEnrollmentStatuses.All
                .Select(x => new EnrollmentFilterOptionResponse { Value = x, Label = x }).ToList(),
            DependentStatuses = EmployeeBenefitDependentStatuses.All
                .Select(x => new EnrollmentFilterOptionResponse { Value = x, Label = x }).ToList()
        };

        public async Task<EmployeeBenefitEnrollmentSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
        {
            var counts = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                .Where(x => !x.IsDelete)
                .GroupBy(x => x.EnrollmentStatus)
                .Select(g => new EnrollmentStatusCountResponse { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return new EmployeeBenefitEnrollmentSummaryResponse { Total = counts.Sum(x => x.Count), ByStatus = counts };
        }

        public async Task<PagedResult<EmployeeBenefitEnrollmentListResponse>> GetPagedAsync(
            EmployeeBenefitEnrollmentPagedQuery query, CancellationToken cancellationToken)
        {
            // Pemilik dibaca lewat WorkforceProfileId (bukan EmployeeId) sehingga enrollment dokter tetap menampilkan
            // nama dan nomor dokter. MstEmployee/MstDoctor masing-masing satu per profil (unique index), jadi
            // join kiri ini tidak menggandakan baris.
            var source = from e in _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                         join emp in _dbContext.MstEmployees.AsNoTracking() on e.WorkforceProfileId equals emp.WorkforceProfileId into empJoin
                         from emp in empJoin.DefaultIfEmpty()
                         join doc in _dbContext.MstDoctors.AsNoTracking() on e.WorkforceProfileId equals doc.WorkforceProfileId into docJoin
                         from doc in docJoin.DefaultIfEmpty()
                         join wfp in _dbContext.MstWorkforceProfiles.AsNoTracking() on e.WorkforceProfileId equals wfp.Id into wfpJoin
                         from wfp in wfpJoin.DefaultIfEmpty()
                         join plan in _dbContext.MstBenefitPlans.AsNoTracking() on e.BenefitPlanId equals plan.Id
                         where !e.IsDelete
                         select new { e, emp, doc, wfp, plan };

            if (query.EmployeeId.HasValue) source = source.Where(x => x.e.EmployeeId == query.EmployeeId.Value);
            if (query.WorkforceProfileId.HasValue) source = source.Where(x => x.e.WorkforceProfileId == query.WorkforceProfileId.Value);
            if (query.BenefitPlanId.HasValue) source = source.Where(x => x.e.BenefitPlanId == query.BenefitPlanId.Value);
            if (!string.IsNullOrWhiteSpace(query.EnrollmentStatus)) source = source.Where(x => x.e.EnrollmentStatus == query.EnrollmentStatus);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                source = source.Where(x =>
                    x.e.EnrollmentNumber.ToLower().Contains(keyword)
                    || (x.emp != null && (x.emp.EmployeeNumber.ToLower().Contains(keyword) || x.emp.FullName.ToLower().Contains(keyword)))
                    || (x.doc != null && (x.doc.DoctorNumber.ToLower().Contains(keyword) || x.doc.FullName.ToLower().Contains(keyword))));
            }

            var pageSize = Math.Clamp(query.PageSize, 1, 50);
            var pageNumber = Math.Max(query.PageNumber, 1);
            var total = await source.CountAsync(cancellationToken);
            var rows = await source
                .OrderByDescending(x => x.e.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.e.Id,
                    x.e.EnrollmentNumber,
                    x.e.WorkforceProfileId,
                    x.e.EmployeeId,
                    UserType = x.wfp != null ? (QuilvianSystemBackend.Enums.UserType?)x.wfp.UserType : null,
                    EmployeeNumber = x.emp != null ? x.emp.EmployeeNumber : null,
                    EmployeeName = x.emp != null ? x.emp.FullName : null,
                    DoctorId = x.doc != null ? (Guid?)x.doc.Id : null,
                    DoctorNumber = x.doc != null ? x.doc.DoctorNumber : null,
                    DoctorName = x.doc != null ? x.doc.FullName : null,
                    x.e.BenefitPlanId,
                    x.plan.BenefitPlanCode,
                    x.plan.BenefitPlanName,
                    x.e.EnrollmentStatus,
                    x.e.EffectiveStartDate,
                    x.e.EffectiveEndDate,
                    x.e.RemainingAmount,
                    x.e.ActiveDependentCount
                })
                .ToListAsync(cancellationToken);

            var items = rows.Select(x =>
            {
                // Jenis pemilik mengikuti jenis SDM pada profil; tanpa profil dibaca dari data peran yang ada.
                var isDoctor = x.UserType == QuilvianSystemBackend.Enums.UserType.PermanentDoctor
                    || (x.UserType is null && x.EmployeeName is null && x.DoctorName is not null);
                var number = isDoctor ? (x.DoctorNumber ?? x.EmployeeNumber) : (x.EmployeeNumber ?? x.DoctorNumber);
                var name = isDoctor ? (x.DoctorName ?? x.EmployeeName) : (x.EmployeeName ?? x.DoctorName);
                return new EmployeeBenefitEnrollmentListResponse
                {
                    Id = x.Id,
                    EnrollmentNumber = x.EnrollmentNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    OwnerType = isDoctor ? WorkforceBenefitOwnerTypes.Doctor : WorkforceBenefitOwnerTypes.Employee,
                    EmployeeId = x.EmployeeId,
                    DoctorId = x.DoctorId,
                    StaffNumber = number,
                    StaffName = name,
                    EmployeeNumber = number,
                    EmployeeName = name,
                    BenefitPlanId = x.BenefitPlanId,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    EnrollmentStatus = x.EnrollmentStatus,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    RemainingAmount = x.RemainingAmount,
                    ActiveDependentCount = x.ActiveDependentCount
                };
            }).ToList();

            return new PagedResult<EmployeeBenefitEnrollmentListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<EmployeeBenefitEnrollmentResponse> GetByIdAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Enrollment benefit tidak ditemukan.");
            return await MapDetailAsync(enrollment, actorUserId, cancellationToken);
        }

        /// <summary>Ringkasan benefit layanan RS satu pegawai (kontrak lama berbasis EmployeeId): enrollment Medical aktif dan saldo per orang.</summary>
        public async Task<EmployeeBenefitSummaryResponse> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
        {
            var resolution = await _ownerResolver.ResolveByEmployeeAsync(employeeId, cancellationToken);
            if (!resolution.IsResolved) throw new KeyNotFoundException("Pegawai tidak ditemukan.");
            return await BuildSummaryAsync(resolution.Owner!, cancellationToken);
        }

        /// <summary>Ringkasan benefit layanan RS satu pemilik (karyawan atau dokter tetap) berdasarkan WorkforceProfileId.</summary>
        public async Task<EmployeeBenefitSummaryResponse> GetByWorkforceProfileAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var resolution = await _ownerResolver.ResolveByWorkforceProfileAsync(workforceProfileId, cancellationToken);
            if (!resolution.IsResolved) throw new KeyNotFoundException(resolution.Message ?? "SDM tidak ditemukan.");
            return await BuildSummaryAsync(resolution.Owner!, cancellationToken);
        }

        private async Task<EmployeeBenefitSummaryResponse> BuildSummaryAsync(WorkforceBenefitOwner owner, CancellationToken cancellationToken)
        {
            var response = new EmployeeBenefitSummaryResponse
            {
                WorkforceProfileId = owner.WorkforceProfileId,
                OwnerType = owner.OwnerType,
                EmployeeId = owner.EmployeeId,
                DoctorId = owner.DoctorId,
                StaffNumber = owner.StaffNumber,
                StaffName = owner.StaffName,
                EmployeeNumber = owner.StaffNumber,
                EmployeeName = owner.StaffName
            };

            var active = await (from e in _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                                join plan in _dbContext.MstBenefitPlans.AsNoTracking() on e.BenefitPlanId equals plan.Id
                                join type in _dbContext.MstBenefitTypes.AsNoTracking() on plan.BenefitTypeId equals type.Id
                                where !e.IsDelete
                                      && e.WorkforceProfileId == owner.WorkforceProfileId
                                      && e.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active
                                      && type.BenefitCategory == BenefitServiceCategories.Medical
                                      && !type.IsClaimBased
                                orderby e.ActivatedAt descending
                                select new { e, plan })
                               .FirstOrDefaultAsync(cancellationToken);

            if (active is null) return response;

            response.HasActiveMedicalEnrollment = true;
            response.EnrollmentId = active.e.Id;
            response.EnrollmentNumber = active.e.EnrollmentNumber;
            response.PlanName = active.plan.BenefitPlanName;
            response.CoverageType = active.e.CoverageLevel;
            response.Persons.Add(new BenefitPersonBalanceResponse
            {
                BenefitPersonKind = BenefitPersonKinds.Enrollment,
                BenefitPersonId = active.e.Id,
                PersonName = owner.StaffName,
                RelationshipCode = BenefitRelationshipCodes.Self,
                LimitAmount = active.e.CoverageLimitAmount,
                UsedAmount = active.e.UsedAmount,
                RemainingAmount = active.e.RemainingAmount,
                Status = active.e.EnrollmentStatus
            });

            var dependents = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                .Where(x => !x.IsDelete && x.EmployeeBenefitEnrollmentId == active.e.Id
                    && x.DependentStatus == EmployeeBenefitDependentStatuses.Verified)
                .OrderBy(x => x.DependentName)
                .ToListAsync(cancellationToken);
            response.Persons.AddRange(dependents.Select(d => new BenefitPersonBalanceResponse
            {
                BenefitPersonKind = BenefitPersonKinds.Dependent,
                BenefitPersonId = d.Id,
                PersonName = d.DependentName,
                RelationshipCode = d.RelationshipType,
                LimitAmount = d.CoverageLimitAmount,
                UsedAmount = d.UsedAmount,
                RemainingAmount = d.RemainingAmount,
                Status = d.DependentStatus
            }));
            return response;
        }

        /// <summary>
        /// Riwayat status diturunkan dari kolom jejak model (dibuat, diajukan, disetujui, diaktifkan,
        /// dibatalkan). Model tidak punya tabel riwayat tersendiri.
        /// </summary>
        public async Task<List<EnrollmentStatusHistoryResponse>> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken)
        {
            var e = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Enrollment benefit tidak ditemukan.");

            var names = await GetUserNamesAsync(new Guid?[] { e.CreateBy, e.SubmittedByUserId, e.ApprovedByUserId, e.ActivatedByUserId, e.CancelledByUserId }, cancellationToken);
            var history = new List<EnrollmentStatusHistoryResponse>
            {
                new() { Status = EmployeeBenefitEnrollmentStatuses.Draft, OccurredAt = e.CreateDateTime, ActorName = names.GetValueOrDefault(e.CreateBy) }
            };
            if (e.SubmittedAt.HasValue) history.Add(new() { Status = EmployeeBenefitEnrollmentStatuses.Submitted, OccurredAt = e.SubmittedAt.Value, ActorName = NameOf(names, e.SubmittedByUserId) });
            if (e.ApprovedAt.HasValue) history.Add(new() { Status = EmployeeBenefitEnrollmentStatuses.Approved, OccurredAt = e.ApprovedAt.Value, ActorName = NameOf(names, e.ApprovedByUserId) });
            if (e.ActivatedAt.HasValue) history.Add(new() { Status = EmployeeBenefitEnrollmentStatuses.Active, OccurredAt = e.ActivatedAt.Value, ActorName = NameOf(names, e.ActivatedByUserId) });
            if (e.CancelledAt.HasValue) history.Add(new() { Status = EmployeeBenefitEnrollmentStatuses.Cancelled, OccurredAt = e.CancelledAt.Value, ActorName = NameOf(names, e.CancelledByUserId), Reason = e.CancellationReason });
            return history.OrderBy(x => x.OccurredAt).ToList();
        }

        // ------------------------------------------------------------------------------------
        // Tulis
        // ------------------------------------------------------------------------------------

        public async Task<EmployeeBenefitEnrollmentResponse> CreateAsync(
            CreateEmployeeBenefitEnrollmentRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            ValidatePeriod(request.EffectiveStartDate, request.EffectiveEndDate);

            // WorkforceProfileId adalah identitas pemilik. EmployeeId lama diterjemahkan ke Workforce; bila keduanya
            // dikirim, keduanya harus orang yang sama. Dokter tetap tidak memerlukan MstEmployee.
            var hasProfile = request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty;
            var hasEmployee = request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty;
            if (!hasProfile && !hasEmployee)
                throw new EmployeeBenefitValidationException("Penerima benefit (WorkforceProfileId) wajib dipilih.");

            var resolution = await _ownerResolver.ResolveRequestAsync(request.WorkforceProfileId, request.EmployeeId, cancellationToken);
            if (!resolution.IsResolved)
                throw new EmployeeBenefitValidationException(
                    resolution.Message ?? "Pegawai tidak ditemukan atau sudah tidak aktif.");
            var owner = resolution.Owner!;

            var plan = await _dbContext.MstBenefitPlans.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.BenefitPlanId && !x.IsDelete && x.IsActive, cancellationToken)
                ?? throw new EmployeeBenefitValidationException("Plan benefit tidak ditemukan atau tidak aktif.");
            await EnsurePlanApplicableAsync(plan, owner, request, cancellationToken);
            var employee = new { WorkforceProfileId = owner.WorkforceProfileId, EmployeeId = owner.EmployeeId };

            // Nomor dialokasikan provider bersama di luar transaksi pemanggil (QBE-CODE-002/003/006).
            var enrollmentNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: SequenceKey,
                Prefix: NumberPrefix,
                ResetPolicy: NumberSeriesResetPolicies.Never,
                SequenceDigits: 6,
                ActorUserId: actorUserId == Guid.Empty ? SystemFallbackUserId : actorUserId,
                Instant: DateTimeOffset.UtcNow), cancellationToken);

            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await BeginTransactionAsync(cancellationToken);
                await AcquireLockAsync($"HRD_BENEFIT_ENROLLMENT_PLAN_{employee.WorkforceProfileId:N}_{plan.Id:N}", cancellationToken);

                var hasOpen = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                    .AnyAsync(x => !x.IsDelete && x.WorkforceProfileId == employee.WorkforceProfileId && x.BenefitPlanId == plan.Id
                        && EmployeeBenefitEnrollmentStatuses.Open.Contains(x.EnrollmentStatus), cancellationToken);
                if (hasOpen)
                    throw new EmployeeBenefitConflictException("Pegawai ini sudah punya enrollment yang masih berjalan pada plan yang sama.");

                var enrollment = new TrxEmployeeBenefitEnrollment
                {
                    WorkforceProfileId = employee.WorkforceProfileId,
                    // Referensi kompatibilitas: hanya bila SDM benar-benar punya data karyawan; tidak pernah DoctorId.
                    EmployeeId = employee.EmployeeId,
                    BenefitPlanId = plan.Id,
                    EnrollmentNumber = enrollmentNumber,
                    EnrollmentStatus = EmployeeBenefitEnrollmentStatuses.Draft,
                    CoverageLevel = plan.CoverageType,
                    CurrencyCode = plan.CurrencyCode,
                    // Tanggal operasional dibaca sebagai jam dinding WIB lalu disimpan UTC (Npgsql timestamptz).
                    EnrollmentDate = AppDateTimeHelper.OperationalDateToUtc(request.EnrollmentDate),
                    EffectiveStartDate = AppDateTimeHelper.OperationalDateToUtc(request.EffectiveStartDate),
                    EffectiveEndDate = ToUtcDate(request.EffectiveEndDate),
                    MaximumDependents = plan.MaximumDependents,
                    Notes = request.Notes,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.TrxEmployeeBenefitEnrollments.Add(enrollment);
                await SaveChangesAsync("Pegawai ini sudah punya enrollment yang masih berjalan pada plan yang sama.", cancellationToken);
                await CommitAsync(transaction, cancellationToken);
                await AuditAsync("Enrollment.Created", enrollment.Id, actorUserId, enrollment.EnrollmentStatus);
                return await MapDetailAsync(enrollment, actorUserId, cancellationToken);
            }
            catch
            {
                await RollbackAsync(transaction);
                throw;
            }
            finally
            {
                if (transaction is not null) await transaction.DisposeAsync();
            }
        }

        public async Task<EmployeeBenefitEnrollmentResponse> UpdateAsync(
            Guid id, UpdateEmployeeBenefitEnrollmentRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            ValidatePeriod(request.EffectiveStartDate, request.EffectiveEndDate);
            return await TransitionAsync(id, request.RowVersion, actorUserId, "Enrollment.Updated", e =>
            {
                RequireStatus(e, "disunting", EmployeeBenefitEnrollmentStatuses.Draft);
                e.EnrollmentDate = AppDateTimeHelper.OperationalDateToUtc(request.EnrollmentDate);
                e.EffectiveStartDate = AppDateTimeHelper.OperationalDateToUtc(request.EffectiveStartDate);
                e.EffectiveEndDate = ToUtcDate(request.EffectiveEndDate);
                e.Notes = request.Notes;
                return Task.CompletedTask;
            }, cancellationToken);
        }

        public Task<EmployeeBenefitEnrollmentResponse> SubmitAsync(
            Guid id, EnrollmentTransitionRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            TransitionAsync(id, request.RowVersion, actorUserId, "Enrollment.Submitted", e =>
            {
                RequireStatus(e, "diajukan", EmployeeBenefitEnrollmentStatuses.Draft);
                e.EnrollmentStatus = EmployeeBenefitEnrollmentStatuses.Submitted;
                e.SubmittedAt = DateTime.UtcNow;
                e.SubmittedByUserId = actorUserId;
                AppendNotes(e, request.Notes);
                return Task.CompletedTask;
            }, cancellationToken);

        /// <summary>FIN-VAL-271: penyetuju berbeda dari pengaju (service + CK_..._MakerChecker).</summary>
        public Task<EmployeeBenefitEnrollmentResponse> ApproveAsync(
            Guid id, EnrollmentTransitionRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            TransitionAsync(id, request.RowVersion, actorUserId, "Enrollment.Approved", e =>
            {
                RequireStatus(e, "disetujui", EmployeeBenefitEnrollmentStatuses.Submitted);
                if (e.SubmittedByUserId.HasValue && e.SubmittedByUserId.Value == actorUserId)
                    throw new EmployeeBenefitForbiddenException("Pengaju tidak boleh menyetujui pengajuannya sendiri.");
                e.EnrollmentStatus = EmployeeBenefitEnrollmentStatuses.Approved;
                e.ApprovedAt = DateTime.UtcNow;
                e.ApprovedByUserId = actorUserId;
                AppendNotes(e, request.Notes);
                return Task.CompletedTask;
            }, cancellationToken);

        /// <summary>
        /// FIN-VAL-249: satu enrollment layanan RS (Medical, bukan klaim) aktif per pegawai pada
        /// periode yang berimpit. FIN-VAL-250: batas plafon plan wajib lebih dari nol. Saldo awal
        /// pegawai sendiri = batas plafon plan (FIN-DEC-207).
        /// </summary>
        public Task<EmployeeBenefitEnrollmentResponse> ActivateAsync(
            Guid id, EnrollmentTransitionRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            TransitionAsync(id, request.RowVersion, actorUserId, "Enrollment.Activated", async e =>
            {
                RequireStatus(e, "diaktifkan", EmployeeBenefitEnrollmentStatuses.Approved);
                await AcquireLockAsync($"HRD_BENEFIT_ENROLLMENT_ACTIVATE_{e.WorkforceProfileId:N}", cancellationToken);

                var plan = await _dbContext.MstBenefitPlans.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == e.BenefitPlanId && !x.IsDelete, cancellationToken)
                    ?? throw new EmployeeBenefitValidationException("Plan benefit tidak ditemukan.");
                if (!plan.CoverageLimitAmount.HasValue || plan.CoverageLimitAmount.Value <= 0)
                    throw new EmployeeBenefitValidationException("Plan benefit ini belum memiliki batas plafon. Lengkapi plan terlebih dahulu.");

                var type = await _dbContext.MstBenefitTypes.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == plan.BenefitTypeId, cancellationToken);
                var isMedical = type is not null && type.BenefitCategory == BenefitServiceCategories.Medical && !type.IsClaimBased;
                if (isMedical)
                {
                    var others = await (from o in _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                                        join p in _dbContext.MstBenefitPlans.AsNoTracking() on o.BenefitPlanId equals p.Id
                                        join t in _dbContext.MstBenefitTypes.AsNoTracking() on p.BenefitTypeId equals t.Id
                                        where !o.IsDelete && o.Id != e.Id
                                              && o.WorkforceProfileId == e.WorkforceProfileId
                                              && o.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active
                                              && t.BenefitCategory == BenefitServiceCategories.Medical && !t.IsClaimBased
                                        select new { o.EffectiveStartDate, o.EffectiveEndDate })
                                       .ToListAsync(cancellationToken);
                    if (others.Any(o => Overlaps(o.EffectiveStartDate, o.EffectiveEndDate, e.EffectiveStartDate, e.EffectiveEndDate)))
                        throw new EmployeeBenefitValidationException("Pegawai ini sudah memiliki benefit layanan RS yang aktif pada periode tersebut.");
                }

                e.EnrollmentStatus = EmployeeBenefitEnrollmentStatuses.Active;
                e.ActivatedAt = DateTime.UtcNow;
                e.ActivatedByUserId = actorUserId;
                e.CoverageLimitAmount = plan.CoverageLimitAmount.Value;
                e.UsedAmount = 0m;
                e.RemainingAmount = plan.CoverageLimitAmount.Value;
                e.MaximumDependents = plan.MaximumDependents;
                e.CoverageLevel = plan.CoverageType;
                e.IsEligible = true;
                e.PlanSnapshotJson = JsonSerializer.Serialize(new
                {
                    plan.BenefitPlanCode,
                    plan.BenefitPlanName,
                    plan.CoverageType,
                    plan.CoverageLimitAmount,
                    plan.MaximumDependents,
                    BenefitCategory = type?.BenefitCategory,
                    IsClaimBased = type?.IsClaimBased
                });
                AppendNotes(e, request.Notes);
            }, cancellationToken);

        public Task<EmployeeBenefitEnrollmentResponse> CancelAsync(
            Guid id, EnrollmentCancelRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            TransitionAsync(id, request.RowVersion, actorUserId, "Enrollment.Cancelled", e =>
            {
                RequireStatus(e, "dibatalkan", EmployeeBenefitEnrollmentStatuses.Open);
                if (string.IsNullOrWhiteSpace(request.CancellationReason))
                    throw new EmployeeBenefitValidationException("Alasan pembatalan wajib diisi.");
                e.EnrollmentStatus = EmployeeBenefitEnrollmentStatuses.Cancelled;
                e.CancelledAt = DateTime.UtcNow;
                e.CancelledByUserId = actorUserId;
                e.CancellationReason = request.CancellationReason.Trim();
                return Task.CompletedTask;
            }, cancellationToken);

        // ------------------------------------------------------------------------------------
        // Bantuan
        // ------------------------------------------------------------------------------------

        private async Task<EmployeeBenefitEnrollmentResponse> TransitionAsync(
            Guid id, Guid rowVersion, Guid actorUserId, string auditAction,
            Func<TrxEmployeeBenefitEnrollment, Task> apply, CancellationToken cancellationToken)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await BeginTransactionAsync(cancellationToken);
                await AcquireLockAsync($"HRD_BENEFIT_ENROLLMENT_{id:N}", cancellationToken);

                var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments
                    .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Enrollment benefit tidak ditemukan.");
                if (enrollment.RowVersion != rowVersion)
                    throw new EmployeeBenefitConflictException("Data sudah diperbarui petugas lain. Muat ulang lalu coba lagi.");

                await apply(enrollment);
                enrollment.RowVersion = Guid.NewGuid();
                enrollment.UpdateDateTime = DateTime.UtcNow;
                enrollment.UpdateBy = actorUserId;

                await SaveChangesAsync("Data sudah diperbarui petugas lain. Muat ulang lalu coba lagi.", cancellationToken);
                await CommitAsync(transaction, cancellationToken);
                await AuditAsync(auditAction, enrollment.Id, actorUserId, enrollment.EnrollmentStatus);
                return await MapDetailAsync(enrollment, actorUserId, cancellationToken);
            }
            catch
            {
                await RollbackAsync(transaction);
                throw;
            }
            finally
            {
                if (transaction is not null) await transaction.DisposeAsync();
            }
        }

        private async Task<EmployeeBenefitEnrollmentResponse> MapDetailAsync(
            TrxEmployeeBenefitEnrollment e, Guid actorUserId, CancellationToken cancellationToken)
        {
            var owners = await _ownerResolver.ResolveManyAsync(new[] { e.WorkforceProfileId }, cancellationToken);
            owners.TryGetValue(e.WorkforceProfileId, out var owner);
            var plan = await _dbContext.MstBenefitPlans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == e.BenefitPlanId, cancellationToken);
            var type = plan is null ? null : await _dbContext.MstBenefitTypes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == plan.BenefitTypeId, cancellationToken);
            var dependents = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                .Where(x => !x.IsDelete && x.EmployeeBenefitEnrollmentId == e.Id)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);
            var names = await GetUserNamesAsync(new Guid?[] { e.SubmittedByUserId, e.ApprovedByUserId, e.ActivatedByUserId, e.CancelledByUserId }, cancellationToken);
            var today = AppDateTimeHelper.OperationalDateToUtc(AppDateTimeHelper.OperationalDate());

            return new EmployeeBenefitEnrollmentResponse
            {
                Id = e.Id,
                EnrollmentNumber = e.EnrollmentNumber,
                WorkforceProfileId = e.WorkforceProfileId,
                OwnerType = owner?.OwnerType ?? WorkforceBenefitOwnerTypes.Employee,
                EmployeeId = e.EmployeeId ?? owner?.EmployeeId,
                DoctorId = owner?.DoctorId,
                StaffNumber = owner?.StaffNumber,
                StaffName = owner?.StaffName,
                EmployeeNumber = owner?.StaffNumber,
                EmployeeName = owner?.StaffName,
                BenefitPlanId = e.BenefitPlanId,
                BenefitPlanCode = plan?.BenefitPlanCode,
                BenefitPlanName = plan?.BenefitPlanName,
                EnrollmentStatus = e.EnrollmentStatus,
                CoverageType = e.CoverageLevel,
                EnrollmentDate = e.EnrollmentDate,
                EffectiveStartDate = e.EffectiveStartDate,
                EffectiveEndDate = e.EffectiveEndDate,
                CurrencyCode = e.CurrencyCode,
                CoverageLimitAmount = e.CoverageLimitAmount,
                UsedAmount = e.UsedAmount,
                RemainingAmount = e.RemainingAmount,
                MaximumDependents = e.MaximumDependents,
                ActiveDependentCount = e.ActiveDependentCount,
                IsMedicalServiceBenefit = type is not null && type.BenefitCategory == BenefitServiceCategories.Medical && !type.IsClaimBased,
                IsWithinEffectivePeriod = (!e.EffectiveStartDate.HasValue || e.EffectiveStartDate.Value <= today)
                                          && (!e.EffectiveEndDate.HasValue || e.EffectiveEndDate.Value >= today),
                Notes = e.Notes,
                CancellationReason = e.CancellationReason,
                SubmittedAt = e.SubmittedAt,
                SubmittedByName = NameOf(names, e.SubmittedByUserId),
                ApprovedAt = e.ApprovedAt,
                ApprovedByName = NameOf(names, e.ApprovedByUserId),
                ActivatedAt = e.ActivatedAt,
                ActivatedByName = NameOf(names, e.ActivatedByUserId),
                CancelledAt = e.CancelledAt,
                CancelledByName = NameOf(names, e.CancelledByUserId),
                RowVersion = e.RowVersion,
                AvailableActions = AvailableActionsFor(e, actorUserId),
                Dependents = dependents.Select(EmployeeBenefitDependentService.Map).ToList()
            };
        }

        /// <summary>
        /// Plan harus berlaku untuk kategori SDM dan jenis kepegawaian pemilik berdasarkan master HR: batas pada plan
        /// itu sendiri dan, bila ada aturan eligibility aktif yang berlaku pada tanggal enrollment, minimal satu aturan
        /// yang terpenuhi (kategori, jenis kepegawaian, dan masa kerja minimum). Nilai kosong berarti tanpa batas.
        /// Pemeriksaan sama untuk karyawan dan dokter tetap.
        /// </summary>
        private async Task EnsurePlanApplicableAsync(
            MstBenefitPlan plan, WorkforceBenefitOwner owner, CreateEmployeeBenefitEnrollmentRequest request, CancellationToken cancellationToken)
        {
            if (plan.EmployeeCategoryId.HasValue && plan.EmployeeCategoryId != owner.EmployeeCategoryId)
                throw new EmployeeBenefitValidationException("Plan benefit ini tidak berlaku untuk kategori SDM penerima.");
            if (plan.EmploymentTypeId.HasValue && plan.EmploymentTypeId != owner.EmploymentTypeId)
                throw new EmployeeBenefitValidationException("Plan benefit ini tidak berlaku untuk jenis kepegawaian penerima.");

            var rules = await _dbContext.MstBenefitEligibilityRules.AsNoTracking()
                .Where(x => x.BenefitPlanId == plan.Id && !x.IsDelete && x.IsActive)
                .ToListAsync(cancellationToken);
            var enrollmentDay = request.EnrollmentDate.Date;
            var applicable = rules
                .Where(x => (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= enrollmentDay)
                            && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= enrollmentDay))
                .ToList();
            if (applicable.Count == 0) return;

            var serviceMonths = owner.JoinDate.HasValue ? MonthsBetween(owner.JoinDate.Value, request.EffectiveStartDate) : 0;
            var satisfied = applicable.Any(x =>
                (!x.EmployeeCategoryId.HasValue || x.EmployeeCategoryId == owner.EmployeeCategoryId)
                && (!x.EmploymentTypeId.HasValue || x.EmploymentTypeId == owner.EmploymentTypeId)
                && serviceMonths >= x.MinimumServiceMonths);
            if (!satisfied)
                throw new EmployeeBenefitValidationException(
                    "Penerima belum memenuhi aturan kelayakan plan benefit (kategori SDM, jenis kepegawaian, atau masa kerja minimum).");
        }

        private static int MonthsBetween(DateTime from, DateTime to)
        {
            var months = (to.Year - from.Year) * 12 + to.Month - from.Month;
            if (to.Day < from.Day) months--;
            return Math.Max(months, 0);
        }

        /// <summary>Bantuan tampilan; setiap aksi tetap memeriksa ulang status dan aktor di backend.</summary>
        private static List<string> AvailableActionsFor(TrxEmployeeBenefitEnrollment e, Guid actorUserId)
        {
            var actions = new List<string>();
            switch (e.EnrollmentStatus)
            {
                case EmployeeBenefitEnrollmentStatuses.Draft:
                    actions.AddRange(new[] { EmployeeBenefitActions.Update, EmployeeBenefitActions.Submit, EmployeeBenefitActions.Cancel });
                    break;
                case EmployeeBenefitEnrollmentStatuses.Submitted:
                    if (e.SubmittedByUserId != actorUserId) actions.Add(EmployeeBenefitActions.Approve);
                    actions.Add(EmployeeBenefitActions.Cancel);
                    break;
                case EmployeeBenefitEnrollmentStatuses.Approved:
                    actions.AddRange(new[] { EmployeeBenefitActions.Activate, EmployeeBenefitActions.Cancel });
                    break;
                case EmployeeBenefitEnrollmentStatuses.Active:
                    actions.Add(EmployeeBenefitActions.Cancel);
                    break;
            }
            return actions;
        }

        private static void RequireStatus(TrxEmployeeBenefitEnrollment e, string action, params string[] allowed)
        {
            if (!allowed.Contains(e.EnrollmentStatus))
                throw new EmployeeBenefitTransitionException($"Enrollment berstatus {e.EnrollmentStatus} sehingga tidak dapat {action}.");
        }

        private static void ValidatePeriod(DateTime start, DateTime? end)
        {
            if (end.HasValue && end.Value.Date < start.Date)
                throw new EmployeeBenefitValidationException("Tanggal akhir berlaku tidak boleh sebelum tanggal mulai berlaku.");
        }

        internal static DateTime? ToUtcDate(DateTime? value) =>
            value.HasValue ? AppDateTimeHelper.OperationalDateToUtc(value.Value) : null;

        private static bool Overlaps(DateTime? aStart, DateTime? aEnd, DateTime? bStart, DateTime? bEnd) =>
            (aStart ?? DateTime.MinValue) <= (bEnd ?? DateTime.MaxValue) && (bStart ?? DateTime.MinValue) <= (aEnd ?? DateTime.MaxValue);

        private static void AppendNotes(TrxEmployeeBenefitEnrollment e, string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return;
            var combined = string.IsNullOrWhiteSpace(e.Notes) ? notes.Trim() : $"{e.Notes}\n{notes.Trim()}";
            e.Notes = combined.Length <= 2000 ? combined : combined[..2000];
        }

        private static string? NameOf(Dictionary<Guid, string?> names, Guid? id) =>
            id.HasValue ? names.GetValueOrDefault(id.Value) : null;

        private async Task<Dictionary<Guid, string?>> GetUserNamesAsync(IEnumerable<Guid?> userIds, CancellationToken cancellationToken)
        {
            var ids = userIds.Where(x => x.HasValue && x.Value != Guid.Empty).Select(x => x!.Value).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, string?>();
            return await _dbContext.Users.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        }

        private async Task SaveChangesAsync(string conflictMessage, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new EmployeeBenefitConflictException(conflictMessage, exception);
            }
            catch (DbUpdateException exception)
            {
                throw new EmployeeBenefitConflictException(conflictMessage, exception);
            }
        }

        private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsRelational()) return null;
            return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        }

        private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
            _dbContext.Database.IsRelational()
                ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
                : Task.CompletedTask;

        private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
            transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

        private static Task RollbackAsync(IDbContextTransaction? transaction) =>
            transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

        // QBE-LOG-001: aktor ikut dicatat; payload hanya Id dan status, tanpa nama atau NIK.
        private Task AuditAsync(string action, Guid enrollmentId, Guid actorUserId, string status) =>
            _loggerService.AuditAsync(LogCategory, action, $"{action} EnrollmentId={enrollmentId}",
                new { EnrollmentId = enrollmentId, Status = status, ActorUserId = actorUserId });
    }

    public sealed class EmployeeBenefitValidationException(string message) : Exception(message);
    public sealed class EmployeeBenefitTransitionException(string message) : Exception(message);
    public sealed class EmployeeBenefitConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
    public sealed class EmployeeBenefitForbiddenException(string message) : Exception(message);
}
