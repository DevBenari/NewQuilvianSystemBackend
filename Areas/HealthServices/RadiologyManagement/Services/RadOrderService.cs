using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using LabPaymentClearanceRules = QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services.LabPaymentClearanceRules;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

using InpatientClinicalContextService =
    QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services.InpatientClinicalContextService;
using MstTariff = QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstTariff;
using EmergencyDoctorAssignmentService = QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services.EmergencyDoctorAssignmentService;
using MstDoctorForInstruction =
    QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstDoctor;

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
    public partial class RadOrderService
    {
        private const string LogCategory = "HealthServices.RadiologyManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;
        private readonly RadOrderNumberService _orderNumberService;

        /// <summary>
        /// Pembaca penugasan dokter pada episode — <c>BE-RWI-104</c>, <c>INT-DOK-19</c>. Radiologi
        /// <b>membaca</b> penugasan lewat service bersama, bukan menyalinnya.
        /// </summary>
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly ClinicalMilestoneFactProducer _clinicalMilestoneFactProducer;
        private readonly EmergencyDoctorAssignmentService? _emergencyDoctorAssignmentService;

        public RadOrderService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService,
            RadOrderNumberService orderNumberService,
            InpatientClinicalContextService clinicalContextService,
            ClinicalMilestoneFactProducer clinicalMilestoneFactProducer,
            RadPhotoNumberService? photoNumberService = null,
            EmergencyDoctorAssignmentService? emergencyDoctorAssignmentService = null)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
            _orderNumberService = orderNumberService;
            _clinicalContextService = clinicalContextService;
            _clinicalMilestoneFactProducer = clinicalMilestoneFactProducer;
            _photoNumberService = photoNumberService;
            _emergencyDoctorAssignmentService = emergencyDoctorAssignmentService;
        }

        private EmergencyDoctorAssignmentService PenentuDokterIgd() =>
            _emergencyDoctorAssignmentService
            ?? throw new InvalidOperationException("Penentu dokter pemberi instruksi IGD tidak tersedia.");

        /// <summary>No. Foto v1, diberikan saat Konfirmasi. Opsional hanya untuk harness uji jalur baca.</summary>
        private readonly RadPhotoNumberService? _photoNumberService;

        /// <summary>
        /// Memberi No. Foto bila pesanan belum punya, atau menggantinya bila kategori alatnya berubah.
        /// Wajib dipanggil di dalam transaksi (kunci alokasinya berlaku per transaksi).
        /// </summary>
        private async Task<string?> BeriNoFotoAsync(RadOrder order, bool gantiBilaKategoriBerubah, CancellationToken cancellationToken)
        {
            if (_photoNumberService == null) return null;

            var kodeAlat = await _dbContext.MstRadModalities.AsNoTracking()
                .Where(m => m.Id == order.ModalityId)
                .Select(m => m.ModalityCode)
                .FirstOrDefaultAsync(cancellationToken);
            var kategori = RadExamCategory.FromModalityCode(kodeAlat);

            if (!string.IsNullOrEmpty(order.PhotoNumber) &&
                (!gantiBilaKategoriBerubah || RadPhotoNumberService.BelongsTo(order.PhotoNumber, kategori)))
            {
                return null;
            }

            var lama = order.PhotoNumber;
            order.PhotoNumber = await _photoNumberService.AllocateAsync(kodeAlat, cancellationToken);
            return lama;
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
                    Name = "onlyPreparation",
                    Type = "boolean",
                    Required = "No",
                    Description = "Hanya pesanan pasien yang membutuhkan persiapan (tab Pasien Persiapan).",
                },
                new()
                {
                    Name = "examinationDay",
                    Type = "string",
                    Required = "No",
                    Description =
                        "today: tanggal pemeriksaan hari ini (WIB), tab Rawat Jalan/Rawat Inap. " +
                        "notToday: selain hari ini, tab Pasien Perjanjian.",
                    Example = "today",
                },
                new()
                {
                    Name = "excludeCancelled",
                    Type = "boolean",
                    Required = "No",
                    Description = "Menyembunyikan pesanan yang dibatalkan atau ditolak (Daftar Pasien).",
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
        ///
        /// Menerima penyaring yang sama dengan daftar (<see cref="TerapkanPenyaring"/>), sehingga
        /// angka tab Daftar Pasien v1 dihitung dari baris yang persis sama dengan isi tabnya.
        /// Tanpa penyaring, hasilnya tetap rekap seluruh pesanan seperti sebelumnya.
        /// </summary>
        public async Task<RadOrderSummaryResponse> GetSummaryAsync(
            RadOrderListQuery? query = null,
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query != null)
            {
                source = TerapkanPenyaring(source, query);
            }

            // Lencana tab v1 menghitung BARIS yang belum terkonfirmasi. Satu baris Daftar Pasien
            // = satu booking v1 = pesanan satu kunjungan pada satu tanggal pemeriksaan (WIB) —
            // maka yang dihitung pasangan kunjungan + tanggal, bukan pesanannya. Pesanan berstatus
            // Diminta sedikit, sehingga tanggal WIB-nya dihitung di memori.
            var belumDikonfirmasi = await source
                .Where(x => x.OrderStatus == RadOrderStatus.Requested)
                .Select(x => new { x.EncounterId, Tanggal = x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime })
                .ToListAsync(cancellationToken);

            var kunjunganBelumDikonfirmasi = belumDikonfirmasi
                .Select(x => (x.EncounterId, TanggalWib(x.Tanggal)))
                .Distinct()
                .Count();

            var rekap = await source
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
                KunjunganBelumDikonfirmasi = kunjunganBelumDikonfirmasi,
            };
        }

        /// <summary>Panjang cakupan Beranda Radiologi, dalam hari kalender WIB.</summary>
        internal const int HariBeranda = 30;

        /// <summary>
        /// Angka Beranda Radiologi v1 — <c>BE-RAD-19</c>, <c>RAD-EVD-001</c> bagian 2.1.
        /// </summary>
        /// <remarks>
        /// v1 menarik hingga 2.000 booking ke browser lalu menghitungnya di sana. Di sini satu kueri
        /// baca-saja menarik kolom yang perlu dari pesanan 30 hari terakhir (tanpa identitas pasien),
        /// dan identitas hanya dibaca untuk lima pesanan terbaru. Pembayaran memakai aturan baca
        /// Laboratorium r43 yang sama dengan kolom Pembayaran Daftar Pasien (<c>BE-RAD-18</c>).
        /// </remarks>
        public async Task<RadDashboardResponse> GetDashboardAsync(
            DateTime? sekarangUtc = null,
            CancellationToken cancellationToken = default)
        {
            var sekarang = sekarangUtc ?? DateTime.UtcNow;
            var (awalHariIni, akhirHariIni) = HariIniWibDalamUtc(sekarang);
            var awalCakupan = awalHariIni.AddDays(-(HariBeranda - 1));
            var wib = TimeSpan.FromHours(7);

            var baris = await _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.OrderStatus != RadOrderStatus.Draft)
                .Where(x => (x.RequestedAt ?? x.CreateDateTime) >= awalCakupan &&
                            (x.RequestedAt ?? x.CreateDateTime) < akhirHariIni)
                .Select(x => new
                {
                    x.EncounterId,
                    x.OrderStatus,
                    x.IsUrgent,
                    Waktu = x.RequestedAt ?? x.CreateDateTime,
                    EncounterType = x.Encounter != null ? (EncounterType?)x.Encounter.EncounterType : null,
                    PaymentType = x.Encounter != null ? (EncounterPaymentType?)x.Encounter.PaymentType : null,
                    IsReferral = x.Encounter != null && x.Encounter.IsReferral,
                })
                .ToListAsync(cancellationToken);

            var pembayaran = await LabPaymentClearanceRules.ReadAsync(
                _dbContext,
                baris
                    .GroupBy(x => x.EncounterId)
                    .Select(g => (g.Key, g.First().PaymentType, g.First().EncounterType))
                    .ToList(),
                cancellationToken);

            var hasil = new RadDashboardResponse
            {
                GeneratedAt = sekarang,
                PeriodStartDate = (awalCakupan + wib).ToString("yyyy-MM-dd"),
                PeriodEndDate = (awalHariIni + wib).ToString("yyyy-MM-dd"),
                TotalOrderCount = baris.Count,
                OutpatientOrderCount = baris.Count(x => x.EncounterType == EncounterType.Outpatient),
                InpatientOrderCount = baris.Count(x => x.EncounterType == EncounterType.Inpatient),
                EmergencyOrderCount = baris.Count(x => x.EncounterType == EncounterType.Emergency),
                PaidOrderCount = baris.Count(x =>
                    pembayaran.TryGetValue(x.EncounterId, out var p) && p.Status == "Paid"),
                WaitingOrderCount = baris.Count(x => x.OrderStatus is RadOrderStatus.Requested
                    or RadOrderStatus.Accepted or RadOrderStatus.Scheduled),
                InProgressOrderCount = baris.Count(x => x.OrderStatus is RadOrderStatus.InProgress
                    or RadOrderStatus.OnHold or RadOrderStatus.CancelRequested),
                CompletedOrderCount = baris.Count(x => x.OrderStatus == RadOrderStatus.Completed),
                CancelledOrderCount = baris.Count(x => x.OrderStatus is RadOrderStatus.Cancelled
                    or RadOrderStatus.Rejected),
                UrgentOrderCount = baris.Count(x => x.IsUrgent),
                Composition = new RadDashboardCompositionResponse
                {
                    Referral = baris.Count(x => x.IsReferral),
                    Outpatient = baris.Count(x => !x.IsReferral && x.EncounterType == EncounterType.Outpatient),
                    Inpatient = baris.Count(x => !x.IsReferral && x.EncounterType == EncounterType.Inpatient),
                    Emergency = baris.Count(x => !x.IsReferral && x.EncounterType == EncounterType.Emergency),
                },
            };

            var perHari = baris
                .GroupBy(x => (x.Waktu + wib).Date)
                .ToDictionary(g => g.Key, g => g.Count());

            for (var i = 0; i < HariBeranda; i++)
            {
                var tanggal = (awalCakupan + wib).Date.AddDays(i);
                hasil.DailyOrders.Add(new RadDashboardDailyResponse
                {
                    Date = tanggal.ToString("yyyy-MM-dd"),
                    OrderCount = perHari.GetValueOrDefault(tanggal),
                });
            }

            var terbaru = await _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.OrderStatus != RadOrderStatus.Draft)
                .OrderByDescending(x => x.RequestedAt ?? x.CreateDateTime)
                .Take(5)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.OrderStatus,
                    x.IsUrgent,
                    Waktu = x.RequestedAt ?? x.CreateDateTime,
                    EncounterType = x.Encounter != null ? (EncounterType?)x.Encounter.EncounterType : null,
                    PatientName = x.Encounter != null && x.Encounter.Patient != null
                        ? x.Encounter.Patient.FullName : null,
                    MedicalRecordNumber = x.Encounter != null && x.Encounter.Patient != null
                        ? x.Encounter.Patient.MedicalRecordNumber : null,
                })
                .ToListAsync(cancellationToken);

            hasil.RecentOrders.AddRange(terbaru.Select(x => new RadDashboardRecentOrderResponse
            {
                RadOrderId = x.Id,
                OrderNumber = x.OrderNumber,
                PatientName = x.PatientName,
                MedicalRecordNumber = x.MedicalRecordNumber,
                EncounterType = x.EncounterType?.ToString(),
                OrderStatus = x.OrderStatus.ToString(),
                IsUrgent = x.IsUrgent,
                OrderedAt = x.Waktu,
            }));

            return hasil;
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

            var daftar = await ProyeksikanDaftarAsync(
                Urutkan(source, query.SortBy, query.SortDirection),
                query.BatasBaris(),
                cancellationToken);

            await LengkapiKolomV1Async(daftar, cancellationToken);

            return daftar;
        }

        /// <summary>
        /// Mengisi kolom v1 Daftar Pasien dan Riwayat Radiologi — <c>BE-RAD-18</c>,
        /// <c>RAD-EVD-001</c>: No. Foto, Diagnosa, Pemesan, Konfirmasi, dan Pembayaran.
        /// </summary>
        /// <remarks>
        /// Satu kueri berkelompok per kolom untuk seluruh halaman, bukan sub-query per baris — pola
        /// <c>LabMonitoringService</c> (<c>LAB-API-v1</c> r47). Seluruhnya baca saja, dan data lintas
        /// modul dibaca dari tabel pemiliknya, bukan disalin ke tabel Radiologi.
        /// </remarks>
        private async Task LengkapiKolomV1Async(
            List<RadOrderListResponse> daftar,
            CancellationToken cancellationToken)
        {
            if (daftar.Count == 0) return;

            var orderIds = daftar.Select(x => x.Id).ToList();
            var encounterIds = daftar.Select(x => x.EncounterId).Distinct().ToList();

            // No. Foto — konsep v1: SATU nomor per pemeriksaan, bukan satu per percobaan. Pesanan
            // V2 memuat satu pemeriksaan, sehingga yang tampil adalah study yang berlaku: urutan
            // tertinggi yang tidak dibatalkan. Study yang dihentikan atau perlu diulang sudah
            // digantikan pengulangannya; seluruh percobaan tetap terbaca di konsol pemeriksaan.
            var study = await _dbContext.RadStudies
                .AsNoTracking()
                .Where(s => orderIds.Contains(s.RadOrderId) && !s.IsDelete
                    && s.StudyStatus != RadStudyStatus.Cancelled)
                .Select(s => new { s.RadOrderId, s.StudySequence, s.StudyNumber })
                .ToListAsync(cancellationToken);

            var nomorFoto = study
                .GroupBy(s => s.RadOrderId)
                .ToDictionary(
                    g => g.Key,
                    g => new List<string> { g.OrderByDescending(s => s.StudySequence).First().StudyNumber });

            // Diagnosa kunjungan — diagnosa utama lebih dulu, lalu menurut waktu ditegakkan.
            var diagnosa = await _dbContext.TrxPatientDiagnoses
                .AsNoTracking()
                .Where(d => encounterIds.Contains(d.EncounterId) && !d.IsDelete)
                .OrderByDescending(d => d.IsPrimary)
                .ThenBy(d => d.DiagnosisDateTime)
                .Select(d => new { d.EncounterId, d.DiagnosisCode, d.DiagnosisName })
                .ToListAsync(cancellationToken);

            var diagnosaPerKunjungan = diagnosa
                .GroupBy(d => d.EncounterId)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join("; ", g
                        .Select(d => string.IsNullOrWhiteSpace(d.DiagnosisCode)
                            ? d.DiagnosisName
                            : $"{d.DiagnosisCode} - {d.DiagnosisName}")
                        .Distinct()));

            // Konfirmasi pertama dari riwayat — sumber yang sama dengan rincian pesanan.
            var konfirmasi = await _dbContext.RadTransitionHistories
                .AsNoTracking()
                .Where(h => orderIds.Contains(h.RadOrderId) && !h.IsDelete && h.Action == "Order.Accept")
                .Select(h => new { h.RadOrderId, h.ActorUserId, h.OccurredAt })
                .ToListAsync(cancellationToken);

            var konfirmasiPertama = konfirmasi
                .GroupBy(h => h.RadOrderId)
                .ToDictionary(g => g.Key, g => g.OrderBy(h => h.OccurredAt).First());

            // Pemesan — RequestedByUserId, cadangan pembuat baris (pola Lab r47).
            var pemesan = await _dbContext.RadOrders
                .AsNoTracking()
                .Where(o => orderIds.Contains(o.Id))
                .Select(o => new { o.Id, UserId = o.RequestedByUserId ?? o.CreateBy })
                .ToListAsync(cancellationToken);

            var userIds = pemesan.Where(p => p.UserId != Guid.Empty).Select(p => p.UserId)
                .Concat(konfirmasiPertama.Values.Select(k => k.ActorUserId))
                .Concat(daftar.Where(x => x.RadiographerUserId.HasValue).Select(x => x.RadiographerUserId!.Value))
                .Distinct()
                .ToList();

            var namaPengguna = userIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, Nama = u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode })
                    .ToDictionaryAsync(u => u.Id, u => (string?)u.Nama, cancellationToken);

            var pemesanPerPesanan = pemesan.ToDictionary(p => p.Id, p => p.UserId);

            // Pembayaran per kunjungan — aturan baca yang sama dengan Laboratorium (r43), dipakai
            // ulang alih-alih disalin supaya kedua modul tidak menilai satu kunjungan berbeda.
            var pembayaran = await LabPaymentClearanceRules.ReadAsync(
                _dbContext,
                daftar
                    .GroupBy(x => x.EncounterId)
                    .Select(g => (
                        g.Key,
                        Enum.TryParse<EncounterPaymentType>(g.First().Patient?.PaymentType, out var jenis)
                            ? jenis
                            : (EncounterPaymentType?)null,
                        Enum.TryParse<EncounterType>(g.First().Patient?.EncounterType, out var jenisKunjungan)
                            ? jenisKunjungan
                            : (EncounterType?)null))
                    .ToList(),
                cancellationToken);

            // BE-RAD-20 — Dokter Pemeriksa, nomor WhatsApp pasien (Kirim Hasil v1), dan tarif
            // pemeriksaan (Rincian Pasien v1).
            var dokterIds = daftar.Where(x => x.ExaminingDoctorId.HasValue)
                .Select(x => x.ExaminingDoctorId!.Value).Distinct().ToList();
            var namaDokter = dokterIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : await _dbContext.Set<MstDoctorForInstruction>()
                    .AsNoTracking()
                    .Where(d => dokterIds.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id, d => (string?)d.FullName, cancellationToken);

            var pasienIds = daftar.Where(x => x.Patient?.PatientId != null)
                .Select(x => x.Patient!.PatientId!.Value).Distinct().ToList();
            var teleponPasien = pasienIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : await _dbContext.MstPatients
                    .AsNoTracking()
                    .Where(p => pasienIds.Contains(p.Id))
                    .Select(p => new { p.Id, Nomor = p.WhatsAppNumber ?? p.PhoneNumber })
                    .ToDictionaryAsync(p => p.Id, p => p.Nomor, cancellationToken);

            var tarifPerBaris = await BacaTarifAsync(daftar, cancellationToken);

            foreach (var baris in daftar)
            {
                if (baris.RadiographerUserId.HasValue)
                {
                    baris.RadiographerName = namaPengguna.GetValueOrDefault(baris.RadiographerUserId.Value);
                }

                if (baris.ExaminingDoctorId.HasValue)
                {
                    baris.ExaminingDoctorName = namaDokter.GetValueOrDefault(baris.ExaminingDoctorId.Value);
                }

                if (baris.Patient?.PatientId is Guid pasienId)
                {
                    var nomorTelepon = teleponPasien.GetValueOrDefault(pasienId);
                    baris.PatientPhoneNumber = string.IsNullOrWhiteSpace(nomorTelepon) ? null : nomorTelepon.Trim();
                }

                // TryGetValue, bukan GetValueOrDefault: tarif yang belum diatur harus kosong, bukan Rp0.
                baris.TariffAmount = tarifPerBaris.TryGetValue(baris.Id, out var harga) ? harga : null;

                baris.StudyNumbers = nomorFoto.TryGetValue(baris.Id, out var nomor) ? nomor : new List<string>();
                baris.DiagnosisText = diagnosaPerKunjungan.GetValueOrDefault(baris.EncounterId);

                if (pemesanPerPesanan.TryGetValue(baris.Id, out var pemesanId) && pemesanId != Guid.Empty)
                {
                    baris.RequestedByName = namaPengguna.GetValueOrDefault(pemesanId);
                }

                if (konfirmasiPertama.TryGetValue(baris.Id, out var k))
                {
                    baris.ConfirmedAt = k.OccurredAt;
                    baris.ConfirmedByName = namaPengguna.GetValueOrDefault(k.ActorUserId);
                }

                if (pembayaran.TryGetValue(baris.EncounterId, out var status))
                {
                    baris.PaymentStatus = status.Status;
                    baris.IsPaymentCleared = status.IsCleared;
                    baris.OutstandingAmount = status.OutstandingAmount;
                }
            }
        }

        /// <summary>
        /// Tarif tampilan per pesanan — cetak Rincian Pasien v1 (<c>BE-RAD-20</c>).
        /// </summary>
        /// <remarks>
        /// Dibaca dari <c>MstTariff</c> yang berlaku saat ini, seperti <c>LabExaminationService</c>.
        /// Tarif yang menyebut kelas pasien kunjungan didahulukan, lalu tarif tanpa kelas; tarif kelas
        /// lain tidak dipakai. Hanya tampilan: jumlah yang ditagih tetap ditetapkan Billing
        /// (<c>BillingSourceTariffResolver</c>) saat tagihan terbit.
        /// </remarks>
        private async Task<Dictionary<Guid, decimal>> BacaTarifAsync(
            IReadOnlyCollection<RadOrderListResponse> daftar,
            CancellationToken cancellationToken)
        {
            var hasil = new Dictionary<Guid, decimal>();
            var procedureIds = daftar.Select(x => x.ProcedureId).Where(x => x != Guid.Empty).Distinct().ToList();
            if (procedureIds.Count == 0) return hasil;

            var sekarang = DateTime.UtcNow;
            var tarif = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(t => t.ProcedureId != null && procedureIds.Contains(t.ProcedureId.Value) &&
                            !t.IsDelete && t.IsActive &&
                            (t.EffectiveStartDate == null || t.EffectiveStartDate <= sekarang) &&
                            (t.EffectiveEndDate == null || t.EffectiveEndDate >= sekarang))
                .Select(t => new { ProcedureId = t.ProcedureId!.Value, t.PatientClassId, t.NormalPrice, t.EffectiveStartDate, t.TariffCode })
                .ToListAsync(cancellationToken);

            foreach (var baris in daftar)
            {
                var kelas = baris.Patient?.PatientClassId;
                var pilihan = tarif
                    .Where(t => t.ProcedureId == baris.ProcedureId &&
                                (t.PatientClassId == null || t.PatientClassId == kelas))
                    .OrderByDescending(t => t.PatientClassId != null)
                    .ThenByDescending(t => t.EffectiveStartDate)
                    .ThenBy(t => t.TariffCode, StringComparer.Ordinal)
                    .FirstOrDefault();

                if (pilihan != null) hasil[baris.Id] = pilihan.NormalPrice;
            }

            return hasil;
        }

        /// <summary>
        /// Batas awal dan akhir (eksklusif) hari ini pada kalender WIB, dinyatakan dalam UTC.
        /// </summary>
        internal static (DateTime AwalUtc, DateTime AkhirUtc) HariIniWibDalamUtc(DateTime sekarangUtc)
        {
            var wib = TimeSpan.FromHours(7);
            var tanggalWib = (sekarangUtc + wib).Date;
            var awal = DateTime.SpecifyKind(tanggalWib - wib, DateTimeKind.Utc);
            return (awal, awal.AddDays(1));
        }

        /// <summary>Tanggal kalender WIB dari waktu UTC — satu baris Daftar Pasien per hari pesan.</summary>
        internal static DateTime TanggalWib(DateTime waktuUtc) => (waktuUtc + TimeSpan.FromHours(7)).Date;

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
                    (x.PhotoNumber != null && x.PhotoNumber.Contains(nomor)) ||
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

            // BE-RAD-18, RAD-EVD-001 — tab Daftar Pasien Radiologi v1.
            if (query.OnlyPreparation == true)
            {
                source = source.Where(x => x.RequiresPreparation);
            }

            if (query.ExcludeCancelled == true)
            {
                source = source.Where(x =>
                    !x.IsCancel &&
                    x.OrderStatus != RadOrderStatus.Cancelled &&
                    x.OrderStatus != RadOrderStatus.Rejected);
            }

            // "Hari ini" dihitung pada kalender WIB, karena petugas dan v1 membaca tanggal lokal;
            // waktu di database UTC. Tanggal pemeriksaan = jadwal bila ada, selain itu waktu pesan.
            var hari = query.ExaminationDay?.Trim();
            if (!string.IsNullOrEmpty(hari))
            {
                var (awalHariUtc, akhirHariUtc) = HariIniWibDalamUtc(DateTime.UtcNow);

                if (string.Equals(hari, "today", StringComparison.OrdinalIgnoreCase))
                {
                    source = source.Where(x =>
                        (x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime) >= awalHariUtc &&
                        (x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime) < akhirHariUtc);
                }
                else if (string.Equals(hari, "notToday", StringComparison.OrdinalIgnoreCase))
                {
                    source = source.Where(x =>
                        (x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime) < awalHariUtc ||
                        (x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime) >= akhirHariUtc);
                }
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
                    x.PhotoNumber,
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
                    x.RequiresPreparation,
                    x.RadiographerUserId,
                    x.ExaminingDoctorId,
                    x.PrintFilm,
                    ExaminationDate = x.ScheduledAt ?? x.RequestedAt ?? x.CreateDateTime,

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
                    PhotoNumber = x.PhotoNumber,
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
                    RequiresPreparation = x.RequiresPreparation,
                    ExaminationDate = x.ExaminationDate,
                    RadiographerUserId = x.RadiographerUserId,
                    ExaminingDoctorId = x.ExaminingDoctorId,
                    PrintFilm = x.PrintFilm,
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

            // BE-MCU-020 — nomor order MCU yang mencakup pesanan ini, ruas baca.
            if (entity.CoveringMcuOrderId.HasValue)
            {
                detail.CoveringMcuOrderNumber = await _dbContext.McuOrders.AsNoTracking()
                    .Where(x => x.Id == entity.CoveringMcuOrderId.Value)
                    .Select(x => x.OrderNumber)
                    .FirstOrDefaultAsync(cancellationToken);
            }

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

            // BE-RWI-104. Nama pemberi instruksi untuk layar; penunjuknya sendiri tidak ditampilkan.
            if (entity.InstructingDoctorId.HasValue)
            {
                detail.InstructingDoctorName = await _dbContext.Set<MstDoctorForInstruction>()
                    .AsNoTracking()
                    .Where(x => x.Id == entity.InstructingDoctorId.Value)
                    .Select(x => x.FullName)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            // BE-RAD-20 — Detail Pasien Radiologi v1 memakai kolom yang sama dengan daftar.
            await LengkapiKolomV1Async(new List<RadOrderListResponse> { detail }, cancellationToken);

            return detail;
        }

        /* ================================================================ *
         * BE-RWI-104 — verifikasi instruksi pesanan rawat inap
         * ================================================================ */

        /// <summary>
        /// Dokter pemberi instruksi memverifikasi pesanan radiologi yang dibuat perawat —
        /// <c>BE-RWI-104</c>, <c>VAL-DOK-50</c>, <c>VAL-DOK-50a</c>.
        /// </summary>
        /// <remarks>
        /// Hanya kolom verifikasi dan token konkurensi yang berubah; penginput, pemeriksaan, dan status
        /// pesanan tidak disentuh. Tidak bergantung status pesanan.
        /// </remarks>
        public async Task<RadOperationResult<RadOrderDetailResponse>> VerifyInstructionAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Order radiologi tidak ditemukan.");
            }

            if (!entity.InstructingDoctorId.HasValue)
            {
                return RadOperationResult<RadOrderDetailResponse>.Conflict(
                    RadErrorCodes.InstructionNotPending,
                    "Pesanan ini sudah diverifikasi atau tidak memerlukan verifikasi.");
            }

            var actorUserId = GetCurrentUserId();
            var actorDoctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);

            if (!actorDoctorId.HasValue || actorDoctorId.Value != entity.InstructingDoctorId.Value)
            {
                return RadOperationResult<RadOrderDetailResponse>.Forbidden(
                    RadErrorCodes.NotInstructingDoctor,
                    "Hanya dokter pemberi instruksi yang dapat memverifikasi pesanan ini.");
            }

            if (entity.InstructionVerificationStatus != RadOrderInstructionVerificationStatus.Pending)
            {
                return RadOperationResult<RadOrderDetailResponse>.Conflict(
                    RadErrorCodes.InstructionNotPending,
                    "Pesanan ini sudah diverifikasi atau tidak memerlukan verifikasi.");
            }

            var now = DateTime.UtcNow;
            entity.InstructionVerificationStatus = RadOrderInstructionVerificationStatus.Verified;
            entity.InstructionVerifiedAt = now;
            entity.InstructionVerifiedByUserId = actorUserId;
            entity.Version++;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "RadOrder.VerifyInstruction",
                "Dokter pemberi instruksi memverifikasi order radiologi.",
                new { entity.Id, entity.InpEpisodeId, entity.InstructingDoctorId, entity.RequestedByUserId, VerifiedBy = actorUserId });

            var detail = await GetDetailAsync(entity.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        /// <summary>
        /// Pesanan radiologi yang menunggu verifikasi dokter login — <c>BE-RWI-104</c>.
        /// </summary>
        public async Task<RadOperationResult<PagedResult<RadOrderInstructionVerificationItemResponse>>> GetInstructionVerificationWorklistAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                _httpContextAccessor.HttpContext?.User, GetCurrentUserId(), cancellationToken);

            if (!doctorId.HasValue)
            {
                return RadOperationResult<PagedResult<RadOrderInstructionVerificationItemResponse>>.Forbidden(
                    RadErrorCodes.DoctorNotIdentified,
                    InpatientClinicalContextService.PenolakanBukanDokter);
            }

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            var query = _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.InstructingDoctorId == doctorId.Value &&
                            x.InstructionVerificationStatus == RadOrderInstructionVerificationStatus.Pending);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RadOrderInstructionVerificationItemResponse
                {
                    OrderId = x.Id,
                    EncounterId = x.EncounterId,
                    InpEpisodeId = x.InpEpisodeId,
                    EpisodeNumber = _dbContext.Set<InpEpisode>()
                        .Where(e => e.Id == x.InpEpisodeId)
                        .Select(e => e.EpisodeNumber)
                        .FirstOrDefault(),
                    PatientName = x.Encounter != null && x.Encounter.Patient != null ? x.Encounter.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Encounter != null && x.Encounter.Patient != null ? x.Encounter.Patient.MedicalRecordNumber : string.Empty,
                    ProcedureId = x.ProcedureId,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    OrderStatus = x.OrderStatus.ToString(),
                    RequestedAt = x.RequestedAt,
                    RequestedByUserId = x.RequestedByUserId,
                    RequestedByName = x.RequestedByUserId == null
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.RequestedByUserId)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    InstructionVerificationStatus = x.InstructionVerificationStatus.ToString()
                })
                .ToListAsync(cancellationToken);

            return RadOperationResult<PagedResult<RadOrderInstructionVerificationItemResponse>>.Success(
                new PagedResult<RadOrderInstructionVerificationItemResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                });
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

        /// <summary>
        /// Menetapkan Petugas Radiologi dan Dokter Pemeriksa — Detail Pasien Radiologi v1
        /// (<c>BE-RAD-20</c>, <c>RAD-EVD-001</c> bagian 7.1).
        /// </summary>
        /// <remarks>
        /// v1 membolehkan pilihan ini diubah kapan saja; di sini hanya pesanan batal atau ditolak yang
        /// dikunci, karena tidak ada lagi pekerjaan yang ditangani. Setiap perubahan dicatat pada
        /// <c>RadTransitionHistory</c> (<c>Order.Assign</c>) beserta pelakunya. Penunjukan Dokter
        /// Pemeriksa tidak memberi hak mengesahkan bacaan (<c>RAD-DEC-003</c>).
        /// </remarks>
        public async Task<RadOperationResult<RadOrderDetailResponse>> AssignAsync(
            Guid id,
            RadOrderAssignmentRequest request,
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

            if (entity.IsCancel || entity.OrderStatus is RadOrderStatus.Cancelled or RadOrderStatus.Rejected)
            {
                return RadOperationResult<RadOrderDetailResponse>.Conflict(
                    RadErrorCodes.AssignmentNotChangeable,
                    $"Pesanan ini berstatus {LabelStatusPesanan(entity.OrderStatus).ToLowerInvariant()}, " +
                    "sehingga petugas dan dokter pemeriksanya tidak dapat diubah.");
            }

            if (request.RadiographerUserId is Guid petugasId &&
                !await _dbContext.Users.AnyAsync(u => u.Id == petugasId && u.IsActive, cancellationToken))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.AssigneeNotFound,
                    "Petugas Radiologi tidak ditemukan atau akunnya nonaktif.");
            }

            if (request.ExaminingDoctorId is Guid dokterId &&
                !await _dbContext.Set<MstDoctorForInstruction>().AnyAsync(d => d.Id == dokterId && d.IsActive && !d.IsDelete, cancellationToken))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.AssigneeNotFound,
                    "Dokter Pemeriksa tidak ditemukan atau nonaktif.");
            }

            if (entity.RadiographerUserId == request.RadiographerUserId &&
                entity.ExaminingDoctorId == request.ExaminingDoctorId)
            {
                // Tidak ada yang berubah — tidak perlu jejak keputusan baru.
                return RadOperationResult<RadOrderDetailResponse>.Success(
                    (await GetDetailAsync(entity.Id, cancellationToken))!);
            }

            var status = entity.OrderStatus.ToString();
            var catatan =
                $"Petugas: {entity.RadiographerUserId?.ToString() ?? "-"} → {request.RadiographerUserId?.ToString() ?? "-"}; " +
                $"Dokter Pemeriksa: {entity.ExaminingDoctorId?.ToString() ?? "-"} → {request.ExaminingDoctorId?.ToString() ?? "-"}";

            entity.RadiographerUserId = request.RadiographerUserId;
            entity.ExaminingDoctorId = request.ExaminingDoctorId;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = now;

            AddHistory(entity, "Order.Assign", status, status, null, catatan, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "Order.Assign",
                $"Petugas Radiologi dan Dokter Pemeriksa pesanan {entity.OrderNumber} diperbarui. {catatan}",
                new { OrderId = entity.Id, ActorUserId = actorUserId });

            return RadOperationResult<RadOrderDetailResponse>.Success(
                (await GetDetailAsync(entity.Id, cancellationToken))!);
        }

        /// <summary>
        /// Ubah Pemeriksaan — Detail Pasien v1 (<c>BE-RAD-25</c>, <c>RAD-DEC-021</c>).
        /// </summary>
        /// <remarks>
        /// Hanya selama pesanan berstatus Diminta/Diterima/Dijadwalkan dan <b>belum punya study aktif</b>:
        /// sesudah itu ada citra, bahan terpakai, dan tagihan yang terikat pada pemeriksaan lama. Alasan
        /// wajib; perubahan tercatat <c>Order.ChangeProcedure</c>. Pemeriksaan baru wajib bertanda
        /// Radiologi dan aktif.
        /// </remarks>
        public async Task<RadOperationResult<RadOrderDetailResponse>> ChangeProcedureAsync(
            Guid id,
            RadOrderChangeProcedureRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var alasan = request.Reason?.Trim();
            if (string.IsNullOrEmpty(alasan))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.ReasonRequired, "Alasan perubahan pemeriksaan wajib diisi.");
            }

            var entity = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (entity.IsCancel || entity.OrderStatus is not (RadOrderStatus.Requested or RadOrderStatus.Accepted or RadOrderStatus.Scheduled))
            {
                return RadOperationResult<RadOrderDetailResponse>.Conflict(
                    RadErrorCodes.ProcedureNotChangeable,
                    $"Pemeriksaan pesanan berstatus {LabelStatusPesanan(entity.OrderStatus).ToLowerInvariant()} tidak dapat diubah.");
            }

            if (await _dbContext.RadStudies.AnyAsync(
                    s => s.RadOrderId == id && !s.IsDelete && s.StudyStatus != RadStudyStatus.Cancelled, cancellationToken))
            {
                return RadOperationResult<RadOrderDetailResponse>.Conflict(
                    RadErrorCodes.ProcedureNotChangeable,
                    "Pesanan ini sudah punya pemeriksaan yang direncanakan atau dikerjakan. Batalkan study-nya lebih dulu, atau batalkan pesanan lalu buat pesanan baru.");
            }

            var tindakan = await _dbContext.MstProcedures
                .AsNoTracking()
                .Where(p => p.Id == request.ProcedureId && !p.IsDelete)
                .Select(p => new { p.Id, p.ProcedureName, p.IsRadiology, p.IsActive })
                .FirstOrDefaultAsync(cancellationToken);
            if (tindakan == null || !tindakan.IsRadiology || !tindakan.IsActive)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.ProcedureNotFound, "Pemeriksaan radiologi tidak ditemukan atau nonaktif.");
            }

            var modalitasBaru = request.ModalityId ?? entity.ModalityId;
            if (!await _dbContext.MstRadModalities.AnyAsync(m => m.Id == modalitasBaru && !m.IsDelete && m.IsActive, cancellationToken))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.ModalityNotFound, "Alat pencitraan tidak ditemukan atau nonaktif.");
            }

            if (entity.ProcedureId == tindakan.Id && entity.ModalityId == modalitasBaru)
            {
                return RadOperationResult<RadOrderDetailResponse>.Success((await GetDetailAsync(id, cancellationToken))!);
            }

            var namaLama = await _dbContext.MstProcedures.AsNoTracking()
                .Where(p => p.Id == entity.ProcedureId).Select(p => p.ProcedureName).FirstOrDefaultAsync(cancellationToken);
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var catatan = $"{namaLama ?? entity.ProcedureId.ToString()} → {tindakan.ProcedureName}. Alasan: {alasan}";

            entity.ProcedureId = tindakan.Id;
            entity.ModalityId = modalitasBaru;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = now;

            // No. Foto mengikuti kategori pemeriksaan: bila alat berganti kategori, nomor kategori
            // lama dilepas (celahnya dibiarkan) dan nomor kategori baru diberikan.
            await using var transaksi = !string.IsNullOrEmpty(entity.PhotoNumber) && _dbContext.Database.CurrentTransaction == null
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;
            if (!string.IsNullOrEmpty(entity.PhotoNumber))
            {
                var nomorLama = await BeriNoFotoAsync(entity, gantiBilaKategoriBerubah: true, cancellationToken);
                if (nomorLama != null) catatan += $" No. Foto {nomorLama} → {entity.PhotoNumber}.";
            }

            var status = entity.OrderStatus.ToString();
            AddHistory(entity, "Order.ChangeProcedure", status, status, null, catatan, actorUserId, now);
            await SaveWithConcurrencyGuardAsync(cancellationToken);
            if (transaksi != null) await transaksi.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "Order.ChangeProcedure",
                $"Pemeriksaan pesanan {entity.OrderNumber} diubah: {catatan}",
                new { OrderId = entity.Id, ActorUserId = actorUserId });

            return RadOperationResult<RadOrderDetailResponse>.Success((await GetDetailAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Tambah Baris — Detail Pasien v1: menambah pemeriksaan pada booking yang sama.
        /// </summary>
        /// <remarks>
        /// Satu pesanan V2 = satu pemeriksaan, sehingga pemeriksaan tambahan adalah pesanan baru yang
        /// <b>mewarisi</b> kunjungan, perawatan, tanggal pemeriksaan (<c>RequestedAt</c>/<c>ScheduledAt</c>),
        /// penanda persiapan, dan Petugas Radiologi pesanan sumbernya. Tanggal pemeriksaan yang sama
        /// membuatnya tetap satu baris Daftar Pasien dengan booking sumber walau ditambahkan di hari
        /// lain; waktu sebenarnya tetap tercatat di <c>CreateDateTime</c> dan riwayat. Statusnya
        /// Diminta — ia dikonfirmasi seperti pemeriksaan lain, dan saat itulah No. Foto-nya terbit.
        /// </remarks>
        public async Task<RadOperationResult<RadOrderDetailResponse>> AddExaminationAsync(
            Guid sourceId,
            RadOrderAddExaminationRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var sumber = await _dbContext.RadOrders.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sourceId && !x.IsDelete, cancellationToken);
            if (sumber == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            var tindakan = await _dbContext.MstProcedures.AsNoTracking()
                .Where(p => p.Id == request.ProcedureId && !p.IsDelete)
                .Select(p => new { p.Id, p.ProcedureName, p.IsRadiology, p.IsActive })
                .FirstOrDefaultAsync(cancellationToken);
            if (tindakan == null || !tindakan.IsRadiology || !tindakan.IsActive)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.ProcedureNotFound, "Pemeriksaan radiologi tidak ditemukan atau nonaktif.");
            }

            var modalitas = request.ModalityId ?? sumber.ModalityId;
            if (!await _dbContext.MstRadModalities.AnyAsync(m => m.Id == modalitas && !m.IsDelete && m.IsActive, cancellationToken))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.ModalityNotFound, "Alat pencitraan tidak ditemukan atau nonaktif.");
            }

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            await using var transaksi = _dbContext.Database.CurrentTransaction == null
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var entity = new RadOrder
            {
                OrderNumber = await _orderNumberService.AllocateOneAsync(cancellationToken),
                EncounterId = sumber.EncounterId,
                InpEpisodeId = sumber.InpEpisodeId,
                ProcedureId = tindakan.Id,
                ModalityId = modalitas,
                OrderStatus = RadOrderStatus.Requested,
                InstructionVerificationStatus = RadOrderInstructionVerificationStatus.NotRequired,
                // Tanggal pemeriksaan booking diwarisi supaya tetap satu baris dengan sumbernya.
                RequestedAt = sumber.RequestedAt ?? sumber.CreateDateTime,
                ScheduledAt = sumber.ScheduledAt,
                RequestedByUserId = actorUserId,
                RequiresPreparation = sumber.RequiresPreparation,
                RadiographerUserId = sumber.RadiographerUserId,
                ExaminingDoctorId = request.ExaminingDoctorId,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            _dbContext.RadOrders.Add(entity);
            AddHistory(entity, "Order.Create", null, entity.OrderStatus.ToString(),
                null, $"Tambah Baris pada booking {sumber.OrderNumber}: {tindakan.ProcedureName}", actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaksi != null) await transaksi.CommitAsync(cancellationToken);

            return RadOperationResult<RadOrderDetailResponse>.Success((await GetDetailAsync(entity.Id, cancellationToken))!);
        }

        /// <summary>Centang Cetak Film — Detail Pasien v1 (<c>BE-RAD-25</c>). Tercatat <c>Order.FilmPrint</c>.</summary>
        public async Task<RadOperationResult<RadOrderDetailResponse>> SetFilmPrintAsync(
            Guid id,
            RadOrderFilmPrintRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (entity.IsCancel || entity.OrderStatus is RadOrderStatus.Cancelled or RadOrderStatus.Rejected)
            {
                return RadOperationResult<RadOrderDetailResponse>.Conflict(
                    RadErrorCodes.AssignmentNotChangeable, "Pesanan batal atau ditolak tidak dapat diubah.");
            }

            if (entity.PrintFilm != request.PrintFilm)
            {
                var actorUserId = GetCurrentUserId();
                var now = DateTime.UtcNow;
                entity.PrintFilm = request.PrintFilm;
                entity.UpdateBy = actorUserId;
                entity.UpdateDateTime = now;

                var status = entity.OrderStatus.ToString();
                AddHistory(entity, "Order.FilmPrint", status, status, null,
                    request.PrintFilm ? "Cetak Film dicentang." : "Cetak Film dicabut.", actorUserId, now);
                await SaveWithConcurrencyGuardAsync(cancellationToken);
            }

            return RadOperationResult<RadOrderDetailResponse>.Success((await GetDetailAsync(id, cancellationToken))!);
        }

        private const int AssigneeMaxPageSize = 50;
        private const int AssigneeMaxSearchLength = 100;

        private static (int Halaman, int Ukuran, string? Cari) RapikanPilihan(RadAssigneeOptionQuery? query)
        {
            var halaman = Math.Max(query?.PageNumber ?? 1, 1);
            var ukuran = Math.Clamp(query?.PageSize ?? 25, 1, AssigneeMaxPageSize);
            var cari = query?.Search?.Trim();
            if (cari?.Length > AssigneeMaxSearchLength) cari = cari[..AssigneeMaxSearchLength];
            return (halaman, ukuran, string.IsNullOrEmpty(cari) ? null : cari.ToLower());
        }

        /// <summary>
        /// Pilihan Dokter Pemeriksa milik Radiologi (<c>BE-RAD-20</c>) — pola
        /// <c>LabOrderService.GetExaminerDoctorOptionsAsync</c>: daftar dokter SDM dijaga
        /// <c>KioskRead</c> dan menolak petugas penunjang. Dokter aktif saja; baca murni.
        /// </summary>
        public async Task<PagedResult<RadExaminerDoctorOptionResponse>> GetExaminerDoctorOptionsAsync(
            RadAssigneeOptionQuery? query,
            CancellationToken cancellationToken = default)
        {
            var (halaman, ukuran, cari) = RapikanPilihan(query);

            var source = _dbContext.Set<MstDoctorForInstruction>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            if (cari != null)
            {
                source = source.Where(x =>
                    x.FullName.ToLower().Contains(cari) ||
                    x.DoctorCode.ToLower().Contains(cari) ||
                    (x.SpecialistName != null && x.SpecialistName.ToLower().Contains(cari)));
            }

            var total = await source.CountAsync(cancellationToken);
            var items = await source
                .OrderBy(x => x.FullName).ThenBy(x => x.DoctorCode)
                .Skip((halaman - 1) * ukuran).Take(ukuran)
                .Select(x => new RadExaminerDoctorOptionResponse
                {
                    Id = x.Id,
                    DoctorCode = x.DoctorCode,
                    FullName = x.FullName,
                    SpecialistName = x.SpecialistName,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RadExaminerDoctorOptionResponse>
            {
                PageNumber = halaman,
                PageSize = ukuran,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)ukuran),
                Items = items,
            };
        }

        /// <summary>
        /// Pilihan Petugas Radiologi (<c>BE-RAD-20</c>) — akun pengguna aktif, seperti v1 yang
        /// memakai daftar pengguna aktif. Hanya nama dan kode pengguna yang dibaca.
        /// </summary>
        public async Task<PagedResult<RadRadiographerOptionResponse>> GetRadiographerOptionsAsync(
            RadAssigneeOptionQuery? query,
            CancellationToken cancellationToken = default)
        {
            var (halaman, ukuran, cari) = RapikanPilihan(query);

            var source = _dbContext.Users
                .AsNoTracking()
                .Where(u => u.IsActive);

            if (cari != null)
            {
                source = source.Where(u =>
                    (u.DisplayName != null && u.DisplayName.ToLower().Contains(cari)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(cari)) ||
                    (u.UserCode != null && u.UserCode.ToLower().Contains(cari)));
            }

            var total = await source.CountAsync(cancellationToken);
            var items = await source
                .OrderBy(u => u.DisplayName ?? u.UserName)
                .Skip((halaman - 1) * ukuran).Take(ukuran)
                .Select(u => new RadRadiographerOptionResponse
                {
                    Id = u.Id,
                    FullName = u.DisplayName ?? u.UserName ?? u.UserCode ?? string.Empty,
                    UserCode = u.UserCode,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RadRadiographerOptionResponse>
            {
                PageNumber = halaman,
                PageSize = ukuran,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)ukuran),
                Items = items,
            };
        }

        /// <summary>
        /// Pilihan pemeriksaan untuk Ubah Pemeriksaan (<c>BE-RAD-25</c>): pemeriksaan radiologi aktif saja,
        /// sama dengan syarat <see cref="ChangeProcedureAsync"/>. Baca murni.
        /// </summary>
        public async Task<PagedResult<RadProcedureOptionResponse>> GetProcedureOptionsAsync(
            RadAssigneeOptionQuery? query,
            CancellationToken cancellationToken = default)
        {
            var (halaman, ukuran, cari) = RapikanPilihan(query);

            var source = _dbContext.MstProcedures
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsRadiology);

            if (cari != null)
            {
                source = source.Where(x =>
                    x.ProcedureName.ToLower().Contains(cari) ||
                    x.ProcedureCode.ToLower().Contains(cari) ||
                    (x.ExternalProcedureCode != null && x.ExternalProcedureCode.ToLower().Contains(cari)) ||
                    (x.ProcedureCategoryName != null && x.ProcedureCategoryName.ToLower().Contains(cari)));
            }

            var total = await source.CountAsync(cancellationToken);
            var items = await source
                .OrderBy(x => x.ProcedureName).ThenBy(x => x.ProcedureCode)
                .Skip((halaman - 1) * ukuran).Take(ukuran)
                .Select(x => new RadProcedureOptionResponse
                {
                    Id = x.Id,
                    ProcedureCode = x.ProcedureCode,
                    ProcedureName = x.ProcedureName,
                    ProcedureCategoryName = x.ProcedureCategoryName,
                    ExternalProcedureCode = x.ExternalProcedureCode,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RadProcedureOptionResponse>
            {
                PageNumber = halaman,
                PageSize = ukuran,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)ukuran),
                Items = items,
            };
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

        public Task<RadOperationResult<RadOrderDetailResponse>> CreateAsync(
            CreateRadOrderRequest request,
            CancellationToken cancellationToken = default) =>
            CreateCoreAsync(request, coverage: null, cancellationToken);

        /// <summary>
        /// Jalur internal MCU (<c>BE-MCU-020</c>, <c>INT-MCU-06</c>, <c>MCU-COORD-008</c>): satu pesanan
        /// per komponen, bertanda paket (<c>CoveringMcuOrderId</c>) dan berdokter pemberi instruksi =
        /// dokter MCU, verifikasi instruksi <c>NotRequired</c>.
        /// </summary>
        /// <remarks>
        /// Sengaja <c>internal</c> dan tidak terhubung ke endpoint mana pun: tanda paket hanya boleh
        /// ditulis MCU. Ikut transaksi pemanggil bila ada, seperti <see cref="CreateAsync"/>.
        /// </remarks>
        internal Task<RadOperationResult<RadOrderDetailResponse>> CreateForMcuAsync(
            CreateRadOrderRequest request,
            Guid coveringMcuOrderId,
            Guid mcuDoctorId,
            CancellationToken cancellationToken = default) =>
            CreateCoreAsync(request, new McuCoverage(coveringMcuOrderId, mcuDoctorId), cancellationToken);

        /// <summary>
        /// Memindah dokter penanggung jawab pesanan paket MCU yang masih terbuka ke dokter MCU pengganti
        /// (<c>BE-MCU-032</c>, <c>MCU-DEC-040</c>, <c>INT-MCU-06</c> baris ganti dokter). Pesanan yang sudah selesai
        /// atau batal tetap atas nama dokter lama.
        /// </summary>
        /// <remarks>Sengaja <c>internal</c>, hanya dipanggil MCU. Ikut transaksi pemanggil; tiap pesanan mencatat riwayat beralasan.</remarks>
        internal async Task<int> ReassignMcuDoctorAsync(
            Guid coveringMcuOrderId,
            Guid newDoctorId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var orders = await _dbContext.RadOrders
                .Where(x => x.CoveringMcuOrderId == coveringMcuOrderId && !x.IsDelete && !x.IsCancel
                    && x.OrderStatus != RadOrderStatus.Completed && x.OrderStatus != RadOrderStatus.Cancelled
                    && x.InstructingDoctorId != newDoctorId)
                .ToListAsync(cancellationToken);
            if (orders.Count == 0)
                return 0;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            foreach (var order in orders)
            {
                order.InstructingDoctorId = newDoctorId;
                Touch(order, actorUserId, now);
                AddHistory(order, "Order.ReassignMcuDoctor", order.OrderStatus.ToString(), order.OrderStatus.ToString(),
                    reasonCode: null, reasonNote: reason, actorUserId, now);
            }

            await SaveWithConcurrencyGuardAsync(cancellationToken);
            return orders.Count;
        }

        /// <summary>Tanda paket MCU untuk pesanan yang dibuat jalur internal MCU.</summary>
        internal sealed record McuCoverage(Guid CoveringMcuOrderId, Guid McuDoctorId);

        private async Task<RadOperationResult<RadOrderDetailResponse>> CreateCoreAsync(
            CreateRadOrderRequest request,
            McuCoverage? coverage,
            CancellationToken cancellationToken)
        {
            // MCU-DEC-006: order MCU tidak pernah menumpang perawatan rawat inap.
            if (coverage != null && request.InpEpisodeId.HasValue && request.InpEpisodeId.Value != Guid.Empty)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    "Pesanan paket MCU tidak boleh menumpang perawatan rawat inap.");
            }

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

            // BE-RWI-104 / FR-DOK-106, VAL-DOK-46, VAL-DOK-47. Hanya pesanan rawat inap yang dibaca;
            // pesanan poliklinik dan IGD melewati blok ini tanpa perubahan perilaku apa pun.
            Guid? instructingDoctorId = null;
            var instructionStatus = RadOrderInstructionVerificationStatus.NotRequired;

            if (request.InpEpisodeId.HasValue && request.InpEpisodeId.Value != Guid.Empty)
            {
                var actorDoctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                    _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);

                if (!actorDoctorId.HasValue)
                {
                    if (!request.InstructingDoctorId.HasValue || request.InstructingDoctorId.Value == Guid.Empty)
                    {
                        return RadOperationResult<RadOrderDetailResponse>.Validation(
                            RadErrorCodes.InstructingDoctorRequired,
                            "Pilih dokter yang memberi instruksi.");
                    }

                    var bertugas = await _clinicalContextService.IsDoctorAssignedAsync(
                        request.InpEpisodeId.Value, request.InstructingDoctorId.Value, now, cancellationToken);

                    if (!bertugas)
                    {
                        return RadOperationResult<RadOrderDetailResponse>.Forbidden(
                            RadErrorCodes.InstructingDoctorNotAssigned,
                            "Dokter yang dipilih tidak sedang bertugas atas pasien ini.");
                    }

                    instructingDoctorId = request.InstructingDoctorId.Value;
                    instructionStatus = RadOrderInstructionVerificationStatus.Pending;
                }
            }
            else if (coverage == null)
            {
                var kunjunganIgdId = await PenentuDokterIgd().AmbilKunjunganUntukEncounterAsync(
                    request.EncounterId, cancellationToken);

                if (kunjunganIgdId.HasValue)
                {
                    var actorDoctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                        _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);

                    if (!actorDoctorId.HasValue)
                    {
                        if (!request.InstructingDoctorId.HasValue || request.InstructingDoctorId.Value == Guid.Empty)
                        {
                            return RadOperationResult<RadOrderDetailResponse>.Validation(
                                RadErrorCodes.InstructingDoctorRequired,
                                "Pilih dokter yang memberi instruksi.");
                        }

                        var sah = await PenentuDokterIgd().ApakahDokterPemberiInstruksiSahAsync(
                            kunjunganIgdId.Value, request.InstructingDoctorId.Value, cancellationToken);

                        if (!sah)
                        {
                            return RadOperationResult<RadOrderDetailResponse>.Forbidden(
                                RadErrorCodes.InstructingDoctorNotAssigned,
                                "Dokter yang dipilih tidak sedang bertugas atas pasien ini.");
                        }

                        instructingDoctorId = request.InstructingDoctorId.Value;
                        instructionStatus = RadOrderInstructionVerificationStatus.Pending;
                    }
                }
            }

            // Kunci alokasi nomor pesanan hanya berlaku di dalam transaksi.
            await using var transaksi = _dbContext.Database.CurrentTransaction == null
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var entity = new RadOrder
            {
                // BE-MCU-020: pesanan paket MCU berdokter pemberi instruksi = dokter MCU, tanpa
                // verifikasi instruksi (bukan pesanan perawat rawat inap).
                InstructingDoctorId = coverage?.McuDoctorId ?? instructingDoctorId,
                InstructionVerificationStatus = instructionStatus,
                CoveringMcuOrderId = coverage?.CoveringMcuOrderId,

                // Bentuk nomor Lab (RAD-RSMMC-000042), dialokasikan MAX + 1 di bawah advisory
                // lock — QBE-CODE-003. Index unik pada database menjadi penjaga terakhirnya.
                OrderNumber = await _orderNumberService.AllocateOneAsync(cancellationToken),

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

                // BE-RAD-18 — tab Pasien Persiapan v1. Kosong berarti tidak.
                RequiresPreparation = request.RequiresPreparation == true,

                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            _dbContext.RadOrders.Add(entity);
            AddHistory(entity, "Order.Create", null, entity.OrderStatus.ToString(),
                null, null, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaksi != null) await transaksi.CommitAsync(cancellationToken);

            if (entity.InpEpisodeId.HasValue && entity.InpEpisodeId.Value != Guid.Empty)
            {
                try
                {
                    var factRequest = new ClinicalMilestoneFactRequest
                    {
                        SourceContext = BillingSourceContract.RadiologySourceContext,
                        SourceAggregateId = entity.Id,
                        SourceItemId = entity.Id,
                        EffectType = BillingSourceContract.RadiologyChargeEffectType,
                        EncounterId = entity.EncounterId,
                        OccurredAt = now,
                        Quantity = 1m,
                        Unit = "Pemeriksaan",
                        TariffSnapshot = JsonSerializer.Serialize(new
                        {
                            source = "RadiologySnapshot",
                            procedureId = entity.ProcedureId,
                            modalityId = entity.ModalityId
                        }),
                        RuleSnapshot = JsonSerializer.Serialize(new
                        {
                            milestone = "OrderRequested",
                            orderNumber = entity.OrderNumber,
                            procedureId = entity.ProcedureId,
                            modalityId = entity.ModalityId
                        })
                    };
                    await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(factRequest, actorUserId, cancellationToken);
                }
                catch (Exception ex)
                {
                    await _loggerService.ErrorAsync(LogCategory, "RadOrder.EmitChargeEligibility", "Gagal menerbitkan tagihan radiologi rawat inap", ex);
                }
            }

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

            var transitionResult = await TransitionAsync(id, "Order.Cancel", RadOrderStatus.Cancelled,
                new[]
                {
                    RadOrderStatus.Draft, RadOrderStatus.Requested, RadOrderStatus.Accepted,
                    RadOrderStatus.Scheduled, RadOrderStatus.OnHold, RadOrderStatus.CancelRequested,
                },
                request, cancellationToken, reasonRequired: true);

            if (transitionResult.IsSuccess && order.InpEpisodeId.HasValue && order.InpEpisodeId.Value != Guid.Empty)
            {
                try
                {
                    var cancelFact = new ClinicalMilestoneFactRequest
                    {
                        SourceContext = BillingSourceContract.RadiologySourceContext,
                        SourceAggregateId = order.Id,
                        SourceItemId = order.Id,
                        EffectType = BillingSourceContract.RadiologyChargeEffectType,
                        EncounterId = order.EncounterId,
                        OccurredAt = DateTime.UtcNow,
                        TariffSnapshot = null,
                        RuleSnapshot = JsonSerializer.Serialize(new
                        {
                            milestone = "OrderCancelled",
                            orderNumber = order.OrderNumber,
                            procedureId = order.ProcedureId,
                            reason = request.Reason
                        })
                    };
                    await _clinicalMilestoneFactProducer.EmitClinicalCancellationAsync(cancelFact, GetCurrentUserId(), cancellationToken);
                }
                catch (Exception ex)
                {
                    await _loggerService.ErrorAsync(LogCategory, "RadOrder.EmitClinicalCancellation", "Gagal membatalkan tagihan radiologi rawat inap", ex);
                }
            }

            return transitionResult;
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

            // v1: No. Foto diberikan saat pemeriksaan dikonfirmasi. Kunci alokasinya berlaku per
            // transaksi, sehingga Konfirmasi dibungkus transaksi sendiri.
            var beriNoFoto = target == RadOrderStatus.Accepted && string.IsNullOrEmpty(order.PhotoNumber) && _photoNumberService != null;
            await using var transaksi = beriNoFoto && _dbContext.Database.CurrentTransaction == null
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;
            if (beriNoFoto)
            {
                await BeriNoFotoAsync(order, gantiBilaKategoriBerubah: false, cancellationToken);
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
                null, beriNoFoto ? $"No. Foto {order.PhotoNumber}" : request?.Reason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);
            if (transaksi != null) await transaksi.CommitAsync(cancellationToken);

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
                PhotoNumber = entity.PhotoNumber,
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
                InstructingDoctorId = entity.InstructingDoctorId,
                InstructionVerificationStatus = entity.InstructionVerificationStatus.ToString(),
                InstructionVerifiedAt = entity.InstructionVerifiedAt,
                InstructionVerifiedByUserId = entity.InstructionVerifiedByUserId,
                RequiresPreparation = entity.RequiresPreparation,
                ExaminationDate = entity.ScheduledAt ?? entity.RequestedAt ?? entity.CreateDateTime,
                RadiographerUserId = entity.RadiographerUserId,
                ExaminingDoctorId = entity.ExaminingDoctorId,
                PrintFilm = entity.PrintFilm,
                IsCoveredByMcuPackage = entity.CoveringMcuOrderId != null,
                Studies = entity.Studies
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.StudySequence)
                    .Select(RadStudyService.MapStudy)
                    .ToList(),
            };
        }
    }
}
