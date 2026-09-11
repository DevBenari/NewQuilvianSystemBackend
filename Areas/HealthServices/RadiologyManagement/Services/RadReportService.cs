using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using System.Data;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Siklus hidup hasil bacaan radiologi — <c>RAD-STATE-001</c> bagian 3 dan 4,
    /// <c>RAD-DEC-003</c>, <c>RAD-DEC-015</c>.
    ///
    /// <b>Satu aturan menjadi alasan keberadaan seluruh berkas ini: yang menulis draf belum tentu
    /// boleh mengesahkannya.</b> Bacaan yang sudah disahkan dan dirilis dipakai dokter lain untuk
    /// memberi obat, menjadwalkan operasi, dan memulangkan pasien. Karena itu pengesahan bukan
    /// sekadar tombol "setuju": ia adalah pernyataan seorang dokter radiolog bahwa isi bacaan itu
    /// sah dipakai.
    ///
    /// <para>
    /// <b>Mengapa pengamannya wajib berupa kode di sini, bukan cukup atribut pada endpoint.</b>
    /// <c>[AccessPermission("RadReport", "Validate")]</c> menjawab pertanyaan "boleh mencoba
    /// mengesahkan atau tidak" untuk sebuah aksi. Ia tidak pernah membandingkan siapa yang menulis
    /// baris yang sedang disahkan. Seorang residen yang diberi <c>Validate</c> — supaya ia dapat
    /// mengesahkan draf yang ditulis radiografer — akan lolos atribut itu walaupun yang ia sahkan
    /// adalah drafnya sendiri. Pembagian tugasnya:
    /// </para>
    ///
    /// <list type="bullet">
    /// <item><b>Endpoint</b> memastikan pelakunya berhak mencoba, lewat
    /// <c>[AccessPermission("RadReport", "Validate")]</c>.</item>
    /// <item><b>Service ini</b> memastikan dua hal yang tidak dapat dijawab sistem izin: peran
    /// penulis <b>pada saat draf itu ditulis</b>, dan apakah pengesahnya orang yang sama.</item>
    /// </list>
    ///
    /// <para>
    /// <b>Peran penulis dibaca dari <c>AuthorRoleSnapshot</c>, bukan dari kewenangan hari ini.</b>
    /// dr. Rian menulis draf pada Januari sebagai residen; drafnya wajib disahkan radiolog lain.
    /// Pada Juli ia lulus menjadi Sp.Rad dan memegang <c>RadReport : ActAsRadiologist</c>. Kalau
    /// perannya dibaca ulang saat itu, draf Januari mendadak terbaca ditulis radiolog dan aturan
    /// "wajib disahkan orang lain" ikut menguap. Yang dinilai adalah keadaan saat bacaan itu
    /// disusun.
    /// </para>
    ///
    /// <para>
    /// <b>Empat kolom haram masuk application log</b> — <c>Findings</c>, <c>Impression</c>,
    /// <c>Recommendation</c>, dan <c>AmendmentReason</c> (<c>RAD-PERM-001</c> bagian 7). Keempatnya
    /// kesimpulan klinis atas seorang pasien; bocornya ke log berarti data medis tersimpan di tempat
    /// yang aturan aksesnya berbeda dari tabel aslinya. Karena itu seluruh muatan log di sini
    /// dibentuk satu pintu lewat <see cref="RadReportLogPayload"/>, yang memang tidak punya kolom
    /// untuk menampungnya.
    /// </para>
    /// </summary>
    public class RadReportService
    {
        private const string LogCategory = "HealthServices.RadiologyManagement";

        /// <summary>Panjang kolom isi sesuai <c>RAD-ERD-DICT-001</c> bagian 2.</summary>
        private const int FindingsMaxLength = 8000;
        private const int ImpressionMaxLength = 4000;
        private const int RecommendationMaxLength = 2000;
        private const int AmendmentReasonMaxLength = 1000;

        /// <summary>
        /// Keadaan bacaan yang berarti hasilnya <b>belum sampai</b> ke dokter pengirim.
        ///
        /// Keadaan koreksi sengaja tidak ikut: bacaan yang sedang dikoreksi sudah pernah dirilis,
        /// sehingga dokter pengirim tetap memegang versi yang berlaku. Memasukkannya ke sini akan
        /// membuat daftar "hasil yang ditunggu" berisi pasien yang hasilnya sebenarnya sudah ada.
        /// </summary>
        private static readonly RadReportStatus[] BelumDirilisStatuses =
        [
            RadReportStatus.Pending,
            RadReportStatus.Drafted,
            RadReportStatus.Validated,
        ];

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly LoggerService _loggerService;
        private readonly RadReportNumberService _reportNumberService;

        public RadReportService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            AccessPermissionService accessPermissionService,
            LoggerService loggerService,
            RadReportNumberService reportNumberService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _accessPermissionService = accessPermissionService;
            _loggerService = loggerService;
            _reportNumberService = reportNumberService;
        }

        /* ================================================================ *
         * Kewenangan
         * ================================================================ */

        /// <summary>
        /// Apakah pelaku saat ini dihitung sebagai dokter radiolog.
        ///
        /// <para>
        /// Dijawab hak akses penanda <c>RadReport : ActAsRadiologist</c>, bukan nama peran dan
        /// bukan tabel pemetaan tersendiri (<c>RAD-DEC-015</c>). Penanda ini sengaja <b>tidak
        /// menempel pada satu endpoint pun</b>: ia menjawab "dihitung sebagai radiolog", sedangkan
        /// <c>RadReport : Validate</c> menjawab "boleh mencoba mengesahkan". Menggabungkan keduanya
        /// menghapus perbedaan itu, dan aturan <c>RAD-DEC-003</c> ikut hilang.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <c>protected virtual</c> semata-mata agar uji dapat menggantinya tanpa menyusun seluruh
        /// struktur RBAC; produksi selalu memakai <c>AccessPermissionService</c> yang sebenarnya.
        /// Pola seam yang sama dipakai <c>EncounterIntakeService</c>.
        /// </remarks>
        protected virtual async Task<bool> HasRadiologistAuthorityAsync(
            CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            return await _accessPermissionService.HasAccessAsync(
                user,
                "RadReport",
                "ActAsRadiologist");
        }

        /* ================================================================ *
         * Pembacaan
         * ================================================================ */

        /// <summary>
        /// Pilihan penyaring, pengurutan, dan aksi untuk layar daftar bacaan.
        ///
        /// Tidak menyentuh database. Daftar aksinya diturunkan langsung dari
        /// <c>RAD-STATE-001</c> bagian 3, sehingga layar tidak perlu menuliskan ulang aturan
        /// perpindahan status di sisi klien lalu berselisih dengan backend ketika aturannya
        /// berubah.
        /// </summary>
        public RadReportFilterMetadataResponse GetFilterMetadata() => new()
        {
            ReportStatuses = Enum.GetValues<RadReportStatus>()
                .Select(x => new RadEnumOptionResponse
                {
                    Value = (int)x,
                    Name = x.ToString(),
                    Label = LabelStatus(x),
                })
                .ToList(),

            VersionStatuses = Enum.GetValues<RadReportVersionStatus>()
                .Select(x => new RadEnumOptionResponse
                {
                    Value = (int)x,
                    Name = x.ToString(),
                    Label = LabelVersi(x),
                })
                .ToList(),

            AuthorRoles = Enum.GetValues<RadReportAuthorRole>()
                .Select(x => new RadEnumOptionResponse
                {
                    Value = (int)x,
                    Name = x.ToString(),
                    Label = LabelPeran(x),
                })
                .ToList(),

            SortOptions =
            [
                new() { Value = "createDateTime", Label = "Tanggal bacaan dibuka" },
                new() { Value = "reportNumber", Label = "Nomor bacaan" },
                new() { Value = "reportStatus", Label = "Keadaan bacaan" },
                new() { Value = "firstReleasedAt", Label = "Tanggal pertama dirilis" },
                new() { Value = "lastReleasedAt", Label = "Tanggal terakhir dirilis" },
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
                    Description = "Dicari pada nomor bacaan dan nomor pemeriksaan.",
                    Example = "RAD-RPT",
                },
                new()
                {
                    Name = "radStudyId",
                    Type = "guid",
                    Required = "No",
                    Description = "Menyaring bacaan atas satu pemeriksaan.",
                },
                new()
                {
                    Name = "radOrderId",
                    Type = "guid",
                    Required = "No",
                    Description = "Menyaring bacaan milik satu pesanan.",
                },
                new()
                {
                    Name = "encounterId",
                    Type = "guid",
                    Required = "No",
                    Description = "Menyaring bacaan milik satu kunjungan.",
                },
                new()
                {
                    Name = "reportStatus",
                    Type = "enum",
                    Required = "No",
                    Description = "Keadaan bacaan. Nilainya diambil dari ReportStatuses.",
                    Example = "2",
                },
                new()
                {
                    Name = "belumDirilis",
                    Type = "bool",
                    Required = "No",
                    Description =
                        "Bernilai true menyaring bacaan yang belum sampai ke dokter pengirim — " +
                        "menunggu draf, masih draf, atau sudah disahkan tetapi belum dirilis.",
                    Example = "true",
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
                    Action = "CreateDraft",
                    Label = "Tulis draf bacaan",
                    FromStatus = nameof(RadReportStatus.Pending),
                    ToStatus = nameof(RadReportStatus.Drafted),
                },
                new()
                {
                    Action = "UpdateDraft",
                    Label = "Ubah draf",
                    FromStatus = nameof(RadReportStatus.Drafted),
                    ToStatus = nameof(RadReportStatus.Drafted),
                },
                new()
                {
                    Action = "Validate",
                    Label = "Sahkan",
                    FromStatus = nameof(RadReportStatus.Drafted),
                    ToStatus = nameof(RadReportStatus.Validated),
                },
                new()
                {
                    Action = "Release",
                    Label = "Rilis ke dokter pengirim",
                    FromStatus = nameof(RadReportStatus.Validated),
                    ToStatus = nameof(RadReportStatus.Released),
                },
                new()
                {
                    Action = "Amend",
                    Label = "Tulis koreksi",
                    FromStatus = nameof(RadReportStatus.Released),
                    ToStatus = nameof(RadReportStatus.AmendmentDrafted),
                },
            ],
        };

        /// <summary>
        /// Rekap jumlah bacaan per keadaan.
        ///
        /// <c>BelumDirilis</c> adalah angka yang benar-benar dipakai kepala unit: ia menghitung
        /// pasien yang hasilnya masih ditunggu, berapa pun keadaan teknis bacaannya.
        /// </summary>
        public async Task<RadReportSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var perStatus = await _dbContext.RadReports
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .GroupBy(x => x.ReportStatus)
                .Select(g => new { Status = g.Key, Jumlah = g.Count() })
                .ToListAsync(cancellationToken);

            int Hitung(RadReportStatus status) =>
                perStatus.FirstOrDefault(x => x.Status == status)?.Jumlah ?? 0;

            var menungguDraf = Hitung(RadReportStatus.Pending);
            var draf = Hitung(RadReportStatus.Drafted);
            var disahkan = Hitung(RadReportStatus.Validated);

            return new RadReportSummaryResponse
            {
                TotalBacaan = perStatus.Sum(x => x.Jumlah),
                MenungguDraf = menungguDraf,
                Draf = draf,
                SudahDisahkan = disahkan,
                SudahDirilis = Hitung(RadReportStatus.Released),
                KoreksiDraf = Hitung(RadReportStatus.AmendmentDrafted),
                KoreksiDisahkan = Hitung(RadReportStatus.AmendmentValidated),
                KoreksiDirilis = Hitung(RadReportStatus.AmendmentReleased),
                BelumDirilis = menungguDraf + draf + disahkan,
            };
        }

        /// <summary>
        /// Daftar bacaan dengan penyaringan, pengurutan, dan halaman.
        /// </summary>
        /// <remarks>
        /// Versi yang sedang berlaku diambil dalam satu query tersendiri, bukan per baris.
        /// Daftar bacaan adalah layar yang paling sering dibuka petugas, dan satu query tambahan
        /// per baris akan terasa begitu satu unit punya ribuan bacaan.
        /// </remarks>
        public async Task<PagedResult<RadReportListResponse>> GetPagedAsync(
            RadReportPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var (pageNumber, pageSize) = NormalkanHalaman(query.PageNumber, query.PageSize);

            var sumber = _dbContext.RadReports
                .AsNoTracking()
                .Include(x => x.RadStudy)
                .Where(x => !x.IsDelete);

            if (query.RadStudyId.HasValue && query.RadStudyId.Value != Guid.Empty)
            {
                sumber = sumber.Where(x => x.RadStudyId == query.RadStudyId.Value);
            }

            if (query.RadOrderId.HasValue && query.RadOrderId.Value != Guid.Empty)
            {
                sumber = sumber.Where(x => x.RadOrderId == query.RadOrderId.Value);
            }

            if (query.EncounterId.HasValue && query.EncounterId.Value != Guid.Empty)
            {
                sumber = sumber.Where(x => x.EncounterId == query.EncounterId.Value);
            }

            if (query.ReportStatus.HasValue)
            {
                sumber = sumber.Where(x => x.ReportStatus == query.ReportStatus.Value);
            }

            if (query.BelumDirilis.HasValue)
            {
                sumber = query.BelumDirilis.Value
                    ? sumber.Where(x => BelumDirilisStatuses.Contains(x.ReportStatus))
                    : sumber.Where(x => !BelumDirilisStatuses.Contains(x.ReportStatus));
            }

            var pencarian = query.Search?.Trim();

            if (!string.IsNullOrWhiteSpace(pencarian))
            {
                sumber = sumber.Where(x =>
                    x.ReportNumber.Contains(pencarian) ||
                    (x.RadStudy != null && x.RadStudy.StudyNumber.Contains(pencarian)));
            }

            var totalData = await sumber.CountAsync(cancellationToken);

            var baris = await Urutkan(sumber, query.SortBy, query.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var versiBerlaku = await LoadCurrentVersionsAsync(baris, cancellationToken);

            return new PagedResult<RadReportListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = baris
                    .Select(x => MapList(
                        x,
                        versiBerlaku.GetValueOrDefault(x.Id)))
                    .ToList(),
            };
        }

        /// <summary>
        /// Rincian satu bacaan beserta versi berlaku dan seluruh riwayat versinya.
        ///
        /// Bacaan yang sudah ditandai terhapus diperlakukan sebagai tidak ada.
        /// </summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> GetByIdAsync(
            Guid reportId,
            CancellationToken cancellationToken = default)
        {
            var ada = await _dbContext.RadReports
                .AsNoTracking()
                .AnyAsync(x => x.Id == reportId && !x.IsDelete, cancellationToken);

            if (!ada)
            {
                return RadOperationResult<RadReportDetailResponse>.NotFound(
                    RadErrorCodes.ReportNotFound,
                    "Bacaan yang dimaksud tidak ditemukan.");
            }

            return RadOperationResult<RadReportDetailResponse>.Success(
                await MapAsync(reportId, cancellationToken));
        }

        /// <summary>
        /// Bacaan atas satu pemeriksaan. Satu study paling banyak punya satu bacaan.
        ///
        /// <b>Tidak ditemukan bukan berarti pemeriksaannya tidak ada</b> — bisa jadi belum ada
        /// yang menuliskan bacaannya. Pesannya dibedakan supaya petugas tidak mencari kesalahan
        /// pada nomor pemeriksaan yang sebenarnya benar.
        /// </summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> GetByStudyAsync(
            Guid radStudyId,
            CancellationToken cancellationToken = default)
        {
            var reportId = await _dbContext.RadReports
                .AsNoTracking()
                .Where(x => x.RadStudyId == radStudyId && !x.IsDelete)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (reportId == null)
            {
                return RadOperationResult<RadReportDetailResponse>.NotFound(
                    RadErrorCodes.ReportNotFound,
                    "Pemeriksaan ini belum memiliki bacaan.");
            }

            return RadOperationResult<RadReportDetailResponse>.Success(
                await MapAsync(reportId.Value, cancellationToken));
        }

        /// <summary>
        /// Bacaan pada satu kunjungan yang <b>sudah pernah dirilis</b>, terbaru lebih dulu.
        /// Dipakai rekam medis dan layar dokter pengirim — <c>RAD-DEC-006</c>,
        /// <c>RAD-INT-001</c> bagian 2.
        ///
        /// <para>
        /// <b>Bacaan yang belum pernah dirilis tidak ikut terbawa</b> (<c>FR-RAD-032</c>).
        /// Draf dan bacaan yang baru disahkan tetapi belum dirilis <b>tidak muncul sama
        /// sekali</b> — bukan muncul dengan penanda, dan bukan muncul dalam keadaan terkunci.
        /// </para>
        ///
        /// <para>
        /// <b>Mengapa disembunyikan seluruhnya, bukan ditandai.</b> Bacaan yang belum dirilis
        /// belum menjadi pernyataan siapa pun. Menampilkannya kepada dokter pengirim, sekalipun
        /// dengan label "draf", membuka satu jalan yang tidak dapat ditutup kembali: dokter yang
        /// sedang terburu-buru membaca kesimpulannya, lalu bertindak atasnya. Penanda hanya
        /// menolong orang yang membacanya.
        /// </para>
        ///
        /// <para>
        /// Petugas Radiologi yang memang perlu melihat draf satu kunjungan memakai
        /// <c>GET /?encounterId=…</c>, yang tidak disaring. Pemisahan itu disengaja: satu
        /// endpoint untuk pekerjaan radiologi, satu endpoint untuk pembaca hasil.
        /// </para>
        ///
        /// <para>
        /// Daftar kosong berarti kunjungan itu memang belum punya bacaan yang dirilis — bukan
        /// kegagalan, dan karena itu tetap dijawab <c>200</c> dengan daftar kosong, bukan
        /// <c>404</c>. Membedakan keduanya adalah tanggung jawab layar
        /// (<c>FR-RAD-031</c>).
        /// </para>
        /// </summary>
        public async Task<List<RadReportListResponse>> GetByEncounterAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var baris = await _dbContext.RadReports
                .AsNoTracking()
                .Include(x => x.RadStudy)
                .Where(x =>
                    x.EncounterId == encounterId &&
                    !x.IsDelete &&

                    // FR-RAD-032. Dua penyaring yang saling menguatkan, bukan salah satu:
                    // statusnya menunjukkan bacaan sudah melewati perilisan, dan waktunya
                    // membuktikan perilisan itu benar-benar pernah terjadi. Baris yang kedua
                    // kolomnya berselisih — misalnya karena data lama — ditahan, bukan
                    // diloloskan.
                    !BelumDirilisStatuses.Contains(x.ReportStatus) &&
                    x.FirstReleasedAt != null)
                .OrderByDescending(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var versiBerlaku = await LoadCurrentVersionsAsync(baris, cancellationToken);

            return baris
                .Select(x => MapList(x, versiBerlaku.GetValueOrDefault(x.Id)))
                .ToList();
        }

        /* ================================================================ *
         * Kelahiran bacaan
         * ================================================================ */

        /// <summary>
        /// Menyiapkan wadah bacaan berstatus <c>Pending</c> untuk satu study yang citranya sudah
        /// dinyatakan layak — baris pertama <c>RAD-STATE-001</c> bagian 3.
        ///
        /// <para>
        /// <b>Idempoten.</b> Dipanggil dua kali untuk study yang sama, panggilan kedua
        /// mengembalikan bacaan yang sudah ada, bukan galat. Ini disengaja: kelahiran bacaan
        /// mengikuti kejadian "mutu citra diterima", dan kejadian yang terkirim ulang tidak boleh
        /// melahirkan bacaan kedua. Yang menolak bacaan kedua adalah
        /// <see cref="CreateDraftAsync"/>, tempat seseorang benar-benar bermaksud menulis bacaan
        /// baru.
        /// </para>
        /// </summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> EnsurePendingReportAsync(
            Guid radStudyId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var transaction = await BeginGuardedAsync($"RAD_REPORT_STUDY_{radStudyId:N}", cancellationToken);

            try
            {
                var study = await LoadStudyAsync(radStudyId, cancellationToken);

                if (study == null)
                {
                    return RadOperationResult<RadReportDetailResponse>.NotFound(
                        RadErrorCodes.StudyNotFound,
                        "Pemeriksaan yang dimaksud tidak ditemukan.");
                }

                var kelayakan = ValidateStudyIsReadable(study);

                if (kelayakan != null)
                {
                    return kelayakan;
                }

                var existing = await FindReportByStudyAsync(radStudyId, cancellationToken);

                if (existing != null)
                {
                    if (transaction != null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }

                    return RadOperationResult<RadReportDetailResponse>.Success(
                        await MapAsync(existing.Id, cancellationToken));
                }

                var report = NewReport(study, actorUserId, now);
                _dbContext.RadReports.Add(report);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException)
                {
                    // Penjaga terakhir. Dua kejadian "mutu citra diterima" yang tiba hampir
                    // bersamaan sama-sama lolos pemeriksaan di atas, lalu index unik pada
                    // RadStudyId menolak yang kedua.
                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        RadErrorCodes.ReportAlreadyExists,
                        "Pemeriksaan ini sudah memiliki bacaan. Muat ulang halaman lalu periksa " +
                        "kembali.");
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.InfoAsync(
                    LogCategory,
                    "RadReport.EnsurePending",
                    "Wadah hasil bacaan radiologi dibuat menunggu draf.",
                    RadReportLogPayload.For(report, null, actorUserId));

                return RadOperationResult<RadReportDetailResponse>.Success(
                    await MapAsync(report.Id, cancellationToken));
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /* ================================================================ *
         * Menulis dan mengubah draf
         * ================================================================ */

        /// <summary>
        /// Menulis draf bacaan pertama atas satu study — <c>Pending</c> menjadi <c>Drafted</c>.
        ///
        /// <para>
        /// Peran penulis <b>dibekukan di sini dan tidak pernah dibaca ulang</b>. Inilah satu-satunya
        /// tempat <c>AuthorRoleSnapshot</c> ditentukan, dan itu yang membuat aturan pengesahan tetap
        /// berarti setelah kewenangan seseorang berubah di kemudian hari.
        /// </para>
        /// </summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> CreateDraftAsync(
            Guid radStudyId,
            CreateRadReportDraftRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var isi = ValidateContent(request.Findings, request.Impression, request.Recommendation);

            if (isi != null)
            {
                return isi;
            }

            var (authorRole, peranDitolak) = await ResolveAuthorRoleAsync(
                request.AuthorRole,
                cancellationToken);

            if (peranDitolak != null)
            {
                return peranDitolak;
            }

            var transaction = await BeginGuardedAsync($"RAD_REPORT_STUDY_{radStudyId:N}", cancellationToken);

            try
            {
                var study = await LoadStudyAsync(radStudyId, cancellationToken);

                if (study == null)
                {
                    return RadOperationResult<RadReportDetailResponse>.NotFound(
                        RadErrorCodes.StudyNotFound,
                        "Pemeriksaan yang dimaksud tidak ditemukan.");
                }

                var kelayakan = ValidateStudyIsReadable(study);

                if (kelayakan != null)
                {
                    return kelayakan;
                }

                var report = await FindReportByStudyAsync(radStudyId, cancellationToken);

                if (report != null && report.CurrentVersionNumber > 0)
                {
                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        RadErrorCodes.ReportAlreadyExists,
                        "Pemeriksaan ini sudah memiliki bacaan. Gunakan koreksi bila ingin " +
                        "mengubahnya.");
                }

                var reportBaruLahir = report == null;

                if (report == null)
                {
                    report = NewReport(study, actorUserId, now);
                    _dbContext.RadReports.Add(report);
                }

                var version = new RadReportVersion
                {
                    RadReportId = report.Id,
                    VersionNumber = 1,
                    PreviousVersionId = null,
                    VersionStatus = RadReportVersionStatus.Drafted,
                    IsAmendment = false,
                    Findings = Rapikan(request.Findings),
                    Impression = request.Impression.Trim(),
                    Recommendation = Rapikan(request.Recommendation),
                    AuthorUserId = actorUserId,
                    AuthorRoleSnapshot = authorRole,
                    DraftedAt = now,
                    CreateBy = actorUserId,
                    CreateDateTime = now,
                };

                _dbContext.RadReportVersions.Add(version);

                report.ReportStatus = RadReportStatus.Drafted;
                report.CurrentVersionNumber = 1;

                // Bacaan yang baru lahir pada panggilan ini belum pernah diubah siapa pun.
                // Menstempel jejak "diubah" pada baris yang belum sempat tersimpan membuat
                // riwayatnya terbaca seolah ada suntingan yang tidak pernah terjadi.
                if (!reportBaruLahir)
                {
                    Touch(report, actorUserId, now);
                }

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException)
                {
                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        RadErrorCodes.ReportAlreadyExists,
                        "Pemeriksaan ini sudah memiliki bacaan. Muat ulang halaman lalu periksa " +
                        "kembali.");
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(
                    LogCategory,
                    "RadReport.CreateDraft",
                    "Draf hasil bacaan radiologi ditulis.",
                    RadReportLogPayload.For(report, version, actorUserId));

                return RadOperationResult<RadReportDetailResponse>.Success(
                    await MapAsync(report.Id, cancellationToken));
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Mengubah draf yang belum disahkan — <c>Drafted</c> tetap <c>Drafted</c>.
        ///
        /// <para>
        /// Tiga hal yang dijaga, berurutan: hanya penulisnya sendiri yang boleh mengubah; versi yang
        /// sudah dirilis atau digantikan tidak dapat disentuh sama sekali; dan draf yang sudah
        /// disahkan berhenti dapat diubah, karena pengesahan adalah pernyataan atas isi yang
        /// tertentu — kalau isinya masih dapat berubah sesudahnya, pernyataan itu tidak
        /// mengikat apa pun.
        /// </para>
        /// </summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> UpdateDraftAsync(
            Guid reportId,
            UpdateRadReportDraftRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var isi = ValidateContent(request.Findings, request.Impression, request.Recommendation);

            if (isi != null)
            {
                return isi;
            }

            var transaction = await BeginGuardedAsync($"RAD_REPORT_{reportId:N}", cancellationToken);

            try
            {
                var (report, version, missing) = await LoadWorkingAsync(reportId, cancellationToken);

                if (missing != null)
                {
                    return missing;
                }

                // RAD-VAL-001: "Hanya penulis draf yang dapat mengubahnya sebelum disahkan."
                // Diperiksa lebih dulu daripada keadaan versinya, mengikuti urutan kontrak.
                if (version!.AuthorUserId != actorUserId)
                {
                    return RadOperationResult<RadReportDetailResponse>.Forbidden(
                        RadErrorCodes.NotDraftAuthor,
                        "Hanya penulis draf yang dapat mengubahnya sebelum disahkan.");
                }

                if (version.VersionStatus is RadReportVersionStatus.Released
                    or RadReportVersionStatus.Superseded)
                {
                    return RadOperationResult<RadReportDetailResponse>.Forbidden(
                        RadErrorCodes.ReportVersionFrozen,
                        "Bacaan yang sudah dirilis tidak dapat diubah. Buat koreksi bila ada yang " +
                        "perlu diperbaiki.");
                }

                if (version.VersionStatus != RadReportVersionStatus.Drafted)
                {
                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        RadErrorCodes.InvalidTransition,
                        "Bacaan ini sudah disahkan, sehingga drafnya tidak dapat diubah lagi. " +
                        "Buat koreksi bila ada yang perlu diperbaiki.");
                }

                version.Findings = Rapikan(request.Findings);
                version.Impression = request.Impression.Trim();
                version.Recommendation = Rapikan(request.Recommendation);
                Touch(version, actorUserId, now);
                Touch(report!, actorUserId, now);

                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(
                    LogCategory,
                    "RadReport.UpdateDraft",
                    "Draf hasil bacaan radiologi diubah penulisnya.",
                    RadReportLogPayload.For(report!, version, actorUserId));

                return RadOperationResult<RadReportDetailResponse>.Success(
                    await MapAsync(reportId, cancellationToken));
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /* ================================================================ *
         * Pengesahan dan rilis
         * ================================================================ */

        /// <summary>
        /// Mengesahkan bacaan — <c>Drafted</c> menjadi <c>Validated</c>. <b>Inti keselamatan
        /// modul.</b>
        ///
        /// <para>
        /// Tiga penjaga berjalan berurutan, dan urutannya disengaja:
        /// </para>
        ///
        /// <list type="number">
        /// <item><b>Keadaan versi</b> — hanya draf yang dapat disahkan. Ini pula yang membuat dua
        /// radiolog yang menekan Sahkan bersamaan tidak dapat sama-sama berhasil: yang kedua
        /// membaca versi yang sudah <c>Validated</c> dan ditolak.</item>
        /// <item><b>Bukan pengesahan sendiri</b> — inti <c>RAD-DEC-003</c>. Diperiksa sebelum
        /// kewenangan supaya residen yang mengesahkan drafnya sendiri membaca sebab yang
        /// sebenarnya, yaitu "draf ini harus disahkan orang lain", bukan "Anda bukan
        /// radiolog".</item>
        /// <item><b>Pengesah dihitung sebagai dokter radiolog</b> — penanda
        /// <c>RadReport : ActAsRadiologist</c>.</item>
        /// </list>
        /// </summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> ValidateAsync(
            Guid reportId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var transaction = await BeginGuardedAsync($"RAD_REPORT_{reportId:N}", cancellationToken);

            try
            {
                var (report, version, missing) = await LoadWorkingAsync(reportId, cancellationToken);

                if (missing != null)
                {
                    return missing;
                }

                if (version!.VersionStatus != RadReportVersionStatus.Drafted)
                {
                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        RadErrorCodes.InvalidTransition,
                        "Hanya draf yang belum disahkan yang dapat disahkan; bacaan ini " +
                        $"berstatus {Sebutan(version.VersionStatus)}.");
                }

                // RAD-DEC-003. Yang dinilai adalah peran penulis PADA SAAT draf ini ditulis,
                // bukan kewenangannya hari ini. Residen yang kemudian menjadi dokter radiolog
                // tetap ditolak atas draf lamanya.
                if (version.AuthorUserId == actorUserId &&
                    version.AuthorRoleSnapshot != RadReportAuthorRole.Radiologist)
                {
                    return RadOperationResult<RadReportDetailResponse>.Forbidden(
                        RadErrorCodes.SelfValidationNotAllowed,
                        "Draf yang Anda tulis harus disahkan dokter radiolog.");
                }

                if (!await HasRadiologistAuthorityAsync(cancellationToken))
                {
                    return RadOperationResult<RadReportDetailResponse>.Forbidden(
                        RadErrorCodes.ValidatorNotRadiologist,
                        "Hanya dokter radiolog yang boleh mengesahkan hasil bacaan.");
                }

                version.VersionStatus = RadReportVersionStatus.Validated;
                version.ValidatorUserId = actorUserId;
                version.ValidatedAt = now;
                Touch(version, actorUserId, now);

                report!.ReportStatus = version.IsAmendment
                    ? RadReportStatus.AmendmentValidated
                    : RadReportStatus.Validated;
                Touch(report, actorUserId, now);

                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(
                    LogCategory,
                    "RadReport.Validate",
                    "Hasil bacaan radiologi disahkan dokter radiolog.",
                    RadReportLogPayload.For(report, version, actorUserId));

                return RadOperationResult<RadReportDetailResponse>.Success(
                    await MapAsync(reportId, cancellationToken));
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Merilis bacaan ke dokter pengirim — <c>Validated</c> menjadi <c>Released</c>.
        ///
        /// <para>
        /// <b>Setelah ini isi versinya beku.</b> Perbaikan apa pun wajib lewat versi baru, dan
        /// itu pekerjaan <c>BE-RAD-10</c>.
        /// </para>
        ///
        /// <para>
        /// <c>FirstReleasedAt</c> hanya diisi sekali seumur hidup bacaan. Pertanyaan "sejak kapan
        /// hasilnya tersedia bagi dokter pengirim" harus punya satu jawaban yang tidak bergeser
        /// setiap kali bacaan dikoreksi.
        /// </para>
        /// </summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> ReleaseAsync(
            Guid reportId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var transaction = await BeginGuardedAsync($"RAD_REPORT_{reportId:N}", cancellationToken);

            try
            {
                var (report, version, missing) = await LoadWorkingAsync(reportId, cancellationToken);

                if (missing != null)
                {
                    return missing;
                }

                if (version!.VersionStatus != RadReportVersionStatus.Validated)
                {
                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        RadErrorCodes.InvalidTransition,
                        "Bacaan harus disahkan lebih dulu sebelum dirilis.");
                }

                if (!await HasRadiologistAuthorityAsync(cancellationToken))
                {
                    return RadOperationResult<RadReportDetailResponse>.Forbidden(
                        RadErrorCodes.ValidatorNotRadiologist,
                        "Hanya dokter radiolog yang boleh merilis hasil bacaan.");
                }

                version.VersionStatus = RadReportVersionStatus.Released;
                version.ReleasedAt = now;
                Touch(version, actorUserId, now);

                // Versi yang digantikan berpindah menjadi Superseded PADA SAAT INI, bukan saat
                // draf koreksinya mulai ditulis. Selama koreksi masih disusun, versi lama tetap
                // Released dan tetap menjadi yang berlaku — itulah satu-satunya bacaan yang sah
                // selama jendela waktu tersebut.
                //
                // Hanya STATUSNYA yang berubah. Findings, Impression, Recommendation, penulis,
                // pengesah, dan seluruh waktunya TIDAK DISENTUH — RJ-BIL-GATE-DEC-004.
                if (version.PreviousVersionId.HasValue)
                {
                    var digantikan = await _dbContext.RadReportVersions
                        .FirstOrDefaultAsync(
                            x => x.Id == version.PreviousVersionId.Value,
                            cancellationToken);

                    if (digantikan != null &&
                        digantikan.VersionStatus == RadReportVersionStatus.Released)
                    {
                        digantikan.VersionStatus = RadReportVersionStatus.Superseded;
                        Touch(digantikan, actorUserId, now);
                    }
                }

                report!.ReportStatus = version.IsAmendment
                    ? RadReportStatus.AmendmentReleased
                    : RadReportStatus.Released;

                // Baru di sini versi yang berlaku berpindah. Sebelum baris ini, pembaca tetap
                // diarahkan ke versi rilis sebelumnya.
                report.CurrentVersionNumber = version.VersionNumber;
                report.FirstReleasedAt ??= now;
                report.LastReleasedAt = now;
                Touch(report, actorUserId, now);

                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(
                    LogCategory,
                    "RadReport.Release",
                    "Hasil bacaan radiologi dirilis ke dokter pengirim.",
                    RadReportLogPayload.For(report, version, actorUserId));

                return RadOperationResult<RadReportDetailResponse>.Success(
                    await MapAsync(reportId, cancellationToken));
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /* ================================================================ *
         * Koreksi berversi
         * ================================================================ */

        /// <summary>
        /// Menulis draf koreksi atas bacaan yang sudah dirilis — <c>Released</c> menjadi
        /// <c>AmendmentDrafted</c>.
        ///
        /// <para>
        /// <b>Koreksi tidak pernah menimpa.</b> Yang terjadi hanyalah satu baris versi baru
        /// ditambahkan, menunjuk versi yang digantikannya lewat <c>PreviousVersionId</c>. Isi
        /// versi lama tidak disentuh satu huruf pun, dan statusnya baru berpindah menjadi
        /// <c>Superseded</c> ketika koreksinya benar-benar dirilis.
        /// </para>
        ///
        /// <para>
        /// <b>Alasannya bukan kerapian data.</b> Bacaan yang sudah dirilis mungkin sudah dipakai
        /// dokter lain untuk memberi obat, menjadwalkan operasi, atau memulangkan pasien. Ketika
        /// bacaan itu kemudian dikoreksi, pertanyaan yang harus tetap dapat dijawab bukan hanya
        /// "apa yang benar sekarang", melainkan juga <b>"apa yang dibaca dokter itu waktu
        /// itu"</b> — dan itu biasanya pertanyaan pertama ketika sebuah keputusan klinis
        /// ditinjau ulang.
        /// </para>
        ///
        /// <para>
        /// Aturan pengesahan berlaku sama persis seperti draf pertama (<c>FR-RAD-023</c>):
        /// peran penulis dibekukan di sini, dan penulis bukan-radiolog tetap tidak boleh
        /// mengesahkan koreksinya sendiri.
        /// </para>
        /// </summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> CreateAmendmentAsync(
            Guid reportId,
            CreateRadReportAmendmentRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            // FR-RAD-021. Diperiksa lebih dulu daripada isinya: tanpa alasan, pembaca riwayat
            // tidak akan pernah tahu mengapa bacaan sebelumnya diganti, dan riwayat yang tidak
            // dapat dijelaskan sama tidak bergunanya dengan riwayat yang hilang.
            if (string.IsNullOrWhiteSpace(request.AmendmentReason))
            {
                return RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.AmendmentReasonRequired,
                    "Alasan koreksi wajib diisi.");
            }

            if (request.AmendmentReason.Trim().Length > AmendmentReasonMaxLength)
            {
                return RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.ValidationFailed,
                    "Alasan koreksi terlalu panjang, maksimal 1.000 huruf.");
            }

            var isi = ValidateContent(request.Findings, request.Impression, request.Recommendation);

            if (isi != null)
            {
                return isi;
            }

            var (authorRole, peranDitolak) = await ResolveAuthorRoleAsync(
                request.AuthorRole,
                cancellationToken);

            if (peranDitolak != null)
            {
                return peranDitolak;
            }

            var transaction = await BeginGuardedAsync($"RAD_REPORT_{reportId:N}", cancellationToken);

            try
            {
                var (report, version, missing) = await LoadWorkingAsync(reportId, cancellationToken);

                if (missing != null)
                {
                    return missing;
                }

                // Dua penolakan yang sengaja dibedakan, karena menuntut tindakan yang berbeda.
                if (version!.VersionStatus != RadReportVersionStatus.Released)
                {
                    var pernahDirilis = report!.FirstReleasedAt.HasValue;

                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        pernahDirilis
                            ? RadErrorCodes.AmendmentAlreadyInProgress
                            : RadErrorCodes.ReportNeverReleased,
                        pernahDirilis
                            ? "Sudah ada koreksi yang sedang disusun untuk bacaan ini. " +
                              "Selesaikan atau rilis koreksi itu lebih dulu."
                            : "Bacaan ini belum pernah dirilis, sehingga belum ada yang perlu " +
                              "dikoreksi.");
                }

                var amandemen = new RadReportVersion
                {
                    RadReportId = report!.Id,
                    VersionNumber = version.VersionNumber + 1,
                    PreviousVersionId = version.Id,
                    VersionStatus = RadReportVersionStatus.Drafted,
                    IsAmendment = true,
                    Findings = Rapikan(request.Findings),
                    Impression = request.Impression.Trim(),
                    Recommendation = Rapikan(request.Recommendation),
                    AmendmentReason = request.AmendmentReason.Trim(),
                    AuthorUserId = actorUserId,
                    AuthorRoleSnapshot = authorRole,
                    DraftedAt = now,
                    CreateBy = actorUserId,
                    CreateDateTime = now,
                };

                _dbContext.RadReportVersions.Add(amandemen);

                // CurrentVersionNumber SENGAJA tidak dinaikkan di sini. Versi rilis sebelumnya
                // tetap yang berlaku sampai koreksinya dirilis.
                report.ReportStatus = RadReportStatus.AmendmentDrafted;
                Touch(report, actorUserId, now);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException)
                {
                    // Penjaga terakhir. Dua koreksi yang dimulai hampir bersamaan sama-sama
                    // lolos pemeriksaan di atas, lalu index unik RadReportId + VersionNumber
                    // menolak yang kedua. Tanpa itu, riwayatnya bercabang tanpa ada yang tahu
                    // cabang mana yang berlaku.
                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        RadErrorCodes.AmendmentAlreadyInProgress,
                        "Petugas lain baru saja memulai koreksi atas bacaan ini. Muat ulang " +
                        "halaman lalu periksa kembali.");
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(
                    LogCategory,
                    "RadReport.CreateAmendment",
                    "Draf koreksi hasil bacaan radiologi ditulis.",
                    RadReportLogPayload.For(report, amandemen, actorUserId));

                return RadOperationResult<RadReportDetailResponse>.Success(
                    await MapAsync(reportId, cancellationToken));
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Seluruh versi bacaan, terbaru lebih dulu.
        ///
        /// <para>
        /// Daftar ini <b>hanya bertambah</b>. Tidak ada method hapus versi di service ini, dan
        /// tidak boleh ada — riwayat klinis yang dapat dihapus tidak dapat dipakai menjawab apa
        /// pun.
        /// </para>
        /// </summary>
        public async Task<RadOperationResult<List<RadReportVersionResponse>>> GetVersionsAsync(
            Guid reportId,
            CancellationToken cancellationToken = default)
        {
            var ada = await _dbContext.RadReports
                .AsNoTracking()
                .AnyAsync(x => x.Id == reportId && !x.IsDelete, cancellationToken);

            if (!ada)
            {
                return RadOperationResult<List<RadReportVersionResponse>>.NotFound(
                    RadErrorCodes.ReportNotFound,
                    "Bacaan yang dimaksud tidak ditemukan.");
            }

            var versions = await _dbContext.RadReportVersions
                .AsNoTracking()
                .Where(x => x.RadReportId == reportId && !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .ToListAsync(cancellationToken);

            return RadOperationResult<List<RadReportVersionResponse>>.Success(
                versions.Select(MapVersion).ToList());
        }

        /* ================================================================ *
         * Validasi
         * ================================================================ */

        /// <summary>
        /// Memeriksa isi bacaan terhadap <c>RAD-VAL-001</c> bagian 1 dan panjang kolom pada
        /// <c>RAD-ERD-DICT-001</c> bagian 2.
        /// </summary>
        private static RadOperationResult<RadReportDetailResponse>? ValidateContent(
            string? findings,
            string? impression,
            string? recommendation)
        {
            if (string.IsNullOrWhiteSpace(impression))
            {
                return RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.ValidationFailed,
                    "Kesimpulan bacaan wajib diisi.");
            }

            if (impression.Trim().Length > ImpressionMaxLength)
            {
                return RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.ValidationFailed,
                    "Kesimpulan bacaan terlalu panjang, maksimal 4.000 huruf.");
            }

            if (findings != null && findings.Trim().Length > FindingsMaxLength)
            {
                return RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.ValidationFailed,
                    "Uraian temuan terlalu panjang, maksimal 8.000 huruf.");
            }

            // Tidak tercantum pada RAD-VAL-001, tetapi kolomnya varchar(2000). Tanpa pemeriksaan
            // ini, saran yang kepanjangan berhenti sebagai galat database yang tidak dapat
            // dibaca petugas.
            if (recommendation != null && recommendation.Trim().Length > RecommendationMaxLength)
            {
                return RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.ValidationFailed,
                    "Saran tindak lanjut terlalu panjang, maksimal 2.000 huruf.");
            }

            return null;
        }

        /// <summary>
        /// Study wajib sudah dinilai mutunya dan dinyatakan layak sebelum ada yang boleh
        /// membacanya.
        ///
        /// <para>
        /// Kedua penolakan sengaja dibedakan. "Belum dinilai" menuntut radiografer menilai mutu
        /// citra; "dinyatakan tidak layak" menuntut pemeriksaan diulang. Meleburnya menjadi satu
        /// pesan membuat petugas menunggu sesuatu yang tidak akan pernah datang.
        /// </para>
        /// </summary>
        private static RadOperationResult<RadReportDetailResponse>? ValidateStudyIsReadable(
            RadStudy study)
        {
            if (study.IsUsable == null)
            {
                return RadOperationResult<RadReportDetailResponse>.BusinessRule(
                    RadErrorCodes.StudyQualityNotDecided,
                    "Mutu citra belum dinilai. Nilai mutu citra lebih dulu sebelum menulis bacaan.");
            }

            if (study.IsUsable == false)
            {
                return RadOperationResult<RadReportDetailResponse>.BusinessRule(
                    RadErrorCodes.StudyNotUsable,
                    "Citra pemeriksaan ini dinyatakan tidak layak dibaca, sehingga bacaan tidak " +
                    "dapat dibuat. Buat pemeriksaan ulang lebih dulu.");
            }

            return null;
        }

        /// <summary>
        /// Menentukan peran yang akan dibekukan pada draf — <c>RAD-PERM-001</c> bagian 6.
        ///
        /// <para>
        /// Menyatakan diri dokter radiolog <b>ditolak</b> bila penandanya tidak dipegang, bukan
        /// diturunkan diam-diam menjadi residen. Penurunan diam-diam membuat penulis mengira
        /// drafnya dapat ia sahkan sendiri, lalu bacaannya tertahan tanpa sebab yang terbaca di
        /// layar mana pun.
        /// </para>
        ///
        /// <para>
        /// Pernyataan yang lebih rendah dari kewenangan sebenarnya diterima apa adanya. Seorang
        /// radiolog yang menandai drafnya sebagai hasil bantuan AI hanya membuat aturan
        /// pengesahan makin ketat, dan kejujuran tentang asal-usul sebuah draf lebih berharga
        /// daripada kerapian datanya.
        /// </para>
        /// </summary>
        private async Task<(RadReportAuthorRole Role,
            RadOperationResult<RadReportDetailResponse>? Error)> ResolveAuthorRoleAsync(
            RadReportAuthorRole? declared,
            CancellationToken cancellationToken)
        {
            var isRadiologist = await HasRadiologistAuthorityAsync(cancellationToken);

            if (declared == null)
            {
                if (isRadiologist)
                {
                    return (RadReportAuthorRole.Radiologist, null);
                }

                // Isian yang kurang, bukan kewenangan yang kurang: penulisnya memang boleh
                // menulis draf, sistem hanya tidak dapat membedakan residen dari radiografer
                // tanpa diberi tahu.
                return (default, RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.AuthorRoleRequired,
                    "Sebutkan peran Anda saat menulis draf ini — residen, radiografer, atau " +
                    "bantuan AI."));
            }

            if (declared == RadReportAuthorRole.Radiologist && !isRadiologist)
            {
                return (default, RadOperationResult<RadReportDetailResponse>.Forbidden(
                    RadErrorCodes.AuthorRoleNotPermitted,
                    "Anda belum terdaftar sebagai dokter radiolog, sehingga draf ini tidak dapat " +
                    "ditulis atas nama dokter radiolog."));
            }

            return (declared.Value, null);
        }

        /* ================================================================ *
         * Perancah persistence
         * ================================================================ */

        /// <summary>
        /// Membuka transaksi beserta kunci per bacaan, hanya pada penyedia relasional.
        ///
        /// <para>
        /// <c>pg_advisory_xact_lock</c> membuat dua permintaan atas bacaan yang sama berjalan
        /// berurutan, bukan bersamaan — mekanisme yang sama dengan yang sudah dipakai
        /// <c>BillingDepositService</c>. Tanpa itu, dua radiolog yang menekan Sahkan pada detik
        /// yang sama sama-sama membaca versi berstatus <c>Drafted</c> lalu sama-sama
        /// menyimpannya.
        /// </para>
        ///
        /// <para>
        /// Penyedia in-memory tidak mendukung transaksi maupun kunci, sehingga uji unit bersandar
        /// pada pemeriksaan keadaan versi. Itu menahan penekanan tombol yang berurutan, tetapi
        /// <b>bukan</b> bukti bahwa yang benar-benar bersamaan tertahan; buktinya menuntut
        /// database sungguhan.
        /// </para>
        /// </summary>
        private async Task<IDbContextTransaction?> BeginGuardedAsync(
            string lockKey,
            CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsRelational())
            {
                return null;
            }

            var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [lockKey],
                cancellationToken);

            return transaction;
        }

        private Task<RadStudy?> LoadStudyAsync(Guid radStudyId, CancellationToken cancellationToken) =>
            _dbContext.RadStudies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == radStudyId && !x.IsDelete, cancellationToken);

        private Task<RadReport?> FindReportByStudyAsync(
            Guid radStudyId,
            CancellationToken cancellationToken) =>
            _dbContext.RadReports
                .FirstOrDefaultAsync(x => x.RadStudyId == radStudyId && !x.IsDelete, cancellationToken);

        /// <summary>
        /// Memuat bacaan beserta <b>versi yang sedang dikerjakan</b>, siap disunting.
        ///
        /// <para>
        /// Yang dimuat adalah versi bernomor <b>tertinggi</b>, bukan versi bernomor
        /// <c>CurrentVersionNumber</c>. Selama koreksi sedang disusun, keduanya berbeda dan
        /// perbedaan itu disengaja:
        /// </para>
        ///
        /// <list type="bullet">
        /// <item><c>CurrentVersionNumber</c> menunjuk versi yang <b>berlaku bagi pembaca</b> —
        /// bacaan yang sudah dirilis dan boleh dipakai mengambil keputusan.</item>
        /// <item>Versi tertinggi adalah yang <b>sedang dikerjakan</b> — draf koreksi yang belum
        /// sah dan belum boleh dipakai siapa pun.</item>
        /// </list>
        ///
        /// <para>
        /// Kalau keduanya disamakan, membuka draf koreksi akan membuat versi rilis lenyap dari
        /// pandangan dokter pengirim selama koreksi itu disusun — padahal selama jendela waktu
        /// itu versi rilis adalah satu-satunya bacaan yang sah. Seorang dokter jaga yang membuka
        /// hasil pada saat itu akan membaca draf yang belum diperiksa siapa pun, atau tidak
        /// membaca apa-apa sama sekali.
        /// </para>
        /// </summary>
        private async Task<(RadReport? Report, RadReportVersion? Version,
            RadOperationResult<RadReportDetailResponse>? Missing)> LoadWorkingAsync(
            Guid reportId,
            CancellationToken cancellationToken)
        {
            var report = await _dbContext.RadReports
                .FirstOrDefaultAsync(x => x.Id == reportId && !x.IsDelete, cancellationToken);

            if (report == null)
            {
                return (null, null, RadOperationResult<RadReportDetailResponse>.NotFound(
                    RadErrorCodes.ReportNotFound,
                    "Bacaan yang dimaksud tidak ditemukan."));
            }

            var version = await _dbContext.RadReportVersions
                .Where(x => x.RadReportId == reportId && !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (version == null)
            {
                return (report, null, RadOperationResult<RadReportDetailResponse>.Conflict(
                    RadErrorCodes.InvalidTransition,
                    "Bacaan ini belum memiliki draf. Tulis draf bacaan lebih dulu."));
            }

            return (report, version, null);
        }

        private RadReport NewReport(RadStudy study, Guid actorUserId, DateTime now) => new()
        {
            RadStudyId = study.Id,
            RadOrderId = study.RadOrderId,
            EncounterId = study.EncounterId,
            ReportNumber = _reportNumberService.Generate(),
            ReportStatus = RadReportStatus.Pending,
            CurrentVersionNumber = 0,
            CreateBy = actorUserId,
            CreateDateTime = now,
        };

        /// <summary>
        /// Mengurutkan daftar bacaan. Kolom yang tidak dikenali jatuh ke tanggal dibuka,
        /// terbaru lebih dulu — bukan galat, karena layar tidak boleh gagal hanya karena
        /// pengurutan.
        /// </summary>
        private static IQueryable<RadReport> Urutkan(
            IQueryable<RadReport> sumber,
            string? sortBy,
            string? sortDirection)
        {
            var menaik = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "reportnumber" => menaik
                    ? sumber.OrderBy(x => x.ReportNumber)
                    : sumber.OrderByDescending(x => x.ReportNumber),

                "reportstatus" => menaik
                    ? sumber.OrderBy(x => x.ReportStatus)
                    : sumber.OrderByDescending(x => x.ReportStatus),

                "firstreleasedat" => menaik
                    ? sumber.OrderBy(x => x.FirstReleasedAt)
                    : sumber.OrderByDescending(x => x.FirstReleasedAt),

                "lastreleasedat" => menaik
                    ? sumber.OrderBy(x => x.LastReleasedAt)
                    : sumber.OrderByDescending(x => x.LastReleasedAt),

                _ => menaik
                    ? sumber.OrderBy(x => x.CreateDateTime)
                    : sumber.OrderByDescending(x => x.CreateDateTime),
            };
        }

        private static (int PageNumber, int PageSize) NormalkanHalaman(int pageNumber, int pageSize)
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

        /// <summary>
        /// Versi berlaku untuk sekumpulan bacaan, diambil dalam satu query — bukan satu query
        /// per baris.
        /// </summary>
        private async Task<Dictionary<Guid, RadReportVersion>> LoadCurrentVersionsAsync(
            IReadOnlyCollection<RadReport> reports,
            CancellationToken cancellationToken)
        {
            var berversi = reports.Where(x => x.CurrentVersionNumber > 0).ToList();

            if (berversi.Count == 0)
            {
                return [];
            }

            var ids = berversi.Select(x => x.Id).ToList();

            var versions = await _dbContext.RadReportVersions
                .AsNoTracking()
                .Where(x => ids.Contains(x.RadReportId) && !x.IsDelete)
                .ToListAsync(cancellationToken);

            return berversi
                .Select(report => versions.FirstOrDefault(v =>
                    v.RadReportId == report.Id &&
                    v.VersionNumber == report.CurrentVersionNumber))
                .Where(v => v != null)
                .ToDictionary(v => v!.RadReportId, v => v!);
        }

        private static RadReportListResponse MapList(
            RadReport report,
            RadReportVersion? current) => new()
            {
                Id = report.Id,
                ReportNumber = report.ReportNumber,
                RadStudyId = report.RadStudyId,
                StudyNumber = report.RadStudy?.StudyNumber,
                RadOrderId = report.RadOrderId,
                EncounterId = report.EncounterId,
                ReportStatus = report.ReportStatus.ToString(),
                ReportStatusLabel = LabelStatus(report.ReportStatus),
                CurrentVersionNumber = report.CurrentVersionNumber,
                AuthorRoleSnapshot = current?.AuthorRoleSnapshot.ToString(),
                AuthorUserId = current?.AuthorUserId,
                ValidatorUserId = current?.ValidatorUserId,
                DraftedAt = current?.DraftedAt,
                FirstReleasedAt = report.FirstReleasedAt,
                LastReleasedAt = report.LastReleasedAt,
            };

        /// <summary>
        /// Aksi yang masuk akal atas sebuah bacaan menurut keadaan versinya.
        ///
        /// <para>
        /// <b>Hanya keadaan yang dinilai, bukan pelakunya.</b> Daftar ini membantu layar
        /// menyusun tombol; ia tidak pernah menjadi pengaman. Seorang residen akan tetap melihat
        /// tombol Sahkan pada draf yang ia tulis, dan permintaannya tetap ditolak `403` di
        /// backend. Menambahkan pemeriksaan kewenangan di sini akan membuat dua tempat yang
        /// harus sama-sama benar, dan yang satu diam-diam berselisih dari yang lain adalah cara
        /// paling umum sebuah aturan keselamatan berhenti berlaku.
        /// </para>
        /// </summary>
        private static List<string> AksiYangTersedia(RadReportVersion? working)
        {
            if (working == null)
            {
                return ["CreateDraft"];
            }

            return working.VersionStatus switch
            {
                RadReportVersionStatus.Drafted => ["UpdateDraft", "Validate"],
                RadReportVersionStatus.Validated => ["Release"],
                RadReportVersionStatus.Released => ["Amend"],
                _ => [],
            };
        }

        private async Task<RadReportDetailResponse> MapAsync(
            Guid reportId,
            CancellationToken cancellationToken)
        {
            var report = await _dbContext.RadReports
                .AsNoTracking()
                .FirstAsync(x => x.Id == reportId, cancellationToken);

            var versions = await _dbContext.RadReportVersions
                .AsNoTracking()
                .Where(x => x.RadReportId == reportId && !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .ToListAsync(cancellationToken);

            // Dua versi yang sengaja dibedakan. "Berlaku" adalah yang boleh dipakai mengambil
            // keputusan; "dikerjakan" adalah yang sedang disusun dan belum tentu sah. Selama
            // koreksi berjalan keduanya berbeda, dan menyamakannya berarti menawarkan draf yang
            // belum diperiksa siapa pun sebagai hasil yang berlaku.
            var current = versions
                .FirstOrDefault(x => x.VersionNumber == report.CurrentVersionNumber);

            var working = versions.FirstOrDefault();

            return new RadReportDetailResponse
            {
                AvailableActions = AksiYangTersedia(working),
                WorkingVersionNumber = working?.VersionNumber ?? 0,
                Id = report.Id,
                RadStudyId = report.RadStudyId,
                RadOrderId = report.RadOrderId,
                EncounterId = report.EncounterId,
                ReportNumber = report.ReportNumber,
                ReportStatus = report.ReportStatus.ToString(),
                CurrentVersionNumber = report.CurrentVersionNumber,
                FirstReleasedAt = report.FirstReleasedAt,
                LastReleasedAt = report.LastReleasedAt,
                CurrentVersion = current == null ? null : MapVersion(current),
                Versions = versions.Select(MapVersion).ToList(),
            };
        }

        private static RadReportVersionResponse MapVersion(RadReportVersion version) => new()
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            PreviousVersionId = version.PreviousVersionId,
            VersionStatus = version.VersionStatus.ToString(),
            IsAmendment = version.IsAmendment,
            Findings = version.Findings,
            Impression = version.Impression,
            Recommendation = version.Recommendation,
            AuthorUserId = version.AuthorUserId,
            AuthorRoleSnapshot = version.AuthorRoleSnapshot.ToString(),
            DraftedAt = version.DraftedAt,
            ValidatorUserId = version.ValidatorUserId,
            ValidatedAt = version.ValidatedAt,
            ReleasedAt = version.ReleasedAt,
            AmendmentReason = version.AmendmentReason,
        };

        private static string? Rapikan(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void Touch(RadReport report, Guid actorUserId, DateTime now)
        {
            report.UpdateBy = actorUserId;
            report.UpdateDateTime = now;
            report.Version += 1;
        }

        private static void Touch(RadReportVersion version, Guid actorUserId, DateTime now)
        {
            version.UpdateBy = actorUserId;
            version.UpdateDateTime = now;
            version.Version += 1;
        }

        /// <summary>Teks keadaan bacaan yang siap ditampilkan pada layar.</summary>
        private static string LabelStatus(RadReportStatus status) => status switch
        {
            RadReportStatus.Pending => "Menunggu draf bacaan",
            RadReportStatus.Drafted => "Draf, belum disahkan",
            RadReportStatus.Validated => "Sudah disahkan, belum dirilis",
            RadReportStatus.Released => "Sudah dirilis ke dokter pengirim",
            RadReportStatus.AmendmentDrafted => "Draf koreksi, belum disahkan",
            RadReportStatus.AmendmentValidated => "Koreksi sudah disahkan, belum dirilis",
            RadReportStatus.AmendmentReleased => "Koreksi sudah dirilis",
            _ => status.ToString(),
        };

        private static string LabelVersi(RadReportVersionStatus status) => status switch
        {
            RadReportVersionStatus.Drafted => "Draf",
            RadReportVersionStatus.Validated => "Sudah disahkan",
            RadReportVersionStatus.Released => "Dirilis — isinya beku",
            RadReportVersionStatus.Superseded => "Digantikan versi yang lebih baru",
            _ => status.ToString(),
        };

        private static string LabelPeran(RadReportAuthorRole role) => role switch
        {
            RadReportAuthorRole.Radiologist => "Dokter radiolog",
            RadReportAuthorRole.Resident => "Residen",
            RadReportAuthorRole.Radiographer => "Radiografer",
            RadReportAuthorRole.AiAssisted => "Bantuan AI",
            _ => role.ToString(),
        };

        private static string Sebutan(RadReportVersionStatus status) => status switch
        {
            RadReportVersionStatus.Drafted => "draf",
            RadReportVersionStatus.Validated => "sudah disahkan",
            RadReportVersionStatus.Released => "sudah dirilis",
            RadReportVersionStatus.Superseded => "sudah digantikan versi yang lebih baru",
            _ => status.ToString(),
        };

        /// <summary>
        /// Identitas pelaku, diambil dari sesi yang sedang berjalan.
        ///
        /// <para>
        /// Pelaku yang tidak dikenali membuat perbandingan "penulis versus pengesah" kehilangan
        /// artinya, dan pengaman yang kehilangan artinya lebih berbahaya daripada pengaman yang
        /// tidak ada — karena ia tetap terlihat bekerja. Karena itu identitas kosong menghentikan
        /// tindakan, bukan dilanjutkan dengan nilai kosong.
        /// </para>
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

    /// <summary>
    /// Muatan log untuk setiap perubahan keadaan hasil bacaan.
    ///
    /// <para>
    /// <b>Berkas ini ada supaya kolom sensitif tidak dapat masuk log karena kelalaian.</b>
    /// <c>RAD-PERM-001</c> bagian 7 mengharamkan <c>Findings</c>, <c>Impression</c>,
    /// <c>Recommendation</c>, dan <c>AmendmentReason</c> muncul di application log. Kalau setiap
    /// pemanggilan logger menyusun muatannya sendiri, cukup satu kali seseorang menambahkan
    /// <c>version.Impression</c> "supaya mudah ditelusuri" untuk memindahkan kesimpulan klinis
    /// seorang pasien ke tempat yang aturan aksesnya berbeda dari tabel aslinya.
    /// </para>
    ///
    /// <para>
    /// Dengan satu jenis muatan yang memang tidak punya kolom untuk menampungnya, penambahan
    /// seperti itu harus dilakukan di sini — di tempat yang diperiksa uji dan terbaca saat
    /// review.
    /// </para>
    /// </summary>
    public sealed class RadReportLogPayload
    {
        public Guid ReportId { get; init; }

        public string ReportNumber { get; init; } = string.Empty;

        public Guid RadStudyId { get; init; }

        public Guid RadOrderId { get; init; }

        public Guid EncounterId { get; init; }

        public string ReportStatus { get; init; } = string.Empty;

        public int? VersionNumber { get; init; }

        public string? VersionStatus { get; init; }

        /// <summary>Peran penulis yang dibekukan. Bukan isi bacaan, dan wajib terekam audit.</summary>
        public string? AuthorRoleSnapshot { get; init; }

        public Guid? AuthorUserId { get; init; }

        public Guid? ValidatorUserId { get; init; }

        public Guid ActorUserId { get; init; }

        public static RadReportLogPayload For(
            RadReport report,
            RadReportVersion? version,
            Guid actorUserId) => new()
            {
                ReportId = report.Id,
                ReportNumber = report.ReportNumber,
                RadStudyId = report.RadStudyId,
                RadOrderId = report.RadOrderId,
                EncounterId = report.EncounterId,
                ReportStatus = report.ReportStatus.ToString(),
                VersionNumber = version?.VersionNumber,
                VersionStatus = version?.VersionStatus.ToString(),
                AuthorRoleSnapshot = version?.AuthorRoleSnapshot.ToString(),
                AuthorUserId = version?.AuthorUserId,
                ValidatorUserId = version?.ValidatorUserId,
                ActorUserId = actorUserId,
            };
    }
}
