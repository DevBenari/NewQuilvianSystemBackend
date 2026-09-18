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
    /// Jalur laporan Patologi Anatomi dan konteks klinis (<c>S4c</c>, <c>r25</c> bagian 20.2).
    ///
    /// <b>Batas berkas ini: MENGISI laporan.</b> Bukan memvalidasi, bukan merilis, bukan
    /// mengirim. Ketiganya milik <c>S4e</c> dan masih tertahan <c>DEC-LAB-011</c>. <b>Nol status
    /// hasil</b> diperkenalkan (<c>INV-36</c>) — selesai-tidaknya dibaca dari <c>FinalizedAt</c>.
    ///
    /// <b>Dua orang bekerja pada satu pesanan, dan berkas ini menjaga batas di antara
    /// keduanya.</b> Dokter pemesan menulis konteks klinis; patolog menulis laporan. Keduanya
    /// tampil pada layar yang sama, tetapi hak aksesnya berbeda — <c>LabOrder : Update</c> versus
    /// <c>LabExamination : Update</c> (<c>LAB-DEC-091</c>, <c>INV-40</c>). Berbagi satu izin
    /// berarti patolog dapat menulis riwayat penyakit yang tidak pernah ia tanyakan.
    ///
    /// <b>Yang paling mudah salah dibangun di sini</b> adalah bentuk formulirnya. Ia bukan
    /// daftar tetap: ia disusun dari golongan yang melekat pada jenis pemeriksaan pesanan, dan
    /// satu ruas yang dipakai dua golongan muncul <b>sekali</b>. Pesanan Histologi +
    /// Imunohistokimia adalah kasus yang membuktikannya.
    /// </summary>
    public class LabPathologyReportService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        /// <summary>
        /// Ruas yang <b>nol boleh</b> dikirim pemanggil, sebab seluruhnya diturunkan server
        /// (<c>AC-140</c>, <c>INV-38</c>). Ditolak terbuka, bukan diabaikan diam-diam.
        /// </summary>
        private static readonly string[] ServerDerivedFields =
        {
            "issuedAt", "effectiveAt", "finalizedAt", "finalizedByUserId",
            "reopenCount", "isFinalized", "parameterName", "parameterNameSnapshot"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabPathologyReportService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        // =================================================================
        // Laporan — baca
        // =================================================================

        public async Task<LabPathologyReportResponse> GetReportAsync(
            Guid labOrderId,
            CancellationToken cancellationToken = default)
        {
            var order = await ResolveAnatomicalPathologyOrderAsync(labOrderId, cancellationToken);
            var report = await FindReportAsync(labOrderId, tracked: false, cancellationToken);

            return await BuildReportResponseAsync(order, report, cancellationToken);
        }

        // =================================================================
        // Laporan — menyimpan
        // =================================================================

        /// <summary>
        /// Menyimpan seluruh isi laporan sekaligus. Ruas yang tidak dikirim <b>dikosongkan</b>;
        /// bentuknya memang mengganti, bukan menambal.
        ///
        /// <b>Nilai kosong atau hanya spasi diperlakukan sebagai belum diisi</b> dan barisnya
        /// dicabut, bukan disimpan sebagai teks kosong. Baris kosong yang tersimpan akan lolos
        /// <c>VAL-95</c> dan membuat laporan dapat difinalkan dalam keadaan tidak lengkap.
        /// </summary>
        public async Task<LabPathologyReportResponse> SaveReportAsync(
            Guid labOrderId,
            LabPathologyReportRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var order = await ResolveAnatomicalPathologyOrderAsync(labOrderId, cancellationToken);

            // AC-140. Diperiksa sebelum apa pun yang lain, supaya permintaan yang membawa ruas
            // turunan ditolak tanpa sempat mengubah satu baris pun.
            RejectServerDerivedFields(request.ExtraFields);

            foreach (var value in request.Values)
                RejectServerDerivedFields(value.ExtraFields);

            var report = await FindReportAsync(labOrderId, tracked: true, cancellationToken);

            // VAL-96. Laporan yang sudah difinalkan hanya dapat diubah sesudah dibuka kembali,
            // dan membukanya kembali menuntut alasan.
            if (report != null && report.FinalizedAt.HasValue)
            {
                throw new LabPathologyReportConflictException(
                    "Laporan ini sudah diselesaikan. Buka kembali lebih dulu bila perlu diubah.");
            }

            // VAL-94.
            var duplicate = request.Values
                .GroupBy(x => x.LabPathologyParameterId)
                .Any(g => g.Count() > 1);

            if (duplicate)
                throw new LabPathologyReportValidationException("Isian yang sama dikirim dua kali.");

            var form = await ResolveFormAsync(order, cancellationToken);

            // VAL-93 dan VAL-99 sekaligus. Himpunan yang berlaku hanya memuat parameter AKTIF
            // pada golongan AKTIF, sehingga parameter yang dinonaktifkan tertolak di sini —
            // sementara nilai lama yang sudah menunjuk ke sana tetap terbaca (INV-37).
            var invalid = request.Values
                .Where(x => !form.ParametersById.ContainsKey(x.LabPathologyParameterId))
                .ToList();

            if (invalid.Count > 0)
            {
                throw new LabPathologyReportValidationException(
                    "Isian ini tidak dipakai pada jenis pemeriksaan tersebut.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (report == null)
            {
                report = new LabPathologyReport
                {
                    LabOrderId = labOrderId,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.LabPathologyReports.Add(report);
            }
            else
            {
                report.UpdateDateTime = now;
                report.UpdateBy = actorUserId;
            }

            report.FindingStatus = request.FindingStatus;
            report.AnalystUserId = request.AnalystUserId;

            // Laporan disimpan lebih dulu supaya nilainya punya induk ber-Id yang sah ketika
            // baris baru ditambahkan pada permintaan pertama.
            await _dbContext.SaveChangesAsync(cancellationToken);

            var existingValues = await _dbContext.LabPathologyReportValues
                .Where(x => x.LabPathologyReportId == report.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var existingByParameterId = existingValues.ToDictionary(x => x.LabPathologyParameterId);

            var sent = request.Values
                .Select(x => new
                {
                    x.LabPathologyParameterId,
                    Value = string.IsNullOrWhiteSpace(x.Value) ? null : x.Value.Trim()
                })
                .ToList();

            var kept = new HashSet<Guid>();

            foreach (var item in sent)
            {
                if (item.Value == null)
                    continue;

                kept.Add(item.LabPathologyParameterId);

                var parameter = form.ParametersById[item.LabPathologyParameterId];

                if (existingByParameterId.TryGetValue(item.LabPathologyParameterId, out var row))
                {
                    if (row.Value == item.Value)
                        continue;

                    row.Value = item.Value;
                    row.ParameterNameSnapshot = parameter.ParameterName;
                    row.UpdateDateTime = now;
                    row.UpdateBy = actorUserId;

                    continue;
                }

                _dbContext.LabPathologyReportValues.Add(new LabPathologyReportValue
                {
                    LabPathologyReportId = report.Id,
                    LabPathologyParameterId = item.LabPathologyParameterId,

                    // Disalin dari data induk, bukan dari pemanggil. Nama data induk yang
                    // diperbarui kelak nol berlaku surut ke laporan ini.
                    ParameterNameSnapshot = parameter.ParameterName,
                    Value = item.Value,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            foreach (var row in existingValues.Where(x => !kept.Contains(x.LabPathologyParameterId)))
            {
                row.IsDelete = true;
                row.DeleteDateTime = now;
                row.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Perubahan SESUDAH laporan pernah difinalkan adalah amandemen, dan ia menuntut
            // alasan. Alasannya diwarisi dari pembukaan kembali yang mendahuluinya — VAL-96
            // memastikan nol perubahan dapat terjadi tanpa melewati reopen lebih dulu.
            var isAmendment = report.ReopenCount > 0;

            await RecordAuditAsync(
                order,
                isAmendment ? "PathologyReport.AmendValue" : "PathologyReport.Write",
                isAmendment ? "Amended" : "Written",
                isAmendment ? await ResolveLastReopenReasonAsync(order.Id, cancellationToken) : null,
                now,
                actorUserId,
                cancellationToken);

            // Payload logger HANYA memuat penunjuk dan jumlah. Nol isi parameter, nol diagnosa,
            // nol status temuan — seluruhnya diagnosis pasien (LAB-PERM-v1 rev 7 bagian 9.3).
            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyReport.Write",
                "Menyimpan laporan Patologi Anatomi.",
                new
                {
                    report.Id,
                    report.LabOrderId,
                    JumlahRuasTerisi = kept.Count,
                    ActorUserId = actorUserId
                });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await BuildReportResponseAsync(order, report, cancellationToken);
        }

        // =================================================================
        // Laporan — menyelesaikan dan membuka kembali
        // =================================================================

        public async Task<LabPathologyReportResponse> FinalizeAsync(
            Guid labOrderId,
            CancellationToken cancellationToken = default)
        {
            var order = await ResolveAnatomicalPathologyOrderAsync(labOrderId, cancellationToken);
            var form = await ResolveFormAsync(order, cancellationToken);

            // VAL-100 diperiksa PALING DULU, dan urutannya menentukan kegunaan pesannya.
            // Pesanan yang belum digolongkan juga belum punya laporan, sehingga memeriksa
            // "belum pernah disimpan" lebih dulu akan menjawab dengan sebab yang benar tetapi
            // NOL menolong — patolog tidak dapat menyimpan apa pun selama formulirnya kosong.
            // Keadaan ini PASTI terjadi pada hari pertama, dan pesannya wajib menyebut apa yang
            // belum diatur DAN siapa yang mengaturnya.
            if (form.Fields.Count == 0)
            {
                throw new LabPathologyReportValidationException(
                    "Jenis pemeriksaan pada pesanan ini belum digolongkan ke kategori Patologi Anatomi. Hubungi kepala instalasi.");
            }

            var report = await FindReportAsync(labOrderId, tracked: true, cancellationToken);

            if (report == null)
            {
                throw new LabPathologyReportValidationException(
                    "Laporan ini belum pernah disimpan, sehingga belum dapat diselesaikan.");
            }

            // Memfinalkan ulang akan menimpa waktu dan pelaku finalisasi yang sudah tercatat —
            // menulis ulang riwayat, bukan mencatatnya. Ditolak terbuka. Aturan ini nol punya
            // nomor VAL; ia turunan langsung INV-35 yang memperlakukan finalisasi sebagai FAKTA.
            if (report.FinalizedAt.HasValue)
            {
                throw new LabPathologyReportConflictException(
                    "Laporan ini sudah diselesaikan.");
            }

            var filled = await _dbContext.LabPathologyReportValues
                .AsNoTracking()
                .Where(x => x.LabPathologyReportId == report.Id && !x.IsDelete)
                .Select(x => x.LabPathologyParameterId)
                .ToListAsync(cancellationToken);

            var filledSet = filled.ToHashSet();

            // VAL-95. Jawabannya WAJIB menyebut ruas mana saja yang kosong: menolak dengan
            // "laporan belum lengkap" saja akan membuat patolog menebak ruas mana yang terlewat
            // pada formulir berisi sampai lima belas isian.
            var missing = form.Fields
                .Where(x => x.IsRequired && !filledSet.Contains(x.LabPathologyParameterId))
                .OrderBy(x => x.SortOrder)
                .Select(x => x.ParameterName)
                .ToList();

            if (missing.Count > 0)
            {
                throw new LabPathologyReportValidationException(
                    $"Laporan belum dapat diselesaikan. Isian berikut masih kosong: {string.Join(", ", missing)}.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            report.FinalizedAt = now;

            // Guid.Empty TIDAK ditulis. Kolom ini nol ber-foreign key, sehingga Guid.Empty akan
            // tersimpan diam-diam sebagai pelaku yang tidak pernah ada — peringatan REG-ACTOR-FK
            // dan BE-EXT-05, dan di sini lebih berbahaya karena nol galat akan muncul.
            report.FinalizedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
            report.UpdateDateTime = now;
            report.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RecordAuditAsync(
                order, "PathologyReport.Finalize", "Finalized", null, now, actorUserId, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyReport.Finalize",
                "Menyelesaikan laporan Patologi Anatomi.",
                new { report.Id, report.LabOrderId, ActorUserId = actorUserId });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await BuildReportResponseAsync(order, report, cancellationToken);
        }

        public async Task<LabPathologyReportResponse> ReopenAsync(
            Guid labOrderId,
            LabPathologyReopenRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var order = await ResolveAnatomicalPathologyOrderAsync(labOrderId, cancellationToken);
            var report = await FindReportAsync(labOrderId, tracked: true, cancellationToken);

            // VAL-98.
            if (report == null || !report.FinalizedAt.HasValue)
            {
                throw new LabPathologyReportConflictException(
                    "Laporan ini belum pernah diselesaikan.");
            }

            // VAL-97.
            var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

            if (reason == null)
            {
                throw new LabPathologyReportValidationException(
                    "Tuliskan alasan membuka kembali laporan ini.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            report.FinalizedAt = null;
            report.FinalizedByUserId = null;
            report.ReopenCount += 1;
            report.UpdateDateTime = now;
            report.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // ReopenCount saja tidak cukup — yang dibutuhkan riwayat per kejadian beserta
            // alasannya, dan inilah tempatnya.
            await RecordAuditAsync(
                order, "PathologyReport.Reopen", "Reopened", reason, now, actorUserId, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyReport.Reopen",
                "Membuka kembali laporan Patologi Anatomi.",
                new { report.Id, report.LabOrderId, report.ReopenCount, ActorUserId = actorUserId });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await BuildReportResponseAsync(order, report, cancellationToken);
        }

        // =================================================================
        // Konteks klinis
        // =================================================================

        public async Task<LabPathologyOrderContextResponse> GetContextAsync(
            Guid labOrderId,
            CancellationToken cancellationToken = default)
        {
            await ResolveAnatomicalPathologyOrderAsync(labOrderId, cancellationToken);

            var context = await _dbContext.LabPathologyOrderContexts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LabOrderId == labOrderId && !x.IsDelete, cancellationToken);

            return MapContext(labOrderId, context);
        }

        public async Task<LabPathologyOrderContextResponse> SaveContextAsync(
            Guid labOrderId,
            LabPathologyOrderContextRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            // VAL-102.
            var order = await ResolveAnatomicalPathologyOrderAsync(
                labOrderId, cancellationToken, "Konteks klinis hanya diisi untuk pemeriksaan Patologi Anatomi.");

            var context = await _dbContext.LabPathologyOrderContexts
                .FirstOrDefaultAsync(x => x.LabOrderId == labOrderId && !x.IsDelete, cancellationToken);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (context == null)
            {
                context = new LabPathologyOrderContext
                {
                    LabOrderId = labOrderId,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.LabPathologyOrderContexts.Add(context);
            }
            else
            {
                context.UpdateDateTime = now;
                context.UpdateBy = actorUserId;
            }

            context.InitialDiagnosis = Normalize(request.InitialDiagnosis);
            context.RelevantHistory = Normalize(request.RelevantHistory);
            context.LastMenstrualPeriod = request.LastMenstrualPeriod;
            context.ClinicalNote = Normalize(request.ClinicalNote);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RecordAuditAsync(
                order, "PathologyContext.Write", "Written", null, now, actorUserId, cancellationToken);

            // Riwayat penyakit dan masa terakhir haid NOL boleh masuk log.
            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyContext.Write",
                "Menyimpan konteks klinis pesanan Patologi Anatomi.",
                new { context.Id, context.LabOrderId, ActorUserId = actorUserId });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapContext(labOrderId, context);
        }

        // =================================================================
        // Penyusun formulir
        // =================================================================

        /// <summary>
        /// Menyusun ruas yang berlaku bagi sebuah pesanan.
        ///
        /// <b>Inti <c>LAB-DEC-086</c>, dan bagian yang paling mudah salah.</b> Jenis pemeriksaan
        /// pesanan digolongkan lewat <see cref="LabProcedurePathologyCategory"/>; golongan itu
        /// menentukan ruasnya; dan satu ruas yang dipakai <b>dua</b> golongan muncul
        /// <b>sekali</b> (<c>AC-136</c>).
        ///
        /// <b>Wajib-tidaknya diambil dari yang paling ketat.</b> Bila sebuah ruas wajib pada
        /// salah satu golongan pesanan, ia wajib — melonggarkannya berarti membiarkan laporan
        /// difinalkan tanpa isian yang salah satu golongannya menuntut.
        /// </summary>
        private async Task<PathologyForm> ResolveFormAsync(
            LabOrder order,
            CancellationToken cancellationToken)
        {
            // Jenis pemeriksaan yang benar-benar dipesan. Pesanan lama yang nol punya baris
            // terpesan jatuh kembali ke penunjuk WAKIL pada pesanan — pola aditif yang sama
            // dengan VAL-68/VAL-69, sehingga pesanan lama nol berubah perilakunya.
            var procedureIds = await _dbContext.LabOrderedProcedures
                .AsNoTracking()
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete)
                .Select(x => x.ProcedureId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (procedureIds.Count == 0)
                procedureIds = new List<Guid> { order.ProcedureId };

            var categoryIds = await _dbContext.LabProcedurePathologyCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete && procedureIds.Contains(x.ProcedureId))
                .Join(
                    _dbContext.LabPathologyCategories.AsNoTracking().Where(c => !c.IsDelete && c.IsActive),
                    pemetaan => pemetaan.LabPathologyCategoryId,
                    category => category.Id,
                    (pemetaan, category) => category.Id)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (categoryIds.Count == 0)
                return new PathologyForm(new List<LabPathologyReportFieldResponse>(), new Dictionary<Guid, LabPathologyParameter>());

            var rows = await _dbContext.LabPathologyParameterCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete && categoryIds.Contains(x.LabPathologyCategoryId))
                .Join(
                    _dbContext.LabPathologyParameters.AsNoTracking().Where(p => !p.IsDelete && p.IsActive),
                    keberlakuan => keberlakuan.LabPathologyParameterId,
                    parameter => parameter.Id,
                    (keberlakuan, parameter) => new { keberlakuan, parameter })
                .Join(
                    _dbContext.LabPathologyCategories.AsNoTracking(),
                    x => x.keberlakuan.LabPathologyCategoryId,
                    category => category.Id,
                    (x, category) => new
                    {
                        x.parameter,
                        x.keberlakuan.IsRequired,
                        category.CategoryCode
                    })
                .ToListAsync(cancellationToken);

            // Pengelompokan inilah yang membuat Anjuran muncul SEKALI pada pesanan Histologi +
            // Imunohistokimia, dan golongan pemakainya tetap terbaca lewat CategoryCodes.
            var fields = rows
                .GroupBy(x => x.parameter.Id)
                .Select(g => new LabPathologyReportFieldResponse
                {
                    LabPathologyParameterId = g.Key,
                    ParameterCode = g.First().parameter.ParameterCode,
                    ParameterName = g.First().parameter.ParameterName,
                    SortOrder = g.First().parameter.SortOrder,
                    IsRequired = g.Any(x => x.IsRequired),
                    CategoryCodes = g.Select(x => x.CategoryCode).Distinct().OrderBy(x => x).ToList()
                })
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ParameterName)
                .ToList();

            var parametersById = rows
                .GroupBy(x => x.parameter.Id)
                .ToDictionary(g => g.Key, g => g.First().parameter);

            return new PathologyForm(fields, parametersById);
        }

        private async Task<LabPathologyReportResponse> BuildReportResponseAsync(
            LabOrder order,
            LabPathologyReport? report,
            CancellationToken cancellationToken)
        {
            var form = await ResolveFormAsync(order, cancellationToken);

            var fields = form.Fields
                .Select(x => new LabPathologyReportFieldResponse
                {
                    LabPathologyParameterId = x.LabPathologyParameterId,
                    ParameterCode = x.ParameterCode,
                    ParameterName = x.ParameterName,
                    SortOrder = x.SortOrder,
                    IsRequired = x.IsRequired,
                    CategoryCodes = x.CategoryCodes
                })
                .ToList();

            if (report != null)
            {
                var values = await _dbContext.LabPathologyReportValues
                    .AsNoTracking()
                    .Where(x => x.LabPathologyReportId == report.Id && !x.IsDelete)
                    .ToListAsync(cancellationToken);

                var byParameterId = values.ToDictionary(x => x.LabPathologyParameterId);

                foreach (var field in fields)
                {
                    if (byParameterId.TryGetValue(field.LabPathologyParameterId, out var row))
                        field.Value = row.Value;
                }

                // Nilai lama yang parameternya kemudian dinonaktifkan, atau yang golongannya
                // dicabut dari pemetaan, TETAP dikembalikan — ditandai tidak wajib dan tanpa
                // golongan. INV-37: laporan lama wajib tetap terbaca utuh. Menyembunyikannya
                // akan membuat isi diagnosis lenyap dari layar tanpa satu pun sebab yang terlihat.
                var known = fields.Select(x => x.LabPathologyParameterId).ToHashSet();

                foreach (var row in values.Where(x => !known.Contains(x.LabPathologyParameterId)))
                {
                    fields.Add(new LabPathologyReportFieldResponse
                    {
                        LabPathologyParameterId = row.LabPathologyParameterId,
                        ParameterCode = string.Empty,
                        ParameterName = row.ParameterNameSnapshot,
                        SortOrder = int.MaxValue,
                        IsRequired = false,
                        Value = row.Value
                    });
                }
            }

            var effectiveAt = await _dbContext.LabSpecimens
                .AsNoTracking()
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete && x.CollectedAt != null)
                .MinAsync(x => (DateTime?)x.CollectedAt, cancellationToken);

            return new LabPathologyReportResponse
            {
                LabOrderId = order.Id,
                Id = report?.Id,
                FindingStatus = report?.FindingStatus,
                AnalystUserId = report?.AnalystUserId,
                FinalizedAt = report?.FinalizedAt,
                FinalizedByUserId = report?.FinalizedByUserId,
                ReopenCount = report?.ReopenCount ?? 0,

                // Diturunkan, bukan dibaca dari kolom status. INV-36.
                IsFinalized = report?.FinalizedAt != null,

                // INV-38. Keduanya nol disimpan.
                IssuedAt = report?.FinalizedAt,
                EffectiveAt = effectiveAt,

                Fields = fields.OrderBy(x => x.SortOrder).ThenBy(x => x.ParameterName).ToList(),

                FormUnavailableReason = form.Fields.Count == 0
                    ? "Jenis pemeriksaan pada pesanan ini belum digolongkan ke kategori Patologi Anatomi. Hubungi kepala instalasi."
                    : null
            };
        }

        // =================================================================
        // Pembantu
        // =================================================================

        /// <summary>
        /// <c>VAL-92</c> dan <c>VAL-102</c>. Pesanan wajib ada, belum dibatalkan, dan berdisiplin
        /// Patologi Anatomi.
        /// </summary>
        private async Task<LabOrder> ResolveAnatomicalPathologyOrderAsync(
            Guid labOrderId,
            CancellationToken cancellationToken,
            string? disciplineMessage = null)
        {
            var order = await _dbContext.LabOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == labOrderId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pesanan laboratorium tidak ditemukan.");

            if (order.OrderStatus == LabOrderStatus.Cancelled)
            {
                throw new LabPathologyReportValidationException(
                    "Pesanan ini sudah dibatalkan, sehingga laporannya tidak dapat diisi.");
            }

            if (order.Discipline != LabDiscipline.AnatomicalPathology)
            {
                throw new LabPathologyReportValidationException(
                    disciplineMessage ?? "Pesanan ini bukan pemeriksaan Patologi Anatomi.");
            }

            return order;
        }

        private async Task<LabPathologyReport?> FindReportAsync(
            Guid labOrderId,
            bool tracked,
            CancellationToken cancellationToken)
        {
            var source = tracked
                ? _dbContext.LabPathologyReports
                : _dbContext.LabPathologyReports.AsNoTracking();

            return await source.FirstOrDefaultAsync(
                x => x.LabOrderId == labOrderId && !x.IsDelete, cancellationToken);
        }

        /// <summary>
        /// <c>AC-140</c>. Menolak ruas yang diturunkan server bila pemanggil tetap mengirimnya.
        ///
        /// Menolak terbuka lebih jujur daripada mengabaikan diam-diam: pemanggil yang mengira
        /// waktu terbitnya tersimpan padahal tidak adalah keadaan yang justru berbahaya, sebab ia
        /// baru ketahuan ketika seseorang membaca laporan yang mengaku terbit pada waktu yang
        /// tidak pernah terjadi.
        /// </summary>
        private static void RejectServerDerivedFields(
            Dictionary<string, System.Text.Json.JsonElement>? extraFields)
        {
            if (extraFields == null || extraFields.Count == 0)
                return;

            var offending = extraFields.Keys
                .Where(k => ServerDerivedFields.Contains(k, StringComparer.OrdinalIgnoreCase))
                .OrderBy(k => k)
                .ToList();

            if (offending.Count == 0)
                return;

            throw new LabPathologyReportValidationException(
                $"Ruas berikut ditentukan sistem dan tidak dapat dikirim: {string.Join(", ", offending)}.");
        }

        private async Task<string?> ResolveLastReopenReasonAsync(
            Guid labOrderId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.LabTransitionHistories
                .AsNoTracking()
                .Where(x => x.LabOrderId == labOrderId && x.Action == "PathologyReport.Reopen")
                .OrderByDescending(x => x.OccurredAt)
                .Select(x => x.ReasonNote)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Mencatat jejak audit pada <see cref="LabTransitionHistory"/>.
        ///
        /// <b><c>ToStatus</c> di sini menyatakan KEJADIANNYA, bukan status laporan.</b> Laporan
        /// Patologi Anatomi nol punya status lifecycle (<c>INV-36</c>), dan nilai pada kolom ini
        /// tidak boleh dibaca sebagai satu. Ia ada karena jejak audit modul ini memang berbentuk
        /// perpindahan, dan laporan menumpang padanya.
        /// </summary>
        private async Task RecordAuditAsync(
            LabOrder order,
            string action,
            string toStatus,
            string? reasonNote,
            DateTime occurredAt,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            _dbContext.LabTransitionHistories.Add(new LabTransitionHistory
            {
                Id = Guid.NewGuid(),
                LabOrderId = order.Id,
                EncounterId = order.EncounterId,
                Scope = LabTransitionScope.LabOrder,
                Action = action,
                ToStatus = toStatus,
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                CorrelationId = order.Id,
                CreateDateTime = occurredAt,
                CreateBy = actorUserId
            });

            await Task.CompletedTask;
        }

        private static LabPathologyOrderContextResponse MapContext(
            Guid labOrderId,
            LabPathologyOrderContext? context) =>
            new()
            {
                LabOrderId = labOrderId,
                Id = context?.Id,
                InitialDiagnosis = context?.InitialDiagnosis,
                RelevantHistory = context?.RelevantHistory,
                LastMenstrualPeriod = context?.LastMenstrualPeriod,
                ClinicalNote = context?.ClinicalNote
            };

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private sealed record PathologyForm(
            List<LabPathologyReportFieldResponse> Fields,
            Dictionary<Guid, LabPathologyParameter> ParametersById);
    }

    /// <summary>Pelanggaran aturan isi laporan Patologi Anatomi. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabPathologyReportValidationException(string message) : Exception(message);

    /// <summary>Bentrokan keadaan laporan. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabPathologyReportConflictException(string message) : Exception(message);
}
