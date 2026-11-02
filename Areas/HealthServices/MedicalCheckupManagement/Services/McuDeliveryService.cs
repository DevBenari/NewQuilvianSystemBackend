using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Repositories;
using E = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services.McuErrorCodes;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Penyerahan hasil MCU — <c>BE-MCU-033</c>: data cetak hasil lengkap dan ringkasan kelayakan
    /// (<c>MCU-API-v1</c> grup Mcu Delivery, <c>MCU-DEC-010</c>, <c>MCU-FE-003</c>); <c>BE-MCU-034</c>: pencatatan
    /// penyerahan manual, pemberitahuan koreksi, dan persetujuan pelepasan (<c>VAL-MCU-31</c>..<c>33</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Ringkasan disusun server, dari kolom yang boleh saja.</b> Ringkasan kelayakan hanya membaca kategori
    /// kelayakan dan rekomendasi kerja; kueri dan tipe responsnya tidak pernah menyentuh nilai pemeriksaan, temuan,
    /// atau kesimpulan, sehingga layar tidak mungkin menyaring sendiri dari data lengkap.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Siti (PT Contoh), resume <i>Unfit sementara</i> dengan temuan <i>hipertensi, HBsAg reaktif</i>.
    /// <c>variant=fitness-summary</c> → <i>Unfit sementara</i> + <i>disarankan tidak bertugas di bagian pengolahan makanan
    /// sampai pemeriksaan ulang</i>; kata <i>HBsAg reaktif</i> tidak ada di respons. <c>variant=full</c> memuat seluruhnya
    /// beserta hasil Lab yang dirilis, laporan Rad yang dirilis, dan isian Pemeriksaan MCU.
    /// </para>
    /// <para>Data saja: tata cetak final menunggu brief layar.</para>
    /// </remarks>
    public class McuDeliveryService
    {
        public const string VariantFull = "full";
        public const string VariantFitnessSummary = "fitness-summary";

        private static readonly CultureInfo Indonesia = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _dbContext;

        public McuDeliveryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Ringkasan kelayakan — untuk perusahaan
        // =====================================================================

        public async Task<McuResult<McuFitnessSummaryDocumentResponse>> GetFitnessSummaryAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var source = await ReadableAsync(orderId, cancellationToken);
            if (source.Order == null)
                return Fail<McuFitnessSummaryDocumentResponse>(source);

            var (order, resume) = (source.Order!, source.Resume!);

            // Sengaja hanya dua kolom resume yang dibaca (MCU-DEC-010).
            var content = await _dbContext.McuResumes.AsNoTracking()
                .Where(x => x.Id == resume.Id)
                .Select(x => new
                {
                    FitnessCategoryName = x.FitnessCategory != null ? x.FitnessCategory.FitnessCategoryName : null,
                    x.WorkRecommendationText
                })
                .SingleAsync(cancellationToken);

            return McuResult<McuFitnessSummaryDocumentResponse>.Ok(new McuFitnessSummaryDocumentResponse
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                ExaminationDate = order.ExaminationDate,
                PackageName = order.PackageNameSnapshot,
                ParticipantName = order.ParticipantName,
                CompanyGuarantorName = await CompanyNameAsync(order.CompanyGuarantorId, cancellationToken),
                ResumeVersionNumber = resume.VersionNumber,
                IsVerified = resume.ResumeStatus == McuResumeStatus.Verified,
                ApprovedByDoctorName = await DoctorNameAsync(resume.ApprovedByDoctorId, cancellationToken),
                VerifiedAt = resume.VerifiedAt,
                FitnessCategoryName = content.FitnessCategoryName,
                WorkRecommendation = content.WorkRecommendationText
            });
        }

        // =====================================================================
        // Hasil lengkap — untuk peserta (atau perusahaan dengan persetujuan)
        // =====================================================================

        public async Task<McuResult<McuResultDocumentResponse>> GetFullDocumentAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var source = await ReadableAsync(orderId, cancellationToken);
            if (source.Order == null)
                return Fail<McuResultDocumentResponse>(source);

            var (order, resume) = (source.Order!, source.Resume!);

            var patient = order.PatientId.HasValue
                ? await _dbContext.Set<MstPatient>().AsNoTracking()
                    .Where(x => x.Id == order.PatientId.Value)
                    .Select(x => new { x.MedicalRecordNumber, x.BirthDate })
                    .SingleOrDefaultAsync(cancellationToken)
                : null;

            var components = await _dbContext.McuOrderComponents.AsNoTracking()
                .Where(x => x.McuOrderId == order.Id && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var verifiedByName = resume.VerifiedByUserId.HasValue
                ? await _dbContext.Users.AsNoTracking().Where(x => x.Id == resume.VerifiedByUserId.Value).Select(x => x.DisplayName).FirstOrDefaultAsync(cancellationToken)
                : null;

            var document = new McuResultDocumentResponse
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                ExaminationDate = order.ExaminationDate,
                PackageName = order.PackageNameSnapshot,
                ParticipantName = order.ParticipantName,
                MedicalRecordNumber = patient?.MedicalRecordNumber,
                ParticipantBirthDate = patient?.BirthDate is { } birth ? DateOnly.FromDateTime(birth) : order.ParticipantBirthDate,
                CompanyGuarantorName = await CompanyNameAsync(order.CompanyGuarantorId, cancellationToken),
                ResumeVersionNumber = resume.VersionNumber,
                ResumeStatus = resume.ResumeStatus,
                ResumeStatusLabel = McuLabels.ResumeStatus(resume.ResumeStatus),
                IsVerified = resume.ResumeStatus == McuResumeStatus.Verified,
                ApprovedByDoctorName = await DoctorNameAsync(resume.ApprovedByDoctorId, cancellationToken),
                ApprovedAt = resume.ApprovedAt,
                VerifiedByName = verifiedByName,
                VerifiedAt = resume.VerifiedAt,
                FitnessCategoryName = resume.FitnessCategoryId.HasValue
                    ? await _dbContext.McuFitnessCategories.AsNoTracking().Where(x => x.Id == resume.FitnessCategoryId.Value).Select(x => x.FitnessCategoryName).FirstOrDefaultAsync(cancellationToken)
                    : null,
                GeneralCondition = resume.GeneralConditionText,
                ImportantFindings = resume.ImportantFindingsText,
                Recommendation = resume.RecommendationText,
                WorkRecommendation = resume.WorkRecommendationText,
                Components = components.Select(x => new McuResultDocumentComponentResponse
                {
                    ComponentName = x.ComponentNameSnapshot,
                    ComponentType = x.ComponentType,
                    ComponentTypeLabel = McuPackageService.ComponentTypeLabel(x.ComponentType),
                    ComponentStatus = x.ComponentStatus,
                    ComponentStatusLabel = McuLabels.ComponentStatus(x.ComponentStatus),
                    CancelReason = x.CancelReason
                }).ToList()
            };

            for (var i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (component.ComponentStatus != McuOrderComponentStatus.Completed)
                    continue;

                switch (component.ComponentType)
                {
                    case McuComponentType.Laboratory when component.LabOrderId.HasValue && component.ProcedureId.HasValue:
                        document.Components[i].Lines = await LabLinesAsync(component.LabOrderId.Value, component.ProcedureId.Value, cancellationToken);
                        break;

                    case McuComponentType.Radiology when component.RadOrderId.HasValue:
                        await FillRadReportAsync(document.Components[i], component.RadOrderId.Value, cancellationToken);
                        break;

                    case McuComponentType.McuExamination:
                        await FillExaminationAsync(document.Components[i], component, cancellationToken);
                        break;
                }
            }

            return McuResult<McuResultDocumentResponse>.Ok(document);
        }

        // =====================================================================
        // Penyerahan — BE-MCU-034
        // =====================================================================

        public async Task<McuResult<List<McuResultDeliveryResponse>>> GetDeliveriesAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<List<McuResultDeliveryResponse>>.NotFound("Order MCU tidak ditemukan.");

            var deliveries = await _dbContext.McuResultDeliveries.AsNoTracking()
                .Where(x => x.McuOrderId == orderId && !x.IsDelete)
                .OrderByDescending(x => x.DeliveredAt)
                .ToListAsync(cancellationToken);

            return McuResult<List<McuResultDeliveryResponse>>.Ok(await MapDeliveriesAsync(order, deliveries, cancellationToken));
        }

        /// <summary>
        /// Catat penyerahan manual (<c>MCU-DEC-019</c>, <c>027</c>): hanya versi terverifikasi (<c>VAL-MCU-32</c>);
        /// hasil lengkap ke perusahaan butuh persetujuan pelepasan yang berlaku (<c>VAL-MCU-31</c>); WhatsApp, email,
        /// dan telepon wajib bertujuan (<c>VAL-MCU-33</c>). Penyerahan pertama versi itu memindah order ke
        /// <i>Diserahkan</i>.
        /// </summary>
        /// <remarks>
        /// <b>Contoh.</b> Dewi menyerahkan isi lengkap ke PT Contoh tanpa persetujuan Siti → <c>409</c> <i>"Peserta belum
        /// menyetujui pelepasan hasil lengkap kepada PT Contoh. Serahkan ringkasan kelayakan."</i> Dewi menyerahkan
        /// ringkasan lewat email <i>hrd@ptcontoh.co.id</i> → tersimpan dengan penerima, isi, saluran, tujuan, versi resume,
        /// petugas, dan waktu; order <i>Diserahkan</i>. Tujuan tidak pernah masuk log aplikasi.
        /// </remarks>
        public async Task<McuResult<McuResultDeliveryResponse>> CreateDeliveryAsync(
            Guid orderId,
            CreateMcuResultDeliveryRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuResultDeliveryResponse>.NotFound("Order MCU tidak ditemukan.");

            if (!Enum.IsDefined(request.RecipientType) || !Enum.IsDefined(request.ContentType) || !Enum.IsDefined(request.Channel))
                return McuResult<McuResultDeliveryResponse>.Invalid(E.InvalidRequest, "Penerima, isi, dan saluran penyerahan wajib dipilih.");

            // VAL-MCU-33
            var destination = string.IsNullOrWhiteSpace(request.DestinationText) ? null : request.DestinationText.Trim();
            var destinationError = DestinationError(request.Channel, destination);
            if (destinationError != null)
                return McuResult<McuResultDeliveryResponse>.Invalid(E.Val33, destinationError);

            var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            if (note?.Length > 500)
                return McuResult<McuResultDeliveryResponse>.Invalid(E.InvalidRequest, "Catatan paling banyak 500 karakter.");

            var now = DateTime.UtcNow;
            var deliveredAt = request.DeliveredAt?.ToUniversalTime() ?? now;
            if (deliveredAt > now.AddMinutes(5))
                return McuResult<McuResultDeliveryResponse>.Invalid(E.InvalidRequest, "Waktu penyerahan tidak boleh di masa depan.");

            if (order.Version != request.Version)
                return VersionConflict<McuResultDeliveryResponse>();

            if (order.OrderStatus == McuOrderStatus.Cancelled)
                return McuResult<McuResultDeliveryResponse>.Conflict(E.InvalidTransition, "Order sudah dibatalkan.");

            // VAL-MCU-32 — versi berlaku harus terverifikasi.
            var resume = await _dbContext.McuResumes.AsNoTracking()
                .SingleOrDefaultAsync(x => x.McuOrderId == order.Id && !x.IsDelete && x.IsCurrent, cancellationToken);
            if (resume?.ResumeStatus != McuResumeStatus.Verified
                || order.OrderStatus is not (McuOrderStatus.Verified or McuOrderStatus.Delivered))
            {
                return McuResult<McuResultDeliveryResponse>.Conflict(E.Val32, "Hasil belum diverifikasi.");
            }

            if (request.RecipientType == McuDeliveryRecipient.Company)
            {
                if (!order.CompanyGuarantorId.HasValue)
                    return McuResult<McuResultDeliveryResponse>.Invalid(E.InvalidRequest, "Order ini bukan peserta perusahaan.");

                // VAL-MCU-31
                if (request.ContentType == McuDeliveryContent.Full && !await HasActiveConsentAsync(order.Id, cancellationToken))
                {
                    var company = await CompanyNameAsync(order.CompanyGuarantorId, cancellationToken) ?? "perusahaan";
                    return McuResult<McuResultDeliveryResponse>.Conflict(
                        E.Val31, $"Peserta belum menyetujui pelepasan hasil lengkap kepada {company}. Serahkan ringkasan kelayakan.");
                }
            }

            var delivery = new McuResultDelivery
            {
                McuOrderId = order.Id,
                McuResumeId = resume.Id,
                RecipientType = request.RecipientType,
                ContentType = request.ContentType,
                Channel = request.Channel,
                DestinationText = destination,
                DeliveredAt = deliveredAt,
                DeliveredByUserId = actorUserId,
                Note = note,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.McuResultDeliveries.Add(delivery);

            // MCU-STATE-v1 1.1 — penyerahan pertama versi terverifikasi.
            if (order.OrderStatus == McuOrderStatus.Verified)
            {
                AddHistory(order.Id, McuOrderStatus.Verified, McuOrderStatus.Delivered, actorUserId, now);
                order.OrderStatus = McuOrderStatus.Delivered;
            }

            Touch(order, actorUserId, now);
            if (!await SaveAsync(cancellationToken))
                return VersionConflict<McuResultDeliveryResponse>();

            return McuResult<McuResultDeliveryResponse>.Ok((await MapDeliveriesAsync(order, [delivery], cancellationToken)).Single());
        }

        /// <summary>
        /// Catat bahwa penerima versi lama sudah diberi tahu koreksinya (<c>MCU-DEC-011</c>) — hanya penyerahan yang
        /// ditandai buka ulang (<c>BE-MCU-031</c>), sekali.
        /// </summary>
        public async Task<McuResult<McuResultDeliveryResponse>> RecordCorrectionNoticeAsync(
            Guid orderId,
            Guid deliveryId,
            RecordCorrectionNoticeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            var delivery = order == null
                ? null
                : await _dbContext.McuResultDeliveries.SingleOrDefaultAsync(x => x.Id == deliveryId && x.McuOrderId == orderId && !x.IsDelete, cancellationToken);
            if (order == null || delivery == null)
                return McuResult<McuResultDeliveryResponse>.NotFound("Penyerahan hasil MCU tidak ditemukan.");

            var now = DateTime.UtcNow;
            var notifiedAt = request.NotifiedAt?.ToUniversalTime() ?? now;
            if (notifiedAt > now.AddMinutes(5))
                return McuResult<McuResultDeliveryResponse>.Invalid(E.InvalidRequest, "Waktu pemberitahuan tidak boleh di masa depan.");

            if (order.Version != request.Version)
                return VersionConflict<McuResultDeliveryResponse>();

            if (!delivery.RequiresCorrectionNotice)
                return McuResult<McuResultDeliveryResponse>.Conflict(E.InvalidTransition, "Penyerahan ini tidak memerlukan pemberitahuan koreksi.");
            if (delivery.CorrectionNoticeRecordedAt.HasValue)
                return McuResult<McuResultDeliveryResponse>.Conflict(E.InvalidTransition, "Pemberitahuan koreksi sudah dicatat.");

            delivery.CorrectionNoticeRecordedAt = notifiedAt;
            delivery.UpdateDateTime = now;
            delivery.UpdateBy = actorUserId;
            Touch(order, actorUserId, now);
            if (!await SaveAsync(cancellationToken))
                return VersionConflict<McuResultDeliveryResponse>();

            return McuResult<McuResultDeliveryResponse>.Ok((await MapDeliveriesAsync(order, [delivery], cancellationToken)).Single());
        }

        // =====================================================================
        // Persetujuan pelepasan — BE-MCU-034
        // =====================================================================

        public async Task<McuResult<List<McuReleaseConsentResponse>>> GetConsentsAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<List<McuReleaseConsentResponse>>.NotFound("Order MCU tidak ditemukan.");

            var consents = await _dbContext.McuReleaseConsents.AsNoTracking()
                .Where(x => x.McuOrderId == orderId && !x.IsDelete)
                .OrderByDescending(x => x.ConsentGivenAt)
                .ToListAsync(cancellationToken);

            return McuResult<List<McuReleaseConsentResponse>>.Ok(await MapConsentsAsync(order, consents, cancellationToken));
        }

        /// <summary>
        /// Catat persetujuan peserta melepas hasil lengkap kepada perusahaan penjamin order (<c>MCU-DEC-010</c>).
        /// Satu persetujuan berlaku per order. Bentuk bukti final menunggu <c>MCU-OQ-003</c>.
        /// </summary>
        public async Task<McuResult<McuReleaseConsentResponse>> CreateConsentAsync(
            Guid orderId,
            CreateMcuReleaseConsentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuReleaseConsentResponse>.NotFound("Order MCU tidak ditemukan.");

            if (!Enum.IsDefined(request.EvidenceType))
                return McuResult<McuReleaseConsentResponse>.Invalid(E.InvalidRequest, "Bentuk bukti persetujuan wajib dipilih.");

            var reference = string.IsNullOrWhiteSpace(request.EvidenceReference) ? null : request.EvidenceReference.Trim();
            if (request.EvidenceType == McuConsentEvidenceType.PaperForm && reference == null)
                return McuResult<McuReleaseConsentResponse>.Invalid(E.InvalidRequest, "Isi nomor formulir persetujuan.");
            if (reference?.Length > 200)
                return McuResult<McuReleaseConsentResponse>.Invalid(E.InvalidRequest, "Nomor formulir paling banyak 200 karakter.");

            var now = DateTime.UtcNow;
            var givenAt = request.ConsentGivenAt?.ToUniversalTime() ?? now;
            if (givenAt > now.AddMinutes(5))
                return McuResult<McuReleaseConsentResponse>.Invalid(E.InvalidRequest, "Waktu persetujuan tidak boleh di masa depan.");

            if (order.Version != request.Version)
                return VersionConflict<McuReleaseConsentResponse>();

            if (order.OrderStatus == McuOrderStatus.Cancelled)
                return McuResult<McuReleaseConsentResponse>.Conflict(E.InvalidTransition, "Order sudah dibatalkan.");
            if (!order.CompanyGuarantorId.HasValue)
                return McuResult<McuReleaseConsentResponse>.Invalid(E.InvalidRequest, "Order ini bukan peserta perusahaan.");
            if (await HasActiveConsentAsync(order.Id, cancellationToken))
                return McuResult<McuReleaseConsentResponse>.Conflict(E.InvalidTransition, "Persetujuan pelepasan yang berlaku sudah ada.");

            var consent = new McuReleaseConsent
            {
                McuOrderId = order.Id,
                CompanyGuarantorId = order.CompanyGuarantorId.Value,
                ConsentGivenAt = givenAt,
                RecordedByUserId = actorUserId,
                EvidenceType = request.EvidenceType,
                EvidenceReference = reference,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.McuReleaseConsents.Add(consent);
            Touch(order, actorUserId, now);
            if (!await SaveAsync(cancellationToken))
                return VersionConflict<McuReleaseConsentResponse>();

            return McuResult<McuReleaseConsentResponse>.Ok((await MapConsentsAsync(order, [consent], cancellationToken)).Single());
        }

        public async Task<McuResult<McuReleaseConsentResponse>> RevokeConsentAsync(
            Guid orderId,
            Guid consentId,
            RevokeMcuReleaseConsentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            var consent = order == null
                ? null
                : await _dbContext.McuReleaseConsents.SingleOrDefaultAsync(x => x.Id == consentId && x.McuOrderId == orderId && !x.IsDelete, cancellationToken);
            if (order == null || consent == null)
                return McuResult<McuReleaseConsentResponse>.NotFound("Persetujuan pelepasan tidak ditemukan.");

            // VAL-MCU-40
            var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
            if (reason == null)
                return McuResult<McuReleaseConsentResponse>.Invalid(E.Val40, "Alasan wajib diisi.");
            if (reason.Length > 500)
                return McuResult<McuReleaseConsentResponse>.Invalid(E.InvalidRequest, "Alasan paling banyak 500 karakter.");

            if (order.Version != request.Version)
                return VersionConflict<McuReleaseConsentResponse>();
            if (consent.RevokedAt.HasValue)
                return McuResult<McuReleaseConsentResponse>.Conflict(E.InvalidTransition, "Persetujuan sudah dicabut.");

            var now = DateTime.UtcNow;
            consent.RevokedAt = now;
            consent.RevokedByUserId = actorUserId;
            consent.RevokeReason = reason;
            consent.UpdateDateTime = now;
            consent.UpdateBy = actorUserId;
            Touch(order, actorUserId, now);
            if (!await SaveAsync(cancellationToken))
                return VersionConflict<McuReleaseConsentResponse>();

            return McuResult<McuReleaseConsentResponse>.Ok((await MapConsentsAsync(order, [consent], cancellationToken)).Single());
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        /// <summary><c>VAL-MCU-33</c> — pesan galat tujuan, atau <c>null</c> bila sah.</summary>
        internal static string? DestinationError(McuDeliveryChannel channel, string? destination)
        {
            if (destination?.Length > 200)
                return "Tujuan penyerahan paling banyak 200 karakter.";

            switch (channel)
            {
                case McuDeliveryChannel.WhatsApp or McuDeliveryChannel.Phone:
                    if (destination == null)
                        return channel == McuDeliveryChannel.WhatsApp ? "Isi nomor WhatsApp tujuan." : "Isi nomor telepon tujuan.";
                    var digits = destination.Replace(" ", string.Empty).Replace("-", string.Empty);
                    if (digits.StartsWith('+'))
                        digits = digits[1..];
                    return digits.Length is >= 8 and <= 15 && digits.All(char.IsDigit) ? null : "Nomor tujuan tidak valid.";

                case McuDeliveryChannel.Email:
                    if (destination == null)
                        return "Isi alamat email tujuan.";
                    return System.Net.Mail.MailAddress.TryCreate(destination, out var address) && address.Address == destination
                        ? null
                        : "Format email tujuan tidak valid.";

                default:
                    return null;
            }
        }

        private Task<bool> HasActiveConsentAsync(Guid orderId, CancellationToken cancellationToken) =>
            _dbContext.McuReleaseConsents.AsNoTracking()
                .AnyAsync(x => x.McuOrderId == orderId && !x.IsDelete && x.RevokedAt == null, cancellationToken);

        private async Task<bool> SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException exception) when (exception is DbUpdateConcurrencyException
                || (exception.InnerException as Npgsql.PostgresException)?.SqlState == Npgsql.PostgresErrorCodes.UniqueViolation)
            {
                // Tabrakan versi order, atau dua persetujuan berlaku serentak (index unik).
                _dbContext.ChangeTracker.Clear();
                return false;
            }
        }

        private static void Touch(McuOrder order, Guid actorUserId, DateTime now)
        {
            order.Version++;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;
        }

        private void AddHistory(Guid orderId, McuOrderStatus from, McuOrderStatus to, Guid actorUserId, DateTime now) =>
            _dbContext.McuOrderStatusHistories.Add(new McuOrderStatusHistory
            {
                McuOrderId = orderId,
                FromStatus = from,
                ToStatus = to,
                ChangedAt = now,
                ChangedByUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

        private static McuResult<T> VersionConflict<T>() =>
            McuResult<T>.Conflict(E.Val46, "Data order sudah diubah petugas lain. Muat ulang lalu coba lagi.");

        private async Task<List<McuResultDeliveryResponse>> MapDeliveriesAsync(McuOrder order, List<McuResultDelivery> deliveries, CancellationToken cancellationToken)
        {
            var resumeIds = deliveries.Select(x => x.McuResumeId).Distinct().ToList();
            var versions = await _dbContext.McuResumes.AsNoTracking()
                .Where(x => resumeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.VersionNumber, cancellationToken);
            var users = await UserNamesAsync(deliveries.Select(x => (Guid?)x.DeliveredByUserId), cancellationToken);

            return deliveries.Select(x => new McuResultDeliveryResponse
            {
                Id = x.Id,
                OrderId = order.Id,
                OrderVersion = order.Version,
                ResumeVersionNumber = versions.GetValueOrDefault(x.McuResumeId),
                RecipientType = x.RecipientType,
                RecipientTypeLabel = McuLabels.DeliveryRecipient(x.RecipientType),
                ContentType = x.ContentType,
                ContentTypeLabel = McuLabels.DeliveryContent(x.ContentType),
                Channel = x.Channel,
                ChannelLabel = McuLabels.DeliveryChannel(x.Channel),
                DestinationText = x.DestinationText,
                DeliveredAt = x.DeliveredAt,
                DeliveredByName = users.GetValueOrDefault(x.DeliveredByUserId),
                Note = x.Note,
                RequiresCorrectionNotice = x.RequiresCorrectionNotice,
                CorrectionNoticeRecordedAt = x.CorrectionNoticeRecordedAt
            }).ToList();
        }

        private async Task<List<McuReleaseConsentResponse>> MapConsentsAsync(McuOrder order, List<McuReleaseConsent> consents, CancellationToken cancellationToken)
        {
            var users = await UserNamesAsync(consents.Select(x => (Guid?)x.RecordedByUserId).Concat(consents.Select(x => x.RevokedByUserId)), cancellationToken);
            var company = await CompanyNameAsync(order.CompanyGuarantorId, cancellationToken);

            return consents.Select(x => new McuReleaseConsentResponse
            {
                Id = x.Id,
                OrderId = order.Id,
                OrderVersion = order.Version,
                CompanyGuarantorName = company,
                ConsentGivenAt = x.ConsentGivenAt,
                RecordedByName = users.GetValueOrDefault(x.RecordedByUserId),
                EvidenceType = x.EvidenceType,
                EvidenceTypeLabel = McuLabels.ConsentEvidence(x.EvidenceType),
                EvidenceReference = x.EvidenceReference,
                IsActive = x.RevokedAt == null,
                RevokedAt = x.RevokedAt,
                RevokedByName = x.RevokedByUserId.HasValue ? users.GetValueOrDefault(x.RevokedByUserId.Value) : null,
                RevokeReason = x.RevokeReason
            }).ToList();
        }

        private async Task<Dictionary<Guid, string>> UserNamesAsync(IEnumerable<Guid?> userIds, CancellationToken cancellationToken)
        {
            var ids = userIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            return ids.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Users.AsNoTracking()
                    .Where(x => ids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }

        private sealed record Readable(McuOrder? Order, McuResume? Resume, bool NotFound = false, string? Code = null, string? Message = null);

        private static McuResult<T> Fail<T>(Readable source) =>
            source.NotFound ? McuResult<T>.NotFound(source.Message!) : McuResult<T>.Conflict(source.Code!, source.Message!);

        /// <summary>Dokumen hanya dari resume berlaku yang sudah disetujui dokter (Disetujui Dokter atau Terverifikasi).</summary>
        private async Task<Readable> ReadableAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return new Readable(null, null, NotFound: true, Message: "Order MCU tidak ditemukan.");

            if (order.OrderStatus == McuOrderStatus.Cancelled)
                return new Readable(null, null, Code: E.InvalidTransition, Message: "Order sudah dibatalkan.");

            var resume = await _dbContext.McuResumes.AsNoTracking()
                .SingleOrDefaultAsync(x => x.McuOrderId == order.Id && !x.IsDelete && x.IsCurrent, cancellationToken);
            if (resume?.ResumeStatus is not (McuResumeStatus.Approved or McuResumeStatus.Verified))
                return new Readable(null, null, Code: E.InvalidTransition, Message: "Resume belum disetujui dokter.");

            return new Readable(order, resume);
        }

        /// <summary>Pemeriksaan Lab komponen yang sudah dirilis — nilai, satuan, rentang normal, dan tanda di luar rujukan.</summary>
        private async Task<List<McuResultDocumentLineResponse>> LabLinesAsync(Guid labOrderId, Guid procedureId, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.LabExaminations.AsNoTracking()
                .Where(x => x.LabOrderId == labOrderId && x.ProcedureId == procedureId && !x.IsDelete && x.ReleasedAt != null
                    && x.ExaminationStatus != LabExaminationStatus.Cancelled && x.ExaminationStatus != LabExaminationStatus.Voided)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.ProcedureNameSnapshot,
                    x.ResultNumeric,
                    OptionName = x.ResultOption != null ? x.ResultOption.OptionName : null,
                    OptionOutOfReference = x.ResultOption != null && x.ResultOption.IsOutOfReference,
                    x.ResultUnitSnapshot,
                    NormalLow = x.ResultValueBound != null ? x.ResultValueBound.NormalLow : null,
                    NormalHigh = x.ResultValueBound != null ? x.ResultValueBound.NormalHigh : null,
                    BoundUnit = x.ResultValueBound != null ? x.ResultValueBound.Unit : null
                })
                .ToListAsync(cancellationToken);

            return rows.Select(x => new McuResultDocumentLineResponse
            {
                Name = x.ProcedureNameSnapshot ?? string.Empty,
                Value = x.ResultNumeric.HasValue ? x.ResultNumeric.Value.ToString("0.####", Indonesia) : x.OptionName,
                Unit = x.ResultUnitSnapshot ?? x.BoundUnit,
                ReferenceText = Reference(x.NormalLow, x.NormalHigh),
                IsOutOfReference = x.ResultNumeric.HasValue
                    ? (x.NormalLow.HasValue && x.ResultNumeric < x.NormalLow) || (x.NormalHigh.HasValue && x.ResultNumeric > x.NormalHigh)
                    : x.OptionOutOfReference
            }).ToList();
        }

        /// <summary>Versi laporan Rad terakhir yang dirilis.</summary>
        private async Task FillRadReportAsync(McuResultDocumentComponentResponse target, Guid radOrderId, CancellationToken cancellationToken)
        {
            var report = await _dbContext.RadReportVersions.AsNoTracking()
                .Where(v => !v.IsDelete && v.VersionStatus == RadReportVersionStatus.Released
                    && v.RadReport!.RadOrderId == radOrderId && !v.RadReport.IsDelete)
                .Select(v => new { v.RadReport!.ReportNumber, v.VersionNumber, v.Findings, v.Impression, v.Recommendation, v.ReleasedAt })
                .OrderByDescending(x => x.ReleasedAt)
                .ThenByDescending(x => x.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (report == null)
                return;

            target.RadReportNumber = report.ReportNumber;
            target.RadFindings = report.Findings;
            target.RadImpression = report.Impression;
            target.RadRecommendation = report.Recommendation;
            target.ReleasedAt = report.ReleasedAt;
        }

        private async Task FillExaminationAsync(McuResultDocumentComponentResponse target, McuOrderComponent component, CancellationToken cancellationToken)
        {
            var result = await _dbContext.McuExaminationResults.AsNoTracking()
                .Where(x => x.McuOrderComponentId == component.Id && !x.IsDelete)
                .Select(x => new
                {
                    x.ConclusionText,
                    Values = x.Values.Where(v => !v.IsDelete).Select(v => new { v.ExaminationFieldId, v.ValueText }).ToList()
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (result == null || !component.ExaminationTypeId.HasValue)
                return;

            var fields = await _dbContext.McuExaminationFields.AsNoTracking()
                .Where(x => x.ExaminationTypeId == component.ExaminationTypeId.Value && !x.IsDelete)
                .OrderBy(x => x.FieldNumber)
                .Select(x => new { x.Id, x.FieldLabel, x.UnitName })
                .ToListAsync(cancellationToken);

            target.Conclusion = result.ConclusionText;
            target.Lines = fields.Select(f => new McuResultDocumentLineResponse
            {
                Name = f.FieldLabel,
                Value = result.Values.FirstOrDefault(v => v.ExaminationFieldId == f.Id)?.ValueText,
                Unit = f.UnitName
            }).ToList();
        }

        private static string? Reference(decimal? low, decimal? high) =>
            (low, high) switch
            {
                ({ } l, { } h) => $"{l.ToString("0.####", Indonesia)} – {h.ToString("0.####", Indonesia)}",
                ({ } l, null) => $"≥ {l.ToString("0.####", Indonesia)}",
                (null, { } h) => $"≤ {h.ToString("0.####", Indonesia)}",
                _ => null
            };

        private Task<string?> CompanyNameAsync(Guid? companyGuarantorId, CancellationToken cancellationToken) =>
            companyGuarantorId.HasValue
                ? _dbContext.Set<MstCompanyGuarantor>().AsNoTracking()
                    .Where(x => x.Id == companyGuarantorId.Value)
                    .Select(x => (string?)x.CompanyGuarantorName)
                    .FirstOrDefaultAsync(cancellationToken)
                : Task.FromResult<string?>(null);

        private Task<string?> DoctorNameAsync(Guid? doctorId, CancellationToken cancellationToken) =>
            doctorId.HasValue
                ? _dbContext.Set<MstDoctor>().AsNoTracking()
                    .Where(x => x.Id == doctorId.Value)
                    .Select(x => (string?)x.FullName)
                    .FirstOrDefaultAsync(cancellationToken)
                : Task.FromResult<string?>(null);
    }
}
