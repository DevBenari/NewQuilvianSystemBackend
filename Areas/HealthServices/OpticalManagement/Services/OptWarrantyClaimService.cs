using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Keluhan garansi dan remake (<c>BE-OPT-050</c>, <c>OPT-API-v1</c> grup Opt Warranty Claim, <c>OPT-STATE-v1</c> bagian 4,
    /// <c>OPT-VAL-v1</c> bagian 5; <c>OPT-DEC-015</c>, <c>041</c>, <c>043</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Alur:</b> catat (<c>Submitted</c>) pada kunjungan keluhan baru → periksa ulang oleh RO/Sp.M (<c>Examined</c>) →
    /// Kepala Optik menyetujui remake (<c>Approved</c>) atau menolak (<c>Rejected</c>). Keduanya terminal.
    /// </para>
    /// <para>
    /// <b>Masa garansi</b> dihitung saat keluhan dicatat dari snapshot pada pesanan asal (bukan pengaturan saat ini):
    /// ketidaknyamanan/ukuran dalam hari, cacat lensa dan cacat frame dalam bulan, sejak tanggal WIB penyerahan; batas akhir
    /// termasuk. Kerusakan karena pemakaian tidak pernah masuk garansi.
    /// </para>
    /// <para>
    /// <b>Remake</b> adalah pesanan turunan <c>IsRemake</c> pada kunjungan keluhan, tertaut ke pesanan asal, langsung
    /// <c>Menunggu Uang Muka</c> dengan minimum Rp 0 dan tanpa fakta tagihan. Ia memakai jalur kirim → siap → serah yang
    /// sama; stok dipotong saat dikirim ke pengerjaan.
    /// </para>
    /// </remarks>
    public class OptWarrantyClaimService
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;

        public const string NotFoundMessage = "Keluhan garansi tidak ditemukan.";
        public const string NotHandedOverMessage = "Keluhan hanya untuk kacamata yang sudah diserahkan.";
        public const string NewEncounterMessage = "Daftarkan kunjungan keluhan baru untuk pasien ini.";
        public const string OutOfWarrantyMessage = "Keluhan di luar masa garansi. Proses sebagai pesanan berbayar.";
        public const string MustExamineMessage = "Keluhan harus diperiksa ulang oleh RO/Sp.M lebih dulu.";
        public const string AlreadyRemadeMessage = "Keluhan ini sudah menghasilkan remake.";
        public const string StaleMessage = "Keluhan sudah diubah petugas lain. Muat ulang halaman.";
        public const string ComplaintNoteMessage = "Tuliskan keluhan pasien.";
        public const string FindingMessage = "Tuliskan hasil pemeriksaan ulang.";
        public const string RejectReasonMessage = "Tuliskan alasan penolakan.";

        private readonly ApplicationDbContext _dbContext;
        private readonly OptNumberService _numberService;
        private readonly OptVisitService _visitService;
        private readonly OptPractitionerGuard _guard;
        private readonly OptPrescriptionService _prescriptionService;
        private readonly TimeProvider _time;

        public OptWarrantyClaimService(
            ApplicationDbContext dbContext,
            OptNumberService numberService,
            OptVisitService visitService,
            OptPractitionerGuard guard,
            OptPrescriptionService prescriptionService,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _numberService = numberService;
            _visitService = visitService;
            _guard = guard;
            _prescriptionService = prescriptionService;
            _time = timeProvider ?? TimeProvider.System;
        }

        /// <summary>
        /// Batas akhir garansi untuk satu jenis keluhan (tanggal WIB, termasuk), atau <c>null</c> bila tidak bergaransi.
        /// <b>Contoh:</b> diserahkan 5 November 2026 dengan garansi ukuran 30 hari → batas 5 Desember 2026; keluhan 6 Desember
        /// (hari ke-31) di luar garansi (<c>OPT-AC-022</c>).
        /// </summary>
        public static DateOnly? WarrantyDeadline(OptComplaintType type, DateOnly handedOverDate, int? fitDays, int? lensMonths, int? frameMonths) => type switch
        {
            OptComplaintType.FitDiscomfort when fitDays is { } d => handedOverDate.AddDays(d),
            OptComplaintType.LensDefect when lensMonths is { } m => handedOverDate.AddMonths(m),
            OptComplaintType.FrameDefect when frameMonths is { } m => handedOverDate.AddMonths(m),
            _ => null,
        };

        // ---------------------------------------------------------------- baca

        public async Task<PagedResult<OptWarrantyClaimResponse>> GetListAsync(OptWarrantyClaimListQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var claims = _dbContext.OptWarrantyClaims.AsNoTracking().Where(x => !x.IsDelete);
            if (query.Status is { } status) claims = claims.Where(x => x.Status == status);
            if (query.OrderId is { } orderId) claims = claims.Where(x => x.OrderId == orderId);
            if (query.PatientId is { } patientId)
                claims = claims.Where(x => _dbContext.OptOrders.Any(o => o.Id == x.OrderId && o.PatientId == patientId));
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var kata = query.Search.Trim().ToLower();
                claims = claims.Where(x => x.ClaimNumber.ToLower().Contains(kata)
                    || _dbContext.OptOrders.Any(o => o.Id == x.OrderId && (o.OrderNumber.ToLower().Contains(kata)
                        || _dbContext.Set<MstPatient>().Any(p => p.Id == o.PatientId
                            && (p.MedicalRecordNumber.ToLower().Contains(kata) || p.FullName.ToLower().Contains(kata))))));
            }

            var total = await claims.CountAsync(cancellationToken);
            var page = await claims.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.ClaimNumber)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            var items = new List<OptWarrantyClaimResponse>();
            foreach (var claim in page)
                items.Add(await ProjectAsync(claim, cancellationToken));

            return new PagedResult<OptWarrantyClaimResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items,
            };
        }

        public async Task<OptResult<OptWarrantyClaimResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.OptWarrantyClaims.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            return claim is null
                ? OptResult<OptWarrantyClaimResponse>.NotFound(NotFoundMessage)
                : OptResult<OptWarrantyClaimResponse>.Ok(await ProjectAsync(claim, cancellationToken));
        }

        // ---------------------------------------------------------------- tindakan

        public async Task<OptResult<OptWarrantyClaimResponse>> CreateAsync(OptWarrantyClaimCreateRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            if (request.OrderId is not { } orderId || orderId == Guid.Empty)
                return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih pesanan yang dikeluhkan.");
            if (request.ComplaintType is not { } jenis || !Enum.IsDefined(jenis))
                return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih jenis keluhan.");
            if (string.IsNullOrWhiteSpace(request.ComplaintNote))
                return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.InvalidRequest, ComplaintNoteMessage);

            var order = await _dbContext.OptOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order is null)
                return OptResult<OptWarrantyClaimResponse>.NotFound(OptOrderService.NotFoundMessage);
            if (order.Status != OptOrderStatus.HandedOver || order.HandedOverAt is not { } diserahkan)
                return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.Val060, NotHandedOverMessage);

            if (request.EncounterId is not { } encounterId || encounterId == order.EncounterId)
                return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.Val061, NewEncounterMessage);
            var encounter = await _visitService.FindOpticalEncounterAsync(encounterId, cancellationToken);
            if (encounter is null || encounter.PatientId != order.PatientId)
                return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.Val061, NewEncounterMessage);
            if (OptVisitService.IsClosed(encounter.EncounterStatus))
                return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.Val030, "Kunjungan sudah ditutup. Daftarkan kunjungan baru.");

            var tanggalSerah = DateOnly.FromDateTime(OptPractitionerLicenseReader.ToOperationalDate(
                new DateTimeOffset(DateTime.SpecifyKind(diserahkan, DateTimeKind.Utc))));
            var batas = WarrantyDeadline(jenis, tanggalSerah, order.WarrantyFitDaysSnapshot, order.WarrantyLensDefectMonthsSnapshot, order.WarrantyFrameDefectMonthsSnapshot);
            var hariIni = DateOnly.FromDateTime(Today());

            var now = Now();
            var claim = new OptWarrantyClaim
            {
                ClaimNumber = await _numberService.AllocateClaimNumberAsync(actorUserId, cancellationToken),
                OrderId = order.Id,
                EncounterId = encounter.Id,
                ComplaintType = jenis,
                ComplaintNote = request.ComplaintNote.Trim(),
                IsWithinWarranty = batas is { } b && hariIni <= b,
                WarrantyDeadlineSnapshot = batas,
                Status = OptWarrantyClaimStatus.Submitted,
                CreateDateTime = now,
                CreateBy = actorUserId,
            };
            _dbContext.OptWarrantyClaims.Add(claim);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OptResult<OptWarrantyClaimResponse>.Ok(await ProjectAsync(claim, cancellationToken));
        }

        public async Task<OptResult<OptWarrantyClaimResponse>> ExamineAsync(Guid id, OptWarrantyClaimExamineRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.OptWarrantyClaims.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (claim is null)
                return OptResult<OptWarrantyClaimResponse>.NotFound(NotFoundMessage);
            if (claim.Status != OptWarrantyClaimStatus.Submitted)
                return InvalidStatus(claim);
            if (request.ExpectedVersion != claim.Version)
                return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.ConcurrencyConflict, StaleMessage);
            if (string.IsNullOrWhiteSpace(request.ExaminationFinding))
                return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.InvalidRequest, FindingMessage);

            if (request.ReexamRefractionExamId is { } examId)
            {
                var pasienPesanan = await _dbContext.OptOrders.AsNoTracking().Where(x => x.Id == claim.OrderId).Select(x => x.PatientId).FirstAsync(cancellationToken);
                var cocok = await _dbContext.OptRefractionExams.AsNoTracking()
                    .AnyAsync(x => x.Id == examId && !x.IsDelete && x.PatientId == pasienPesanan && x.Status == OptRefractionExamStatus.Final, cancellationToken);
                if (!cocok)
                    return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pemeriksaan ulang harus pemeriksaan refraksi final milik pasien ini.");
            }

            // Pemeriksaan ulang hanya oleh RO/Sp.M dengan SIP dan STR berlaku (OPT-DEC-005, 056).
            var pemeriksa = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!pemeriksa.IsSuccess)
                return pemeriksa.Kind switch
                {
                    OptResultKind.Unavailable => OptResult<OptWarrantyClaimResponse>.Unavailable(pemeriksa.Code!, pemeriksa.Message!),
                    _ => OptResult<OptWarrantyClaimResponse>.Forbidden(pemeriksa.Code!, pemeriksa.Message!),
                };

            var now = Now();
            claim.Status = OptWarrantyClaimStatus.Examined;
            claim.ExaminedByWorkforceProfileId = pemeriksa.Value!.WorkforceProfileId;
            claim.ExaminedAt = now;
            claim.ExaminationFinding = request.ExaminationFinding.Trim();
            claim.ReexamRefractionExamId = request.ReexamRefractionExamId;
            Touch(claim, actorUserId, now);
            return await SaveAsync(claim, cancellationToken);
        }

        public async Task<OptResult<OptWarrantyClaimResponse>> DecideAsync(Guid id, OptWarrantyClaimDecisionRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.OptWarrantyClaims.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (claim is null)
                return OptResult<OptWarrantyClaimResponse>.NotFound(NotFoundMessage);
            if (claim.Status == OptWarrantyClaimStatus.Submitted)
                return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.Val063, MustExamineMessage);
            if (claim.RemakeOrderId is not null)
                return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.Val064, AlreadyRemadeMessage);
            if (claim.Status != OptWarrantyClaimStatus.Examined)
                return InvalidStatus(claim);
            if (request.ExpectedVersion != claim.Version)
                return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.ConcurrencyConflict, StaleMessage);

            var now = Now();
            var alasan = string.IsNullOrWhiteSpace(request.DecisionReason) ? null : request.DecisionReason.Trim();
            if (!request.Approve)
            {
                if (alasan is null)
                    return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.InvalidRequest, RejectReasonMessage);
                claim.Status = OptWarrantyClaimStatus.Rejected;
                claim.DecidedBy = actorUserId;
                claim.DecidedAt = now;
                claim.DecisionReason = alasan;
                Touch(claim, actorUserId, now);
                return await SaveAsync(claim, cancellationToken);
            }

            if (!claim.IsWithinWarranty)
                return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.Val062, OutOfWarrantyMessage);

            var asal = await _dbContext.OptOrders.AsNoTracking().Include(x => x.Items)
                .FirstAsync(x => x.Id == claim.OrderId, cancellationToken);
            var barisAsal = asal.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).OrderBy(x => x.LineNo).ToList();
            if (request.RemakeItems is { Count: > 0 } pilihan)
            {
                if (pilihan.Any(p => barisAsal.All(b => b.Id != p)))
                    return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.InvalidRequest, "Barang remake harus barang dari pesanan asal.");
                barisAsal = barisAsal.Where(b => pilihan.Contains(b.Id)).ToList();
            }
            if (barisAsal.Count == 0)
                return OptResult<OptWarrantyClaimResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih barang yang dibuat ulang.");

            var (resepId, versiId) = (asal.PrescriptionId, asal.PrescriptionVersionId);
            if (request.PrescriptionVersionId is { } versiBaru && versiBaru != Guid.Empty)
            {
                var versi = await _dbContext.OptPrescriptionVersions.AsNoTracking().Where(x => x.Id == versiBaru && !x.IsDelete)
                    .Select(x => new { x.PrescriptionId, x.VersionNo, x.Prescription!.CurrentVersionNo }).FirstOrDefaultAsync(cancellationToken);
                if (versi is null)
                    return OptResult<OptWarrantyClaimResponse>.NotFound("Resep tidak ditemukan.");
                if (versi.VersionNo != versi.CurrentVersionNo)
                    return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.InvalidState, OptOrderService.OutdatedVersionMessage);
                var jenisResep = barisAsal.Any(x => x.ItemCategory == OptItemCategory.ContactLens) ? OptPrescriptionType.ContactLens : OptPrescriptionType.Spectacle;
                var layak = await _prescriptionService.CheckUsableForOrderAsync(versi.PrescriptionId, asal.PatientId, jenisResep, cancellationToken);
                if (!layak.IsSuccess)
                    return OptResult<OptWarrantyClaimResponse>.Conflict(layak.Code ?? OptErrorCodes.InvalidState, layak.Message ?? "Resep tidak dapat dipakai.");
                (resepId, versiId) = (versi.PrescriptionId, versiBaru);
            }

            var remake = new OptOrder
            {
                OrderNumber = await _numberService.AllocateOrderNumberAsync(actorUserId, cancellationToken),
                EncounterId = claim.EncounterId,
                PatientId = asal.PatientId,
                OrderKind = OptOrderKind.Remake,
                IsRemake = true,
                ParentOrderId = asal.Id,
                PrescriptionId = resepId,
                PrescriptionVersionId = versiId,
                Status = OptOrderStatus.AwaitingDownPayment,
                DownPaymentMinPercentSnapshot = 0m,
                CreateDateTime = now,
                CreateBy = actorUserId,
            };
            var nomor = 0;
            foreach (var b in barisAsal)
                remake.Items.Add(new OptOrderItem
                {
                    OrderId = remake.Id, LineNo = ++nomor, DrugId = b.DrugId, ItemCategory = b.ItemCategory, LensSupply = b.LensSupply,
                    Quantity = b.Quantity, CreateDateTime = now, CreateBy = actorUserId,
                });
            remake.StatusHistories.Add(new OptOrderStatusHistory
            {
                OrderId = remake.Id, FromStatus = null, ToStatus = OptOrderStatus.AwaitingDownPayment, ActorUserId = actorUserId, OccurredAt = now,
                Reason = $"Remake dari keluhan {claim.ClaimNumber} atas pesanan {asal.OrderNumber}", CreateDateTime = now, CreateBy = actorUserId,
            });
            _dbContext.OptOrders.Add(remake);

            claim.Status = OptWarrantyClaimStatus.Approved;
            claim.DecidedBy = actorUserId;
            claim.DecidedAt = now;
            claim.DecisionReason = alasan;
            claim.RemakeOrderId = remake.Id;
            Touch(claim, actorUserId, now);
            return await SaveAsync(claim, cancellationToken);
        }

        // ---------------------------------------------------------------- penolong

        private static OptResult<OptWarrantyClaimResponse> InvalidStatus(OptWarrantyClaim claim) =>
            OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.Val057,
                $"Tindakan ini tidak berlaku untuk keluhan berstatus {OptLabels.WarrantyClaimStatus(claim.Status)}.");

        private static void Touch(OptWarrantyClaim claim, Guid actorUserId, DateTime now)
        {
            claim.Version += 1;
            claim.UpdateDateTime = now;
            claim.UpdateBy = actorUserId;
        }

        private async Task<OptResult<OptWarrantyClaimResponse>> SaveAsync(OptWarrantyClaim claim, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return OptResult<OptWarrantyClaimResponse>.Conflict(OptErrorCodes.ConcurrencyConflict, StaleMessage);
            }
            return OptResult<OptWarrantyClaimResponse>.Ok(await ProjectAsync(claim, cancellationToken));
        }

        private async Task<OptWarrantyClaimResponse> ProjectAsync(OptWarrantyClaim claim, CancellationToken cancellationToken)
        {
            var pesanan = await _dbContext.OptOrders.AsNoTracking().Where(x => x.Id == claim.OrderId)
                .Select(x => new { x.OrderNumber, x.PatientId }).FirstAsync(cancellationToken);
            var pasien = await _dbContext.Set<MstPatient>().AsNoTracking().Where(x => x.Id == pesanan.PatientId)
                .Select(x => new { x.MedicalRecordNumber, x.FullName }).FirstOrDefaultAsync(cancellationToken);
            var kunjungan = await _dbContext.Set<RegPatientEncounter>().AsNoTracking().Where(x => x.Id == claim.EncounterId)
                .Select(x => x.EncounterNumber).FirstOrDefaultAsync(cancellationToken);
            var remake = claim.RemakeOrderId is { } remakeId
                ? await _dbContext.OptOrders.AsNoTracking().Where(x => x.Id == remakeId).Select(x => x.OrderNumber).FirstOrDefaultAsync(cancellationToken)
                : null;

            return new OptWarrantyClaimResponse
            {
                Id = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                OrderId = claim.OrderId,
                OrderNumber = pesanan.OrderNumber,
                EncounterId = claim.EncounterId,
                EncounterNumber = kunjungan ?? string.Empty,
                PatientId = pesanan.PatientId,
                MedicalRecordNumber = pasien?.MedicalRecordNumber ?? string.Empty,
                FullName = pasien?.FullName ?? string.Empty,
                ComplaintType = claim.ComplaintType,
                ComplaintTypeLabel = OptLabels.ComplaintType(claim.ComplaintType),
                ComplaintNote = claim.ComplaintNote,
                IsWithinWarranty = claim.IsWithinWarranty,
                WarrantyDeadlineSnapshot = claim.WarrantyDeadlineSnapshot,
                Status = claim.Status,
                StatusLabel = OptLabels.WarrantyClaimStatus(claim.Status),
                CreatedAt = claim.CreateDateTime,
                ExaminedByWorkforceProfileId = claim.ExaminedByWorkforceProfileId,
                ExaminedAt = claim.ExaminedAt,
                ExaminationFinding = claim.ExaminationFinding,
                ReexamRefractionExamId = claim.ReexamRefractionExamId,
                DecidedAt = claim.DecidedAt,
                DecisionReason = claim.DecisionReason,
                RemakeOrderId = claim.RemakeOrderId,
                RemakeOrderNumber = remake,
                Version = claim.Version,
            };
        }

        private DateTime Now() => _time.GetUtcNow().UtcDateTime;

        private DateTime Today() => OptPractitionerLicenseReader.ToOperationalDate(_time.GetUtcNow());
    }
}
