using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Siklus hidup pesanan radiologi sesuai <c>RJ-BIL-GATE-DEC-004</c>.
    ///
    /// Pesanan tidak pernah menerbitkan fakta kelayakan tagih. `Requested`, `Accepted`, dan
    /// `Scheduled` secara tegas **bukan** pemicu tagihan; yang menerbitkan hanyalah study yang
    /// acquisition-nya benar-benar dikerjakan dan menghasilkan citra yang dapat dipakai.
    ///
    /// Pembatalan sebelum acquisition dimulai karena itu tidak menerbitkan koreksi apa pun:
    /// tidak ada yang perlu dikoreksi karena tidak pernah ada yang tertagih.
    /// </summary>
    public class RadOrderService
    {
        private const string LogCategory = "HealthServices.RadiologyManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;
        private readonly RadOrderNumberService _orderNumberService;

        public RadOrderService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService,
            RadOrderNumberService orderNumberService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
            _orderNumberService = orderNumberService;
        }

        /* ================================================================ *
         * Metadata penyaring dan ringkasan
         * ================================================================ */

        /// <summary>
        /// Pilihan penyaring, pengurutan, dan parameter query untuk layar daftar pesanan.
        ///
        /// Tidak menyentuh database sama sekali: seluruh isinya berasal dari enum yang sudah
        /// dikunci kontrak. Layar memakainya supaya tidak perlu menuliskan ulang daftar status
        /// di sisi klien — daftar yang disalin adalah daftar yang cepat atau lambat berbeda.
        /// </summary>
        public RadOrderFilterMetadataResponse GetFilterMetadata() => new()
        {
            OrderStatuses = Enum.GetValues<RadOrderStatus>()
                .Select(x => new RadEnumOptionResponse
                {
                    Value = (int)x,
                    Name = x.ToString(),
                    Label = LabelStatusPesanan(x),
                })
                .ToList(),

            SortOptions =
            [
                new() { Value = "createDateTime", Label = "Tanggal pesanan dibuat" },
                new() { Value = "orderStatus", Label = "Status pesanan" },
                new() { Value = "orderNumber", Label = "Nomor pesanan" },
                new() { Value = "scheduledAt", Label = "Jadwal pemeriksaan" },
            ],

            SortDirections = ["asc", "desc"],

            // RAD-CONF-001 bagian 8 butir 3. Daftar ini wajib menyebut persis parameter yang
            // benar-benar diproses GetListAsync. Metadata yang menjanjikan penyaring yang tidak
            // dilayani adalah cacat kontrak, dan layar yang mempercayainya akan menampilkan
            // hasil yang tidak tersaring tanpa memberi tahu siapa pun.
            QueryParameters =
            [
                new()
                {
                    Name = "encounterId",
                    Type = "guid",
                    Required = "No",
                    Description = "Menyaring pesanan milik satu kunjungan pasien.",
                },
                new()
                {
                    Name = "patientId",
                    Type = "guid",
                    Required = "No",
                    Description = "Menyaring pesanan milik satu pasien, lintas kunjungan.",
                },
                new()
                {
                    Name = "medicalRecordNumber",
                    Type = "string",
                    Required = "No",
                    Description = "Nomor rekam medis, cocok sebagian.",
                },
                new()
                {
                    Name = "encounterNumber",
                    Type = "string",
                    Required = "No",
                    Description = "Nomor registrasi kunjungan, cocok sebagian.",
                },
                new()
                {
                    Name = "orderNumber",
                    Type = "string",
                    Required = "No",
                    Description = "Nomor pesanan radiologi, cocok sebagian.",
                    Example = "RAD-ORD-260911074012-A1B2C3",
                },
                new()
                {
                    Name = "studyNumber",
                    Type = "string",
                    Required = "No",
                    Description =
                        "Nomor foto/study. Pesanan ikut tersaring bila salah satu study-nya " +
                        "bernomor itu.",
                },
                new()
                {
                    Name = "startDate",
                    Type = "date",
                    Required = "No",
                    Description = "Awal periode, dihitung dari waktu pemesanan.",
                    Example = "2026-09-01",
                },
                new()
                {
                    Name = "endDate",
                    Type = "date",
                    Required = "No",
                    Description = "Akhir periode.",
                    Example = "2026-09-30",
                },
                new()
                {
                    Name = "encounterType",
                    Type = "int",
                    Required = "No",
                    Description =
                        "Jenis kunjungan: rawat jalan, rawat inap, atau gawat darurat. " +
                        "Inilah yang memisahkan kategori daftar pasien radiologi.",
                },
                new()
                {
                    Name = "serviceUnitId",
                    Type = "guid",
                    Required = "No",
                    Description = "Unit layanan asal pesanan.",
                },
                new()
                {
                    Name = "roomId",
                    Type = "guid",
                    Required = "No",
                    Description = "Ruangan asal pesanan.",
                },
                new()
                {
                    Name = "modalityId",
                    Type = "guid",
                    Required = "No",
                    Description = "Alat pencitraan yang diminta.",
                },
                new()
                {
                    Name = "procedureId",
                    Type = "guid",
                    Required = "No",
                    Description = "Jenis pemeriksaan yang diminta.",
                },
                new()
                {
                    Name = "orderStatus",
                    Type = "int",
                    Required = "No",
                    Description = "Status pesanan. Nilainya diambil dari OrderStatuses.",
                },
                new()
                {
                    Name = "onlyUrgent",
                    Type = "bool",
                    Required = "No",
                    Description = "Menyaring pesanan bertanda cito saja.",
                },
                new()
                {
                    Name = "onlyScheduled",
                    Type = "bool",
                    Required = "No",
                    Description =
                        "Menyaring pesanan yang sudah punya tanggal pemeriksaan. Inilah yang " +
                        "membentuk daftar Pasien Perjanjian.",
                },
                new()
                {
                    Name = "scheduledFrom",
                    Type = "date",
                    Required = "No",
                    Description = "Awal periode jadwal pemeriksaan.",
                },
                new()
                {
                    Name = "scheduledTo",
                    Type = "date",
                    Required = "No",
                    Description = "Akhir periode jadwal pemeriksaan.",
                },
                new()
                {
                    Name = "search",
                    Type = "string",
                    Required = "No",
                    Description =
                        "Pencarian bebas pada nama pasien, nomor rekam medis, nomor kunjungan, " +
                        "dan nomor pesanan.",
                },
                new()
                {
                    Name = "limit",
                    Type = "int",
                    Required = "No",
                    Description =
                        "Batas jumlah baris. Bawaannya 200, paling banyak 1000. Daftar ini " +
                        "belum memakai PagedResult, sehingga batas inilah yang menjaga " +
                        "pencarian se-rumah-sakit tetap terbatas.",
                    Example = "200",
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
            ],
        };

        /// <summary>
        /// Rekap jumlah pesanan radiologi menurut statusnya.
        ///
        /// Dihitung di database dengan satu perjalanan, bukan dengan menarik seluruh baris lalu
        /// menghitungnya di memori.
        /// </summary>
        public async Task<RadOrderSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rekap = await _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Diminta = g.Count(x => x.OrderStatus == RadOrderStatus.Requested),
                    Diterima = g.Count(x => x.OrderStatus == RadOrderStatus.Accepted),
                    Dijadwalkan = g.Count(x => x.OrderStatus == RadOrderStatus.Scheduled),
                    SedangDikerjakan = g.Count(x => x.OrderStatus == RadOrderStatus.InProgress),
                    Selesai = g.Count(x => x.OrderStatus == RadOrderStatus.Completed),
                    Ditahan = g.Count(x => x.OrderStatus == RadOrderStatus.OnHold),
                    Dibatalkan = g.Count(x => x.OrderStatus == RadOrderStatus.Cancelled),
                    Ditolak = g.Count(x => x.OrderStatus == RadOrderStatus.Rejected),
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new RadOrderSummaryResponse
            {
                TotalPesanan = rekap?.Total ?? 0,
                Diminta = rekap?.Diminta ?? 0,
                Diterima = rekap?.Diterima ?? 0,
                Dijadwalkan = rekap?.Dijadwalkan ?? 0,
                SedangDikerjakan = rekap?.SedangDikerjakan ?? 0,
                Selesai = rekap?.Selesai ?? 0,
                Ditahan = rekap?.Ditahan ?? 0,
                Dibatalkan = rekap?.Dibatalkan ?? 0,
                Ditolak = rekap?.Ditolak ?? 0,
                BelumDikerjakan =
                    (rekap?.Diminta ?? 0) + (rekap?.Diterima ?? 0) + (rekap?.Dijadwalkan ?? 0),
            };
        }

        private static string LabelStatusPesanan(RadOrderStatus status) => status switch
        {
            RadOrderStatus.Draft => "Draf — tidak dipakai",
            RadOrderStatus.Requested => "Diminta dokter",
            RadOrderStatus.Accepted => "Diterima Radiologi",
            RadOrderStatus.Scheduled => "Dijadwalkan",
            RadOrderStatus.InProgress => "Sedang dikerjakan",
            RadOrderStatus.Completed => "Selesai dikerjakan",
            RadOrderStatus.OnHold => "Ditahan sementara",
            RadOrderStatus.CancelRequested => "Menunggu pembatalan",
            RadOrderStatus.Cancelled => "Dibatalkan",
            _ => "Ditolak Radiologi",
        };

        /* ================================================================ *
         * Pembacaan
         * ================================================================ */

        /// <summary>
        /// Daftar pesanan radiologi beserta penyaringnya.
        /// </summary>
        /// <remarks>
        /// <c>RAD-CONF-001</c> bagian 8 butir 3. Lima kategori daftar pasien radiologi dan
        /// delapan kriteria pencarian riwayat seluruhnya dilayani dari satu endpoint ini —
        /// bukan dari lima endpoint yang isinya hampir sama.
        /// </remarks>
        public async Task<List<RadOrderListResponse>> GetListAsync(
            RadOrderListQuery? query = null,
            CancellationToken cancellationToken = default)
        {
            query ??= new RadOrderListQuery();

            var source = _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            source = TerapkanPenyaring(source, query);

            return await ProyeksikanDaftarAsync(
                Urutkan(source, query.SortBy, query.SortDirection),
                query.BatasBaris(),
                cancellationToken);
        }

        /// <summary>
        /// Menerapkan seluruh penyaring daftar pesanan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Ditulis satu kali dan dipakai satu pintu, mengikuti <c>LabMonitoringService</c> yang
        /// sudah berjalan. Penyaring yang disalin ke beberapa tempat lambat laun menyimpang satu
        /// sama lain, dan penyimpangannya baru ketahuan ketika dua layar menampilkan jumlah
        /// pasien yang berbeda untuk pertanyaan yang sama.
        /// </para>
        /// <para>
        /// <b>Seluruh penyaring lintas modul dikerjakan sebagai subquery ke tabel pemiliknya,
        /// bukan lewat kolom salinan.</b> Radiologi tidak menyimpan nama pasien maupun nomor
        /// rekam medis, dan tidak boleh mulai menyimpannya hanya demi penyaringan.
        /// </para>
        /// </remarks>
        private IQueryable<RadOrder> TerapkanPenyaring(
            IQueryable<RadOrder> source,
            RadOrderListQuery query)
        {
            // Penyaring kunjungan disediakan sejak awal. Modul Laboratorium tidak memilikinya,
            // dan akibatnya layar Billing terpaksa menyaring seluruh pesanan rumah sakit di
            // sisi klien — batas yang tidak perlu diulang di sini.
            if (query.EncounterId.HasValue && query.EncounterId.Value != Guid.Empty)
            {
                source = source.Where(x => x.EncounterId == query.EncounterId.Value);
            }

            if (query.PatientId.HasValue && query.PatientId.Value != Guid.Empty)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.PatientId == query.PatientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.MedicalRecordNumber))
            {
                var nomor = query.MedicalRecordNumber.Trim();

                source = source.Where(x =>
                    x.Encounter != null &&
                    _dbContext.MstPatients.Any(p =>
                        p.Id == x.Encounter.PatientId && p.MedicalRecordNumber.Contains(nomor)));
            }

            if (!string.IsNullOrWhiteSpace(query.EncounterNumber))
            {
                var nomor = query.EncounterNumber.Trim();

                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.EncounterNumber.Contains(nomor));
            }

            if (!string.IsNullOrWhiteSpace(query.OrderNumber))
            {
                var nomor = query.OrderNumber.Trim();
                source = source.Where(x => x.OrderNumber.Contains(nomor));
            }

            // Nomor foto tinggal pada study, bukan pesanan. Sebuah pesanan ikut tersaring bila
            // SALAH SATU study-nya bernomor itu — petugas yang memegang amplop citra hanya
            // memegang satu nomor, dan yang ia cari adalah pesanan yang memuatnya.
            if (!string.IsNullOrWhiteSpace(query.StudyNumber))
            {
                var nomor = query.StudyNumber.Trim();

                source = source.Where(x =>
                    x.Studies.Any(s => !s.IsDelete && s.StudyNumber.Contains(nomor)));
            }

            if (query.StartDate.HasValue)
            {
                var mulai = query.StartDate.Value;
                source = source.Where(x => (x.RequestedAt ?? x.CreateDateTime) >= mulai);
            }

            if (query.EndDate.HasValue)
            {
                var sampai = query.EndDate.Value;
                source = source.Where(x => (x.RequestedAt ?? x.CreateDateTime) <= sampai);
            }

            if (query.EncounterType.HasValue)
            {
                var jenis = (EncounterType)query.EncounterType.Value;

                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.EncounterType == jenis);
            }

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.ServiceUnitId == query.ServiceUnitId.Value);
            }

            if (query.RoomId.HasValue && query.RoomId.Value != Guid.Empty)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.RoomId == query.RoomId.Value);
            }

            if (query.ModalityId.HasValue && query.ModalityId.Value != Guid.Empty)
            {
                source = source.Where(x => x.ModalityId == query.ModalityId.Value);
            }

            if (query.ProcedureId.HasValue && query.ProcedureId.Value != Guid.Empty)
            {
                source = source.Where(x => x.ProcedureId == query.ProcedureId.Value);
            }

            if (query.OrderStatus.HasValue)
            {
                source = source.Where(x => x.OrderStatus == query.OrderStatus.Value);
            }

            if (query.OnlyUrgent == true)
            {
                source = source.Where(x => x.IsUrgent);
            }

            // Daftar Pasien Perjanjian: pesanan yang sudah punya tanggal pemeriksaan.
            if (query.OnlyScheduled == true)
            {
                source = source.Where(x => x.ScheduledAt != null);
            }

            if (query.ScheduledFrom.HasValue)
            {
                var mulai = query.ScheduledFrom.Value;
                source = source.Where(x => x.ScheduledAt != null && x.ScheduledAt >= mulai);
            }

            if (query.ScheduledTo.HasValue)
            {
                var sampai = query.ScheduledTo.Value;
                source = source.Where(x => x.ScheduledAt != null && x.ScheduledAt <= sampai);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.OrderNumber.Contains(search) ||
                    (x.Encounter != null &&
                     (x.Encounter.EncounterNumber.Contains(search) ||
                      _dbContext.MstPatients.Any(p =>
                          p.Id == x.Encounter.PatientId &&
                          (p.FullName.Contains(search) ||
                           p.MedicalRecordNumber.Contains(search))))));
            }

            return source;
        }

        /// <summary>
        /// Menerapkan pengurutan yang diminta layar.
        ///
        /// Hanya kolom yang benar-benar disebut <c>SortOptions</c> yang dilayani; nilai lain
        /// jatuh ke urutan bawaan, bukan ditolak. Metadata yang menjanjikan pengurutan yang
        /// tidak diproses daftar adalah cacat kontrak, sehingga keduanya sengaja ditulis
        /// berdampingan di berkas ini.
        /// </summary>
        private static IOrderedQueryable<RadOrder> Urutkan(
            IQueryable<RadOrder> query,
            string? sortBy,
            string? sortDirection)
        {
            var menaik = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "orderstatus" => menaik
                    ? query.OrderBy(x => x.OrderStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderByDescending(x => x.OrderStatus)
                        .ThenByDescending(x => x.CreateDateTime),

                "ordernumber" => menaik
                    ? query.OrderBy(x => x.OrderNumber)
                    : query.OrderByDescending(x => x.OrderNumber),

                // Pesanan tanpa jadwal ditaruh di belakang pada urutan menaik, supaya daftar
                // perjanjian tidak dibuka oleh sederet baris kosong.
                "scheduledat" => menaik
                    ? query.OrderBy(x => x.ScheduledAt == null)
                        .ThenBy(x => x.ScheduledAt)
                        .ThenByDescending(x => x.CreateDateTime)
                    : query.OrderByDescending(x => x.ScheduledAt)
                        .ThenByDescending(x => x.CreateDateTime),

                _ => menaik
                    ? query.OrderBy(x => x.CreateDateTime)
                    : query.OrderByDescending(x => x.CreateDateTime),
            };
        }

        /// <summary>
        /// Pesanan radiologi beserta ketersediaan hasilnya untuk satu perawatan rawat inap.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-052</c>, <c>INV-DOK-12</c>, <c>RUL-DOK-02</c>. Penyaringnya adalah penanda
        /// perawatan, bukan pasien, sehingga hasil perawatan lama tidak muncul pada layar
        /// perawatan yang sedang berjalan.
        /// </para>
        /// <para>
        /// <b>Nol tabel salinan.</b> Yang dibaca adalah baris pesanan dan study milik Radiologi
        /// apa adanya.
        /// </para>
        /// </remarks>
        /// <param name="episodeId">Perawatan yang pesanannya dibaca.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<List<RadOrderListResponse>> GetByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.InpEpisodeId == episodeId);

            return await ProyeksikanDaftarAsync(
                query.OrderBy(x => x.CreateDateTime), null, cancellationToken);
        }

        /// <summary>
        /// Memproyeksikan pesanan menjadi baris daftar beserta penanda ketersediaan hasilnya.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-30</c>. Hasil dinyatakan final hanya ketika pesanannya selesai, tidak
        /// dibatalkan, dan memiliki sedikitnya satu study yang mutunya sudah diterima. Study
        /// yang belum lolos mutu bukan hasil sah, dan menampilkannya seolah-olah hasil adalah
        /// risiko keselamatan.
        /// </remarks>
        /// <param name="query">Pesanan yang sudah disaring dan diurutkan.</param>
        /// <param name="batasBaris">
        /// Batas jumlah baris. <c>null</c> berarti tanpa batas, dan itu hanya dipakai jalur yang
        /// dengan sendirinya sempit — daftar satu perawatan rawat inap. Jalur daftar umum selalu
        /// mengirim batas, karena pencarian riwayat adalah pertanyaan se-rumah-sakit.
        /// </param>
        /// <param name="cancellationToken">Token pembatalan.</param>
        private static async Task<List<RadOrderListResponse>> ProyeksikanDaftarAsync(
            IOrderedQueryable<RadOrder> query,
            int? batasBaris,
            CancellationToken cancellationToken)
        {
            // Pembatasan dilakukan SEBELUM proyeksi, supaya baris yang tidak akan dikembalikan
            // tidak ikut menggabungkan tabel pasien, ruangan, kelas, dan penjamin.
            IQueryable<RadOrder> terbatas = batasBaris.HasValue
                ? query.Take(batasBaris.Value)
                : query;

            var baris = await terbatas
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.EncounterId,
                    x.InpEpisodeId,
                    x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    x.ModalityId,
                    ModalityCode = x.Modality != null ? x.Modality.ModalityCode : string.Empty,
                    ModalityName = x.Modality != null ? x.Modality.ModalityName : string.Empty,
                    x.OrderStatus,
                    StudyCount = x.Studies.Count(s => !s.IsDelete),
                    UsableStudyCount = x.Studies.Count(s =>
                        !s.IsDelete && s.StudyStatus == RadStudyStatus.QualityAccepted),
                    x.IsCancel,
                    x.IsUrgent,
                    x.CreateDateTime,

                    // Konteks pasien — RAD-CONF-001 bagian 8 butir 1.
                    //
                    // Dibaca lewat navigasi kunjungan dalam SATU perjalanan ke database. Inilah
                    // yang menggantikan pola "tarik daftar lalu panggil registrasi sekali per
                    // baris" yang membuat daftar 50 pasien menjadi 51 permintaan.
                    //
                    // Enum sengaja dibawa apa adanya dan baru diubah menjadi teks setelah baris
                    // terwujud di memori. ToString() atas enum tidak dapat diterjemahkan menjadi
                    // SQL, dan memaksanya akan memindahkan seluruh penyaringan ke memori.
                    HasEncounter = x.Encounter != null,
                    PatientId = x.Encounter != null ? (Guid?)x.Encounter.PatientId : null,
                    EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                    EncounterType = x.Encounter != null
                        ? (EncounterType?)x.Encounter.EncounterType : null,
                    VisitType = x.Encounter != null
                        ? (VisitType?)x.Encounter.VisitType : null,
                    PaymentType = x.Encounter != null
                        ? (EncounterPaymentType?)x.Encounter.PaymentType : null,
                    AgeText = x.Encounter != null ? x.Encounter.AgeTextAtEncounter : null,
                    ServiceUnitId = x.Encounter != null ? (Guid?)x.Encounter.ServiceUnitId : null,
                    ServiceUnitName = x.Encounter != null && x.Encounter.ServiceUnit != null
                        ? x.Encounter.ServiceUnit.ServiceUnitName : null,
                    RoomId = x.Encounter != null ? x.Encounter.RoomId : null,
                    RoomName = x.Encounter != null && x.Encounter.Room != null
                        ? x.Encounter.Room.RoomName : null,
                    PatientClassId = x.Encounter != null ? x.Encounter.PatientClassId : null,
                    PatientClassName = x.Encounter != null && x.Encounter.PatientClass != null
                        ? x.Encounter.PatientClass.PatientClassName : null,
                    GuarantorName = x.Encounter != null && x.Encounter.PaymentSource != null
                        ? x.Encounter.PaymentSource.PaymentSourceNameSnapshot : null,
                    PatientName = x.Encounter != null && x.Encounter.Patient != null
                        ? x.Encounter.Patient.FullName : null,
                    MedicalRecordNumber = x.Encounter != null && x.Encounter.Patient != null
                        ? x.Encounter.Patient.MedicalRecordNumber : null,
                    Gender = x.Encounter != null && x.Encounter.Patient != null
                        ? x.Encounter.Patient.Gender : null,
                    BirthDate = x.Encounter != null && x.Encounter.Patient != null
                        ? x.Encounter.Patient.BirthDate : null,
                })
                .ToListAsync(cancellationToken);

            return baris.Select(x =>
            {
                var final = x.OrderStatus == RadOrderStatus.Completed
                            && !x.IsCancel
                            && x.UsableStudyCount > 0;

                return new RadOrderListResponse
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    EncounterId = x.EncounterId,
                    Patient = !x.HasEncounter ? null : new RadOrderPatientContextResponse
                    {
                        PatientId = x.PatientId,
                        MedicalRecordNumber = x.MedicalRecordNumber,
                        PatientName = x.PatientName,
                        Gender = x.Gender?.ToString(),
                        BirthDate = x.BirthDate,
                        AgeText = x.AgeText,
                        EncounterNumber = x.EncounterNumber,
                        EncounterType = x.EncounterType?.ToString(),
                        VisitType = x.VisitType?.ToString(),
                        ServiceUnitId = x.ServiceUnitId,
                        ServiceUnitName = x.ServiceUnitName,
                        RoomId = x.RoomId,
                        RoomName = x.RoomName,
                        PatientClassId = x.PatientClassId,
                        PatientClassName = x.PatientClassName,
                        PaymentType = x.PaymentType?.ToString(),
                        GuarantorName = x.GuarantorName,
                    },
                    InpEpisodeId = x.InpEpisodeId,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.ProcedureCode,
                    ProcedureName = x.ProcedureName,
                    ModalityId = x.ModalityId,
                    ModalityCode = x.ModalityCode,
                    ModalityName = x.ModalityName,
                    OrderStatus = x.OrderStatus.ToString(),
                    StudyCount = x.StudyCount,
                    UsableStudyCount = x.UsableStudyCount,
                    IsCancel = x.IsCancel,
                    IsUrgent = x.IsUrgent,
                    IsResultFinal = final,
                    ResultAvailabilityNote = x.IsCancel
                        ? "Pesanan dibatalkan; tidak ada hasil."
                        : final
                            ? "Hasil sudah final."
                            : "Hasil belum final. Jangan dipakai sebagai dasar keputusan klinis.",
                    CreateDateTime = x.CreateDateTime,
                };
            }).ToList();
        }

        public async Task<RadOrderDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.RadOrders
                .AsNoTracking()
                .Include(x => x.Procedure)
                .Include(x => x.Modality)
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                    .ThenInclude(s => s.SafetyChecks.Where(c => !c.IsDelete))
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                    .ThenInclude(s => s.Consumptions.Where(c => !c.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return null;
            }

            var detail = MapDetail(entity);

            detail.Patient = await BacaKonteksPasienAsync(entity.EncounterId, cancellationToken);

            // RAD-CONF-001 bagian 8 butir 4. Konfirmator dan waktunya dibaca dari riwayat, bukan
            // dari kolom pada pesanan. Yang dipakai adalah konfirmasi PERTAMA: pesanan yang
            // sempat ditahan lalu dilanjutkan tidak berganti konfirmator, karena yang ditanya
            // layar adalah siapa yang dahulu menerima pesanan ini ke radiologi.
            var konfirmasi = await _dbContext.RadTransitionHistories
                .AsNoTracking()
                .Where(x => x.RadOrderId == entity.Id
                            && !x.IsDelete
                            && x.Action == "Order.Accept")
                .OrderBy(x => x.OccurredAt)
                .Select(x => new { x.ActorUserId, x.OccurredAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (konfirmasi != null)
            {
                detail.ConfirmedByUserId = konfirmasi.ActorUserId;
                detail.ConfirmedAt = konfirmasi.OccurredAt;
            }

            return detail;
        }

        /// <summary>
        /// Membaca konteks pasien dan kunjungan untuk satu kunjungan.
        /// </summary>
        /// <remarks>
        /// <c>RAD-CONF-001</c> bagian 8 butir 1. Dipakai layar detail; daftar memakai proyeksi
        /// tersendiri supaya tidak menerbitkan satu permintaan per baris. Keduanya membaca
        /// kolom yang sama persis, sehingga layar detail dan layar daftar tidak pernah
        /// menampilkan identitas pasien yang berbeda untuk pesanan yang sama.
        /// </remarks>
        private async Task<RadOrderPatientContextResponse?> BacaKonteksPasienAsync(
            Guid encounterId,
            CancellationToken cancellationToken)
        {
            var konteks = await _dbContext.RegPatientEncounters
                .AsNoTracking()
                .Where(x => x.Id == encounterId && !x.IsDelete)
                .Select(x => new
                {
                    x.PatientId,
                    x.EncounterNumber,
                    x.EncounterType,
                    x.VisitType,
                    x.PaymentType,
                    x.AgeTextAtEncounter,
                    x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    x.RoomId,
                    RoomName = x.Room != null ? x.Room.RoomName : null,
                    x.PatientClassId,
                    PatientClassName = x.PatientClass != null
                        ? x.PatientClass.PatientClassName : null,
                    GuarantorName = x.PaymentSource != null
                        ? x.PaymentSource.PaymentSourceNameSnapshot : null,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    Gender = x.Patient != null ? x.Patient.Gender : null,
                    BirthDate = x.Patient != null ? x.Patient.BirthDate : null,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (konteks == null)
            {
                return null;
            }

            return new RadOrderPatientContextResponse
            {
                PatientId = konteks.PatientId,
                MedicalRecordNumber = konteks.MedicalRecordNumber,
                PatientName = konteks.PatientName,
                Gender = konteks.Gender?.ToString(),
                BirthDate = konteks.BirthDate,
                AgeText = konteks.AgeTextAtEncounter,
                EncounterNumber = konteks.EncounterNumber,
                EncounterType = konteks.EncounterType.ToString(),
                VisitType = konteks.VisitType.ToString(),
                ServiceUnitId = konteks.ServiceUnitId,
                ServiceUnitName = konteks.ServiceUnitName,
                RoomId = konteks.RoomId,
                RoomName = konteks.RoomName,
                PatientClassId = konteks.PatientClassId,
                PatientClassName = konteks.PatientClassName,
                PaymentType = konteks.PaymentType.ToString(),
                GuarantorName = konteks.GuarantorName,
            };
        }

        /* ================================================================ *
         * Daftar kerja petugas — RAD-DEC-012
         * ================================================================ */

        /// <summary>
        /// Daftar kerja petugas pada satu alat pencitraan.
        ///
        /// <para>
        /// <b>Tidak ada tabel daftar kerja, dan tidak boleh ada.</b> Seluruh isinya dihitung dari
        /// <c>RadOrder</c> dan <c>RadStudy</c> yang sudah ada. Akibatnya sebuah pesanan yang
        /// dibatalkan langsung hilang dari daftar begitu layar dibuka lagi — tanpa proses
        /// penyelarasan apa pun, karena memang tidak ada apa pun yang perlu diselaraskan
        /// (<c>FR-RAD-061</c>).
        /// </para>
        ///
        /// <para>
        /// <b>Alat wajib dipilih.</b> Daftar kerja tanpa alat bukan daftar kerja siapa pun: di
        /// radiologi, penempatan petugas mengikuti ruang alat, bukan mengikuti pasien
        /// (<c>RAD-DEC-012</c>). Mengembalikan seluruh pekerjaan rumah sakit ketika alatnya tidak
        /// disebut juga berarti memuat ribuan baris yang tidak seorang pun minta.
        /// </para>
        ///
        /// <para>
        /// <b>Pesanan cito berada di urutan atas</b> (<c>FR-RAD-062</c>), dan di antara sesama
        /// cito urutannya kembali mengikuti waktu — yang lebih dulu dipesan dikerjakan lebih
        /// dulu.
        /// </para>
        /// </summary>
        /// <param name="modalityId">Alat pencitraan. <b>Wajib.</b></param>
        /// <param name="date">
        /// Hari kerja yang diminta, dibaca sebagai tanggal kalender Waktu Indonesia Barat.
        /// Kosong berarti <b>hari ini</b>.
        /// </param>
        /// <param name="status">Menyaring satu keadaan pesanan saja. Kosong berarti seluruhnya.</param>
        /// <param name="cancellationToken">Token pembatalan.</param>
        public async Task<RadOperationResult<List<RadWorklistItemResponse>>> GetWorklistAsync(
            Guid? modalityId,
            DateTime? date,
            RadOrderStatus? status,
            CancellationToken cancellationToken = default)
        {
            if (!modalityId.HasValue || modalityId.Value == Guid.Empty)
            {
                return RadOperationResult<List<RadWorklistItemResponse>>.Validation(
                    RadErrorCodes.WorklistModalityRequired,
                    "Alat pencitraan wajib dipilih untuk membuka daftar kerja.");
            }

            var (awal, akhir) = RentangHariKerja(date);

            var query = _dbContext.RadOrders
                .AsNoTracking()
                .Include(x => x.Procedure)
                .Include(x => x.Modality)
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                .Where(x =>
                    !x.IsDelete &&
                    x.ModalityId == modalityId.Value &&
                    (x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime) >= awal &&
                    (x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime) < akhir);

            if (status.HasValue)
            {
                query = query.Where(x => x.OrderStatus == status.Value);
            }

            // Urutannya dikerjakan database, bukan di memori: index gabungan
            // ModalityId + IsUrgent + OrderStatus dibuat BE-RAD-12 justru untuk ini.
            var baris = await query
                .OrderByDescending(x => x.IsUrgent)
                .ThenBy(x => x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime)
                .ToListAsync(cancellationToken);

            return RadOperationResult<List<RadWorklistItemResponse>>.Success(
                baris.Select(MapWorklist).ToList());
        }

        /// <summary>
        /// Mengubah penanda cito setelah pesanan dibuat — <c>RAD-DEC-013</c>.
        ///
        /// <para>
        /// Perubahannya dicatat pada <c>RadTransitionHistory</c>, bukan hanya pada kolom
        /// pesanannya. Kolom hanya menyimpan keadaan <b>sekarang</b>; riwayat menyimpan
        /// <b>setiap</b> kali penanda dinyalakan dan dicabut beserta pelakunya — dan pertanyaan
        /// "siapa yang mencabut penanda cito pasien ini" sama pentingnya dengan "siapa yang
        /// memasangnya".
        /// </para>
        /// </summary>
        public async Task<RadOperationResult<RadOrderDetailResponse>> SetUrgencyAsync(
            Guid id,
            RadOrderUrgencyRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var entity = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound,
                    "Pesanan radiologi tidak ditemukan.");
            }

            // Pesanan yang sudah selesai, dibatalkan, atau ditolak tidak lagi mengantre di
            // daftar kerja mana pun. Mengubah penandanya tidak mendahulukan apa pun, dan hanya
            // meninggalkan jejak yang membingungkan pembacanya kelak.
            if (entity.OrderStatus is RadOrderStatus.Completed
                or RadOrderStatus.Cancelled
                or RadOrderStatus.Rejected)
            {
                return RadOperationResult<RadOrderDetailResponse>.Conflict(
                    RadErrorCodes.UrgencyNotChangeable,
                    $"Pesanan ini berstatus {LabelStatusPesanan(entity.OrderStatus).ToLowerInvariant()}, " +
                    "sehingga penanda citonya tidak dapat diubah lagi.");
            }

            if (entity.IsUrgent == request.IsUrgent)
            {
                // Bukan kegagalan. Menekan tombol yang sama dua kali menghasilkan keadaan yang
                // sama, dan permintaan yang tidak mengubah apa pun tidak perlu meninggalkan
                // jejak seolah ada keputusan baru.
                return RadOperationResult<RadOrderDetailResponse>.Success(
                    (await GetDetailAsync(entity.Id, cancellationToken))!);
            }

            var sebelum = entity.IsUrgent;

            entity.IsUrgent = request.IsUrgent;
            entity.UrgentMarkedByUserId = request.IsUrgent ? actorUserId : null;
            entity.UrgentMarkedAt = request.IsUrgent ? now : null;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = now;

            AddHistory(
                entity,
                "Order.Urgency",
                sebelum ? "Urgent" : "NotUrgent",
                request.IsUrgent ? "Urgent" : "NotUrgent",
                null,
                null,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "Order.Urgency",
                request.IsUrgent
                    ? "Pesanan radiologi ditandai cito."
                    : "Penanda cito pesanan radiologi dicabut.",
                new { OrderId = entity.Id, entity.ModalityId, entity.IsUrgent, ActorUserId = actorUserId });

            return RadOperationResult<RadOrderDetailResponse>.Success(
                (await GetDetailAsync(entity.Id, cancellationToken))!);
        }

        private static RadWorklistItemResponse MapWorklist(RadOrder entity) => new()
        {
            RadOrderId = entity.Id,
            EncounterId = entity.EncounterId,
            ProcedureId = entity.ProcedureId,
            ProcedureCode = entity.Procedure?.ProcedureCode ?? string.Empty,
            ProcedureName = entity.Procedure?.ProcedureName ?? string.Empty,
            ModalityId = entity.ModalityId,
            ModalityCode = entity.Modality?.ModalityCode ?? string.Empty,
            ModalityName = entity.Modality?.ModalityName ?? string.Empty,
            OrderStatus = entity.OrderStatus.ToString(),
            OrderStatusLabel = LabelStatusPesanan(entity.OrderStatus),
            IsUrgent = entity.IsUrgent,
            UrgentMarkedAt = entity.UrgentMarkedAt,
            RequestedAt = entity.RequestedAt,
            ScheduledAt = entity.ScheduledAt,
            WorkAt = entity.ScheduledAt ?? entity.RequestedAt ?? entity.CreateDateTime,
            CreateDateTime = entity.CreateDateTime,
            Studies = entity.Studies
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.StudySequence)
                .Select(x => new RadWorklistStudyResponse
                {
                    Id = x.Id,
                    StudyNumber = x.StudyNumber,
                    StudySequence = x.StudySequence,
                    StudyStatus = x.StudyStatus.ToString(),
                    StudyStatusLabel = RadStudyService.LabelStatusStudy(x.StudyStatus),

                    // AC-41. Diturunkan dari pesanannya, bukan disalin ke kolom pada RadStudy.
                    IsUrgent = entity.IsUrgent,

                    IsUsable = x.IsUsable,
                })
                .ToList(),
        };

        /// <summary>
        /// Batas awal dan akhir satu hari kerja, dikembalikan dalam UTC.
        ///
        /// <para>
        /// <b>Harinya dihitung menurut Waktu Indonesia Barat, bukan UTC.</b> Selisihnya tujuh
        /// jam, dan itu bukan perkara kerapian: shift pagi yang mulai pukul 06.00 WIB berjalan
        /// pada pukul 23.00 UTC <b>hari sebelumnya</b>. Memakai tanggal UTC akan menyajikan
        /// daftar kerja hari kemarin kepada petugas yang baru masuk — tepat pada jam ketika
        /// seluruh pekerjaan hari itu belum satu pun terlihat.
        /// </para>
        /// </summary>
        private static (DateTime Awal, DateTime Akhir) RentangHariKerja(DateTime? date)
        {
            var zona = ZonaBisnis();
            var hari = date?.Date ?? TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zona).Date;
            var besok = hari.AddDays(1);

            var awal = new DateTimeOffset(hari, zona.GetUtcOffset(hari));
            var akhir = new DateTimeOffset(besok, zona.GetUtcOffset(besok));

            return (awal.UtcDateTime, akhir.UtcDateTime);
        }

        /// <summary>
        /// Zona waktu bisnis. Bentuknya disamakan dengan <c>BillingNumberSeriesService</c> supaya
        /// seluruh modul memakai definisi "hari" yang sama.
        /// </summary>
        private static TimeZoneInfo ZonaBisnis()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }

        /* ================================================================ *
         * Pembuatan dan perpindahan status
         * ================================================================ */

        public async Task<RadOperationResult<RadOrderDetailResponse>> CreateAsync(
            CreateRadOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var modalityExists = await _dbContext.MstRadModalities
                .AnyAsync(x => x.Id == request.ModalityId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (!modalityExists)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.ModalityNotFound,
                    "Modalitas tidak ditemukan atau sedang tidak aktif.");
            }

            // BE-RWI-052 kriteria 2, VAL-DOK-22. Penanda perawatan boleh kosong - pesanan
            // poliklinik dan IGD memang tidak punya perawatan rawat inap. Yang dijaga adalah
            // KECOCOKANNYA ketika penandanya dikirim.
            if (request.InpEpisodeId.HasValue && request.InpEpisodeId.Value != Guid.Empty)
            {
                var episodeCocok = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.InpEpisodeId.Value
                                   && x.EncounterId == request.EncounterId
                                   && !x.IsDelete,
                              cancellationToken);

                if (!episodeCocok)
                {
                    return RadOperationResult<RadOrderDetailResponse>.Validation(
                        RadErrorCodes.InvalidTransition,
                        "Pesanan ini tidak cocok dengan perawatan pasien.");
                }
            }

            var entity = new RadOrder
            {
                // RAD-CONF-001 bagian 8 butir 2. Nomor dibentuk dari waktu dan enam karakter
                // acak, bukan dari hitungan baris — QBE-CODE-003. Index unik pada database
                // menjadi penjaga terakhirnya.
                OrderNumber = _orderNumberService.Generate(),

                EncounterId = request.EncounterId,
                // BE-RWI-052. Konteks perawatan distempel saat pesanan lahir.
                InpEpisodeId = request.InpEpisodeId,
                ProcedureId = request.ProcedureId,
                ModalityId = request.ModalityId,
                ClinicalIndication = request.ClinicalIndication,
                OrderStatus = RadOrderStatus.Requested,
                RequestedAt = now,
                RequestedByUserId = actorUserId,

                // BE-RAD-12, RAD-DEC-013. Field-nya boleh kosong; kosong berarti tidak cito,
                // sehingga pemanggil lama yang tidak mengenalnya tetap berhasil — FR-RAD-065.
                //
                // Jejak pelakunya hanya distempel ketika penandanya benar-benar dinyalakan.
                // Menstempelnya pada seluruh pesanan akan membuat kolom "siapa menandai cito"
                // terisi pada pesanan yang tidak pernah ditandai cito oleh siapa pun.
                IsUrgent = request.IsUrgent == true,
                UrgentMarkedByUserId = request.IsUrgent == true ? actorUserId : null,
                UrgentMarkedAt = request.IsUrgent == true ? now : null,

                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            _dbContext.RadOrders.Add(entity);
            AddHistory(entity, "Order.Create", null, entity.OrderStatus.ToString(),
                null, null, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var detail = await GetDetailAsync(entity.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        public Task<RadOperationResult<RadOrderDetailResponse>> AcceptAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Accept", RadOrderStatus.Accepted,
                new[] { RadOrderStatus.Requested }, request, cancellationToken);

        public Task<RadOperationResult<RadOrderDetailResponse>> ScheduleAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Schedule", RadOrderStatus.Scheduled,
                new[] { RadOrderStatus.Accepted }, request, cancellationToken);

        public Task<RadOperationResult<RadOrderDetailResponse>> StartAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Start", RadOrderStatus.InProgress,
                new[] { RadOrderStatus.Accepted, RadOrderStatus.Scheduled }, request, cancellationToken);

        public Task<RadOperationResult<RadOrderDetailResponse>> CompleteAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Complete", RadOrderStatus.Completed,
                new[] { RadOrderStatus.InProgress }, request, cancellationToken);

        public Task<RadOperationResult<RadOrderDetailResponse>> RejectAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Reject", RadOrderStatus.Rejected,
                new[] { RadOrderStatus.Requested }, request, cancellationToken, reasonRequired: true);

        /// <summary>
        /// Membatalkan pesanan.
        ///
        /// Pembatalan menolak berjalan bila ada study yang acquisition-nya sudah dimulai.
        /// Bukan karena tidak boleh dibatalkan, tetapi karena pembatalan pada tingkat pesanan
        /// akan menyembunyikan paparan yang sudah terjadi. Study yang sudah berjalan harus
        /// diselesaikan atau dihentikan pada tingkat study, tempat sebab dan konsumsinya dicatat.
        /// </summary>
        public async Task<RadOperationResult<RadOrderDetailResponse>> CancelAsync(
            Guid id,
            RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.RadOrders
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            var adaStudyBerjalan = order.Studies.Any(x => x.AcquisitionStartedAt != null);

            if (adaStudyBerjalan)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    "Pesanan ini memiliki study yang acquisition-nya sudah dimulai. " +
                    "Selesaikan atau hentikan study tersebut lebih dulu agar paparan yang " +
                    "sudah terjadi tetap tercatat.");
            }

            return await TransitionAsync(id, "Order.Cancel", RadOrderStatus.Cancelled,
                new[]
                {
                    RadOrderStatus.Draft, RadOrderStatus.Requested, RadOrderStatus.Accepted,
                    RadOrderStatus.Scheduled, RadOrderStatus.OnHold, RadOrderStatus.CancelRequested,
                },
                request, cancellationToken, reasonRequired: true);
        }

        public async Task<RadOperationResult<RadOrderDetailResponse>> HoldAsync(
            Guid id,
            RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var order = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (order.OrderStatus == RadOrderStatus.OnHold)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition, "Pesanan sudah berstatus OnHold.");
            }

            var from = order.OrderStatus;
            order.StatusBeforeHold = from;
            order.OrderStatus = RadOrderStatus.OnHold;
            Touch(order, actorUserId, now);

            AddHistory(order, "Order.Hold", from.ToString(), order.OrderStatus.ToString(),
                null, request?.Reason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            var detail = await GetDetailAsync(order.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        /// <summary>
        /// Melanjutkan pesanan yang ditahan, kembali ke status sebelum penahanan.
        ///
        /// Status sebelumnya dibaca dari kolomnya sendiri, bukan ditebak dari riwayat. Menebak
        /// akan membuat pesanan yang ditahan dari `Scheduled` kembali sebagai `Accepted`, dan
        /// jadwal yang sudah disepakati dengan pasien hilang tanpa ada yang menyadarinya.
        /// </summary>
        public async Task<RadOperationResult<RadOrderDetailResponse>> ResumeAsync(
            Guid id,
            RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var order = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (order.OrderStatus != RadOrderStatus.OnHold)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    "Hanya pesanan berstatus OnHold yang dapat dilanjutkan.");
            }

            var from = order.OrderStatus;
            order.OrderStatus = order.StatusBeforeHold ?? RadOrderStatus.Requested;
            order.StatusBeforeHold = null;
            Touch(order, actorUserId, now);

            AddHistory(order, "Order.Resume", from.ToString(), order.OrderStatus.ToString(),
                null, request?.Reason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            var detail = await GetDetailAsync(order.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        /* ================================================================ *
         * Pembantu
         * ================================================================ */

        private async Task<RadOperationResult<RadOrderDetailResponse>> TransitionAsync(
            Guid id,
            string action,
            RadOrderStatus target,
            IReadOnlyCollection<RadOrderStatus> allowedFrom,
            RadOrderTransitionRequest? request,
            CancellationToken cancellationToken,
            bool reasonRequired = false)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            if (reasonRequired && string.IsNullOrWhiteSpace(request?.Reason))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.ReasonRequired,
                    "Tindakan ini wajib disertai alasan yang tercatat.");
            }

            var order = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (!allowedFrom.Contains(order.OrderStatus))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    $"Pesanan berstatus {order.OrderStatus} tidak dapat berpindah ke {target}.");
            }

            var from = order.OrderStatus;
            order.OrderStatus = target;

            if (target == RadOrderStatus.Scheduled && request?.ScheduledAt != null)
            {
                order.ScheduledAt = request.ScheduledAt;
            }

            if (target == RadOrderStatus.Completed)
            {
                order.CompletedAt = now;
            }

            if (target is RadOrderStatus.Cancelled or RadOrderStatus.Rejected)
            {
                order.ClosureReason = request?.Reason;
            }

            Touch(order, actorUserId, now);
            AddHistory(order, action, from.ToString(), target.ToString(),
                null, request?.Reason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            var detail = await GetDetailAsync(order.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        private static void Touch(RadOrder order, Guid actorUserId, DateTime now)
        {
            order.UpdateBy = actorUserId;
            order.UpdateDateTime = now;
            order.Version += 1;
        }

        private void AddHistory(
            RadOrder order,
            string action,
            string? fromStatus,
            string toStatus,
            string? reasonCode,
            string? reasonNote,
            Guid actorUserId,
            DateTime now)
        {
            _dbContext.RadTransitionHistories.Add(new RadTransitionHistory
            {
                RadOrderId = order.Id,
                RadStudyId = null,
                EncounterId = order.EncounterId,
                Scope = RadTransitionScope.RadOrder,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ReasonCode = reasonCode,
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now,
            });
        }

        private async Task SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new RadConcurrencyException(
                    "Pesanan ini sudah diubah petugas lain. Muat ulang lalu ulangi tindakan Anda.");
            }
        }

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

        private static RadOrderDetailResponse MapDetail(RadOrder entity)
        {
            return new RadOrderDetailResponse
            {
                Id = entity.Id,
                OrderNumber = entity.OrderNumber,
                EncounterId = entity.EncounterId,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = entity.Procedure?.ProcedureCode ?? string.Empty,
                ProcedureName = entity.Procedure?.ProcedureName ?? string.Empty,
                ModalityId = entity.ModalityId,
                ModalityCode = entity.Modality?.ModalityCode ?? string.Empty,
                ModalityName = entity.Modality?.ModalityName ?? string.Empty,
                OrderStatus = entity.OrderStatus.ToString(),
                StudyCount = entity.Studies.Count(x => !x.IsDelete),
                UsableStudyCount = entity.Studies.Count(x =>
                    !x.IsDelete && x.StudyStatus == RadStudyStatus.QualityAccepted),
                IsCancel = entity.IsCancel,
                IsUrgent = entity.IsUrgent,
                UrgentMarkedByUserId = entity.UrgentMarkedByUserId,
                UrgentMarkedAt = entity.UrgentMarkedAt,
                CreateDateTime = entity.CreateDateTime,
                ClinicalIndication = entity.ClinicalIndication,
                RequestedAt = entity.RequestedAt,
                ScheduledAt = entity.ScheduledAt,
                CompletedAt = entity.CompletedAt,
                StatusBeforeHold = entity.StatusBeforeHold?.ToString(),
                ClosureReason = entity.ClosureReason,
                Version = entity.Version,
                Studies = entity.Studies
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.StudySequence)
                    .Select(RadStudyService.MapStudy)
                    .ToList(),
            };
        }
    }
}
