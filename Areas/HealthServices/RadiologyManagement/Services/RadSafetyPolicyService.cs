using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Siklus pengesahan aturan keselamatan radiologi — <c>RAD-DEC-005</c>.
    ///
    /// <b>Satu aturan menjadi alasan keberadaan seluruh berkas ini: yang menyusun tidak boleh
    /// mengesahkan.</b> Aturan keselamatan menentukan pertanyaan apa yang wajib dijawab sebelum
    /// seorang pasien disinari. Membiarkan satu orang menyusun sekaligus mengesahkannya sama
    /// saja dengan tidak punya pengesahan sama sekali.
    ///
    /// Pengaman itu wajib berupa kode di sini, bukan sekadar konfigurasi hak akses.
    /// <c>AccessPermissionService.HasAccessAsync</c> menjawab "boleh mengesahkan atau tidak"
    /// untuk sebuah aksi, dan tidak pernah membandingkan siapa pelaku sebelumnya pada baris
    /// yang sama. Seseorang yang memegang <c>RadSafetyRule : Approve</c> akan lolos pemeriksaan
    /// izin walaupun dialah yang menyusun aturan itu. Pembagian tugasnya:
    ///
    /// <list type="bullet">
    /// <item><b>Endpoint</b> memastikan pelakunya penanggung jawab klinis, lewat penanda
    /// <c>[AccessPermission("RadSafetyRule", "Approve")]</c> sesuai <c>RAD-DEC-015</c>.</item>
    /// <item><b>Service ini</b> memastikan penanggung jawab klinis itu bukan orang yang
    /// menyusun atau mengajukan aturannya.</item>
    /// </list>
    ///
    /// Empat keadaan aturan dibedakan, dan hanya satu yang mengikat pasien:
    ///
    /// <list type="number">
    /// <item><c>Draft</c> — sedang disusun. Tidak menahan dan tidak meloloskan apa pun.</item>
    /// <item><c>PendingApproval</c> — sudah diajukan, belum diputuskan. Masih belum berlaku.</item>
    /// <item><c>Active</c> — berlaku. Inilah satu-satunya keadaan yang dinilai gerbang.</item>
    /// <item><c>Inactive</c> — pernah berlaku, sudah dihentikan.</item>
    /// </list>
    ///
    /// Kolom <c>IsActive</c> tetap diisi mengikuti <c>RuleStatus</c> supaya kedua kolom itu
    /// tidak pernah berselisih pada baris yang dibuat lewat jalur ini. Yang menentukan
    /// tetap <c>RuleStatus</c>; <c>IsActive</c> hanya ikut, dan akan dihapus pada pekerjaan
    /// tersendiri.
    /// </summary>
    public class RadSafetyPolicyService
    {
        private const string LogCategory = "HealthServices.RadiologyManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public RadSafetyPolicyService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /* ================================================================ *
         * Pembacaan
         * ================================================================ */

        /// <summary>
        /// Pilihan penyaring, pengurutan, dan aksi untuk layar aturan keselamatan.
        ///
        /// Tidak menyentuh database. Daftar aksinya diturunkan langsung dari
        /// <c>RAD-STATE-001</c> bagian 5, sehingga layar tidak perlu menuliskan ulang aturan
        /// perpindahan status di sisi klien.
        /// </summary>
        public RadSafetyRuleFilterMetadataResponse GetFilterMetadata() => new()
        {
            RuleStatuses = Enum.GetValues<RadSafetyRuleStatus>()
                .Select(x => new RadEnumOptionResponse
                {
                    Value = (int)x,
                    Name = x.ToString(),
                    Label = LabelStatus(x),
                })
                .ToList(),

            SortOptions =
            [
                new() { Value = "createDateTime", Label = "Tanggal aturan disusun" },
                new() { Value = "effectiveFrom", Label = "Tanggal mulai berlaku" },
                new() { Value = "ruleStatus", Label = "Keadaan pengesahan" },
                new() { Value = "ruleVersion", Label = "Nomor versi" },
            ],

            SortDirections = ["asc", "desc"],
            PageSizeOptions = [10, 25, 50, 100],

            QueryParameters =
            [
                new()
                {
                    Name = "search",
                    Type = "string",
                    Required = "No",
                    Description = "Dicari pada kode dan nama alat, serta kode dan nama butir keselamatan.",
                    Example = "CT",
                },
                new()
                {
                    Name = "modalityId",
                    Type = "guid",
                    Required = "No",
                    Description = "Menyaring aturan milik satu alat pencitraan.",
                },
                new()
                {
                    Name = "safetyRequirementId",
                    Type = "guid",
                    Required = "No",
                    Description = "Menyaring aturan yang memakai satu butir keselamatan.",
                },
                new()
                {
                    Name = "ruleStatus",
                    Type = "enum",
                    Required = "No",
                    Description = "Keadaan pengesahan. Nilainya diambil dari RuleStatuses.",
                    Example = "3",
                },
                new()
                {
                    Name = "isMandatory",
                    Type = "bool",
                    Required = "No",
                    Description = "Menyaring butir yang wajib dijawab saja, atau yang tidak wajib saja.",
                },
                new()
                {
                    Name = "sortBy",
                    Type = "string",
                    Required = "No",
                    Description = "Kolom pengurutan. Nilainya diambil dari SortOptions.",
                    Example = "createDateTime",
                },
                new()
                {
                    Name = "sortDirection",
                    Type = "string",
                    Required = "No",
                    Description = "Arah pengurutan, asc atau desc. Bawaannya desc.",
                    Example = "desc",
                },
                new()
                {
                    Name = "pageNumber",
                    Type = "int",
                    Required = "No",
                    Description = "Halaman yang diminta. Bawaannya 1.",
                    Example = "1",
                },
                new()
                {
                    Name = "pageSize",
                    Type = "int",
                    Required = "No",
                    Description = "Jumlah baris per halaman. Bawaannya 25, paling banyak 100.",
                    Example = "25",
                },
            ],

            Actions =
            [
                new()
                {
                    Action = "submit",
                    Label = "Ajukan pengesahan",
                    FromStatus = nameof(RadSafetyRuleStatus.Draft),
                    ToStatus = nameof(RadSafetyRuleStatus.PendingApproval),
                },
                new()
                {
                    Action = "approve",
                    Label = "Sahkan",
                    FromStatus = nameof(RadSafetyRuleStatus.PendingApproval),
                    ToStatus = nameof(RadSafetyRuleStatus.Active),
                },
                new()
                {
                    Action = "reject",
                    Label = "Tolak",
                    FromStatus = nameof(RadSafetyRuleStatus.PendingApproval),
                    ToStatus = nameof(RadSafetyRuleStatus.Draft),
                },
                new()
                {
                    Action = "deactivate",
                    Label = "Hentikan",
                    FromStatus = nameof(RadSafetyRuleStatus.Active),
                    ToStatus = nameof(RadSafetyRuleStatus.Inactive),
                },
            ],
        };

        /// <summary>
        /// Rekap aturan keselamatan menurut keadaan pengesahannya, ditambah jumlah alat yang
        /// belum tercakup satu pun aturan berlaku.
        /// </summary>
        public async Task<RadSafetyRuleSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var rekap = await _dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Draf = g.Count(x => x.RuleStatus == RadSafetyRuleStatus.Draft),
                    Menunggu = g.Count(x => x.RuleStatus == RadSafetyRuleStatus.PendingApproval),
                    Berlaku = g.Count(x => x.RuleStatus == RadSafetyRuleStatus.Active),
                    Dihentikan = g.Count(x => x.RuleStatus == RadSafetyRuleStatus.Inactive),
                    BerlakuWajib = g.Count(x =>
                        x.RuleStatus == RadSafetyRuleStatus.Active && x.IsMandatory),
                })
                .FirstOrDefaultAsync(cancellationToken);

            var belumTercakup = await AlatBelumTercakupQuery(now).CountAsync(cancellationToken);

            return new RadSafetyRuleSummaryResponse
            {
                TotalAturan = rekap?.Total ?? 0,
                Draf = rekap?.Draf ?? 0,
                MenungguPengesahan = rekap?.Menunggu ?? 0,
                Berlaku = rekap?.Berlaku ?? 0,
                Dihentikan = rekap?.Dihentikan ?? 0,
                BerlakuDanWajib = rekap?.BerlakuWajib ?? 0,
                AlatBelumTercakup = belumTercakup,
            };
        }

        /// <summary>
        /// Daftar aturan keselamatan dengan penyaringan, pencarian, pengurutan, dan halaman.
        /// </summary>
        public async Task<PagedResult<RadSafetyRuleResponse>> GetPagedAsync(
            RadSafetyRulePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var (pageNumber, pageSize) = NormalkanHalaman(query.PageNumber, query.PageSize);

            var sumber = _dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .Include(x => x.Modality)
                .Include(x => x.SafetyRequirement)
                .Where(x => !x.IsDelete);

            if (query.ModalityId.HasValue && query.ModalityId.Value != Guid.Empty)
            {
                sumber = sumber.Where(x => x.ModalityId == query.ModalityId.Value);
            }

            if (query.SafetyRequirementId.HasValue &&
                query.SafetyRequirementId.Value != Guid.Empty)
            {
                sumber = sumber.Where(
                    x => x.SafetyRequirementId == query.SafetyRequirementId.Value);
            }

            if (query.RuleStatus.HasValue)
            {
                sumber = sumber.Where(x => x.RuleStatus == query.RuleStatus.Value);
            }

            if (query.IsMandatory.HasValue)
            {
                sumber = sumber.Where(x => x.IsMandatory == query.IsMandatory.Value);
            }

            var pencarian = query.Search?.Trim();

            if (!string.IsNullOrWhiteSpace(pencarian))
            {
                sumber = sumber.Where(x =>
                    (x.Modality != null &&
                     (x.Modality.ModalityCode.Contains(pencarian) ||
                      x.Modality.ModalityName.Contains(pencarian))) ||
                    (x.SafetyRequirement != null &&
                     (x.SafetyRequirement.RequirementCode.Contains(pencarian) ||
                      x.SafetyRequirement.RequirementName.Contains(pencarian))));
            }

            var totalData = await sumber.CountAsync(cancellationToken);

            var baris = await Urutkan(sumber, query.SortBy, query.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<RadSafetyRuleResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = baris.Select(Map).ToList(),
            };
        }

        /// <summary>
        /// Rincian satu aturan keselamatan, untuk halaman detail dan pengisian form ubah.
        ///
        /// Aturan yang sudah ditandai terhapus diperlakukan sebagai tidak ada, bukan
        /// dikembalikan dengan penanda — layar tidak perlu tahu bedanya.
        /// </summary>
        public async Task<RadOperationResult<RadSafetyRuleResponse>> GetByIdAsync(
            Guid ruleId,
            CancellationToken cancellationToken = default)
        {
            var rule = await _dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .Include(x => x.Modality)
                .Include(x => x.SafetyRequirement)
                .FirstOrDefaultAsync(x => x.Id == ruleId && !x.IsDelete, cancellationToken);

            return rule == null
                ? NotFound()
                : RadOperationResult<RadSafetyRuleResponse>.Success(Map(rule));
        }

        /// <summary>
        /// Alat pencitraan yang <b>belum</b> punya satu pun aturan keselamatan berlaku.
        ///
        /// Daftar ini sengaja hanya memuat alat yang bermasalah. Gerbang bersifat fail-closed,
        /// sehingga setiap baris yang muncul di sini adalah alat yang akan menolak seluruh
        /// pemeriksaannya hari ini. <b>Daftar kosong berarti modul siap dipakai</b> — itulah
        /// bentuk jawaban yang diminta <c>BE-RAD-15</c>.
        /// </summary>
        public async Task<List<RadModalityCoverageResponse>> GetCoverageAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await AlatBelumTercakupQuery(now)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ModalityCode)
                .Select(x => new RadModalityCoverageResponse
                {
                    ModalityId = x.Id,
                    ModalityCode = x.ModalityCode,
                    ModalityName = x.ModalityName,
                    UsesIonisingRadiation = x.UsesIonisingRadiation,
                    DraftOrPendingRuleCount = x.SafetyRules.Count(r =>
                        !r.IsDelete &&
                        (r.RuleStatus == RadSafetyRuleStatus.Draft ||
                         r.RuleStatus == RadSafetyRuleStatus.PendingApproval)),
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Alat aktif yang tidak punya satu pun aturan berlaku pada saat <paramref name="now"/>.
        ///
        /// Penyaringnya sama persis dengan yang dipakai gerbang keselamatan. Kalau keduanya
        /// berbeda, layar akan menyatakan sebuah alat sudah siap sementara gerbangnya tetap
        /// menolak.
        /// </summary>
        private IQueryable<MstRadModality> AlatBelumTercakupQuery(DateTime now) =>
            _dbContext.MstRadModalities
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    !x.SafetyRules.Any(r =>
                        !r.IsDelete &&
                        r.RuleStatus == RadSafetyRuleStatus.Active &&
                        r.EffectiveFrom <= now &&
                        (r.EffectiveTo == null || r.EffectiveTo > now)));

        private static IOrderedQueryable<MstRadModalitySafetyRule> Urutkan(
            IQueryable<MstRadModalitySafetyRule> query,
            string? sortBy,
            string? sortDirection)
        {
            var menaik = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "effectivefrom" => menaik
                    ? query.OrderBy(x => x.EffectiveFrom)
                    : query.OrderByDescending(x => x.EffectiveFrom),

                "rulestatus" => menaik
                    ? query.OrderBy(x => x.RuleStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderByDescending(x => x.RuleStatus)
                        .ThenByDescending(x => x.CreateDateTime),

                "ruleversion" => menaik
                    ? query.OrderBy(x => x.RuleVersion)
                    : query.OrderByDescending(x => x.RuleVersion),

                _ => menaik
                    ? query.OrderBy(x => x.CreateDateTime)
                    : query.OrderByDescending(x => x.CreateDateTime),
            };
        }

        private static (int PageNumber, int PageSize) NormalkanHalaman(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static string LabelStatus(RadSafetyRuleStatus status) => status switch
        {
            RadSafetyRuleStatus.Draft => "Draf — belum berlaku",
            RadSafetyRuleStatus.PendingApproval => "Menunggu pengesahan",
            RadSafetyRuleStatus.Active => "Berlaku — dinilai gerbang",
            _ => "Sudah dihentikan",
        };

        /* ================================================================ *
         * Menyusun dan mengubah draf
         * ================================================================ */

        /// <summary>
        /// Menyusun draf aturan keselamatan.
        ///
        /// Draf sengaja tidak diperiksa tabrakannya dengan aturan yang sedang berlaku. Admin
        /// memang perlu dapat menyiapkan pengganti sebuah aturan sebelum aturan lamanya
        /// dihentikan; yang dijaga adalah saat pengesahan, bukan saat penyusunan.
        /// </summary>
        public async Task<RadOperationResult<RadSafetyRuleResponse>> CreateDraftAsync(
            CreateRadSafetyRuleRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var masterData = await ValidateMasterDataAsync(
                request.ModalityId,
                request.ProcedureId,
                request.SafetyRequirementId,
                cancellationToken);

            if (masterData != null)
            {
                return masterData;
            }

            var periode = ValidateEffectivePeriod(request.EffectiveFrom, request.EffectiveTo);

            if (periode != null)
            {
                return periode;
            }

            var rule = new MstRadModalitySafetyRule
            {
                ModalityId = request.ModalityId,
                ProcedureId = request.ProcedureId,
                SafetyRequirementId = request.SafetyRequirementId,
                IsMandatory = request.IsMandatory,
                EffectiveFrom = request.EffectiveFrom ?? now,
                EffectiveTo = request.EffectiveTo,
                Note = request.Note,
                RuleVersion = 1,
                RuleStatus = RadSafetyRuleStatus.Draft,
                IsActive = false,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            _dbContext.MstRadModalitySafetyRules.Add(rule);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "SafetyRule.CreateDraft",
                "Draf aturan keselamatan radiologi disusun.",
                new { RuleId = rule.Id, rule.ModalityId, rule.SafetyRequirementId });

            return RadOperationResult<RadSafetyRuleResponse>.Success(
                await MapAsync(rule.Id, cancellationToken));
        }

        /// <summary>
        /// Mengubah draf aturan keselamatan.
        ///
        /// Aturan yang sedang berlaku <b>tidak</b> dapat diubah di sini. Kalau isinya boleh
        /// berubah tanpa menaikkan versi, study yang sudah dinyatakan lolos akan terbaca memakai
        /// aturan yang sebenarnya belum ada saat itu — dan jejak keselamatan yang menunjuk pada
        /// aturan yang salah lebih buruk daripada tidak punya jejak.
        /// </summary>
        public async Task<RadOperationResult<RadSafetyRuleResponse>> UpdateDraftAsync(
            Guid ruleId,
            UpdateRadSafetyRuleRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var rule = await LoadRuleAsync(ruleId, cancellationToken);

            if (rule == null)
            {
                return NotFound();
            }

            if (rule.RuleStatus != RadSafetyRuleStatus.Draft)
            {
                return NotEditable(rule.RuleStatus);
            }

            var masterData = await ValidateMasterDataAsync(
                request.ModalityId,
                request.ProcedureId,
                request.SafetyRequirementId,
                cancellationToken);

            if (masterData != null)
            {
                return masterData;
            }

            var periode = ValidateEffectivePeriod(request.EffectiveFrom, request.EffectiveTo);

            if (periode != null)
            {
                return periode;
            }

            rule.ModalityId = request.ModalityId;
            rule.ProcedureId = request.ProcedureId;
            rule.SafetyRequirementId = request.SafetyRequirementId;
            rule.IsMandatory = request.IsMandatory;
            rule.EffectiveFrom = request.EffectiveFrom ?? rule.EffectiveFrom;
            rule.EffectiveTo = request.EffectiveTo;
            rule.Note = request.Note;
            Touch(rule, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return RadOperationResult<RadSafetyRuleResponse>.Success(
                await MapAsync(rule.Id, cancellationToken));
        }

        /* ================================================================ *
         * Pengajuan dan keputusan
         * ================================================================ */

        /// <summary>
        /// Mengajukan draf untuk disahkan.
        ///
        /// Keaktifan alat dan butir keselamatan diperiksa ulang di sini, bukan hanya saat draf
        /// disusun. Jarak waktu antara menyusun dan mengajukan bisa panjang, dan alat yang
        /// sudah dipensiunkan tidak boleh ikut naik ke meja pengesahan.
        /// </summary>
        public async Task<RadOperationResult<RadSafetyRuleResponse>> SubmitAsync(
            Guid ruleId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var rule = await LoadRuleAsync(ruleId, cancellationToken);

            if (rule == null)
            {
                return NotFound();
            }

            if (rule.RuleStatus != RadSafetyRuleStatus.Draft)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Conflict(
                    RadErrorCodes.InvalidTransition,
                    $"Hanya draf yang dapat diajukan untuk disahkan; aturan ini berstatus " +
                    $"{Sebutan(rule.RuleStatus)}.");
            }

            var masterData = await ValidateMasterDataAsync(
                rule.ModalityId,
                rule.ProcedureId,
                rule.SafetyRequirementId,
                cancellationToken);

            if (masterData != null)
            {
                return masterData;
            }

            rule.RuleStatus = RadSafetyRuleStatus.PendingApproval;
            rule.SubmittedByUserId = actorUserId;
            rule.SubmittedAt = now;
            Touch(rule, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "SafetyRule.Submit",
                "Aturan keselamatan radiologi diajukan untuk disahkan.",
                new { RuleId = rule.Id, rule.ModalityId, rule.SafetyRequirementId });

            return RadOperationResult<RadSafetyRuleResponse>.Success(
                await MapAsync(rule.Id, cancellationToken));
        }

        /// <summary>
        /// Mengesahkan aturan. Sejak saat ini aturan tersebut ikut menahan pemeriksaan.
        ///
        /// Tiga penjaga berjalan berurutan, dan urutannya disengaja:
        ///
        /// <list type="number">
        /// <item>Keadaan aturan — hanya pengajuan yang dapat disahkan.</item>
        /// <item><b>Bukan pengesahan sendiri</b> — inti <c>RAD-DEC-005</c>.</item>
        /// <item>Tidak ada aturan berlaku lain untuk kombinasi yang sama.</item>
        /// </list>
        ///
        /// Nomor versi naik <b>tepat satu</b> pada langkah ini, dan tidak di tempat lain mana
        /// pun. Study yang sudah lolos membekukan nomor itu, sehingga penilaian lama tetap
        /// dapat ditelusuri ke aturan yang benar-benar berlaku saat itu.
        /// </summary>
        public async Task<RadOperationResult<RadSafetyRuleResponse>> ApproveAsync(
            Guid ruleId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var rule = await LoadRuleAsync(ruleId, cancellationToken);

            if (rule == null)
            {
                return NotFound();
            }

            if (rule.RuleStatus != RadSafetyRuleStatus.PendingApproval)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Conflict(
                    RadErrorCodes.InvalidTransition,
                    $"Hanya aturan yang sedang diajukan yang dapat disahkan; aturan ini " +
                    $"berstatus {Sebutan(rule.RuleStatus)}.");
            }

            // AC-13. Yang menyusun dan yang mengajukan sama-sama tidak boleh mengesahkan.
            // Memeriksa keduanya, bukan hanya pengaju, menutup jalan memutar yang paling
            // mudah: menyusun draf lalu meminta orang lain sekadar menekan tombol Ajukan.
            if (actorUserId == rule.SubmittedByUserId || actorUserId == rule.CreateBy)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Forbidden(
                    RadErrorCodes.SelfApprovalNotAllowed,
                    "Aturan yang Anda susun atau ajukan sendiri harus disahkan penanggung " +
                    "jawab klinis lain.");
            }

            var bertabrakan = await _dbContext.MstRadModalitySafetyRules
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.Id != rule.Id &&
                    x.RuleStatus == RadSafetyRuleStatus.Active &&
                    x.ModalityId == rule.ModalityId &&
                    x.ProcedureId == rule.ProcedureId &&
                    x.SafetyRequirementId == rule.SafetyRequirementId,
                    cancellationToken);

            if (bertabrakan)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Conflict(
                    RadErrorCodes.ActiveSafetyRuleExists,
                    "Sudah ada aturan aktif untuk alat, pemeriksaan, dan butir keselamatan " +
                    "yang sama. Nonaktifkan aturan lama lebih dulu.");
            }

            rule.RuleStatus = RadSafetyRuleStatus.Active;
            rule.IsActive = true;
            rule.RuleVersion += 1;
            rule.ApprovedByUserId = actorUserId;
            rule.ApprovedAt = now;
            Touch(rule, actorUserId, now);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir. Dua pengesahan yang berjalan hampir bersamaan sama-sama
                // lolos pemeriksaan di atas, lalu index unik pada database menolak yang kedua.
                // Ditangkap di sini supaya penanggung jawab klinis membaca sebabnya, bukan
                // pesan galat database.
                return RadOperationResult<RadSafetyRuleResponse>.Conflict(
                    RadErrorCodes.ActiveSafetyRuleExists,
                    "Aturan lain untuk alat, pemeriksaan, dan butir keselamatan yang sama baru " +
                    "saja disahkan petugas lain. Muat ulang halaman lalu periksa kembali.");
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "SafetyRule.Approve",
                "Aturan keselamatan radiologi disahkan.",
                new
                {
                    RuleId = rule.Id,
                    rule.ModalityId,
                    rule.SafetyRequirementId,
                    rule.RuleVersion
                });

            return RadOperationResult<RadSafetyRuleResponse>.Success(
                await MapAsync(rule.Id, cancellationToken));
        }

        /// <summary>
        /// Menolak pengajuan. Aturan kembali menjadi draf, dan alasannya wajib diisi.
        ///
        /// Alasan itu bukan formalitas: tanpa alasan, penyusun aturan tidak tahu apa yang harus
        /// diperbaiki, dan pengajuan yang sama akan kembali berulang.
        /// </summary>
        public async Task<RadOperationResult<RadSafetyRuleResponse>> RejectAsync(
            Guid ruleId,
            RadSafetyRuleRejectRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var rule = await LoadRuleAsync(ruleId, cancellationToken);

            if (rule == null)
            {
                return NotFound();
            }

            if (rule.RuleStatus != RadSafetyRuleStatus.PendingApproval)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Conflict(
                    RadErrorCodes.InvalidTransition,
                    $"Hanya aturan yang sedang diajukan yang dapat ditolak; aturan ini " +
                    $"berstatus {Sebutan(rule.RuleStatus)}.");
            }

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return RadOperationResult<RadSafetyRuleResponse>.Validation(
                    RadErrorCodes.ReasonRequired,
                    "Alasan penolakan wajib diisi.");
            }

            rule.RuleStatus = RadSafetyRuleStatus.Draft;
            rule.RejectedByUserId = actorUserId;
            rule.RejectedAt = now;
            rule.RejectionReason = request.RejectionReason.Trim();
            Touch(rule, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "SafetyRule.Reject",
                "Pengajuan aturan keselamatan radiologi ditolak.",
                new { RuleId = rule.Id, rule.ModalityId, rule.SafetyRequirementId });

            return RadOperationResult<RadSafetyRuleResponse>.Success(
                await MapAsync(rule.Id, cancellationToken));
        }

        /// <summary>
        /// Menghentikan aturan yang sedang berlaku.
        ///
        /// Barisnya tidak dihapus dan nomor versinya tidak diubah. Study lama yang lolos memakai
        /// aturan ini tetap dapat ditelusuri; yang berubah hanya bahwa aturan itu tidak lagi
        /// dinilai untuk pemeriksaan berikutnya.
        /// </summary>
        public async Task<RadOperationResult<RadSafetyRuleResponse>> DeactivateAsync(
            Guid ruleId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var rule = await LoadRuleAsync(ruleId, cancellationToken);

            if (rule == null)
            {
                return NotFound();
            }

            if (rule.RuleStatus != RadSafetyRuleStatus.Active)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Conflict(
                    RadErrorCodes.InvalidTransition,
                    $"Hanya aturan yang sedang berlaku yang dapat dinonaktifkan; aturan ini " +
                    $"berstatus {Sebutan(rule.RuleStatus)}.");
            }

            rule.RuleStatus = RadSafetyRuleStatus.Inactive;
            rule.IsActive = false;
            Touch(rule, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "SafetyRule.Deactivate",
                "Aturan keselamatan radiologi dinonaktifkan.",
                new { RuleId = rule.Id, rule.ModalityId, rule.SafetyRequirementId });

            return RadOperationResult<RadSafetyRuleResponse>.Success(
                await MapAsync(rule.Id, cancellationToken));
        }

        /* ================================================================ *
         * Penjaga bersama
         * ================================================================ */

        /// <summary>
        /// Memastikan alat, pemeriksaan, dan butir keselamatan yang dirujuk memang ada dan
        /// masih dipakai. Mengembalikan <c>null</c> bila seluruhnya sah.
        /// </summary>
        private async Task<RadOperationResult<RadSafetyRuleResponse>?> ValidateMasterDataAsync(
            Guid modalityId,
            Guid? procedureId,
            Guid safetyRequirementId,
            CancellationToken cancellationToken)
        {
            if (modalityId == Guid.Empty)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Validation(
                    RadErrorCodes.ValidationFailed, "Alat pencitraan wajib dipilih.");
            }

            if (safetyRequirementId == Guid.Empty)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Validation(
                    RadErrorCodes.ValidationFailed, "Butir keselamatan wajib dipilih.");
            }

            var modality = await _dbContext.MstRadModalities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == modalityId && !x.IsDelete, cancellationToken);

            if (modality == null)
            {
                return RadOperationResult<RadSafetyRuleResponse>.NotFound(
                    RadErrorCodes.ModalityNotFound, "Alat pencitraan tidak ditemukan.");
            }

            if (!modality.IsActive)
            {
                return RadOperationResult<RadSafetyRuleResponse>.BusinessRule(
                    RadErrorCodes.MasterDataInactive,
                    "Alat pencitraan yang dipilih sudah tidak aktif.");
            }

            var requirement = await _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == safetyRequirementId && !x.IsDelete, cancellationToken);

            if (requirement == null)
            {
                return RadOperationResult<RadSafetyRuleResponse>.NotFound(
                    RadErrorCodes.SafetyRequirementNotFound,
                    "Butir keselamatan tidak ditemukan.");
            }

            if (!requirement.IsActive)
            {
                return RadOperationResult<RadSafetyRuleResponse>.BusinessRule(
                    RadErrorCodes.MasterDataInactive,
                    "Butir keselamatan yang dipilih sudah tidak aktif.");
            }

            if (procedureId.HasValue)
            {
                var adaPemeriksaan = await _dbContext.Set<MstProcedure>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == procedureId.Value && !x.IsDelete, cancellationToken);

                if (!adaPemeriksaan)
                {
                    return RadOperationResult<RadSafetyRuleResponse>.NotFound(
                        RadErrorCodes.ProcedureNotFound, "Pemeriksaan tidak ditemukan.");
                }
            }

            return null;
        }

        /// <summary>
        /// Memastikan masa berlaku aturan masuk akal. Mengembalikan <c>null</c> bila sah.
        /// </summary>
        private static RadOperationResult<RadSafetyRuleResponse>? ValidateEffectivePeriod(
            DateTime? effectiveFrom,
            DateTime? effectiveTo)
        {
            if (effectiveFrom.HasValue &&
                effectiveTo.HasValue &&
                effectiveTo.Value < effectiveFrom.Value)
            {
                return RadOperationResult<RadSafetyRuleResponse>.Validation(
                    RadErrorCodes.ValidationFailed,
                    "Tanggal akhir berlaku tidak boleh lebih awal dari tanggal mulai berlaku.");
            }

            return null;
        }

        private static RadOperationResult<RadSafetyRuleResponse> NotFound() =>
            RadOperationResult<RadSafetyRuleResponse>.NotFound(
                RadErrorCodes.SafetyRuleNotFound, "Aturan keselamatan tidak ditemukan.");

        /// <summary>
        /// Penolakan perubahan, dengan sebab yang berbeda untuk setiap keadaan.
        ///
        /// Aturan yang sedang berlaku ditolak <c>403</c> sesuai <c>RAD-VAL-001</c> bagian 2:
        /// bukan isian yang salah, melainkan memang tidak boleh. Dua keadaan lainnya adalah
        /// soal urutan kerja, sehingga ditolak <c>409</c>.
        /// </summary>
        private static RadOperationResult<RadSafetyRuleResponse> NotEditable(
            RadSafetyRuleStatus status) => status switch
            {
                RadSafetyRuleStatus.Active =>
                    RadOperationResult<RadSafetyRuleResponse>.Forbidden(
                        RadErrorCodes.SafetyRuleNotEditable,
                        "Aturan yang sedang berlaku tidak dapat diubah. Susun draf baru lalu " +
                        "ajukan pengesahan."),

                RadSafetyRuleStatus.PendingApproval =>
                    RadOperationResult<RadSafetyRuleResponse>.Conflict(
                        RadErrorCodes.SafetyRuleNotEditable,
                        "Aturan yang sedang diajukan tidak dapat diubah. Tunggu keputusan " +
                        "penanggung jawab klinis lebih dulu."),

                _ =>
                    RadOperationResult<RadSafetyRuleResponse>.Conflict(
                        RadErrorCodes.SafetyRuleNotEditable,
                        "Aturan yang sudah dihentikan tidak dapat diubah. Susun draf baru bila " +
                        "aturan itu hendak diberlakukan kembali."),
            };

        /// <summary>Sebutan keadaan aturan dalam bahasa yang dibaca pengguna.</summary>
        private static string Sebutan(RadSafetyRuleStatus status) => status switch
        {
            RadSafetyRuleStatus.Draft => "masih berupa draf",
            RadSafetyRuleStatus.PendingApproval => "sedang menunggu pengesahan",
            RadSafetyRuleStatus.Active => "sedang berlaku",
            _ => "sudah dihentikan",
        };

        private Task<MstRadModalitySafetyRule?> LoadRuleAsync(
            Guid ruleId,
            CancellationToken cancellationToken) =>
            _dbContext.MstRadModalitySafetyRules
                .FirstOrDefaultAsync(x => x.Id == ruleId && !x.IsDelete, cancellationToken);

        private static void Touch(
            MstRadModalitySafetyRule rule,
            Guid actorUserId,
            DateTime now)
        {
            rule.UpdateBy = actorUserId;
            rule.UpdateDateTime = now;
        }

        /// <summary>
        /// Membaca ulang aturan beserta nama alat dan butirnya untuk ditampilkan.
        ///
        /// Dibaca ulang, bukan dipetakan dari entity yang baru disimpan, supaya nama alat dan
        /// butir keselamatan ikut terisi tanpa memaksa setiap tindakan memuat navigasinya lebih
        /// dulu.
        /// </summary>
        private async Task<RadSafetyRuleResponse> MapAsync(
            Guid ruleId,
            CancellationToken cancellationToken)
        {
            var rule = await _dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .Include(x => x.Modality)
                .Include(x => x.SafetyRequirement)
                .FirstAsync(x => x.Id == ruleId, cancellationToken);

            return Map(rule);
        }

        /// <summary>
        /// Memetakan satu baris aturan menjadi bentuk yang dikirim ke layar.
        ///
        /// Nama alat dan butir hanya terisi bila navigasinya ikut dimuat pemanggil.
        /// </summary>
        private static RadSafetyRuleResponse Map(MstRadModalitySafetyRule rule)
        {
            return new RadSafetyRuleResponse
            {
                Id = rule.Id,
                ModalityId = rule.ModalityId,
                ModalityCode = rule.Modality?.ModalityCode,
                ModalityName = rule.Modality?.ModalityName,
                ProcedureId = rule.ProcedureId,
                SafetyRequirementId = rule.SafetyRequirementId,
                RequirementCode = rule.SafetyRequirement?.RequirementCode,
                RequirementName = rule.SafetyRequirement?.RequirementName,
                IsMandatory = rule.IsMandatory,
                EffectiveFrom = rule.EffectiveFrom,
                EffectiveTo = rule.EffectiveTo,
                RuleVersion = rule.RuleVersion,
                RuleStatus = rule.RuleStatus.ToString(),
                Note = rule.Note,
                SubmittedByUserId = rule.SubmittedByUserId,
                SubmittedAt = rule.SubmittedAt,
                ApprovedByUserId = rule.ApprovedByUserId,
                ApprovedAt = rule.ApprovedAt,
                RejectedByUserId = rule.RejectedByUserId,
                RejectedAt = rule.RejectedAt,
                RejectionReason = rule.RejectionReason,
            };
        }

        /// <summary>
        /// Identitas pelaku, diambil dari sesi yang sedang berjalan.
        ///
        /// Pelaku yang tidak dikenali membuat perbandingan "penyusun versus pengesah" kehilangan
        /// artinya, dan pengaman yang kehilangan artinya lebih berbahaya daripada pengaman yang
        /// tidak ada — karena ia tetap terlihat bekerja. Karena itu identitas kosong menghentikan
        /// tindakan, bukan dilanjutkan dengan nilai kosong.
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            if (!Guid.TryParse(value, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Identitas petugas tidak dapat ditentukan dari sesi yang sedang berjalan. " +
                    "Tindakan radiologi tidak dijalankan.");
            }

            return userId;
        }
    }
}
