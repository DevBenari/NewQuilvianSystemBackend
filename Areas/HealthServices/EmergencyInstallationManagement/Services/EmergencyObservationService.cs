using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Validasi periode observasi, konsistensi waktu, transisi status observasi IGD, dan
    /// lingkup tautan pada baris pemantauan.
    /// </summary>
    public class EmergencyObservationService
    {
        // BE-IGD-046 - pesan penolakan dikunci kontrak validation 0.6.0 bagian 9. Ditulis
        // sebagai konstanta supaya teksnya tidak menyimpang ketika dipakai ulang, dan supaya
        // perubahan kontrak berikutnya cukup diubah di satu tempat.
        public const string PesanPeriodeTidakDitemukan =
            "Periode observasi tidak ditemukan.";

        public const string PesanPeriodeTertutup =
            "Periode observasi ini sudah ditutup, pemantauan baru tidak dapat ditambahkan. " +
            "Buka periode observasi baru bila pasien masih perlu dipantau.";

        public const string PesanTandaVitalTidakDitemukan =
            "Tanda vital yang dipilih tidak ditemukan. Pilih tanda vital lain atau catat tanda vital baru.";

        public const string PesanTandaVitalBedaPasien =
            "Tanda vital yang dipilih bukan milik pasien pada kunjungan ini.";

        public const string PesanTandaVitalBedaKunjungan =
            "Tanda vital yang dipilih berasal dari kunjungan lain. " +
            "Pilih tanda vital dari kunjungan IGD yang sedang dibuka.";

        public const string PesanTandaVitalTidakBerlaku =
            "Tanda vital yang dipilih sudah tidak berlaku. Pilih tanda vital lain atau catat tanda vital baru.";

        public const string PesanCatatanPerkembanganDiLuarLingkup =
            "Catatan perkembangan yang dipilih bukan milik kunjungan pasien ini.";

        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDocumentNumberService _documentNumberService;

        public EmergencyObservationService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyObservationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyObservationStatus), request.ObservationStatus))
                return "Nilai ObservationStatus tidak valid.";

            if (request.EndedAt.HasValue && request.EndedAt.Value < request.StartedAt)
                return "EndedAt tidak boleh lebih awal dari StartedAt.";

            if (request.ObservationStatus == EmergencyObservationStatus.Completed && !request.EndedAt.HasValue)
                return "EndedAt wajib diisi ketika observasi selesai.";

            var visitExists = await _dbContext.Set<EmgVisit>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.EmergencyVisitId &&
                         !x.IsDelete &&
                         x.VisitStatus != EmergencyVisitStatus.Disposed &&
                         x.VisitStatus != EmergencyVisitStatus.Cancelled,
                    cancellationToken);

            return visitExists
                ? null
                : "EmergencyVisitId tidak ditemukan atau kunjungan sudah ditutup.";
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyObservationRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyObservationRequest)request, cancellationToken);

        public bool CanTransition(EmergencyObservationStatus current, EmergencyObservationStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyObservationStatus.Active => target is EmergencyObservationStatus.Completed
                    or EmergencyObservationStatus.Escalated
                    or EmergencyObservationStatus.Cancelled,
                EmergencyObservationStatus.Escalated => target is EmergencyObservationStatus.Completed
                    or EmergencyObservationStatus.Cancelled,
                _ => false
            };
        }

        /// <summary>
        /// Hasil pemeriksaan lingkup satu baris pemantauan observasi.
        /// </summary>
        /// <remarks>
        /// Mengikuti bentuk <c>Hasil</c> yang sudah dipakai <c>EmergencyDepartureService</c>:
        /// kode status ikut dibawa karena penolakan pada task ini tidak selalu <c>400</c> -
        /// periode yang sudah ditutup dijawab <c>409</c>.
        /// </remarks>
        public sealed record HasilPemeriksaanPemantauan(int StatusCode, string? Penolakan)
        {
            public bool Lolos => Penolakan == null;

            public static HasilPemeriksaanPemantauan Ok()
                => new(StatusCodes.Status200OK, null);

            public static HasilPemeriksaanPemantauan Gagal(int statusCode, string penolakan)
                => new(statusCode, penolakan);
        }

        /// <summary>
        /// Memeriksa apakah satu baris pemantauan boleh disimpan pada periode observasi yang
        /// diminta, dan apakah tanda vital serta catatan perkembangan yang ditautkan memang
        /// milik pasien dan kunjungan yang sama.
        /// </summary>
        /// <param name="emergencyObservationId">Periode observasi tujuan baris pemantauan.</param>
        /// <param name="patientVitalSignId">Tanda vital yang ditautkan; boleh kosong.</param>
        /// <param name="progressNoteId">Catatan perkembangan yang ditautkan; boleh kosong.</param>
        /// <param name="tolakPeriodeTertutup">
        /// <c>true</c> untuk pencatatan pemantauan baru. Periode <c>Completed</c> dan
        /// <c>Cancelled</c> menolak pemantauan baru (<c>IGD-DEC-126</c>); periode
        /// <c>Active</c> dan <c>Escalated</c> tidak berubah perilakunya.
        /// </param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        /// <remarks>
        /// <para>
        /// <c>BE-IGD-046</c>. Urutan pemeriksaannya dikunci kontrak validation 0.6.0
        /// bagian 9.1 dan bukan pilihan implementasi: periode ada, periode belum ditutup,
        /// baru tautannya. Dengan begitu pemantauan yang menautkan tanda vital pasien lain
        /// pada periode yang sudah ditutup tetap dijawab <c>409</c> lebih dulu, karena
        /// mengganti tanda vitalnya pun tidak membuat permintaan itu diterima.
        /// </para>
        /// <para>
        /// Lingkupnya ditelusuri lewat jalur
        /// <c>EmgObservationDetail</c> ke <c>EmgObservation</c> ke <c>EmgVisit</c> ke
        /// pasangan <c>(PatientId, EncounterId)</c>, lalu dibandingkan dengan
        /// <c>TrxPatientVitalSign.PatientId</c> dan <c>TrxPatientVitalSign.EncounterId</c>.
        /// Seluruh pemeriksaan selesai sebelum ada satu pun data yang berubah.
        /// </para>
        /// </remarks>
        public async Task<HasilPemeriksaanPemantauan> ValidateDetailScopeAsync(
            Guid emergencyObservationId,
            Guid? patientVitalSignId,
            Guid? progressNoteId,
            bool tolakPeriodeTertutup,
            CancellationToken cancellationToken = default)
        {
            // Aturan 1 - periode harus ada. GUID nol diperlakukan sama dengan tidak ada,
            // karena baris pemantauan tidak dapat berdiri tanpa periode.
            if (emergencyObservationId == Guid.Empty)
                return HasilPemeriksaanPemantauan.Gagal(StatusCodes.Status400BadRequest, PesanPeriodeTidakDitemukan);

            // Status periode beserta identitas pasien dan kunjungannya diambil sekaligus
            // dalam satu kueri lewat navigation EmgVisit, supaya pemeriksaan lingkup di
            // bawah tidak perlu memuat ulang kunjungan yang sama.
            var periode = await _dbContext.Set<EmgObservation>()
                .AsNoTracking()
                .Where(x => x.Id == emergencyObservationId && !x.IsDelete)
                .Select(x => new
                {
                    x.ObservationStatus,
                    PatientId = x.EmergencyVisit != null ? x.EmergencyVisit.PatientId : null,
                    EncounterId = x.EmergencyVisit != null ? x.EmergencyVisit.EncounterId : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (periode == null)
                return HasilPemeriksaanPemantauan.Gagal(StatusCodes.Status400BadRequest, PesanPeriodeTidakDitemukan);

            // Aturan 2 - periode yang sudah ditutup menolak pemantauan baru.
            if (tolakPeriodeTertutup &&
                periode.ObservationStatus is EmergencyObservationStatus.Completed
                    or EmergencyObservationStatus.Cancelled)
            {
                return HasilPemeriksaanPemantauan.Gagal(StatusCodes.Status409Conflict, PesanPeriodeTertutup);
            }

            // Aturan 4 sampai 7 - tautan tanda vital. Tautan bersifat opsional: pemantauan
            // tanpa tanda vital tetap sah (aturan 11).
            if (patientVitalSignId.HasValue && patientVitalSignId.Value != Guid.Empty)
            {
                var tandaVital = await _dbContext.Set<TrxPatientVitalSign>()
                    .AsNoTracking()
                    .Where(x => x.Id == patientVitalSignId.Value && !x.IsDelete)
                    .Select(x => new { x.PatientId, x.EncounterId, x.VitalSignStatus, x.IsActive })
                    .FirstOrDefaultAsync(cancellationToken);

                if (tandaVital == null)
                    return HasilPemeriksaanPemantauan.Gagal(StatusCodes.Status400BadRequest, PesanTandaVitalTidakDitemukan);

                // Pasien pada kunjungan IGD boleh belum punya identitas (PatientId kosong),
                // sedangkan tanda vital selalu punya PatientId. Perbandingan di bawah membuat
                // kunjungan tanpa identitas menolak penautan - perilaku yang memang dituntut
                // kontrak, dan dicatat sebagai gap domain pada laporan task.
                if (tandaVital.PatientId != periode.PatientId)
                    return HasilPemeriksaanPemantauan.Gagal(StatusCodes.Status400BadRequest, PesanTandaVitalBedaPasien);

                if (tandaVital.EncounterId != periode.EncounterId)
                    return HasilPemeriksaanPemantauan.Gagal(StatusCodes.Status400BadRequest, PesanTandaVitalBedaKunjungan);

                // "Masih berlaku" memakai arti yang sudah dipakai ClinicalManagement sendiri
                // pada PatientVitalSignController: aktif, tidak dibatalkan, dan tidak
                // ditandai salah entri.
                if (!tandaVital.IsActive ||
                    tandaVital.VitalSignStatus is PatientVitalSignStatus.Cancelled
                        or PatientVitalSignStatus.EnteredInError)
                {
                    return HasilPemeriksaanPemantauan.Gagal(StatusCodes.Status400BadRequest, PesanTandaVitalTidakBerlaku);
                }
            }

            // Aturan 8 - catatan perkembangan memakai lingkup yang sama dengan tanda vital,
            // dengan satu pesan penolakan sebagaimana kontrak menuliskannya. Arti "masih
            // berlaku" mengikuti PatientIntegratedProgressNoteController: aktif dan tidak
            // dibatalkan.
            if (progressNoteId.HasValue && progressNoteId.Value != Guid.Empty)
            {
                var catatan = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                    .AsNoTracking()
                    .Where(x => x.Id == progressNoteId.Value && !x.IsDelete)
                    .Select(x => new { x.PatientId, x.EncounterId, x.IsActive, x.IsCancel })
                    .FirstOrDefaultAsync(cancellationToken);

                if (catatan == null ||
                    catatan.PatientId != periode.PatientId ||
                    catatan.EncounterId != periode.EncounterId ||
                    !catatan.IsActive ||
                    catatan.IsCancel)
                {
                    return HasilPemeriksaanPemantauan.Gagal(
                        StatusCodes.Status400BadRequest,
                        PesanCatatanPerkembanganDiLuarLingkup);
                }
            }

            return HasilPemeriksaanPemantauan.Ok();
        }

        public string GenerateNumber(DateTime now)
            => _documentNumberService.Generate("OBS", now);
    }
}
