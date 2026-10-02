using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan pemantauan pengkajian rawat inap: lini masa, keadaan tenggat,
    /// dan daftar pantau kepatuhan pengkajian awal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>BE-RWI-058</c> dan <c>BE-RWI-064</c>. Seluruh isinya <b>hanya membaca</b>. Tidak satu
    /// baris pun ditulis, dan tidak satu pun tabel baru dibuat — <c>PatientAssessmentController</c>
    /// karena itu tidak menyentuh <c>ApplicationDbContext</c> untuk ketiga permukaan baru ini,
    /// sesuai QBE-SVC-001.
    /// </para>
    /// <para>
    /// <b>Aturan yang paling menentukan di berkas ini.</b> Keadaan tenggat dihitung dari
    /// <c>DueAt</c> yang <b>sudah tersimpan</b> pada pengkajian, bukan dari kebijakan yang
    /// berlaku hari ini. Akibatnya, mengubah kebijakan tidak pernah mengubah penilaian
    /// pengkajian yang lalu — <c>AC-CAP012-04</c>.
    /// </para>
    /// <para>
    /// <b>Contoh berangka.</b> Batas pengkajian awal 24 jam. Tn. Budi masuk kamar pukul 08.00
    /// tanggal 1, pengkajian awalnya dibuat pukul 09.00 dan selesai pukul 11.00 — tenggatnya
    /// pukul 09.00 tanggal 2, sehingga keadaannya <c>OnTime</c>. Bila esok harinya batasnya
    /// diperketat menjadi 8 jam, pengkajian Tn. Budi <b>tetap</b> <c>OnTime</c>, karena tenggat
    /// miliknya sudah tersimpan sejak ia dibuat.
    /// </para>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class NursingAssessmentMonitoringService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        /// <summary>
        /// Kalimat yang dipakai ketika master kebijakan kosong. Sengaja berbeda dari kalimat
        /// "sudah tepat waktu", karena keduanya menghasilkan daftar kosong dengan arti yang
        /// sama sekali berbeda — <c>FR-KEP-025</c>.
        /// </summary>
        public const string PesanKebijakanKosong =
            "Batas waktu pengkajian belum ditetapkan.";

        public const string PesanTepatWaktu =
            "Seluruh pengkajian awal sudah tepat waktu.";

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalAssessmentPolicyService _policyService;

        public NursingAssessmentMonitoringService(
            ApplicationDbContext dbContext,
            ClinicalAssessmentPolicyService policyService)
        {
            _dbContext = dbContext;
            _policyService = policyService;
        }

        /// <summary>
        /// Jenis pengkajian yang dimiliki keperawatan. Kajian medis milik DPJP sengaja tidak
        /// ikut, karena lini masa ini dibaca ruang kerja perawat.
        /// </summary>
        private static readonly PatientAssessmentType[] JenisKeperawatan =
        [
            PatientAssessmentType.Initial,
            PatientAssessmentType.Reassessment,
            PatientAssessmentType.DailyReassessment,
            PatientAssessmentType.DischargePlanning
        ];

        // =====================================================================
        // BE-RWI-058 — lini masa
        // =====================================================================

        /// <summary>Lini masa pengkajian keperawatan satu perawatan, terurut waktu.</summary>
        /// <returns>Kosong bila perawatannya tidak ditemukan.</returns>
        public async Task<AssessmentTimelineResponse?> GetTimelineAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await AmbilEpisodeAsync(episodeId, cancellationToken);

            if (episode == null)
                return null;

            var pengkajian = await AmbilPengkajianKeperawatanAsync(episodeId, cancellationToken);
            var koreksi = await AmbilRingkasanKoreksiAsync(
                pengkajian.Select(x => x.Id).ToList(), cancellationToken);
            var kodeKebijakan = await AmbilKodeKebijakanAsync(pengkajian, cancellationToken);

            var now = DateTime.UtcNow;

            var entries = pengkajian
                .Select(x => BuatBarisLiniMasa(x, koreksi, kodeKebijakan, now))
                .ToList();

            return new AssessmentTimelineResponse
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                PatientId = episode.PatientId,
                PatientName = episode.PatientName,
                MedicalRecordNumber = episode.MedicalRecordNumber,
                AdmittedAt = episode.AdmittedAt,
                TotalAssessment = entries.Count,
                IsPolicyMasterEmpty = await _policyService.IsMasterEmptyAsync(cancellationToken),
                Entries = entries,
                Series = BuatDeretPengukuran(pengkajian, koreksi)
            };
        }

        // =====================================================================
        // BE-RWI-058 — keadaan tenggat
        // =====================================================================

        /// <summary>Keadaan tenggat seluruh pengkajian keperawatan pada satu perawatan.</summary>
        /// <returns>Kosong bila perawatannya tidak ditemukan.</returns>
        public async Task<AssessmentDueStatusResponse?> GetDueStatusAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await AmbilEpisodeAsync(episodeId, cancellationToken);

            if (episode == null)
                return null;

            var pengkajian = await AmbilPengkajianKeperawatanAsync(episodeId, cancellationToken);
            var kodeKebijakan = await AmbilKodeKebijakanAsync(pengkajian, cancellationToken);
            var masterKosong = await _policyService.IsMasterEmptyAsync(cancellationToken);

            var now = DateTime.UtcNow;

            var items = pengkajian
                .Select(x =>
                {
                    var keadaan = HitungKeadaan(x.DueAt, x.CompletedAt, x.AssessmentStatus, now);

                    return new AssessmentDueStatusItemResponse
                    {
                        AssessmentId = x.Id,
                        AssessmentNumber = x.AssessmentNumber,
                        AssessmentType = x.AssessmentType,
                        AssessmentTypeName =
                            ClinicalAssessmentPolicyService.NamaJenisPengkajian(x.AssessmentType),
                        AssessmentDateTime = x.AssessmentDateTime,
                        DueAt = x.DueAt,
                        CompletedAt = x.CompletedAt,
                        DueState = keadaan.State,
                        LateByMinutes = keadaan.LateByMinutes,
                        PolicyId = x.PolicyId,
                        PolicyCode = x.PolicyId.HasValue
                            ? kodeKebijakan.GetValueOrDefault(x.PolicyId.Value)
                            : null
                    };
                })
                .ToList();

            var pengkajianAwal = pengkajian.FirstOrDefault(x =>
                x.AssessmentType == PatientAssessmentType.Initial &&
                x.AssessmentStatus != PatientAssessmentStatus.Cancelled);

            var tenggatAwal = pengkajianAwal?.DueAt
                ?? await HitungTenggatPengkajianAwalTanpaBarisAsync(episode, cancellationToken);

            var keadaanAwal = pengkajianAwal != null
                ? HitungKeadaan(pengkajianAwal.DueAt, pengkajianAwal.CompletedAt,
                    pengkajianAwal.AssessmentStatus, now).State
                : HitungKeadaan(tenggatAwal, null, PatientAssessmentStatus.Draft, now).State;

            var terlambat = items.Count(x => x.DueState == AssessmentDueState.Late);

            return new AssessmentDueStatusResponse
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                PatientId = episode.PatientId,
                PatientName = episode.PatientName,
                AdmittedAt = episode.AdmittedAt,
                IsPolicyMasterEmpty = masterKosong,
                Explanation = masterKosong
                    ? PesanKebijakanKosong
                    : terlambat == 0
                        ? "Seluruh pengkajian pada perawatan ini masih dalam tenggat."
                        : $"{terlambat} pengkajian melewati tenggat.",
                HasInitialAssessment = pengkajianAwal != null,
                InitialAssessmentDueAt = tenggatAwal,
                InitialAssessmentDueState = keadaanAwal,
                TotalAssessment = items.Count,
                OnTimeCount = items.Count(x => x.DueState == AssessmentDueState.OnTime),
                PendingCount = items.Count(x => x.DueState == AssessmentDueState.Pending),
                LateCount = terlambat,
                NotMonitoredCount = items.Count(x => x.DueState == AssessmentDueState.NotMonitored),
                Items = items
            };
        }

        // =====================================================================
        // BE-RWI-064 — daftar pantau kepatuhan pengkajian awal
        // =====================================================================

        /// <summary>
        /// Perawatan berjalan yang pengkajian awalnya <b>belum ada</b> atau <b>sudah lewat
        /// tenggat</b>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Daftar ini tidak pernah menahan pekerjaan siapa pun. Ia hanya menunjukkan apa yang
        /// tertinggal — <c>INV-KEP-03</c>, <c>VAL-KEP-18</c>. Perawat tetap dapat mencatat
        /// tindakan pada perawatan yang muncul di sini.
        /// </para>
        /// <para>
        /// Perawatan yang <b>belum punya</b> pengkajian awal tetap ikut walaupun tenggatnya belum
        /// lewat, karena justru itu yang dicari kepala ruangan: pekerjaan yang belum dimulai.
        /// Penyaring <paramref name="onlyLate"/> mempersempitnya menjadi yang benar-benar
        /// terlambat saja.
        /// </para>
        /// </remarks>
        /// <param name="serviceUnitId">Menyaring menurut unit perawatan. Kosong berarti seluruhnya.</param>
        /// <param name="onlyLate">Benar berarti hanya yang sudah melewati tenggat.</param>
        /// <param name="pageNumber">Halaman keberapa.</param>
        /// <param name="pageSize">Banyak baris per halaman.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<PagedResult<InitialAssessmentComplianceItemResponse>>
            GetInitialAssessmentComplianceAsync(
                Guid? serviceUnitId,
                bool onlyLate,
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var now = DateTime.UtcNow;

            var episodeQuery = _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    (x.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     x.EpisodeStatus == InpEpisodeStatus.DischargePending));

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                episodeQuery = episodeQuery.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            var episodes = await episodeQuery
                .OrderBy(x => x.AdmittedAt)
                .Select(x => new
                {
                    x.Id,
                    x.EpisodeNumber,
                    x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                    x.AdmittedAt
                })
                .ToListAsync(cancellationToken);

            if (episodes.Count == 0)
            {
                return new PagedResult<InitialAssessmentComplianceItemResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = 0,
                    TotalPage = 0,
                    Items = []
                };
            }

            var episodeIds = episodes.Select(x => x.Id).ToList();

            var pengkajianAwal = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x =>
                    x.InpEpisodeId != null &&
                    episodeIds.Contains(x.InpEpisodeId.Value) &&
                    x.AssessmentType == PatientAssessmentType.Initial &&
                    !x.IsDelete &&
                    x.AssessmentStatus != PatientAssessmentStatus.Cancelled)
                .Select(x => new
                {
                    EpisodeId = x.InpEpisodeId!.Value,
                    x.Id,
                    x.AssessmentNumber,
                    x.AssessmentDateTime,
                    x.AssessmentStatus,
                    x.CompletedAt,
                    x.DueAt
                })
                .ToListAsync(cancellationToken);

            var penempatan = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x =>
                    episodeIds.Contains(x.EpisodeId) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.EndDateTime == null)
                .Select(x => new
                {
                    x.EpisodeId,
                    RoomName = x.Room != null ? x.Room.RoomName : null,
                    BedName = x.Bed != null ? x.Bed.BedName : null
                })
                .ToListAsync(cancellationToken);

            var peta = pengkajianAwal
                .GroupBy(x => x.EpisodeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.AssessmentDateTime).First());

            var petaLokasi = penempatan
                .GroupBy(x => x.EpisodeId)
                .ToDictionary(g => g.Key, g => g.First());

            var baris = new List<InitialAssessmentComplianceItemResponse>();

            foreach (var episode in episodes)
            {
                peta.TryGetValue(episode.Id, out var kajian);
                petaLokasi.TryGetValue(episode.Id, out var lokasi);

                var tenggat = kajian?.DueAt
                    ?? await HitungTenggatPengkajianAwalAsync(
                        episode.ServiceUnitId, episode.AdmittedAt, cancellationToken);

                var keadaan = kajian != null
                    ? HitungKeadaan(kajian.DueAt, kajian.CompletedAt, kajian.AssessmentStatus, now)
                    : HitungKeadaan(tenggat, null, PatientAssessmentStatus.Draft, now);

                // Yang sudah selesai tepat waktu tidak perlu ditampilkan: daftar pantau memuat
                // pekerjaan yang tertinggal, bukan seluruh pasien.
                if (kajian != null && keadaan.State == AssessmentDueState.OnTime)
                    continue;

                if (onlyLate && keadaan.State != AssessmentDueState.Late)
                    continue;

                baris.Add(new InitialAssessmentComplianceItemResponse
                {
                    EpisodeId = episode.Id,
                    EpisodeNumber = episode.EpisodeNumber,
                    PatientId = episode.PatientId,
                    PatientName = episode.PatientName,
                    MedicalRecordNumber = episode.MedicalRecordNumber,
                    ServiceUnitId = episode.ServiceUnitId,
                    ServiceUnitName = episode.ServiceUnitName,
                    RoomName = lokasi?.RoomName,
                    BedName = lokasi?.BedName,
                    AdmittedAt = episode.AdmittedAt,
                    AssessmentId = kajian?.Id,
                    AssessmentNumber = kajian?.AssessmentNumber,
                    HasInitialAssessment = kajian != null,
                    DueAt = tenggat,
                    CompletedAt = kajian?.CompletedAt,
                    DueState = keadaan.State,
                    LateByMinutes = keadaan.LateByMinutes
                });
            }

            var terurut = baris
                .OrderByDescending(x => x.DueState == AssessmentDueState.Late)
                .ThenByDescending(x => x.LateByMinutes ?? 0)
                .ThenBy(x => x.AdmittedAt)
                .ToList();

            return new PagedResult<InitialAssessmentComplianceItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = terurut.Count,
                TotalPage = (int)Math.Ceiling(terurut.Count / (double)pageSize),
                Items = terurut.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        /// <summary>
        /// Kalimat yang menjelaskan daftar pantau kosong, dibedakan menurut sebabnya.
        /// </summary>
        public async Task<string> JelaskanDaftarPantauAsync(
            int jumlahBaris,
            CancellationToken cancellationToken = default)
        {
            if (jumlahBaris > 0)
                return "Daftar pantau kepatuhan pengkajian awal berhasil diambil.";

            return await _policyService.IsMasterEmptyAsync(cancellationToken)
                ? PesanKebijakanKosong
                : PesanTepatWaktu;
        }

        // =====================================================================
        // Bagian bersama
        // =====================================================================

        /// <summary>
        /// Menghitung keadaan tenggat satu pengkajian.
        /// </summary>
        /// <remarks>
        /// Urutannya mengikat, dan urutan itulah yang membedakan "belum dipantau" dari "tepat
        /// waktu": tanpa tenggat, tidak ada penilaian sama sekali.
        /// </remarks>
        public static (AssessmentDueState State, int? LateByMinutes) HitungKeadaan(
            DateTime? dueAt,
            DateTime? completedAt,
            PatientAssessmentStatus status,
            DateTime nowUtc)
        {
            if (!dueAt.HasValue)
                return (AssessmentDueState.NotMonitored, null);

            if (status == PatientAssessmentStatus.Completed && completedAt.HasValue)
            {
                return completedAt.Value <= dueAt.Value
                    ? (AssessmentDueState.OnTime, null)
                    : (AssessmentDueState.Late,
                       (int)Math.Round((completedAt.Value - dueAt.Value).TotalMinutes));
            }

            return nowUtc <= dueAt.Value
                ? (AssessmentDueState.Pending, null)
                : (AssessmentDueState.Late,
                   (int)Math.Round((nowUtc - dueAt.Value).TotalMinutes));
        }

        private async Task<DateTime?> HitungTenggatPengkajianAwalTanpaBarisAsync(
            EpisodeRingkas episode,
            CancellationToken cancellationToken)
            => await HitungTenggatPengkajianAwalAsync(
                episode.ServiceUnitId, episode.AdmittedAt, cancellationToken);

        /// <summary>
        /// Tenggat pengkajian awal bagi perawatan yang <b>belum punya</b> barisnya sama sekali.
        /// </summary>
        /// <remarks>
        /// Dihitung dari saat pasien masuk kamar, bukan dari saat daftar dibuka. Tanpa
        /// <c>AdmittedAt</c> — perawatan yang masih <c>Draft</c> — tidak ada tenggat yang masuk
        /// akal, sehingga hasilnya kosong.
        /// </remarks>
        private async Task<DateTime?> HitungTenggatPengkajianAwalAsync(
            Guid serviceUnitId,
            DateTime? admittedAt,
            CancellationToken cancellationToken)
        {
            if (!admittedAt.HasValue)
                return null;

            var jenisPelayanan = await _dbContext
                .Set<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstServiceUnit>()
                .AsNoTracking()
                .Where(x => x.Id == serviceUnitId)
                .Select(x => (QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums.ServiceUnitType?)x.ServiceUnitType)
                .FirstOrDefaultAsync(cancellationToken);

            var hasil = await _policyService.CalculateDueAsync(
                PatientAssessmentType.Initial, jenisPelayanan, admittedAt.Value, cancellationToken);

            return hasil.DueAt;
        }

        private sealed record EpisodeRingkas(
            Guid Id,
            string EpisodeNumber,
            Guid PatientId,
            string PatientName,
            string MedicalRecordNumber,
            Guid ServiceUnitId,
            DateTime? AdmittedAt);

        private Task<EpisodeRingkas?> AmbilEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
            => _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new EpisodeRingkas(
                    x.Id,
                    x.EpisodeNumber,
                    x.PatientId,
                    x.Patient != null ? x.Patient.FullName : string.Empty,
                    x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    x.ServiceUnitId,
                    x.AdmittedAt))
                .FirstOrDefaultAsync(cancellationToken);

        private sealed record PengkajianRingkas(
            Guid Id,
            string AssessmentNumber,
            PatientAssessmentType AssessmentType,
            DateTime AssessmentDateTime,
            PatientAssessmentStatus AssessmentStatus,
            DateTime? CompletedAt,
            DateTime? DueAt,
            Guid? PolicyId,
            Guid? AssessmentByUserId,
            string? AssessmentByUserName,
            bool HasPain,
            int? PainScale,
            FallRiskStatus FallRiskStatus,
            int? FallRiskScore,
            NutritionRiskStatus NutritionRiskStatus,
            int? NutritionRiskScore);

        private Task<List<PengkajianRingkas>> AmbilPengkajianKeperawatanAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
            => _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x =>
                    x.InpEpisodeId == episodeId &&
                    !x.IsDelete &&
                    JenisKeperawatan.Contains(x.AssessmentType))
                .OrderBy(x => x.AssessmentDateTime)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => new PengkajianRingkas(
                    x.Id,
                    x.AssessmentNumber,
                    x.AssessmentType,
                    x.AssessmentDateTime,
                    x.AssessmentStatus,
                    x.CompletedAt,
                    x.DueAt,
                    x.PolicyId,
                    x.AssessmentByUserId,
                    x.AssessmentByUser != null ? x.AssessmentByUser.DisplayName : null,
                    x.HasPain,
                    x.PainScale,
                    x.FallRiskStatus,
                    x.FallRiskScore,
                    x.NutritionRiskStatus,
                    x.NutritionRiskScore))
                .ToListAsync(cancellationToken);

        private sealed record RingkasanKoreksi(int Count, int? LastSequence);

        /// <summary>
        /// Banyaknya koreksi beserta nomor urut terakhir untuk setiap pengkajian.
        /// </summary>
        /// <remarks>
        /// Dibaca dari mesin keutuhan milik <c>MedicalRecordManagement</c>, bukan dari tabel
        /// mana pun milik <c>ClinicalManagement</c> — sub-modul ini tidak punya mesin koreksi
        /// sendiri (<c>RWI-DEC-087</c>).
        /// </remarks>
        private async Task<Dictionary<Guid, RingkasanKoreksi>> AmbilRingkasanKoreksiAsync(
            List<Guid> assessmentIds,
            CancellationToken cancellationToken)
        {
            if (assessmentIds.Count == 0)
                return [];

            var keutuhan = await _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x =>
                    x.DocumentKind == ClinicalDocumentKind.Assessment &&
                    assessmentIds.Contains(x.DocumentId) &&
                    !x.IsDelete)
                .Select(x => new { x.Id, x.DocumentId, x.AddendumCount })
                .ToListAsync(cancellationToken);

            if (keutuhan.Count == 0)
                return [];

            var integrityIds = keutuhan.Select(x => x.Id).ToList();

            var urutan = await _dbContext.Set<MrcClinicalNoteAddendum>()
                .AsNoTracking()
                .Where(x => integrityIds.Contains(x.IntegrityId) && !x.IsDelete)
                .GroupBy(x => x.IntegrityId)
                .Select(g => new { IntegrityId = g.Key, LastSequence = g.Max(x => x.Sequence) })
                .ToListAsync(cancellationToken);

            var petaUrutan = urutan.ToDictionary(x => x.IntegrityId, x => x.LastSequence);

            return keutuhan.ToDictionary(
                x => x.DocumentId,
                x => new RingkasanKoreksi(
                    x.AddendumCount,
                    petaUrutan.TryGetValue(x.Id, out var urut) ? urut : null));
        }

        private async Task<Dictionary<Guid, string>> AmbilKodeKebijakanAsync(
            List<PengkajianRingkas> pengkajian,
            CancellationToken cancellationToken)
        {
            var ids = pengkajian
                .Where(x => x.PolicyId.HasValue)
                .Select(x => x.PolicyId!.Value)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return [];

            return await _dbContext
                .Set<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstClinicalAssessmentPolicy>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.PolicyCode, cancellationToken);
        }

        private static AssessmentTimelineEntryResponse BuatBarisLiniMasa(
            PengkajianRingkas x,
            Dictionary<Guid, RingkasanKoreksi> koreksi,
            Dictionary<Guid, string> kodeKebijakan,
            DateTime nowUtc)
        {
            var keadaan = HitungKeadaan(x.DueAt, x.CompletedAt, x.AssessmentStatus, nowUtc);
            koreksi.TryGetValue(x.Id, out var ringkasanKoreksi);

            return new AssessmentTimelineEntryResponse
            {
                AssessmentId = x.Id,
                AssessmentNumber = x.AssessmentNumber,
                AssessmentType = x.AssessmentType,
                AssessmentTypeName =
                    ClinicalAssessmentPolicyService.NamaJenisPengkajian(x.AssessmentType),
                AssessmentDateTime = x.AssessmentDateTime,
                AssessmentStatus = x.AssessmentStatus,
                CompletedAt = x.CompletedAt,
                DueAt = x.DueAt,
                PolicyId = x.PolicyId,
                PolicyCode = x.PolicyId.HasValue
                    ? kodeKebijakan.GetValueOrDefault(x.PolicyId.Value)
                    : null,
                DueState = keadaan.State,
                LateByMinutes = keadaan.LateByMinutes,
                AssessmentByUserId = x.AssessmentByUserId,
                AssessmentByUserName = x.AssessmentByUserName,
                AddendumCount = ringkasanKoreksi?.Count ?? 0,
                LastAddendumSequence = ringkasanKoreksi?.LastSequence,
                HasPain = x.HasPain,
                PainScale = x.PainScale,
                FallRiskStatus = x.FallRiskStatus,
                FallRiskScore = x.FallRiskScore,
                NutritionRiskStatus = x.NutritionRiskStatus,
                NutritionRiskScore = x.NutritionRiskScore
            };
        }

        /// <summary>
        /// Tiga deret pengukuran yang dituntut <c>FR-KEP-007</c>: nyeri, risiko jatuh, dan
        /// skrining gizi.
        /// </summary>
        /// <remarks>
        /// Setiap deret memuat <b>seluruh</b> titik, bukan yang terakhir saja. Itulah inti
        /// <c>AC-CAP012-02</c>: perawat perlu melihat apakah nyeri membaik, bukan sekadar berapa
        /// nilainya hari ini.
        /// </remarks>
        private static List<AssessmentMeasurementSeriesResponse> BuatDeretPengukuran(
            List<PengkajianRingkas> pengkajian,
            Dictionary<Guid, RingkasanKoreksi> koreksi)
        {
            AssessmentMeasurementPointResponse Titik(
                PengkajianRingkas x, int? skor, string? kategori)
            {
                koreksi.TryGetValue(x.Id, out var ringkasan);

                return new AssessmentMeasurementPointResponse
                {
                    AssessmentId = x.Id,
                    AssessmentNumber = x.AssessmentNumber,
                    AssessmentType = x.AssessmentType,
                    MeasuredAt = x.AssessmentDateTime,
                    Score = skor,
                    Category = kategori,
                    AddendumCount = ringkasan?.Count ?? 0,
                    LastAddendumSequence = ringkasan?.LastSequence
                };
            }

            return
            [
                new AssessmentMeasurementSeriesResponse
                {
                    Measurement = "pain",
                    Label = "Nyeri",
                    Points = pengkajian
                        .Where(x => x.HasPain || x.PainScale.HasValue)
                        .Select(x => Titik(x, x.PainScale, x.HasPain ? "Ada nyeri" : "Tidak ada nyeri"))
                        .ToList()
                },
                new AssessmentMeasurementSeriesResponse
                {
                    Measurement = "fallRisk",
                    Label = "Risiko jatuh",
                    Points = pengkajian
                        .Where(x => x.FallRiskStatus != FallRiskStatus.Unknown || x.FallRiskScore.HasValue)
                        .Select(x => Titik(x, x.FallRiskScore, x.FallRiskStatus.ToString()))
                        .ToList()
                },
                new AssessmentMeasurementSeriesResponse
                {
                    Measurement = "nutrition",
                    Label = "Skrining gizi",
                    Points = pengkajian
                        .Where(x => x.NutritionRiskStatus != NutritionRiskStatus.Unknown || x.NutritionRiskScore.HasValue)
                        .Select(x => Titik(x, x.NutritionRiskScore, x.NutritionRiskStatus.ToString()))
                        .ToList()
                }
            ];
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }
    }
}
