using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Siklus hidup pesanan laboratorium sesuai <c>RJ-BIL-GATE-DEC-003</c>.
    ///
    /// Pesanan tidak pernah menerbitkan fakta kelayakan tagih. Yang menerbitkan hanyalah
    /// penetapan layak pada tingkat sampel, karena kelayakan tagih dinilai per komponen
    /// pemeriksaan sesuai keputusan author <c>RJ-BIL-OQ-008</c>.
    /// </summary>
    public class LabOrderService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LabSpecimenService _labSpecimenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabOrderService(
            ApplicationDbContext dbContext,
            LabSpecimenService labSpecimenService,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _labSpecimenService = labSpecimenService;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Keterangan bentuk layar daftar pesanan. Tidak menyentuh database sama sekali.
        /// </summary>
        public LabOrderFilterMetadataResponse GetFilterMetadata() =>
            LabFilterMetadataFactory.LabOrder();

        /// <summary>
        /// Rekap pesanan pada satu rentang waktu, dihitung dari baris yang belum ditandai
        /// terhapus. Rentangnya memakai waktu pesanan dibuat.
        /// </summary>
        public async Task<LabOrderSummaryResponse> GetSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.CreateDateTime >= startDate &&
                            x.CreateDateTime <= endDate);

            // Satu perjalanan ke database, bukan sebelas. Pencacahan per status dan per
            // disiplin dikerjakan di sisi server lewat satu proyeksi agregat.
            var rekap = await source
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Draft = g.Count(x => x.OrderStatus == LabOrderStatus.Draft),
                    Diminta = g.Count(x => x.OrderStatus == LabOrderStatus.Requested),
                    Diterima = g.Count(x => x.OrderStatus == LabOrderStatus.Accepted),
                    SedangDikerjakan = g.Count(x => x.OrderStatus == LabOrderStatus.InProcess),
                    Selesai = g.Count(x => x.OrderStatus == LabOrderStatus.Completed),
                    Ditahan = g.Count(x => x.OrderStatus == LabOrderStatus.OnHold),
                    PembatalanDiminta = g.Count(x => x.OrderStatus == LabOrderStatus.CancelRequested),
                    Dibatalkan = g.Count(x => x.OrderStatus == LabOrderStatus.Cancelled),
                    PatologiKlinik = g.Count(x => x.Discipline == LabDiscipline.ClinicalPathology),
                    PatologiAnatomi = g.Count(x => x.Discipline == LabDiscipline.AnatomicalPathology),
                    Mikrobiologi = g.Count(x => x.Discipline == LabDiscipline.Microbiology),
                    TanpaDisiplin = g.Count(x => x.Discipline == null)
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new LabOrderSummaryResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalPesanan = rekap?.Total ?? 0,
                Draft = rekap?.Draft ?? 0,
                Diminta = rekap?.Diminta ?? 0,
                Diterima = rekap?.Diterima ?? 0,
                SedangDikerjakan = rekap?.SedangDikerjakan ?? 0,
                Selesai = rekap?.Selesai ?? 0,
                Ditahan = rekap?.Ditahan ?? 0,
                PembatalanDiminta = rekap?.PembatalanDiminta ?? 0,
                Dibatalkan = rekap?.Dibatalkan ?? 0,
                PatologiKlinik = rekap?.PatologiKlinik ?? 0,
                PatologiAnatomi = rekap?.PatologiAnatomi ?? 0,
                Mikrobiologi = rekap?.Mikrobiologi ?? 0,
                TanpaDisiplin = rekap?.TanpaDisiplin ?? 0
            };
        }

        /// <summary>
        /// Daftar pesanan dengan penyaring, pengurutan, dan pagination di sisi server.
        ///
        /// Penyaring <c>EncounterId</c> adalah yang paling menentukan: tanpanya, pemanggil yang
        /// hanya butuh pesanan satu pasien terpaksa menarik seluruh tabel lalu menyaringnya
        /// sendiri — dan pesanan pasien lain ikut terkirim ke browsernya. Itu keadaan yang
        /// sebelumnya benar-benar terjadi pada layar IGD (<c>IGD-DEC-105</c>).
        /// </summary>
        /// <remarks>
        /// <c>BE-LAB-18</c> dan <c>BE-RWI-042</c> bertemu di jalur yang sama. Penyaring
        /// kunjungan yang semula berdiri sendiri sebagai parameter lepas kini menjadi ruas
        /// <c>EncounterId</c> pada <see cref="LabOrderPagedQuery"/>, sehingga hanya ada satu
        /// daftar pesanan yang perlu dirawat — bukan dua yang perlahan berbeda isi.
        /// </remarks>
        public async Task<PagedResult<LabOrderListResponse>> GetListAsync(
            LabOrderPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.EncounterId.HasValue && query.EncounterId.Value != Guid.Empty)
                source = source.Where(x => x.EncounterId == query.EncounterId.Value);

            if (query.OrderStatus.HasValue)
                source = source.Where(x => x.OrderStatus == query.OrderStatus.Value);

            if (query.Discipline.HasValue)
                source = source.Where(x => x.Discipline == query.Discipline.Value);

            if (query.StartDate.HasValue)
                source = source.Where(x => x.CreateDateTime >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                source = source.Where(x => x.CreateDateTime <= query.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.Procedure != null &&
                    (EF.Functions.ILike(x.Procedure.ProcedureCode, $"%{search}%") ||
                     EF.Functions.ILike(x.Procedure.ProcedureName, $"%{search}%")));
            }

            var totalData = await source.CountAsync(cancellationToken);

            // Nama kolom yang tidak dikenal dikembalikan ke bawaan, bukan ditolak. Layar lama
            // yang mengirim kolom yang sudah tidak ada tetap memperoleh daftar yang masuk akal.
            var menaik = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            var terurut = query.SortBy?.Trim().ToLowerInvariant() switch
            {
                "orderstatus" => menaik
                    ? source.OrderBy(x => x.OrderStatus).ThenByDescending(x => x.CreateDateTime).ThenBy(x => x.Id)
                    : source.OrderByDescending(x => x.OrderStatus).ThenByDescending(x => x.CreateDateTime).ThenBy(x => x.Id),
                _ => menaik
                    ? source.OrderBy(x => x.CreateDateTime).ThenBy(x => x.Id)
                    : source.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.Id)
            };

            // Proyeksinya dipinjam dari jalur per-perawatan supaya penanda hasil final ikut
            // terisi di sini juga. Daftar berpagination yang mengirim IsResultFinal selalu
            // false akan menyatakan setiap hasil belum final - keliru ke arah yang berlawanan,
            // tetapi tetap keliru.
            var items = await ProyeksikanDaftarAsync(
                terurut.Skip((pageNumber - 1) * pageSize).Take(pageSize),
                cancellationToken);

            return new PagedResult<LabOrderListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Pesanan laboratorium beserta ketersediaan hasilnya untuk satu perawatan rawat inap.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-052</c>, <c>INV-DOK-12</c>, <c>RUL-DOK-02</c>. Penyaringnya adalah penanda
        /// perawatan, bukan pasien. Pasien yang dirawat dua kali dalam sebulan memiliki dua
        /// rangkaian pesanan yang berbeda, dan menyaring per pasien akan menampilkan hasil
        /// perawatan lama pada layar perawatan yang sedang berjalan.
        /// </para>
        /// <para>
        /// <b>Nol tabel salinan.</b> Yang dibaca adalah baris pesanan milik Laboratorium apa
        /// adanya. Rawat Inap tidak menyimpan satu baris hasil pun; menyalinnya berarti
        /// menampilkan hasil yang sudah basi ketika Laboratorium merevisinya.
        /// </para>
        /// </remarks>
        /// <param name="episodeId">Perawatan yang pesanannya dibaca.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<List<LabOrderListResponse>> GetByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.InpEpisodeId == episodeId);

            return await ProyeksikanDaftarAsync(
                query.OrderBy(x => x.CreateDateTime), cancellationToken);
        }

        /// <summary>
        /// Memproyeksikan pesanan menjadi baris daftar beserta penanda ketersediaan hasilnya.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-30</c>. Hasil dinyatakan final hanya ketika pesanannya benar-benar selesai
        /// dan tidak dibatalkan. Selain itu, barisnya ditandai belum final beserta kalimat yang
        /// siap ditampilkan apa adanya - dokter harus melihat perbedaannya tanpa perlu
        /// menerjemahkan nama status.
        ///
        /// Dipakai bersama oleh daftar berpagination dan daftar per perawatan, supaya penanda
        /// keselamatan ini tidak dapat hilang hanya karena barisnya dibaca lewat jalur lain.
        ///
        /// <para>
        /// Sejak <c>LAB-API-v1</c> <c>r13</c> method ini <b>bukan lagi</b> <c>static</c>: kedua
        /// nama yang ditambahkan amandemen itu diterjemahkan di dalam proyeksi yang sama,
        /// sehingga ia membutuhkan <c>DbContext</c>. Menjadikannya method instance lebih murah
        /// daripada meneruskan context sebagai parameter, dan ia tetap privat sehingga nol
        /// pemanggil di luar kelas ini terpengaruh.
        /// </para>
        /// </remarks>
        private async Task<List<LabOrderListResponse>> ProyeksikanDaftarAsync(
            IQueryable<LabOrder> query,
            CancellationToken cancellationToken)
        {
            var baris = await query
                .Select(x => new
                {
                    x.Id,
                    x.EncounterId,
                    x.InpEpisodeId,
                    x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    x.OrderStatus,
                    SpecimenCount = x.Specimens.Count(s => !s.IsDelete),
                    AcceptedSpecimenCount = x.Specimens.Count(s =>
                        !s.IsDelete && s.SpecimenStatus == LabSpecimenStatus.Accepted),
                    x.IsCancel,
                    x.CreateDateTime,

                    // LAB-API-v1 r13. Kedua nama diambil di dalam proyeksi yang sama, bukan
                    // lewat pencarian per baris sesudahnya — daftar berisi 25 pesanan tidak
                    // boleh berubah menjadi 51 perjalanan ke database hanya untuk menerjemahkan
                    // dua penunjuk. Cara yang sama sudah dipakai RequestedByName pada
                    // GetDetailAsync.
                    x.ConfirmedAt,
                    ConfirmedByName = x.ConfirmedByUserId == null
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.ConfirmedByUserId)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    ExaminerDoctorName = x.ExaminerDoctorId == null
                        ? null
                        : _dbContext.Set<MstDoctor>()
                            .Where(d => d.Id == x.ExaminerDoctorId)
                            .Select(d => d.FullName)
                            .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            return baris.Select(x =>
            {
                var final = x.OrderStatus == LabOrderStatus.Completed && !x.IsCancel;

                return new LabOrderListResponse
                {
                    Id = x.Id,
                    EncounterId = x.EncounterId,
                    InpEpisodeId = x.InpEpisodeId,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.ProcedureCode,
                    ProcedureName = x.ProcedureName,
                    OrderStatus = x.OrderStatus.ToString(),
                    SpecimenCount = x.SpecimenCount,
                    AcceptedSpecimenCount = x.AcceptedSpecimenCount,
                    IsCancel = x.IsCancel,
                    IsResultFinal = final,
                    ResultAvailabilityNote = x.IsCancel
                        ? "Pesanan dibatalkan; tidak ada hasil."
                        : final
                            ? "Hasil sudah final."
                            : "Hasil belum final. Jangan dipakai sebagai dasar keputusan klinis.",
                    CreateDateTime = x.CreateDateTime,
                    ConfirmedAt = x.ConfirmedAt,
                    ConfirmedByName = x.ConfirmedByName,
                    ExaminerDoctorName = x.ExaminerDoctorName
                };
            }).ToList();
        }

        public async Task<LabOrderDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new LabOrderDetailResponse
                {
                    Id = x.Id,
                    EncounterId = x.EncounterId,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    OrderStatus = x.OrderStatus.ToString(),
                    SpecimenCount = x.Specimens.Count(s => !s.IsDelete),
                    AcceptedSpecimenCount = x.Specimens.Count(s =>
                        !s.IsDelete && s.SpecimenStatus == LabSpecimenStatus.Accepted),
                    IsCancel = x.IsCancel,
                    CreateDateTime = x.CreateDateTime,
                    Discipline = x.Discipline != null ? x.Discipline.ToString() : null,
                    RequestedAt = x.RequestedAt,
                    RequestedByUserId = x.RequestedByUserId,
                    RequestedByName = x.RequestedByUserId == null
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.RequestedByUserId)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    CompletedAt = x.CompletedAt,
                    StatusBeforeHold = x.StatusBeforeHold != null ? x.StatusBeforeHold.ToString() : null,
                    Version = x.Version,
                    CancelDateTime = x.CancelDateTime,
                    CancelBy = x.CancelBy == Guid.Empty ? null : x.CancelBy,

                    // LAB-API-v1 r13. Penunjuknya dikirim di sini karena aksi lanjutan
                    // membutuhkannya; namanya dikirim juga supaya layar detail tidak perlu
                    // menerjemahkan sendiri.
                    ConfirmedAt = x.ConfirmedAt,
                    ConfirmedByUserId = x.ConfirmedByUserId,
                    ConfirmedByName = x.ConfirmedByUserId == null
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.ConfirmedByUserId)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    ExaminerDoctorId = x.ExaminerDoctorId,
                    ExaminerDoctorName = x.ExaminerDoctorId == null
                        ? null
                        : _dbContext.Set<MstDoctor>()
                            .Where(d => d.Id == x.ExaminerDoctorId)
                            .Select(d => d.FullName)
                            .FirstOrDefault(),

                    // LAB-API-v1 r15. Isi pesanan yang sebenarnya, karena ProcedureId di atas
                    // hanyalah penunjuk WAKIL — satu pesanan hasil by-examinations dapat memuat
                    // beberapa pemeriksaan sedisiplin sekaligus.
                    //
                    // Dibaca dari kolom snapshot, bukan dari katalog yang berlaku hari ini:
                    // dokumen resmi harus menyebut apa yang dipesan waktu itu, dan nama katalog
                    // yang kemudian diganti tidak boleh mengubah isi dokumen yang sudah dicetak.
                    //
                    // Baris yang dibatalkan IKUT dikembalikan beserta statusnya. Menyaringnya di
                    // sini berarti konsumen tidak dapat membedakan pemeriksaan yang tidak pernah
                    // dipesan dari yang dipesan lalu dibatalkan.
                    OrderedProcedures = _dbContext.LabOrderedProcedures
                        .Where(p => p.LabOrderId == x.Id && !p.IsDelete)
                        .OrderBy(p => p.CreateDateTime)
                        .ThenBy(p => p.ProcedureNameSnapshot)
                        .Select(p => new LabOrderedProcedureResponse
                        {
                            ProcedureCode = p.ProcedureCodeSnapshot,
                            ProcedureName = p.ProcedureNameSnapshot,
                            Urgency = p.Urgency.ToString(),
                            OrderedStatus = p.OrderedStatus.ToString()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<LabOrderDetailResponse> CreateAsync(
            CreateLabOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EncounterId == Guid.Empty)
                throw new ArgumentException("EncounterId wajib diisi.");

            if (request.ProcedureId == Guid.Empty)
                throw new ArgumentException("ProcedureId wajib diisi.");

            // LAB-DEC-025: hanya tiga disiplin yang ada. Angka di luar ketiganya ditolak di
            // sini, bukan disimpan diam-diam sebagai nilai enum yang tidak berarti apa pun.
            if (request.Discipline.HasValue && !Enum.IsDefined(request.Discipline.Value))
                throw new ArgumentException("Disiplin laboratorium tidak dikenal.");

            var encounterExists = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.EncounterId && !x.IsDelete, cancellationToken);

            if (!encounterExists)
                throw new KeyNotFoundException("Encounter tidak ditemukan.");

            // BE-RWI-052 kriteria 1, VAL-DOK-22. Penanda perawatan boleh kosong - pesanan
            // poliklinik dan IGD memang tidak punya perawatan rawat inap. Yang dijaga adalah
            // KECOCOKANNYA ketika penandanya dikirim: pesanan perawatan A tidak boleh diproses
            // sebagai milik perawatan B.
            if (request.InpEpisodeId.HasValue && request.InpEpisodeId.Value != Guid.Empty)
            {
                var episodeCocok = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.InpEpisodeId.Value
                                   && x.EncounterId == request.EncounterId
                                   && !x.IsDelete,
                              cancellationToken);

                if (!episodeCocok)
                    throw new ArgumentException("Pesanan ini tidak cocok dengan perawatan pasien.");
            }

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ProcedureId &&
                    x.IsLaboratory &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (procedure == null)
                throw new ArgumentException("Procedure tidak ditemukan, tidak aktif, atau bukan procedure laboratorium.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // Endpoint pembuatan yang sudah ada sejak sebelum RJ-BIL-BE-003 berarti "pesanan
            // dikirim ke laboratorium", sehingga status awalnya Requested dan bukan Draft.
            // Mengubah artinya menjadi Draft akan mengubah perilaku endpoint lama tanpa manfaat.
            var entity = new LabOrder
            {
                EncounterId = request.EncounterId,
                // BE-RWI-052. Konteks perawatan distempel saat pesanan lahir, sehingga
                // pembacaan per perawatan menjadi pemeriksaan satu kolom.
                InpEpisodeId = request.InpEpisodeId,
                ProcedureId = request.ProcedureId,
                // Disiplin hanya boleh ditetapkan di sini. Setelah baris ini tersimpan, EF
                // menolak setiap upaya mengubahnya (INV-21).
                //
                // BE-LAB-29: bila permintaan tidak membawa disiplin, ia DITURUNKAN dari
                // penggolongan katalog. Sebelumnya ruas ini disalin apa adanya, dan karena
                // ruasnya tidak wajib, setiap pesanan yang dibuat tanpa memilihnya tersimpan
                // berdisiplin kosong — lalu HILANG dari ketiga layar Pemeriksaan, yang menyaring
                // tepat atas kolom ini. Pasiennya tersimpan dengan benar tetapi tidak muncul di
                // layar yang justru dipakai petugas mengerjakannya.
                //
                // AC-83 menjanjikan hal ini sejak LAB-DEC-048, tetapi janji itu selama ini hanya
                // ditegakkan layar. Pemanggil lain mana pun masih dapat menembusnya, dan memang
                // sudah terjadi.
                //
                // Nilai yang DIKIRIM pemanggil tetap dihormati apa adanya — penurunan ini
                // mengisi yang kosong, bukan menimpa yang terisi. Katalog yang belum digolongkan
                // tetap menghasilkan pesanan tanpa disiplin, dan itu sah (AC-85).
                Discipline = request.Discipline ?? procedure.LabDiscipline,
                OrderStatus = LabOrderStatus.Requested,
                RequestedAt = now,
                RequestedByUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabOrders.Add(entity);

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Request",
                fromStatus: null,
                LabOrderStatus.Requested.ToString(),
                reasonCode: null,
                reasonNote: null,
                actorUserId,
                now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrder.Create",
                "Membuat order laboratorium.",
                new
                {
                    entity.Id,
                    entity.EncounterId,
                    entity.ProcedureId,
                    Discipline = entity.Discipline?.ToString(),
                    ActorUserId = actorUserId
                });

            return MapDetailResponse(entity, procedure, await ResolveUserNameAsync(entity.RequestedByUserId, cancellationToken));
        }

        /// <summary>
        /// Memesan beberapa pemeriksaan sekaligus, dipecah menjadi <b>satu pesanan per disiplin</b>
        /// (<c>LAB-DEC-055</c>, <c>LAB-DEC-056</c>).
        ///
        /// <b>Kenapa dipecah, bukan satu pesanan lintas disiplin.</b>
        /// <see cref="LabOrder.Discipline"/> adalah satu nilai dan terkunci setelah pesanan
        /// dibuat (<c>INV-21</c>); ketiga layar Pemeriksaan menyaring tepat atas kolom itu; dan
        /// <c>VAL-46</c> sudah menolak pemeriksaan yang disiplinnya tidak cocok. Memindahkan
        /// disiplin ke baris pemeriksaan berarti membongkar ketiganya sekaligus. Pemecahan
        /// mencapai hasil yang sama — setiap pemeriksaan muncul di menu yang benar — tanpa
        /// menyentuh satu pun invariant yang sudah berjalan.
        ///
        /// <b>Satu transaksi.</b> Seluruh pemeriksaan divalidasi <b>sebelum</b> satu baris pun
        /// ditambahkan, dan seluruh pesanan beserta permintaannya disimpan lewat satu
        /// <c>SaveChangesAsync</c>. Satu pemeriksaan ditolak berarti nol pesanan terbentuk —
        /// pemesanan yang gagal separuh akan meninggalkan pasien dengan satu disiplin terpesan
        /// dan satu disiplin hilang, dan hilangnya tidak terlihat sampai hasil yang ditunggu
        /// tidak pernah keluar.
        ///
        /// <b>Endpoint lama tidak disentuh.</b> <see cref="CreateAsync"/> tetap menerima satu
        /// procedure dan tetap mengembalikan satu pesanan (<c>AC-88</c>).
        /// </summary>
        public async Task<List<LabOrderDetailResponse>> CreateByExaminationsAsync(
            CreateLabOrderByExaminationsRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EncounterId == Guid.Empty)
                throw new ArgumentException("EncounterId wajib diisi.");

            var procedureIds = request.Examinations?
                .Where(x => x != Guid.Empty)
                .ToList() ?? new List<Guid>();

            // VAL-64.
            if (procedureIds.Count == 0)
                throw new LabOrderValidationException("Pilih sekurang-kurangnya satu pemeriksaan.");

            // VAL-65. Pesannya menyebut duplo karena tanpa itu petugas yang benar-benar perlu
            // mengerjakan satu pemeriksaan dua kali akan mencoba memilihnya dua kali, ditolak,
            // lalu tidak tahu harus berbuat apa.
            if (procedureIds.Count != procedureIds.Distinct().Count())
            {
                throw new LabOrderValidationException(
                    "Pemeriksaan yang sama tidak boleh dipilih dua kali. " +
                    "Untuk pengerjaan ganda, tandai duplo saat mencatat wadah.");
            }

            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EncounterId && !x.IsDelete, cancellationToken);

            if (encounter == null)
                throw new KeyNotFoundException("Encounter tidak ditemukan.");

            // VAL-67.
            if (encounter.EncounterStatus is EncounterStatus.Completed
                or EncounterStatus.Cancelled
                or EncounterStatus.NoShow)
            {
                throw new LabOrderValidationException(
                    "Kunjungan ini sudah selesai, pemeriksaan baru tidak dapat dipesankan.");
            }

            // BE-RWI-052, VAL-DOK-22 — penjagaan yang sama persis dengan CreateAsync.
            if (request.InpEpisodeId.HasValue && request.InpEpisodeId.Value != Guid.Empty)
            {
                var episodeCocok = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.InpEpisodeId.Value
                                   && x.EncounterId == request.EncounterId
                                   && !x.IsDelete,
                              cancellationToken);

                if (!episodeCocok)
                    throw new ArgumentException("Pesanan ini tidak cocok dengan perawatan pasien.");
            }

            var procedures = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x =>
                    procedureIds.Contains(x.Id) &&
                    x.IsLaboratory &&
                    x.IsActive &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            // VAL-66.
            if (procedures.Count != procedureIds.Count)
            {
                throw new LabOrderValidationException(
                    "Ada pemeriksaan yang tidak dapat dipesan. Periksa kembali pilihan Anda.");
            }

            var cito = new HashSet<Guid>(request.CitoExaminations ?? new List<Guid>());
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // Urutan pilihan pemanggil dipertahankan di dalam setiap kelompok, supaya pemeriksaan
            // pertama yang menjadi penunjuk wakil pesanan bukan hasil pengurutan yang sewenang.
            var berurutan = procedureIds
                .Select(id => procedures.First(p => p.Id == id))
                .ToList();

            // Kelompok tanpa disiplin diletakkan paling akhir. Ia tetap dibentuk — AC-85
            // menyatakan pemeriksaan yang belum digolongkan tetap sah dipesan, dan pesanannya
            // memang tidak akan muncul di ketiga layar Pemeriksaan sampai katalognya dirawat.
            var kelompok = berurutan
                .GroupBy(x => x.LabDiscipline)
                .OrderBy(g => g.Key.HasValue ? (int)g.Key.Value : int.MaxValue)
                .ToList();

            var terbentuk = new List<(LabOrder Order, MstProcedure Wakil)>();

            foreach (var g in kelompok)
            {
                var wakil = g.First();

                var entity = new LabOrder
                {
                    EncounterId = request.EncounterId,
                    InpEpisodeId = request.InpEpisodeId,
                    // Penunjuk wakil, bukan satu-satunya pemeriksaan pesanan ini. Kolomnya tidak
                    // dapat dikosongkan tanpa mengubah endpoint lama, dan pembaca yang sudah ada
                    // tetap memperoleh nilai yang masuk akal.
                    ProcedureId = wakil.Id,
                    Discipline = g.Key,
                    OrderStatus = LabOrderStatus.Requested,
                    RequestedAt = now,
                    RequestedByUserId = actorUserId,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.LabOrders.Add(entity);

                _labSpecimenService.AppendHistory(
                    entity,
                    specimen: null,
                    LabTransitionScope.LabOrder,
                    "Order.Request",
                    fromStatus: null,
                    LabOrderStatus.Requested.ToString(),
                    reasonCode: null,
                    reasonNote: null,
                    actorUserId,
                    now);

                foreach (var procedure in g)
                {
                    _dbContext.LabOrderedProcedures.Add(new LabOrderedProcedure
                    {
                        LabOrderId = entity.Id,
                        ProcedureId = procedure.Id,
                        ProcedureCodeSnapshot = procedure.ProcedureCode,
                        ProcedureNameSnapshot = procedure.ProcedureName,
                        DisciplineSnapshot = procedure.LabDiscipline,
                        Urgency = cito.Contains(procedure.Id)
                            ? LabExaminationUrgency.Cito
                            : LabExaminationUrgency.Routine,
                        OrderedStatus = LabOrderedProcedureStatus.Ordered,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                }

                terbentuk.Add((entity, wakil));
            }

            // Satu penyimpanan untuk seluruhnya. EF membungkusnya dalam satu transaksi, sehingga
            // kegagalan di tengah tidak meninggalkan sebagian pesanan.
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrder.CreateByExaminations",
                "Membuat pesanan laboratorium dari daftar pemeriksaan.",
                new
                {
                    request.EncounterId,
                    ExaminationCount = berurutan.Count,
                    OrderCount = terbentuk.Count,
                    Disciplines = terbentuk.Select(x => x.Order.Discipline?.ToString() ?? "TanpaDisiplin").ToList(),
                    ActorUserId = actorUserId
                });

            var namaPemesan = await ResolveUserNameAsync(actorUserId, cancellationToken);

            return terbentuk
                .Select(x => MapDetailResponse(x.Order, x.Wakil, namaPemesan))
                .ToList();
        }

        /// <summary>
        /// Mengonfirmasi pesanan laboratorium beserta dokter pemeriksanya
        /// (<c>LAB-DEC-061</c>, <c>LAB-STATE-v1</c> <c>r3</c> bagian 1a, <c>AC-94</c>,
        /// <c>AC-95</c>).
        ///
        /// <para>
        /// <b>Konfirmator dan waktunya diturunkan di sini, tidak pernah dari badan permintaan.</b>
        /// Nama konfirmator adalah pertanyaan audit: siapa yang menyatakan pesanan ini siap
        /// dikerjakan. Ruas yang boleh dikirim pemanggil adalah ruas yang boleh dipalsukan
        /// pemanggil, dan ini bukan salah satunya.
        /// </para>
        ///
        /// <para>
        /// <b>Jalur lama tidak disentuh.</b> Konfirmasi tidak diwajibkan: pesanan yang tidak
        /// pernah dikonfirmasi tetap berpindah <c>Requested</c> ke <c>Accepted</c> ketika wadah
        /// pertamanya dinyatakan layak. Mewajibkannya akan menghentikan seluruh pesanan yang
        /// sedang berjalan, dan keputusan itu belum diambil (<c>LAB-OPEN-027</c>).
        /// </para>
        /// </summary>
        public async Task<LabOrderDetailResponse> ConfirmAsync(
            Guid id,
            ConfirmLabOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            // VAL-70 diperiksa lebih dulu daripada VAL-71, dan urutannya bermakna. Pesanan yang
            // sudah dikonfirmasi lalu berpindah ke Accepted memenuhi kedua syarat sekaligus;
            // yang benar dibaca petugas adalah "sudah dikonfirmasi", bukan "sudah melewati
            // tahap konfirmasi". Jejaknya dibaca dari ConfirmedAt, bukan dari statusnya saja,
            // supaya pesanan yang sudah melaju tetap terbaca pernah dikonfirmasi.
            if (entity.OrderStatus == LabOrderStatus.Confirmed || entity.ConfirmedAt != null)
                throw new LabOrderConflictException("Pesanan ini sudah dikonfirmasi.");

            // VAL-71.
            if (entity.OrderStatus != LabOrderStatus.Requested)
                throw new LabOrderConflictException("Pesanan ini sudah melewati tahap konfirmasi.");

            // VAL-72.
            if (request == null || request.ExaminerDoctorId == Guid.Empty)
                throw new LabOrderValidationException("Pilih dokter pemeriksa terlebih dahulu.");

            // VAL-73. Dokter yang sudah dihapus maupun yang sudah tidak aktif sama-sama ditolak:
            // keduanya berarti tidak ada orang yang dapat dimintai pertanggungjawaban atas
            // pemeriksaan ini.
            var examinerIsSelectable = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.ExaminerDoctorId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (!examinerIsSelectable)
                throw new LabOrderValidationException("Dokter pemeriksa tidak ditemukan atau tidak aktif.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = entity.OrderStatus;

            entity.OrderStatus = LabOrderStatus.Confirmed;
            entity.ConfirmedByUserId = actorUserId;
            entity.ConfirmedAt = now;
            entity.ExaminerDoctorId = request.ExaminerDoctorId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Confirm",
                fromStatus.ToString(),
                LabOrderStatus.Confirmed.ToString(),
                reasonCode: null,
                reasonNote: null,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrder.Confirm",
                "Mengonfirmasi pesanan laboratorium.",
                new
                {
                    entity.Id,
                    entity.EncounterId,
                    entity.ExaminerDoctorId,
                    ActorUserId = actorUserId
                });

            return await GetDetailOrThrowAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Menandai pesanan mulai dikerjakan. Tidak menerbitkan fakta apa pun; tagihan sudah
        /// terbentuk pada saat sampel dinyatakan layak, bukan di sini.
        /// </summary>
        public Task<LabOrderDetailResponse> StartProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            MoveOrderStatusAsync(
                id,
                new[] { LabOrderStatus.Accepted },
                LabOrderStatus.InProcess,
                "Order.StartProcess",
                note: null,
                cancellationToken);

        public Task<LabOrderDetailResponse> CompleteAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            MoveOrderStatusAsync(
                id,
                new[] { LabOrderStatus.InProcess },
                LabOrderStatus.Completed,
                "Order.Complete",
                note: null,
                cancellationToken);

        public async Task<LabOrderDetailResponse> HoldAsync(
            Guid id,
            HoldLabRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            if (entity.OrderStatus == LabOrderStatus.OnHold)
                throw new InvalidOperationException("Pesanan laboratorium sudah ditahan.");

            if (entity.OrderStatus is LabOrderStatus.Cancelled or LabOrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    $"Pesanan laboratorium berstatus {entity.OrderStatus} tidak dapat ditahan.");
            }

            var reason = request.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Alasan penahanan wajib diisi.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = entity.OrderStatus;

            entity.StatusBeforeHold = fromStatus;
            entity.OrderStatus = LabOrderStatus.OnHold;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Hold",
                fromStatus.ToString(),
                LabOrderStatus.OnHold.ToString(),
                reasonCode: null,
                reasonNote: reason,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return await GetDetailOrThrowAsync(entity.Id, cancellationToken);
        }

        public async Task<LabOrderDetailResponse> ResumeAsync(
            Guid id,
            ResumeLabRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            if (entity.OrderStatus != LabOrderStatus.OnHold)
                throw new InvalidOperationException("Pesanan laboratorium tidak sedang ditahan.");

            var resumeTo = entity.StatusBeforeHold ?? LabOrderStatus.Requested;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.OrderStatus = resumeTo;
            entity.StatusBeforeHold = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Resume",
                LabOrderStatus.OnHold.ToString(),
                resumeTo.ToString(),
                reasonCode: null,
                reasonNote: string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return await GetDetailOrThrowAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Membatalkan pesanan laboratorium beserta seluruh sampel yang masih berjalan.
        ///
        /// Pembatalan klinis bukan pembatalan finansial. Untuk setiap sampel yang sebelumnya
        /// sudah dinyatakan layak, diterbitkan fakta pembatalan sebagai revisi baru atas fakta
        /// yang sama, sehingga tagihan lama tetap utuh dan Billing yang menentukan koreksinya.
        /// Sampel yang belum pernah layak tidak menghasilkan koreksi apa pun karena tagihannya
        /// memang belum pernah terbentuk.
        ///
        /// <para>
        /// <b>Dua aturan ditambahkan <c>BE-LAB-32</c> (<c>LAB-DEC-063</c>, <c>LAB-VAL-v1</c>
        /// <c>r6</c>).</b> Alasan pembatalan kini <b>wajib</b> (<c>VAL-74</c>), dan pembatalan
        /// hanya sah pada <c>Requested</c> serta <c>Confirmed</c> (<c>VAL-75</c>). Alasannya
        /// tetap disimpan sebagai <c>ReasonNote</c> pada jejak audit; nol kolom baru dibutuhkan.
        /// </para>
        ///
        /// <para>
        /// <b>Sampel yang sudah layak tidak akan pernah lagi sampai ke sini.</b> Penerbitan
        /// fakta pembatalan di bawah karena itu menjadi jalur yang secara praktis tidak lagi
        /// tercapai lewat pembatalan pesanan — ia sengaja <b>tidak dibongkar</b>, karena
        /// pembatalan pada tingkat wadah tetap memakainya dan aturan koreksi pesanan yang sudah
        /// berjalan belum diputuskan (<c>LAB-P0-003</c>).
        /// </para>
        /// </summary>
        public async Task<LabOrderCancellationResult> CancelAsync(
            Guid id,
            CancelLabOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            // VAL-75 — satu-satunya pengetatan pada amandemen r12, dan ia disengaja.
            //
            // Pembatalan kini hanya sah pada Requested dan Confirmed. Pesanan yang wadahnya
            // sudah dinyatakan layak berarti bahan pasien sudah diambil dan pekerjaan sudah
            // dimulai; membatalkannya bukan lagi pembatalan melainkan koreksi — dan aturan
            // koreksi belum diputuskan (LAB-P0-003).
            //
            // Satu penjaga digantikan tiga: baris ini sekaligus menutup Cancelled, Completed,
            // Accepted, InProcess, OnHold, Draft, dan CancelRequested, persis seperti yang
            // ditagih T-97a. IsCancel ikut diperiksa supaya pesanan yang sudah ditandai batal
            // tanpa sempat berpindah status tidak dapat dibatalkan dua kali.
            if (entity.IsCancel ||
                entity.OrderStatus is not (LabOrderStatus.Requested or LabOrderStatus.Confirmed))
            {
                throw new LabOrderConflictException("Pesanan yang sudah diproses tidak dapat dibatalkan.");
            }

            // VAL-74. Diperiksa sesudah VAL-75, mengikuti urutan yang sama dengan ConfirmAsync:
            // keadaan pesanan lebih dulu, isi permintaan sesudahnya. Pesanan yang memang tidak
            // boleh dibatalkan tidak perlu diminta alasannya lebih dulu.
            var reason = request?.CancelReason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                throw new LabOrderValidationException("Alasan pembatalan wajib diisi.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = entity.OrderStatus;

            var previouslyAccepted = await _labSpecimenService.CancelAllForOrderInMemoryAsync(
                entity,
                reason,
                actorUserId,
                now,
                cancellationToken);

            // Status klinis dan status pemenuhan saja. Tidak ada status pembayaran yang
            // disentuh dari sini — sejalan dengan keputusan author 1B pada RJ-BIL-BE-002.
            entity.OrderStatus = LabOrderStatus.Cancelled;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Cancel",
                fromStatus.ToString(),
                LabOrderStatus.Cancelled.ToString(),
                reasonCode: null,
                reasonNote: reason,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrder.Cancel",
                "Membatalkan order laboratorium.",
                new
                {
                    entity.Id,
                    entity.EncounterId,
                    PreviouslyAcceptedSpecimens = previouslyAccepted.Count,
                    ActorUserId = actorUserId
                });

            // Penyerahan ke Billing dilakukan setelah perubahan klinis tersimpan. Billing yang
            // tidak dapat dihubungi tidak boleh membatalkan pembatalan klinis yang sudah sah.
            var handoffs = new List<LabBillingHandoffResponse>();

            foreach (var specimen in previouslyAccepted)
            {
                var emission = await _labSpecimenService.EmitClinicalCancellationAsync(
                    specimen,
                    entity,
                    actorUserId,
                    cancellationToken);

                handoffs.Add(MapHandoff(emission));
            }

            var detail = await GetDetailOrThrowAsync(entity.Id, cancellationToken);

            return new LabOrderCancellationResult(detail, handoffs);
        }

        public static LabBillingHandoffResponse MapHandoff(ClinicalFactEmissionResult emission) =>
            new()
            {
                Kind = emission.Kind.ToString(),
                IsClinicallySafe = emission.IsClinicallySafe,
                MilestoneFactId = emission.MilestoneFactId,
                MilestoneFactVersion = emission.MilestoneFactVersion,
                Code = emission.Code,
                Message = emission.Message,
                MilestoneFactIds = emission.MilestoneFactId.HasValue
                    ? new List<Guid> { emission.MilestoneFactId.Value }
                    : new List<Guid>()
            };

        /// <summary>
        /// Bentuk jawaban untuk keputusan yang menerbitkan fakta <b>per pemeriksaan</b>
        /// (<c>FR-05.1</c>).
        ///
        /// <c>MilestoneFactId</c> tetap diisi identitas fakta pertama supaya pemanggil lama
        /// tidak putus, sementara <c>MilestoneFactIds</c> membawa seluruhnya. Satu wadah berisi
        /// tiga pemeriksaan menerbitkan tiga fakta, dan satu ruas tidak dapat mewakili
        /// ketiganya.
        /// </summary>
        public static LabBillingHandoffResponse MapHandoff(LabFactEmission emission)
        {
            var response = MapHandoff(emission.Perwakilan);

            response.MilestoneFactIds = emission.FactIds.ToList();
            response.MilestoneFactCount = emission.Count;

            return response;
        }

        private async Task<LabOrderDetailResponse> MoveOrderStatusAsync(
            Guid id,
            LabOrderStatus[] allowedFrom,
            LabOrderStatus target,
            string action,
            string? note,
            CancellationToken cancellationToken)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            if (Array.IndexOf(allowedFrom, entity.OrderStatus) < 0)
            {
                throw new InvalidOperationException(
                    $"Pesanan berstatus {entity.OrderStatus} tidak dapat dipindahkan ke {target}.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = entity.OrderStatus;

            entity.OrderStatus = target;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            if (target == LabOrderStatus.Completed)
                entity.CompletedAt = now;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                action,
                fromStatus.ToString(),
                target.ToString(),
                reasonCode: null,
                reasonNote: note,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return await GetDetailOrThrowAsync(entity.Id, cancellationToken);
        }

        private async Task<LabOrder> LoadTrackedAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.LabOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Order laboratorium tidak ditemukan.");

            return entity;
        }

        private async Task<LabOrderDetailResponse> GetDetailOrThrowAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var detail = await GetDetailAsync(id, cancellationToken);

            if (detail == null)
                throw new KeyNotFoundException("Order laboratorium tidak ditemukan.");

            return detail;
        }

        private async Task SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new LabConcurrencyException(
                    "Data laboratorium sudah diubah oleh petugas lain. Muat ulang lalu ulangi tindakan Anda.");
            }
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Nama pengguna untuk ditampilkan. Sumbernya sama dengan yang dipakai Master Data,
        /// sehingga satu orang tidak terbaca dengan dua nama berbeda antar layar.
        /// </summary>
        private async Task<string?> ResolveUserNameAsync(
            Guid? userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == null || userId == Guid.Empty)
            {
                return null;
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId.Value)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static LabOrderDetailResponse MapDetailResponse(
            LabOrder entity,
            MstProcedure? procedure,
            string? requestedByName = null)
        {
            return new LabOrderDetailResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = procedure?.ProcedureCode ?? string.Empty,
                ProcedureName = procedure?.ProcedureName ?? string.Empty,
                OrderStatus = entity.OrderStatus.ToString(),
                SpecimenCount = 0,
                AcceptedSpecimenCount = 0,
                IsCancel = entity.IsCancel,
                CreateDateTime = entity.CreateDateTime,
                Discipline = entity.Discipline?.ToString(),
                RequestedAt = entity.RequestedAt,
                RequestedByUserId = entity.RequestedByUserId,
                RequestedByName = requestedByName,
                CompletedAt = entity.CompletedAt,
                StatusBeforeHold = entity.StatusBeforeHold?.ToString(),
                Version = entity.Version,
                CancelDateTime = entity.CancelDateTime,
                CancelBy = entity.CancelBy == Guid.Empty ? null : entity.CancelBy,

                // LAB-API-v1 r13. Penunjuknya dipetakan dari entity; kedua namanya sengaja
                // dibiarkan kosong, dan itu benar untuk pemanggil mapper ini. Ia hanya dipakai
                // TEPAT SESUDAH pesanan dibuat — CreateAsync dan CreateByExaminationsAsync —
                // dan pesanan yang baru lahir belum mungkin dikonfirmasi. Menerjemahkan dua
                // penunjuk yang pasti kosong berarti dua perjalanan ke database untuk menghasilkan
                // null. Jalur yang mengembalikan pesanan terkonfirmasi adalah GetDetailAsync,
                // dan di sana kedua namanya memang diisi.
                ConfirmedAt = entity.ConfirmedAt,
                ConfirmedByUserId = entity.ConfirmedByUserId,
                ExaminerDoctorId = entity.ExaminerDoctorId
            };
        }
    }

    /// <summary>
    /// Hasil pembatalan pesanan beserta ringkasan penyerahan fakta pembatalan untuk setiap
    /// sampel yang sebelumnya sudah dinyatakan layak.
    /// </summary>
    public sealed record LabOrderCancellationResult(
        LabOrderDetailResponse Order,
        List<LabBillingHandoffResponse> BillingHandoffs);

    /// <summary>
    /// Pelanggaran aturan isi permintaan pemesanan. Dipetakan menjadi <c>422</c>.
    ///
    /// Dibedakan dari <see cref="ArgumentException"/> yang tetap menjadi <c>400</c>, mengikuti
    /// pembagian yang sudah dipakai <c>LabSpecimenService</c>: <c>400</c> berarti permintaannya
    /// cacat bentuk, <c>422</c> berarti bentuknya benar tetapi isinya melanggar aturan bisnis.
    /// Matriks validasi menetapkan kode yang berbeda untuk aturan yang berbeda, dan layar
    /// membedakan keduanya.
    /// </summary>
    public sealed class LabOrderValidationException(string message) : Exception(message);

    /// <summary>
    /// Tindakan yang bertabrakan dengan keadaan pesanan saat ini. Dipetakan menjadi <c>409</c>.
    ///
    /// Dibedakan dari <see cref="LabOrderValidationException"/> yang menjadi <c>422</c>:
    /// <c>422</c> berarti isi permintaannya yang salah dan pemanggil dapat memperbaikinya,
    /// sedangkan <c>409</c> berarti permintaannya benar tetapi pesanannya sudah tidak berada
    /// pada keadaan yang menerimanya — memperbaiki isian tidak akan menolong. Matriks validasi
    /// menetapkan kode yang berbeda untuk keduanya, dan layar membedakannya.
    /// </summary>
    public sealed class LabOrderConflictException(string message) : Exception(message);
}
