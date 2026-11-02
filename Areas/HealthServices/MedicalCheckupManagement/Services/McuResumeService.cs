using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using E = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services.McuErrorCodes;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Resume MCU — <c>BE-MCU-030</c>: draf, approve, kembalikan, verifikasi (<c>MCU-API-v1</c> grup Mcu Resume,
    /// <c>MCU-STATE-v1</c> bagian 1.1 dan 3, <c>VAL-MCU-35</c>..<c>40</c>, <c>46</c>), dan buka ulang berversi
    /// (<c>BE-MCU-031</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pembagian tugas</b> (<c>MCU-DEC-008</c>): dokter MCU yang ditetapkan (<c>VAL-MCU-38</c>) menyusun dan
    /// menyetujui isi klinis; petugas MCU memverifikasi kelengkapan administratif atau mengembalikannya ke dokter,
    /// tanpa pernah mengubah isi. Pemverifikasi bukan dokter pemberi approve (<c>VAL-MCU-39</c>).
    /// </para>
    /// <para>
    /// <b>Kunci approve</b> (<c>MCU-DEC-009</c>, <c>018</c>): semua komponen tuntas (selesai atau dibatalkan resmi),
    /// skrining selesai, lalu kategori kelayakan, kondisi umum, dan rekomendasi terisi.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Thorax Budi belum diekspertise → approve dr. Rina <c>409</c> <i>"Masih ada 1 komponen yang belum
    /// tuntas: Thorax PA."</i> Sesudah tuntas, dr. Rina approve <i>Fit dengan catatan</i> → order <i>Disetujui
    /// Dokter</i>. dr. Rina mencoba memverifikasi sendiri → <c>403</c>; Dewi memverifikasi → <i>Terverifikasi</i>.
    /// </para>
    /// <para>Isi resume SENSITIF dan tidak pernah masuk log aplikasi.</para>
    /// </remarks>
    public class McuResumeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public McuResumeService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService clinicalContextService,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _clinicalContextService = clinicalContextService;
            _httpContextAccessor = httpContextAccessor;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<McuResult<List<McuResumeResponse>>> GetAllAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<List<McuResumeResponse>>.NotFound("Order MCU tidak ditemukan.");

            var resumes = await _dbContext.McuResumes.AsNoTracking()
                .Where(x => x.McuOrderId == orderId && !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .ToListAsync(cancellationToken);

            return McuResult<List<McuResumeResponse>>.Ok(await MapAsync(order, resumes, cancellationToken));
        }

        // =====================================================================
        // Draf — dokter MCU yang ditetapkan
        // =====================================================================

        public async Task<McuResult<McuResumeResponse>> SaveCurrentAsync(
            Guid orderId,
            SaveMcuResumeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuResumeResponse>.NotFound("Order MCU tidak ditemukan.");

            var actorDoctorId = await ActorDoctorIdAsync(actorUserId, cancellationToken);
            if (!IsAssignedDoctor(order, actorDoctorId))
                return Val38<McuResumeResponse>();

            if (order.Version != request.Version)
                return VersionConflict<McuResumeResponse>();

            var statusBlock = order.OrderStatus switch
            {
                McuOrderStatus.InExamination or McuOrderStatus.AwaitingResume => null,
                McuOrderStatus.Scheduled or McuOrderStatus.CheckedIn => "Pemeriksaan belum dimulai.",
                McuOrderStatus.Cancelled => "Order sudah dibatalkan.",
                McuOrderStatus.DoctorApproved or McuOrderStatus.Verified or McuOrderStatus.Delivered
                    => "Resume sudah disetujui. Minta dokter membuka ulang resume.",
                _ => $"Order berstatus {McuLabels.OrderStatus(order.OrderStatus)} tidak dapat diisi resumenya."
            };
            if (statusBlock != null)
                return McuResult<McuResumeResponse>.Conflict(E.InvalidTransition, statusBlock);

            var generalCondition = Clean(request.GeneralCondition);
            var importantFindings = Clean(request.ImportantFindings);
            var recommendation = Clean(request.Recommendation);
            var workRecommendation = Clean(request.WorkRecommendation);

            if (generalCondition?.Length > 4000 || importantFindings?.Length > 4000 || recommendation?.Length > 4000)
                return McuResult<McuResumeResponse>.Invalid(E.InvalidRequest, "Kondisi umum, temuan penting, dan rekomendasi masing-masing paling banyak 4.000 karakter.");
            if (workRecommendation?.Length > 1000)
                return McuResult<McuResumeResponse>.Invalid(E.InvalidRequest, "Rekomendasi kerja paling banyak 1.000 karakter.");

            if (request.FitnessCategoryId.HasValue && !await _dbContext.McuFitnessCategories.AsNoTracking()
                    .AnyAsync(x => x.Id == request.FitnessCategoryId.Value && x.IsActive && !x.IsDelete, cancellationToken))
            {
                return McuResult<McuResumeResponse>.Invalid(E.InvalidRequest, "Kategori kelayakan tidak ditemukan atau tidak aktif.");
            }

            var now = DateTime.UtcNow;
            var resume = await CurrentAsync(order.Id, cancellationToken);
            if (resume == null)
            {
                resume = new McuResume
                {
                    McuOrderId = order.Id,
                    VersionNumber = 1,
                    ResumeStatus = McuResumeStatus.Draft,
                    IsCurrent = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.McuResumes.Add(resume);
            }
            else if (resume.ResumeStatus != McuResumeStatus.Draft)
            {
                return McuResult<McuResumeResponse>.Conflict(E.InvalidTransition, "Resume sudah disetujui. Minta dokter membuka ulang resume.");
            }
            else
            {
                resume.UpdateDateTime = now;
                resume.UpdateBy = actorUserId;
            }

            resume.GeneralConditionText = generalCondition;
            resume.ImportantFindingsText = importantFindings;
            resume.RecommendationText = recommendation;
            resume.FitnessCategoryId = request.FitnessCategoryId;
            resume.WorkRecommendationText = workRecommendation;
            resume.AuthorDoctorId = actorDoctorId;
            Touch(order, actorUserId, now);

            return await SaveAndMapAsync(order, resume, cancellationToken);
        }

        // =====================================================================
        // Approve — dokter MCU yang ditetapkan
        // =====================================================================

        public async Task<McuResult<McuResumeResponse>> ApproveAsync(
            Guid orderId,
            McuResumeActionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuResumeResponse>.NotFound("Order MCU tidak ditemukan.");

            var actorDoctorId = await ActorDoctorIdAsync(actorUserId, cancellationToken);
            if (!IsAssignedDoctor(order, actorDoctorId))
                return Val38<McuResumeResponse>();

            if (order.Version != request.Version)
                return VersionConflict<McuResumeResponse>();

            var statusBlock = order.OrderStatus switch
            {
                McuOrderStatus.InExamination or McuOrderStatus.AwaitingResume => null,
                McuOrderStatus.Scheduled or McuOrderStatus.CheckedIn => "Pemeriksaan belum dimulai.",
                McuOrderStatus.Cancelled => "Order sudah dibatalkan.",
                McuOrderStatus.DoctorApproved or McuOrderStatus.Verified or McuOrderStatus.Delivered => "Resume sudah disetujui.",
                _ => $"Order berstatus {McuLabels.OrderStatus(order.OrderStatus)} tidak dapat disetujui resumenya."
            };
            if (statusBlock != null)
                return McuResult<McuResumeResponse>.Conflict(E.InvalidTransition, statusBlock);

            // VAL-MCU-35 — komponen paket dan tambahan sama-sama dihitung (MCU-DEC-036).
            var pending = await _dbContext.McuOrderComponents.AsNoTracking()
                .Where(x => x.McuOrderId == order.Id && !x.IsDelete
                    && x.ComponentStatus != McuOrderComponentStatus.Completed
                    && x.ComponentStatus != McuOrderComponentStatus.Cancelled)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => x.ComponentNameSnapshot)
                .ToListAsync(cancellationToken);
            if (pending.Count > 0)
            {
                return McuResult<McuResumeResponse>.Conflict(
                    E.Val35, $"Masih ada {pending.Count} komponen yang belum tuntas: {string.Join(", ", pending)}.");
            }

            // VAL-MCU-36
            if (!await IsScreeningCompletedAsync(order.EncounterId, cancellationToken))
                return McuResult<McuResumeResponse>.Conflict(E.Val36, "Skrining awal peserta belum selesai.");

            // VAL-MCU-37 — yang pertama kosong disebut.
            var resume = await CurrentAsync(order.Id, cancellationToken);
            var missing = resume?.FitnessCategoryId == null ? "kategori kelayakan"
                : string.IsNullOrWhiteSpace(resume.GeneralConditionText) ? "kondisi umum"
                : string.IsNullOrWhiteSpace(resume.RecommendationText) ? "rekomendasi"
                : null;
            if (missing != null)
                return McuResult<McuResumeResponse>.Invalid(E.Val37, $"Lengkapi {missing} sebelum menyetujui.");

            if (resume!.ResumeStatus != McuResumeStatus.Draft)
                return McuResult<McuResumeResponse>.Conflict(E.InvalidTransition, "Resume sudah disetujui.");

            var now = DateTime.UtcNow;
            resume.ResumeStatus = McuResumeStatus.Approved;
            resume.ApprovedByDoctorId = actorDoctorId;
            resume.ApprovedAt = now;
            resume.ReturnReason = null;
            resume.UpdateDateTime = now;
            resume.UpdateBy = actorUserId;

            // Komponen terakhir tuntas tanpa sinkron (MCU-STATE-v1 1.1): perpindahan sistem dicatat lebih dulu,
            // satu mikrodetik sebelum approve agar urutan riwayat tetap.
            if (order.OrderStatus == McuOrderStatus.InExamination)
                AddHistory(order.Id, McuOrderStatus.InExamination, McuOrderStatus.AwaitingResume, null, "Semua komponen tuntas.", actorUserId, now.AddTicks(-10));

            AddHistory(order.Id, McuOrderStatus.AwaitingResume, McuOrderStatus.DoctorApproved, actorUserId, null, actorUserId, now);
            order.OrderStatus = McuOrderStatus.DoctorApproved;
            Touch(order, actorUserId, now);

            return await SaveAndMapAsync(order, resume, cancellationToken);
        }

        // =====================================================================
        // Kembalikan dan verifikasi — petugas MCU
        // =====================================================================

        public async Task<McuResult<McuResumeResponse>> ReturnAsync(
            Guid orderId,
            ReturnMcuResumeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuResumeResponse>.NotFound("Order MCU tidak ditemukan.");

            // VAL-MCU-40
            var reason = Clean(request.Reason);
            if (reason == null)
                return McuResult<McuResumeResponse>.Invalid(E.Val40, "Alasan wajib diisi.");
            if (reason.Length > 500)
                return McuResult<McuResumeResponse>.Invalid(E.InvalidRequest, "Alasan paling banyak 500 karakter.");

            if (order.Version != request.Version)
                return VersionConflict<McuResumeResponse>();

            var resume = await CurrentAsync(order.Id, cancellationToken);
            var block = ApprovedBlock(order, resume);
            if (block != null)
                return McuResult<McuResumeResponse>.Conflict(E.InvalidTransition, block);

            var now = DateTime.UtcNow;
            resume!.ResumeStatus = McuResumeStatus.Draft;
            resume.ReturnReason = reason;
            resume.UpdateDateTime = now;
            resume.UpdateBy = actorUserId;

            AddHistory(order.Id, McuOrderStatus.DoctorApproved, McuOrderStatus.AwaitingResume, actorUserId, reason, actorUserId, now);
            order.OrderStatus = McuOrderStatus.AwaitingResume;
            Touch(order, actorUserId, now);

            return await SaveAndMapAsync(order, resume, cancellationToken);
        }

        public async Task<McuResult<McuResumeResponse>> VerifyAsync(
            Guid orderId,
            McuResumeActionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuResumeResponse>.NotFound("Order MCU tidak ditemukan.");

            if (order.Version != request.Version)
                return VersionConflict<McuResumeResponse>();

            var resume = await CurrentAsync(order.Id, cancellationToken);
            var block = ApprovedBlock(order, resume);
            if (block != null)
                return McuResult<McuResumeResponse>.Conflict(E.InvalidTransition, block);

            // VAL-MCU-39 — pemisahan tugas.
            var actorDoctorId = await ActorDoctorIdAsync(actorUserId, cancellationToken);
            if (actorDoctorId.HasValue && actorDoctorId == resume!.ApprovedByDoctorId)
            {
                return McuResult<McuResumeResponse>.Forbidden(
                    E.Val39, "Verifikasi tidak boleh dilakukan oleh dokter yang menyetujui resume.");
            }

            // MCU-STATE-v1 1.1 — hasil Lab/Rad komponen yang selesai masih tersedia di modulnya.
            var missingResult = await MissingPartnerResultAsync(order.Id, cancellationToken);
            if (missingResult != null)
                return McuResult<McuResumeResponse>.Conflict(E.InvalidTransition, $"Hasil {missingResult} belum tersedia di modulnya.");

            var now = DateTime.UtcNow;
            resume!.ResumeStatus = McuResumeStatus.Verified;
            resume.VerifiedByUserId = actorUserId;
            resume.VerifiedAt = now;
            resume.UpdateDateTime = now;
            resume.UpdateBy = actorUserId;

            AddHistory(order.Id, McuOrderStatus.DoctorApproved, McuOrderStatus.Verified, actorUserId, null, actorUserId, now);
            order.OrderStatus = McuOrderStatus.Verified;
            Touch(order, actorUserId, now);

            return await SaveAndMapAsync(order, resume, cancellationToken);
        }

        // =====================================================================
        // Buka ulang — dokter MCU yang ditetapkan (BE-MCU-031)
        // =====================================================================

        /// <summary>
        /// Buka ulang resume yang sudah disetujui, diverifikasi, atau diserahkan (<c>MCU-DEC-011</c>): versi lama
        /// <i>Dikoreksi</i> dan tetap terbaca, versi baru <i>Draf</i> berisi salinan isinya, order kembali
        /// <i>Menunggu Resume</i> sehingga isian Pemeriksaan MCU terbuka lagi (perubahan nilai tercatat revisinya),
        /// dan setiap penyerahan versi lama ditandai perlu diberi tahu.
        /// </summary>
        /// <remarks>
        /// <b>Contoh.</b> Resume Budi versi 1 (<i>Fit</i>) sudah dikirim ke Budi dan PT Contoh. dr. Rina membuka ulang
        /// dengan alasan <i>salah baca nilai tekanan darah</i> → versi 1 <i>Dikoreksi</i>, versi 2 <i>Draf</i>, kedua
        /// penyerahan versi 1 ditandai perlu diberi tahu. Versi 2 wajib disetujui dan diverifikasi ulang.
        /// </remarks>
        public async Task<McuResult<McuResumeResponse>> ReopenAsync(
            Guid orderId,
            ReopenMcuResumeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuResumeResponse>.NotFound("Order MCU tidak ditemukan.");

            var actorDoctorId = await ActorDoctorIdAsync(actorUserId, cancellationToken);
            if (!IsAssignedDoctor(order, actorDoctorId))
                return Val38<McuResumeResponse>();

            // VAL-MCU-40
            var reason = Clean(request.Reason);
            if (reason == null)
                return McuResult<McuResumeResponse>.Invalid(E.Val40, "Alasan wajib diisi.");
            if (reason.Length > 500)
                return McuResult<McuResumeResponse>.Invalid(E.InvalidRequest, "Alasan paling banyak 500 karakter.");

            if (order.Version != request.Version)
                return VersionConflict<McuResumeResponse>();

            var previous = await CurrentAsync(order.Id, cancellationToken);
            var reopenable = order.OrderStatus is McuOrderStatus.DoctorApproved or McuOrderStatus.Verified or McuOrderStatus.Delivered
                && previous?.ResumeStatus is McuResumeStatus.Approved or McuResumeStatus.Verified;
            if (!reopenable)
            {
                return McuResult<McuResumeResponse>.Conflict(
                    E.InvalidTransition,
                    order.OrderStatus == McuOrderStatus.Cancelled
                        ? "Order sudah dibatalkan."
                        : "Resume belum disetujui dokter; ubah drafnya langsung.");
            }

            var now = DateTime.UtcNow;
            var fromStatus = order.OrderStatus;

            previous!.ResumeStatus = McuResumeStatus.Corrected;
            previous.IsCurrent = false;
            previous.ReopenReason = reason;
            previous.ReopenedByDoctorId = actorDoctorId;
            previous.ReopenedAt = now;
            previous.UpdateDateTime = now;
            previous.UpdateBy = actorUserId;

            var next = new McuResume
            {
                McuOrderId = order.Id,
                VersionNumber = previous.VersionNumber + 1,
                ResumeStatus = McuResumeStatus.Draft,
                IsCurrent = true,
                GeneralConditionText = previous.GeneralConditionText,
                ImportantFindingsText = previous.ImportantFindingsText,
                RecommendationText = previous.RecommendationText,
                FitnessCategoryId = previous.FitnessCategoryId,
                WorkRecommendationText = previous.WorkRecommendationText,
                AuthorDoctorId = actorDoctorId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            // Penerima versi lama wajib diberi tahu (MCU-DEC-011).
            var deliveries = await _dbContext.McuResultDeliveries
                .Where(x => x.McuResumeId == previous.Id && !x.IsDelete && !x.RequiresCorrectionNotice)
                .ToListAsync(cancellationToken);
            foreach (var delivery in deliveries)
            {
                delivery.RequiresCorrectionNotice = true;
                delivery.UpdateDateTime = now;
                delivery.UpdateBy = actorUserId;
            }

            AddHistory(order.Id, fromStatus, McuOrderStatus.AwaitingResume, actorUserId, reason, actorUserId, now);
            order.OrderStatus = McuOrderStatus.AwaitingResume;
            Touch(order, actorUserId, now);

            // Index unik versi berlaku: versi lama dilepas lebih dulu, lalu versi baru ditambah — satu transaksi.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.McuResumes.Add(next);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (exception is DbUpdateConcurrencyException
                || (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return VersionConflict<McuResumeResponse>();
            }

            return McuResult<McuResumeResponse>.Ok((await MapAsync(order, [next], cancellationToken)).Single());
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        /// <summary>Kembalikan/verifikasi hanya atas resume yang sedang <i>Disetujui Dokter</i>.</summary>
        private static string? ApprovedBlock(McuOrder order, McuResume? resume) =>
            order.OrderStatus == McuOrderStatus.DoctorApproved && resume?.ResumeStatus == McuResumeStatus.Approved ? null
            : order.OrderStatus == McuOrderStatus.Cancelled ? "Order sudah dibatalkan."
            : order.OrderStatus is McuOrderStatus.Verified or McuOrderStatus.Delivered ? "Resume sudah diverifikasi."
            : "Resume belum disetujui dokter.";

        /// <summary>Nama komponen Lab/Rad <i>Selesai</i> yang pesanannya tidak lagi selesai di modulnya, atau <c>null</c>.</summary>
        private async Task<string?> MissingPartnerResultAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var completed = await _dbContext.McuOrderComponents.AsNoTracking()
                .Where(x => x.McuOrderId == orderId && !x.IsDelete && x.ComponentStatus == McuOrderComponentStatus.Completed
                    && (x.LabOrderId != null || x.RadOrderId != null))
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.ComponentNameSnapshot,
                    LabDone = x.LabOrderId == null || _dbContext.LabOrders
                        .Any(l => l.Id == x.LabOrderId && !l.IsCancel && l.OrderStatus == LabOrderStatus.Completed),
                    RadDone = x.RadOrderId == null || _dbContext.RadOrders
                        .Any(r => r.Id == x.RadOrderId && !r.IsCancel && r.OrderStatus == RadOrderStatus.Completed)
                })
                .ToListAsync(cancellationToken);

            return completed.FirstOrDefault(x => !x.LabDone || !x.RadDone)?.ComponentNameSnapshot;
        }

        private Task<McuResume?> CurrentAsync(Guid orderId, CancellationToken cancellationToken) =>
            _dbContext.McuResumes.SingleOrDefaultAsync(x => x.McuOrderId == orderId && !x.IsDelete && x.IsCurrent, cancellationToken);

        private Task<Guid?> ActorDoctorIdAsync(Guid actorUserId, CancellationToken cancellationToken) =>
            _clinicalContextService.ResolveActorDoctorIdAsync(_httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);

        private static bool IsAssignedDoctor(McuOrder order, Guid? actorDoctorId) =>
            actorDoctorId.HasValue && actorDoctorId == order.McuDoctorId;

        /// <summary>Skrining perawat kunjungan MCU selesai (<c>INT-MCU-11</c>) — sama dengan isi Pemeriksaan MCU.</summary>
        private Task<bool> IsScreeningCompletedAsync(Guid? encounterId, CancellationToken cancellationToken) =>
            encounterId == null
                ? Task.FromResult(false)
                : _dbContext.Set<RegQueue>().AsNoTracking()
                    .AnyAsync(x => x.EncounterId == encounterId.Value && !x.IsDelete && x.ScreeningCompletedAt != null, cancellationToken);

        private async Task<McuResult<McuResumeResponse>> SaveAndMapAsync(McuOrder order, McuResume resume, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (exception is DbUpdateConcurrencyException
                || (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                // Dua simpan draf pertama serentak bertabrakan di index unik versi berlaku.
                _dbContext.ChangeTracker.Clear();
                return VersionConflict<McuResumeResponse>();
            }

            return McuResult<McuResumeResponse>.Ok((await MapAsync(order, [resume], cancellationToken)).Single());
        }

        private async Task<List<McuResumeResponse>> MapAsync(McuOrder order, List<McuResume> resumes, CancellationToken cancellationToken)
        {
            var doctorIds = resumes
                .SelectMany(x => new[] { x.AuthorDoctorId, x.ApprovedByDoctorId, x.ReopenedByDoctorId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var doctors = doctorIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Set<MstDoctor>().AsNoTracking()
                    .Where(x => doctorIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

            var userIds = resumes.Where(x => x.VerifiedByUserId.HasValue).Select(x => x.VerifiedByUserId!.Value).Distinct().ToList();
            var users = userIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Users.AsNoTracking()
                    .Where(x => userIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            var categoryIds = resumes.Where(x => x.FitnessCategoryId.HasValue).Select(x => x.FitnessCategoryId!.Value).Distinct().ToList();
            var categories = categoryIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.McuFitnessCategories.AsNoTracking()
                    .Where(x => categoryIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.FitnessCategoryName, cancellationToken);

            string? Name(Dictionary<Guid, string> source, Guid? id) => id.HasValue ? source.GetValueOrDefault(id.Value) : null;

            return resumes.Select(x => new McuResumeResponse
            {
                Id = x.Id,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderVersion = order.Version,
                VersionNumber = x.VersionNumber,
                ResumeStatus = x.ResumeStatus,
                ResumeStatusLabel = McuLabels.ResumeStatus(x.ResumeStatus),
                IsCurrent = x.IsCurrent,
                GeneralCondition = x.GeneralConditionText,
                ImportantFindings = x.ImportantFindingsText,
                Recommendation = x.RecommendationText,
                FitnessCategoryId = x.FitnessCategoryId,
                FitnessCategoryName = Name(categories, x.FitnessCategoryId),
                WorkRecommendation = x.WorkRecommendationText,
                AuthorDoctorName = Name(doctors, x.AuthorDoctorId),
                ApprovedByDoctorName = Name(doctors, x.ApprovedByDoctorId),
                ApprovedAt = x.ApprovedAt,
                ReturnReason = x.ReturnReason,
                VerifiedByName = Name(users, x.VerifiedByUserId),
                VerifiedAt = x.VerifiedAt,
                ReopenReason = x.ReopenReason,
                ReopenedByDoctorName = Name(doctors, x.ReopenedByDoctorId),
                ReopenedAt = x.ReopenedAt
            }).ToList();
        }

        private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        private static void Touch(McuOrder order, Guid actorUserId, DateTime now)
        {
            order.Version++;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;
        }

        private void AddHistory(
            Guid orderId,
            McuOrderStatus from,
            McuOrderStatus to,
            Guid? changedByUserId,
            string? reason,
            Guid actorUserId,
            DateTime now) =>
            _dbContext.McuOrderStatusHistories.Add(new McuOrderStatusHistory
            {
                McuOrderId = orderId,
                FromStatus = from,
                ToStatus = to,
                ChangedAt = now,
                ChangedByUserId = changedByUserId,
                Reason = reason,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

        private static McuResult<T> Val38<T>() =>
            McuResult<T>.Forbidden(E.Val38, "Hanya dokter MCU yang ditetapkan untuk peserta ini yang dapat melakukan tindakan ini.");

        private static McuResult<T> VersionConflict<T>() =>
            McuResult<T>.Conflict(E.Val46, "Data order sudah diubah petugas lain. Muat ulang lalu coba lagi.");
    }
}
