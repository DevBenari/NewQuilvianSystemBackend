using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using RegPatientEncounter = QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Penilaian kelengkapan berkas satu kunjungan dan daftar kerja petugas rekam medis (`BE-39`,
    /// `AGG-RM-03`, `RM-DEC-045`, `046`, `050`, kontrak API 0.2.0 A2).
    /// </summary>
    /// <remarks>
    /// <b>Tiga syarat lengkap</b> (`RM-DEC-045`): (1) catatan yang tunduk keutuhan sudah
    /// ditandatangani atau disahkan lewat addendum pengesahan (`RM-DEC-049`); (2) tidak ada hasil
    /// penunjang yang ditunggu; (3) dokumen wajib menurut profil (`BE-37`) ada.
    ///
    /// <b>Keadaan utama</b> = kekurangan berprioritas tertinggi: <c>Undetermined</c> →
    /// <c>AwaitingResult</c> → <c>Incomplete</c> → <c>Complete</c>. <c>Complete</c> hanya bila nol
    /// kekurangan DAN nol gangguan sumber (`RM-DEC-044`). Kriteria yang sumbernya belum tersambung
    /// menghasilkan <c>SourceUnavailable</c>, bukan dianggap terpenuhi.
    ///
    /// <b>Tidak menahan apa pun</b> (`RM-DEC-046`). Service ini tidak dipanggil dari jalur
    /// penutupan; penutupan hanya menaruh id kunjungan pada <see cref="MedicalRecordCompletenessQueue"/>.
    ///
    /// <b>Sumber dibaca sebelum menyimpan</b> (A4.1). Seluruh pembacaan selesai lebih dulu; baru
    /// sesudahnya satu <c>SaveChanges</c> menyimpan penilaian kunjungan itu.
    /// </remarks>
    public class MedicalRecordCompletenessService
    {
        public const string RunningMessage = "Kunjungan ini masih berjalan. Kelengkapan dinilai setelah pelayanan selesai.";
        public const string CancelledMessage = "Kunjungan ini dibatalkan dan tidak dinilai kelengkapannya.";
        public const string NotFoundMessage = "Kunjungan tidak ditemukan.";
        public const string NotEvaluatedMessage = "Kunjungan ini belum dinilai kelengkapannya.";
        public const string BusyMessage = "Penilaian kunjungan ini sedang diperbarui. Coba lagi.";

        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        /// <summary>Kriteria yang dijawab rekam medis sendiri dari model `ClinicalManagement` (A4.11).</summary>
        public static readonly IReadOnlySet<CompletenessCriterion> KriteriaGenerik = new HashSet<CompletenessCriterion>
        {
            CompletenessCriterion.InitialAssessmentAny,
            CompletenessCriterion.InitialNursingAssessment,
            CompletenessCriterion.InitialMedicalAssessment,
            CompletenessCriterion.PrimaryDiagnosis,
            CompletenessCriterion.DoctorNoteOrConsultation,
            CompletenessCriterion.ProgressNoteAny,
            CompletenessCriterion.TransfusionMonitoring
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IReadOnlyList<IMedicalRecordCompletenessProbe> _probes;
        private readonly TimeSpan _batasWaktuSumber;
        private readonly int _menitPeriksaUlangBaru;
        private readonly int _jamPeriksaUlangLama;
        private readonly int _ukuranPutaran;
        private readonly TimeProvider _jam;

        /// <param name="timeProvider">Jam; kosong = jam sistem. Disuntikkan uji pemeriksaan berkala (`BE-40`).</param>
        public MedicalRecordCompletenessService(
            ApplicationDbContext dbContext,
            IEnumerable<IMedicalRecordCompletenessProbe> probes,
            IConfiguration configuration,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _probes = probes.ToList();
            _jam = timeProvider ?? TimeProvider.System;
            _ukuranPutaran = Math.Clamp(configuration.GetValue("MedicalRecordCompleteness:BatchSize", UkuranPutaranBawaan), 1, 1000);

            _batasWaktuSumber = TimeSpan.FromSeconds(Math.Max(1,
                configuration.GetValue("MedicalRecord:SourceTimeoutSeconds", MedicalRecordTimelineService.BatasWaktuSumberBawaanDetik)));

            // A4.13: bawaan teknis, bukan kebijakan rumah sakit.
            _menitPeriksaUlangBaru = Math.Max(1, configuration.GetValue("MedicalRecordCompleteness:RecentRecheckMinutes", 60));
            _jamPeriksaUlangLama = Math.Max(1, configuration.GetValue("MedicalRecordCompleteness:OldRecheckHours", 24));
        }

        // =====================================================================
        // Penilaian
        // =====================================================================

        /// <summary>
        /// Menilai satu kunjungan sekarang. Kunjungan yang tutup klinis sejak rilis disimpan;
        /// kunjungan sebelum rilis dinilai tanpa disimpan (`RM-DEC-050`).
        /// </summary>
        public async Task<MedicalRecordCompletenessResult> EvaluateAsync(
            Guid encounterId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kunjungan = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == encounterId && !x.IsDelete)
                .Select(x => new Kunjungan(
                    x.Id, x.EncounterNumber, x.PatientId, x.EncounterType, x.EncounterStatus,
                    x.ServiceUnit != null ? (ServiceUnitType?)x.ServiceUnit.ServiceUnitType : null,
                    x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    x.Patient != null ? x.Patient.FullName : null))
                .FirstOrDefaultAsync(cancellationToken);

            if (kunjungan == null)
                return Gagal(MedicalRecordCompletenessStatus.NotFound, NotFoundMessage);

            if (kunjungan.Status is EncounterStatus.Cancelled or EncounterStatus.NoShow)
            {
                await KeluarkanDariDaftarKerjaAsync(encounterId, actorUserId, cancellationToken);
                return Gagal(MedicalRecordCompletenessStatus.Cancelled, CancelledMessage);
            }

            var tutup = (await ClinicalClosureTimeReader.ReadAsync(_dbContext, [encounterId], cancellationToken))
                .GetValueOrDefault(encounterId);

            if (tutup.Waktu == default)
                return Gagal(MedicalRecordCompletenessStatus.Running, RunningMessage);

            var sekarang = _jam.GetUtcNow().UtcDateTime;
            var hasil = await NilaiAsync(kunjungan, cancellationToken);

            var detail = KeDetail(kunjungan, tutup, hasil, sekarang, firstCompletedAt: null, persisted: false);
            await LengkapiNamaPenanggungJawabAsync(detail, hasil.Kekurangan, cancellationToken);

            if (tutup.Turunan || tutup.ClosureId == null)
            {
                return new MedicalRecordCompletenessResult(
                    MedicalRecordCompletenessStatus.Success, detail,
                    "Kunjungan ini ditutup sebelum fitur kelengkapan dirilis. Penilaiannya ditampilkan tanpa disimpan.");
            }

            var simpan = await SimpanAsync(kunjungan, tutup, hasil, sekarang, actorUserId, cancellationToken);
            if (simpan == null)
                return Gagal(MedicalRecordCompletenessStatus.Conflict, BusyMessage);

            detail.IsPersisted = true;
            detail.FirstCompletedAt = simpan.FirstCompletedAt;

            return new MedicalRecordCompletenessResult(
                MedicalRecordCompletenessStatus.Success, detail, "Kelengkapan berkas kunjungan berhasil dinilai.");
        }

        /// <summary>
        /// Untuk antrean latar: <c>false</c> bila penutupan klinis kunjungan belum terlihat (transaksi
        /// penutupan belum di-commit), supaya dicoba lagi; selain itu <c>true</c>.
        /// </summary>
        public async Task<bool> EvaluateQueuedAsync(Guid encounterId, CancellationToken cancellationToken = default)
        {
            var adaPenutupan = await _dbContext.Set<MrcEncounterClinicalClosure>()
                .AsNoTracking()
                .AnyAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

            if (!adaPenutupan)
                return false;

            await EvaluateAsync(encounterId, Guid.Empty, cancellationToken);
            return true;
        }

        // =====================================================================
        // Pemeriksaan berkala (BE-40, A4.13)
        // =====================================================================

        /// <summary>Jumlah kunjungan per putaran bawaan (A4.13 — sama dengan ukuran potongan penguncian).</summary>
        public const int UkuranPutaranBawaan = 200;

        /// <summary>
        /// Satu putaran pemeriksaan berkala. Paling banyak <c>MedicalRecordCompleteness:BatchSize</c>
        /// kunjungan (bawaan 200), dalam urutan:
        /// <list type="number">
        /// <item>penutupan klinis sejak rilis yang belum punya penilaian — susulan antrean memori yang
        /// hilang saat server dinyalakan ulang (`BE-39`);</item>
        /// <item>penilaian yang belum <c>Complete</c> dan <c>NextCheckAt</c>-nya sudah lewat, paling
        /// lama jatuh tempo lebih dulu.</item>
        /// </list>
        /// Kunjungan <c>Complete</c> tidak diperiksa berkala (<c>NextCheckAt</c> kosong). Kegagalan satu
        /// kunjungan dicatat sebagai jumlah dan tidak menghentikan putaran.
        /// </summary>
        public async Task<MedicalRecordCompletenessRecheckSummary> RunRecheckRoundAsync(CancellationToken cancellationToken = default)
        {
            var sekarang = _jam.GetUtcNow().UtcDateTime;
            var ringkasan = new MedicalRecordCompletenessRecheckSummary();

            var belumDinilai = await _dbContext.Set<MrcEncounterClinicalClosure>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && !_dbContext.Set<MrcEncounterCompletenessAssessment>().Any(a => a.EncounterId == x.EncounterId && !a.IsDelete)
                            && x.Encounter != null
                            && x.Encounter.EncounterStatus != EncounterStatus.Cancelled
                            && x.Encounter.EncounterStatus != EncounterStatus.NoShow)
                .OrderBy(x => x.ClinicalClosedAt)
                .Select(x => x.EncounterId)
                .Take(_ukuranPutaran)
                .ToListAsync(cancellationToken);

            var jatuhTempo = belumDinilai.Count >= _ukuranPutaran
                ? []
                : await _dbContext.Set<MrcEncounterCompletenessAssessment>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && !x.IsExcluded
                                && x.State != CompletenessState.Complete
                                && x.NextCheckAt != null && x.NextCheckAt <= sekarang)
                    .OrderBy(x => x.NextCheckAt)
                    .Select(x => x.EncounterId)
                    .Take(_ukuranPutaran - belumDinilai.Count)
                    .ToListAsync(cancellationToken);

            ringkasan.FirstEvaluations = belumDinilai.Count;

            foreach (var kunjungan in belumDinilai.Concat(jatuhTempo))
            {
                try
                {
                    var hasil = await EvaluateAsync(kunjungan, Guid.Empty, cancellationToken);
                    ringkasan.Considered++;

                    if (hasil.Detail != null)
                        ringkasan.ByState[hasil.Detail.State] = ringkasan.ByState.GetValueOrDefault(hasil.Detail.State) + 1;
                    else
                        ringkasan.Skipped++;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    ringkasan.Considered++;
                    ringkasan.Failed++;
                    _dbContext.ChangeTracker.Clear();
                }
            }

            return ringkasan;
        }

        /// <summary>
        /// Pemicu perubahan profil dokumen wajib (`EVT-RM-03`, `BE-40` kriteria 3): setiap penilaian
        /// yang belum <c>Complete</c> dijadikan jatuh tempo sekarang, sehingga putaran berikutnya
        /// menilainya ulang dengan aturan baru. Tidak menilai di tempat — perubahan profil tidak
        /// menunggu ribuan penilaian.
        /// </summary>
        /// <returns>Jumlah penilaian yang ditandai.</returns>
        public static Task<int> MarkIncompleteDueAsync(
            ApplicationDbContext dbContext,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
            // Satu UPDATE berbasis set: tidak memuat ribuan baris, dan Version ikut naik supaya
            // penilaian yang sedang berjalan bersamaan tidak menimpa tanda ini tanpa ketahuan.
            => dbContext.Set<MrcEncounterCompletenessAssessment>()
                .Where(x => !x.IsDelete && !x.IsExcluded && x.State != CompletenessState.Complete
                            && (x.NextCheckAt == null || x.NextCheckAt > nowUtc))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.NextCheckAt, nowUtc)
                    .SetProperty(x => x.Version, x => x.Version + 1), cancellationToken);

        private async Task<HasilPenilaian> NilaiAsync(Kunjungan kunjungan, CancellationToken cancellationToken)
        {
            var kekurangan = new List<BarisKekurangan>();

            // ---- Profil (A4.11 "Penyusunan profil satu kunjungan") ----
            var profilAktif = await _dbContext.Set<MstMedicalRecordCompletenessProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .Select(x => new Profil(
                    x.Code, x.EncounterType, x.ServiceUnitType, x.Condition,
                    x.Items.Where(i => i.IsActive && !i.IsDelete).Select(i => i.Criterion).ToList()))
                .ToListAsync(cancellationToken);

            var profilDasar = PilihProfilDasar(profilAktif, kunjungan.EncounterType, kunjungan.ServiceUnitType);
            var profilKondisi = profilAktif.Where(x => x.Condition != CompletenessCondition.None).ToList();

            if (profilDasar == null)
            {
                kekurangan.Add(new BarisKekurangan(
                    CompletenessDeficiencyType.RuleNotFound,
                    $"Tidak ada profil dokumen wajib untuk {MedicalRecordCompletenessProfileService.EncounterTypeLabel(kunjungan.EncounterType)} " +
                    $"di unit {MedicalRecordCompletenessProfileService.ServiceUnitTypeLabel(kunjungan.ServiceUnitType)}."));
            }

            var kriteriaKandidat = (profilDasar?.Criteria ?? [])
                .Concat(profilKondisi.SelectMany(x => x.Criteria))
                .ToHashSet();
            var kondisiKandidat = profilKondisi.Select(x => x.Condition).ToHashSet();

            // ---- Pembacaan sumber: generik, lalu setiap pembaca uji (berurutan, satu context) ----
            var terpenuhi = await NilaiKriteriaGenerikAsync(kunjungan.Id, kriteriaKandidat, cancellationToken);
            var kondisiAda = new HashSet<CompletenessCondition>();

            if (kondisiKandidat.Contains(CompletenessCondition.Transfusion) && await AdaTransfusiAsync(kunjungan.Id, cancellationToken))
                kondisiAda.Add(CompletenessCondition.Transfusion);

            var kriteriaGagal = new HashSet<CompletenessCriterion>();
            var tertunda = new List<MedicalRecordPendingResult>();

            foreach (var probe in _probes)
            {
                var kriteriaDiminta = probe.Criteria.Where(kriteriaKandidat.Contains).ToList();
                var perluDipanggil = kriteriaDiminta.Count > 0
                                     || probe.Conditions.Any(kondisiKandidat.Contains)
                                     || probe.ReportsPendingResults;
                if (!perluDipanggil)
                    continue;

                using var batas = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                batas.CancelAfter(_batasWaktuSumber);

                try
                {
                    var jawaban = await probe.ProbeAsync(new MedicalRecordCompletenessProbeQuery
                    {
                        EncounterId = kunjungan.Id,
                        PatientId = kunjungan.PatientId,
                        Criteria = kriteriaDiminta
                    }, batas.Token);

                    terpenuhi.UnionWith(jawaban.SatisfiedCriteria.Where(probe.Criteria.Contains));
                    kondisiAda.UnionWith(jawaban.PresentConditions.Where(probe.Conditions.Contains));
                    if (probe.ReportsPendingResults)
                        tertunda.AddRange(jawaban.PendingResults);
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    // RM-DEC-044: gangguan sumber dinyatakan, tidak pernah dibaca sebagai "lengkap".
                    kriteriaGagal.UnionWith(kriteriaDiminta);
                    kekurangan.Add(new BarisKekurangan(
                        CompletenessDeficiencyType.SourceUnavailable,
                        $"Sumber {probe.SourceModule} tidak dapat dibaca saat menilai. Kelengkapan belum dapat dipastikan."));
                }
            }

            // Kondisi yang tidak dijawab sumber mana pun (operasi, sesi hemodialisa sampai pembacanya
            // tersambung) dianggap tidak ada: menuntutnya akan membuat SETIAP kunjungan tidak dapat
            // dipastikan. Kunjungan yang memang berkondisi itu tetap tertahan oleh kriteria jenis
            // kunjungannya sendiri yang juga belum tersambung.
            var kriteriaDituntut = (profilDasar?.Criteria ?? [])
                .Concat(profilKondisi.Where(x => kondisiAda.Contains(x.Condition)).SelectMany(x => x.Criteria))
                .Distinct()
                .OrderBy(x => (int)x)
                .ToList();

            var dapatDijawab = KriteriaGenerik.Concat(_probes.SelectMany(x => x.Criteria)).ToHashSet();

            foreach (var kriteria in kriteriaDituntut)
            {
                if (kriteriaGagal.Contains(kriteria))
                    continue; // sudah dinyatakan lewat gangguan sumbernya

                if (!dapatDijawab.Contains(kriteria))
                {
                    kekurangan.Add(new BarisKekurangan(
                        CompletenessDeficiencyType.SourceUnavailable,
                        $"{MedicalRecordCompletenessProfileService.CriterionLabel(kriteria)} belum dapat diperiksa: sumbernya belum tersambung ke rekam medis."));
                    continue;
                }

                if (!terpenuhi.Contains(kriteria))
                {
                    kekurangan.Add(new BarisKekurangan(
                        CompletenessDeficiencyType.MissingRequiredDocument,
                        $"{MedicalRecordCompletenessProfileService.CriterionLabel(kriteria)} belum ada."));
                }
            }

            // ---- Syarat 2: hasil penunjang yang ditunggu ----
            foreach (var hasil in tertunda)
            {
                kekurangan.Add(new BarisKekurangan(
                    CompletenessDeficiencyType.PendingResult,
                    $"{(string.IsNullOrWhiteSpace(hasil.Name) ? MedicalRecordTimelineService.NamaJenis(hasil.Kind) : hasil.Name)} belum dirilis.",
                    hasil.Kind, hasil.DocumentId));
            }

            // ---- Syarat 1: catatan yang belum ditandatangani dan belum disahkan ----
            kekurangan.AddRange(await CatatanBelumDitandatanganiAsync(kunjungan.Id, cancellationToken));

            var profilDipakai = new List<Profil>();
            if (profilDasar != null)
                profilDipakai.Add(profilDasar);
            profilDipakai.AddRange(profilKondisi.Where(x => kondisiAda.Contains(x.Condition)));

            return new HasilPenilaian(TentukanKeadaan(kekurangan), kekurangan, profilDipakai);
        }

        /// <summary>
        /// Profil berunit yang persis menggantikan profil umum jenis kunjungan yang sama; bila
        /// keduanya tidak ada, <c>null</c> (`RuleNotFound`).
        /// </summary>
        private static Profil? PilihProfilDasar(
            List<Profil> profil,
            EncounterType jenisKunjungan,
            ServiceUnitType? jenisUnit)
        {
            var dasar = profil.Where(x => x.Condition == CompletenessCondition.None && x.EncounterType == jenisKunjungan).ToList();

            return dasar.FirstOrDefault(x => jenisUnit != null && x.ServiceUnitType == jenisUnit)
                   ?? dasar.FirstOrDefault(x => x.ServiceUnitType == null);
        }

        /// <summary>Urutan prioritas S-B: Undetermined → AwaitingResult → Incomplete → Complete.</summary>
        public static CompletenessState TentukanKeadaan(IReadOnlyCollection<BarisKekurangan> kekurangan)
        {
            if (kekurangan.Any(x => x.Type is CompletenessDeficiencyType.SourceUnavailable or CompletenessDeficiencyType.RuleNotFound))
                return CompletenessState.Undetermined;

            if (kekurangan.Any(x => x.Type == CompletenessDeficiencyType.PendingResult))
                return CompletenessState.AwaitingResult;

            return kekurangan.Count > 0 ? CompletenessState.Incomplete : CompletenessState.Complete;
        }

        private async Task<HashSet<CompletenessCriterion>> NilaiKriteriaGenerikAsync(
            Guid kunjungan,
            HashSet<CompletenessCriterion> diminta,
            CancellationToken cancellationToken)
        {
            var hasil = new HashSet<CompletenessCriterion>();

            async Task CekAsync(CompletenessCriterion kriteria, Func<Task<bool>> cek)
            {
                if (diminta.Contains(kriteria) && await cek())
                    hasil.Add(kriteria);
            }

            IQueryable<TrxPatientAssessment> Asesmen(params PatientAssessmentType[] jenis) => _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x => x.EncounterId == kunjungan && !x.IsDelete && !x.IsCancel
                            && x.AssessmentStatus == PatientAssessmentStatus.Completed
                            && jenis.Contains(x.AssessmentType));

            var cppt = _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .Where(x => x.EncounterId == kunjungan && !x.IsDelete && !x.IsCancel);

            // RM-DEC-053
            await CekAsync(CompletenessCriterion.InitialAssessmentAny, () =>
                Asesmen(PatientAssessmentType.Initial, PatientAssessmentType.MedicalInitial).AnyAsync(cancellationToken));
            await CekAsync(CompletenessCriterion.InitialNursingAssessment, () =>
                Asesmen(PatientAssessmentType.Initial).AnyAsync(cancellationToken));
            await CekAsync(CompletenessCriterion.InitialMedicalAssessment, () =>
                Asesmen(PatientAssessmentType.MedicalInitial).AnyAsync(cancellationToken));

            // RM-DEC-054: IsPrimary ATAU berjenis Primary.
            await CekAsync(CompletenessCriterion.PrimaryDiagnosis, () => _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .AnyAsync(x => x.EncounterId == kunjungan && !x.IsDelete && !x.IsCancel
                               && (x.IsPrimary || x.DiagnosisType == PatientDiagnosisType.Primary)
                               && x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled
                               && x.DiagnosisStatus != PatientDiagnosisStatus.RuledOut, cancellationToken));

            // RM-DEC-055: CPPT berprofesi dokter ATAU konsultasi dokter.
            await CekAsync(CompletenessCriterion.DoctorNoteOrConsultation, async () =>
                await cppt.AnyAsync(x => x.ProfessionType == "Doctor" || x.NoteKind == CpptNoteKind.PhysicianNote, cancellationToken)
                || await _dbContext.Set<TrxDoctorConsultation>()
                    .AsNoTracking()
                    .AnyAsync(x => x.EncounterId == kunjungan && !x.IsDelete && !x.IsCancel
                                   && x.ConsultationStatus != DoctorConsultationStatus.Cancelled, cancellationToken));

            await CekAsync(CompletenessCriterion.ProgressNoteAny, () => cppt.AnyAsync(cancellationToken));

            // Setiap monitoring yang tidak batal berstatus selesai.
            await CekAsync(CompletenessCriterion.TransfusionMonitoring, async () => !await _dbContext.Set<CliTransfusionMonitoring>()
                .AsNoTracking()
                .AnyAsync(x => x.EncounterId == kunjungan && !x.IsDelete && !x.IsCancel
                               && x.Status != CliTransfusionMonitoringStatus.Cancelled
                               && x.Status != CliTransfusionMonitoringStatus.Completed, cancellationToken));

            return hasil;
        }

        private Task<bool> AdaTransfusiAsync(Guid kunjungan, CancellationToken cancellationToken)
            => _dbContext.Set<CliTransfusionMonitoring>()
                .AsNoTracking()
                .AnyAsync(x => x.EncounterId == kunjungan && !x.IsDelete && !x.IsCancel
                               && x.Status != CliTransfusionMonitoringStatus.Cancelled, cancellationToken);

        /// <summary>
        /// Syarat 1: catatan konsep, dan catatan <c>LockedUnsigned</c> yang belum punya addendum
        /// pengesahan (`RM-DEC-049`, `BE-38`).
        /// </summary>
        private async Task<List<BarisKekurangan>> CatatanBelumDitandatanganiAsync(Guid kunjungan, CancellationToken cancellationToken)
        {
            var catatan = await _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.EncounterId == kunjungan && !x.IsDelete
                            && (x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Draft
                                || (x.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned
                                    && !x.Addendums.Any(a => !a.IsDelete && a.AddendumPurpose == ClinicalNoteAddendumPurpose.Attestation))))
                .Select(x => new { x.DocumentKind, x.DocumentId, x.AuthorUserId, x.IsAuthorKnown })
                .ToListAsync(cancellationToken);

            return catatan
                .OrderBy(x => (int)x.DocumentKind)
                .Select(x => new BarisKekurangan(
                    CompletenessDeficiencyType.UnsignedDocument,
                    $"{MedicalRecordTimelineService.NamaJenis(x.DocumentKind)} belum ditandatangani.",
                    x.DocumentKind, x.DocumentId,
                    x.IsAuthorKnown && x.AuthorUserId != Guid.Empty ? x.AuthorUserId : null))
                .ToList();
        }

        // =====================================================================
        // Penyimpanan
        // =====================================================================

        /// <summary>
        /// Satu <c>SaveChanges</c> per kunjungan. Token konkurensi <c>Version</c> dan index unik
        /// kunjungan menjaga dua penilaian serentak; yang kalah dicoba sekali lagi dengan keadaan
        /// terbaru, lalu menyerah (`NFR-003`).
        /// </summary>
        private async Task<MrcEncounterCompletenessAssessment?> SimpanAsync(
            Kunjungan kunjungan,
            ClinicalClosureTime tutup,
            HasilPenilaian hasil,
            DateTime sekarang,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            for (var percobaan = 0; percobaan < 2; percobaan++)
            {
                var penilaian = await _dbContext.Set<MrcEncounterCompletenessAssessment>()
                    .FirstOrDefaultAsync(x => x.EncounterId == kunjungan.Id && !x.IsDelete, cancellationToken);

                if (penilaian == null)
                {
                    penilaian = new MrcEncounterCompletenessAssessment
                    {
                        Id = Guid.NewGuid(),
                        EncounterId = kunjungan.Id,
                        PatientId = kunjungan.PatientId,
                        ClinicalClosureId = tutup.ClosureId!.Value,
                        Version = 1,
                        CreateDateTime = sekarang,
                        CreateBy = actorUserId
                    };
                    _dbContext.Set<MrcEncounterCompletenessAssessment>().Add(penilaian);
                }
                else
                {
                    penilaian.Version += 1;
                    penilaian.UpdateDateTime = sekarang;
                    penilaian.UpdateBy = actorUserId;
                }

                penilaian.State = hasil.Keadaan;
                penilaian.DeficiencyCount = hasil.Kekurangan.Count;
                penilaian.Deficiencies = JsonSerializer.Serialize(hasil.Kekurangan, Json);
                penilaian.EvaluatedAt = sekarang;
                penilaian.ProfileFingerprint = SidikProfil(hasil.Profil);
                penilaian.IsExcluded = false;

                // Waktu pertama kali lengkap tidak pernah dihapus bila keadaan turun (S-B).
                if (hasil.Keadaan == CompletenessState.Complete && penilaian.FirstCompletedAt == null)
                    penilaian.FirstCompletedAt = sekarang;

                penilaian.NextCheckAt = hasil.Keadaan == CompletenessState.Complete
                    ? null
                    : sekarang - tutup.Waktu <= TimeSpan.FromDays(30)
                        ? sekarang.AddMinutes(_menitPeriksaUlangBaru)
                        : sekarang.AddHours(_jamPeriksaUlangLama);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return penilaian;
                }
                catch (DbUpdateException)
                {
                    _dbContext.Entry(penilaian).State = EntityState.Detached;
                }
            }

            return null;
        }

        private async Task KeluarkanDariDaftarKerjaAsync(Guid encounterId, Guid actorUserId, CancellationToken cancellationToken)
        {
            var penilaian = await _dbContext.Set<MrcEncounterCompletenessAssessment>()
                .FirstOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete && !x.IsExcluded, cancellationToken);

            if (penilaian == null)
                return;

            // Baris tidak dihapus (S-B).
            penilaian.IsExcluded = true;
            penilaian.NextCheckAt = null;
            penilaian.Version += 1;
            penilaian.UpdateDateTime = _jam.GetUtcNow().UtcDateTime;
            penilaian.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penilaian lain menyimpan bersamaan; penandaan diulang pada penilaian berikutnya.
                _dbContext.Entry(penilaian).State = EntityState.Detached;
            }
        }

        private static string SidikProfil(IEnumerable<Profil> profil)
        {
            var teks = string.Join("|", profil
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .Select(x => x.Code + ":" + string.Join(",", x.Criteria.Select(c => (int)c).OrderBy(c => c))));

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(teks))).ToLowerInvariant();
        }

        // =====================================================================
        // Pembacaan daftar kerja
        // =====================================================================

        public async Task<MedicalRecordCompletenessDetailResponse?> GetDetailAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var baris = await DaftarKerja()
                .Where(x => x.EncounterId == encounterId)
                .Select(x => new
                {
                    x.EncounterId,
                    x.Encounter!.EncounterNumber,
                    x.PatientId,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    x.Encounter.EncounterType,
                    ServiceUnitType = x.Encounter.ServiceUnit != null ? (ServiceUnitType?)x.Encounter.ServiceUnit.ServiceUnitType : null,
                    ServiceUnitName = x.Encounter.ServiceUnit != null ? x.Encounter.ServiceUnit.ServiceUnitName : null,
                    x.Encounter.EncounterStatus,
                    x.ClinicalClosure!.ClinicalClosedAt,
                    x.State,
                    x.Deficiencies,
                    x.EvaluatedAt,
                    x.FirstCompletedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (baris == null)
                return null;

            var kekurangan = BacaKekurangan(baris.Deficiencies);

            // Kode profil tidak disimpan (hanya sidiknya); dipilih ulang dari master saat ini.
            var profilAktif = await _dbContext.Set<MstMedicalRecordCompletenessProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .Select(x => new Profil(x.Code, x.EncounterType, x.ServiceUnitType, x.Condition, new List<CompletenessCriterion>()))
                .ToListAsync(cancellationToken);

            var detail = new MedicalRecordCompletenessDetailResponse
            {
                EncounterId = baris.EncounterId,
                EncounterNumber = baris.EncounterNumber,
                PatientId = baris.PatientId,
                MedicalRecordNumber = baris.MedicalRecordNumber,
                PatientName = baris.PatientName,
                EncounterType = baris.EncounterType,
                ServiceUnitName = baris.ServiceUnitName,
                ClinicalClosedAt = baris.ClinicalClosedAt,
                IsClinicalClosureDerived = false,
                State = baris.State,
                StateLabel = StateLabel(baris.State),
                Deficiencies = kekurangan.Select(KeRespons).ToList(),
                EvaluatedAt = baris.EvaluatedAt,
                FirstCompletedAt = baris.FirstCompletedAt,
                IsPersisted = true,
                ProfileCodes = PilihProfilDasar(profilAktif, baris.EncounterType, baris.ServiceUnitType) is { } dasar
                    ? [dasar.Code]
                    : []
            };

            await LengkapiNamaPenanggungJawabAsync(detail, kekurangan, cancellationToken);
            return detail;
        }

        public async Task<PagedResult<MedicalRecordCompletenessListItemResponse>> GetPagedAsync(
            MedicalRecordCompletenessPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var halaman = query.Page < 1 ? 1 : query.Page;
            var ukuran = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var dasar = Saring(DaftarKerja(), query);
            dasar = query.State.HasValue
                ? dasar.Where(x => x.State == query.State.Value)
                : dasar.Where(x => x.State != CompletenessState.Complete);

            var total = await dasar.CountAsync(cancellationToken);

            var baris = await dasar
                .OrderBy(x => x.ClinicalClosure!.ClinicalClosedAt)
                .ThenBy(x => x.EncounterId)
                .Skip((halaman - 1) * ukuran)
                .Take(ukuran)
                .Select(x => new MedicalRecordCompletenessListItemResponse
                {
                    EncounterId = x.EncounterId,
                    EncounterNumber = x.Encounter!.EncounterNumber,
                    PatientId = x.PatientId,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    EncounterType = x.Encounter.EncounterType,
                    ServiceUnitName = x.Encounter.ServiceUnit != null ? x.Encounter.ServiceUnit.ServiceUnitName : null,
                    ClinicalClosedAt = x.ClinicalClosure!.ClinicalClosedAt,
                    State = x.State,
                    DeficiencyCount = x.DeficiencyCount,
                    EvaluatedAt = x.EvaluatedAt
                })
                .ToListAsync(cancellationToken);

            foreach (var item in baris)
                item.StateLabel = StateLabel(item.State);

            return new PagedResult<MedicalRecordCompletenessListItemResponse>
            {
                PageNumber = halaman,
                PageSize = ukuran,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)ukuran),
                Items = baris
            };
        }

        public async Task<MedicalRecordCompletenessSummaryResponse> GetSummaryAsync(
            MedicalRecordCompletenessPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var jumlah = await Saring(DaftarKerja(), query)
                .GroupBy(x => x.State)
                .Select(x => new { State = x.Key, Jumlah = x.Count() })
                .ToListAsync(cancellationToken);

            int Hitung(CompletenessState keadaan) => jumlah.Where(x => x.State == keadaan).Sum(x => x.Jumlah);

            return new MedicalRecordCompletenessSummaryResponse
            {
                Total = jumlah.Sum(x => x.Jumlah),
                Complete = Hitung(CompletenessState.Complete),
                Incomplete = Hitung(CompletenessState.Incomplete),
                AwaitingResult = Hitung(CompletenessState.AwaitingResult),
                Undetermined = Hitung(CompletenessState.Undetermined)
            };
        }

        /// <summary>Nilai penyaring enum yang tidak sah ditolak, bukan dijawab daftar kosong.</summary>
        public static string? ValidateQuery(MedicalRecordCompletenessPagedQuery query)
        {
            if (query.State.HasValue && !Enum.IsDefined(query.State.Value))
                return "Keadaan kelengkapan tidak dikenal.";
            if (query.EncounterType.HasValue && !Enum.IsDefined(query.EncounterType.Value))
                return "Jenis kunjungan tidak dikenal.";
            if (query.DeficiencyType.HasValue && !Enum.IsDefined(query.DeficiencyType.Value))
                return "Jenis kekurangan tidak dikenal.";
            if (query.ClosedFrom.HasValue && query.ClosedTo.HasValue && query.ClosedFrom > query.ClosedTo)
                return "Tanggal awal tidak boleh melewati tanggal akhir.";
            return null;
        }

        public static MedicalRecordCompletenessFilterMetadataResponse BuildFilterMetadata() => new()
        {
            StateOptions = Enum.GetValues<CompletenessState>()
                .Select(x => new MedicalRecordCompletenessOptionResponse { Value = (int)x, Name = x.ToString(), Label = StateLabel(x) })
                .ToList(),
            EncounterTypeOptions = Enum.GetValues<EncounterType>()
                .Select(x => new MedicalRecordCompletenessOptionResponse
                {
                    Value = (int)x, Name = x.ToString(), Label = MedicalRecordCompletenessProfileService.EncounterTypeLabel(x)
                })
                .ToList(),
            DeficiencyTypeOptions = Enum.GetValues<CompletenessDeficiencyType>()
                .Select(x => new MedicalRecordCompletenessOptionResponse { Value = (int)x, Name = x.ToString(), Label = DeficiencyTypeLabel(x) })
                .ToList(),
            PageSizeOptions = [10, 25, 50, 100]
        };

        /// <summary>
        /// Daftar kerja = kunjungan yang tutup klinis sejak rilis (punya baris penilaian), tidak
        /// dikeluarkan, dan kunjungannya tidak batal (`RM-DEC-046`, `050`).
        /// </summary>
        private IQueryable<MrcEncounterCompletenessAssessment> DaftarKerja()
            => _dbContext.Set<MrcEncounterCompletenessAssessment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsExcluded
                            && x.Encounter != null
                            && x.Encounter.EncounterStatus != EncounterStatus.Cancelled
                            && x.Encounter.EncounterStatus != EncounterStatus.NoShow);

        private IQueryable<MrcEncounterCompletenessAssessment> Saring(
            IQueryable<MrcEncounterCompletenessAssessment> sumber,
            MedicalRecordCompletenessPagedQuery query)
        {
            if (query.EncounterType.HasValue)
                sumber = sumber.Where(x => x.Encounter!.EncounterType == query.EncounterType.Value);

            if (query.ServiceUnitId.HasValue)
                sumber = sumber.Where(x => x.Encounter!.ServiceUnitId == query.ServiceUnitId.Value);

            if (query.ClosedFrom.HasValue)
                sumber = sumber.Where(x => x.ClinicalClosure!.ClinicalClosedAt >= query.ClosedFrom.Value);

            if (query.ClosedTo.HasValue)
                sumber = sumber.Where(x => x.ClinicalClosure!.ClinicalClosedAt <= query.ClosedTo.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var kata = query.Search.Trim().ToLower();
                sumber = sumber.Where(x =>
                    x.Encounter!.EncounterNumber.ToLower().Contains(kata)
                    || (x.Patient != null && (x.Patient.MedicalRecordNumber.ToLower().Contains(kata)
                                              || x.Patient.FullName.ToLower().Contains(kata))));
            }

            if (query.DeficiencyType.HasValue)
            {
                var jenis = (int)query.DeficiencyType.Value;

                // jsonb di PostgreSQL; teks biasa di basis data uji SQLite.
                sumber = _dbContext.Database.IsNpgsql()
                    ? sumber.Where(x => EF.Functions.JsonContains(x.Deficiencies, $"[{{\"type\":{jenis}}}]"))
                    : sumber.Where(x => x.Deficiencies.Contains($"\"type\":{jenis},"));
            }

            return sumber;
        }

        // =====================================================================
        // Alat bantu
        // =====================================================================

        public static string StateLabel(CompletenessState keadaan) => keadaan switch
        {
            CompletenessState.Complete => "Lengkap",
            CompletenessState.Incomplete => "Belum lengkap",
            CompletenessState.AwaitingResult => "Menunggu hasil",
            CompletenessState.Undetermined => "Tidak dapat dipastikan",
            _ => keadaan.ToString()
        };

        public static string DeficiencyTypeLabel(CompletenessDeficiencyType jenis) => jenis switch
        {
            CompletenessDeficiencyType.UnsignedDocument => "Catatan belum ditandatangani",
            CompletenessDeficiencyType.PendingResult => "Hasil belum dirilis",
            CompletenessDeficiencyType.MissingRequiredDocument => "Dokumen wajib belum ada",
            CompletenessDeficiencyType.SourceUnavailable => "Sumber tidak terbaca",
            CompletenessDeficiencyType.RuleNotFound => "Aturan dokumen wajib tidak ditemukan",
            _ => jenis.ToString()
        };

        private static List<BarisKekurangan> BacaKekurangan(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<BarisKekurangan>>(string.IsNullOrWhiteSpace(json) ? "[]" : json, Json) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static MedicalRecordCompletenessDeficiencyResponse KeRespons(BarisKekurangan x) => new()
        {
            Type = x.Type,
            TypeLabel = DeficiencyTypeLabel(x.Type),
            Label = x.Label,
            DocumentKind = x.DocumentKind,
            DocumentId = x.DocumentId
        };

        private static MedicalRecordCompletenessDetailResponse KeDetail(
            Kunjungan kunjungan,
            ClinicalClosureTime tutup,
            HasilPenilaian hasil,
            DateTime sekarang,
            DateTime? firstCompletedAt,
            bool persisted) => new()
            {
                EncounterId = kunjungan.Id,
                EncounterNumber = kunjungan.EncounterNumber,
                PatientId = kunjungan.PatientId,
                MedicalRecordNumber = kunjungan.MedicalRecordNumber,
                PatientName = kunjungan.PatientName,
                EncounterType = kunjungan.EncounterType,
                ServiceUnitName = kunjungan.ServiceUnitName,
                ClinicalClosedAt = tutup.Waktu,
                IsClinicalClosureDerived = tutup.Turunan,
                State = hasil.Keadaan,
                StateLabel = StateLabel(hasil.Keadaan),
                Deficiencies = hasil.Kekurangan.Select(KeRespons).ToList(),
                EvaluatedAt = sekarang,
                FirstCompletedAt = firstCompletedAt,
                IsPersisted = persisted,
                ProfileCodes = hasil.Profil.Select(x => x.Code).ToList()
            };

        private async Task LengkapiNamaPenanggungJawabAsync(
            MedicalRecordCompletenessDetailResponse detail,
            IReadOnlyList<BarisKekurangan> kekurangan,
            CancellationToken cancellationToken)
        {
            var id = kekurangan.Where(x => x.ResponsibleUserId.HasValue).Select(x => x.ResponsibleUserId!.Value).Distinct().ToList();
            if (id.Count == 0)
                return;

            var nama = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => id.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            for (var i = 0; i < kekurangan.Count && i < detail.Deficiencies.Count; i++)
            {
                if (kekurangan[i].ResponsibleUserId is { } penulis)
                    detail.Deficiencies[i].ResponsibleUserName = nama.GetValueOrDefault(penulis);
            }
        }

        private static MedicalRecordCompletenessResult Gagal(MedicalRecordCompletenessStatus status, string pesan)
            => new(status, null, pesan);

        private sealed record Kunjungan(
            Guid Id,
            string EncounterNumber,
            Guid PatientId,
            EncounterType EncounterType,
            EncounterStatus Status,
            ServiceUnitType? ServiceUnitType,
            string? ServiceUnitName,
            string? MedicalRecordNumber,
            string? PatientName);

        private sealed record Profil(
            string Code,
            EncounterType? EncounterType,
            ServiceUnitType? ServiceUnitType,
            CompletenessCondition Condition,
            List<CompletenessCriterion> Criteria);

        private sealed record HasilPenilaian(
            CompletenessState Keadaan,
            List<BarisKekurangan> Kekurangan,
            List<Profil> Profil);
    }

    /// <summary>
    /// Satu kekurangan seperti disimpan di kolom <c>Deficiencies</c> (jsonb). Memuat id penulis —
    /// SENSITIF, tidak pernah masuk payload logger. Tidak memuat isi klinis.
    /// </summary>
    public sealed record BarisKekurangan(
        CompletenessDeficiencyType Type,
        string Label,
        ClinicalDocumentKind? DocumentKind = null,
        Guid? DocumentId = null,
        Guid? ResponsibleUserId = null);

    public enum MedicalRecordCompletenessStatus
    {
        Success = 0,
        NotFound = 1,
        Running = 2,
        Cancelled = 3,
        Conflict = 4
    }

    /// <summary>Ringkasan satu putaran pemeriksaan berkala — hanya jumlah, tanpa rincian (`BE-40` kriteria 4).</summary>
    public sealed class MedicalRecordCompletenessRecheckSummary
    {
        public int Considered { get; set; }

        /// <summary>Penutupan klinis yang baru dinilai pertama kali pada putaran ini.</summary>
        public int FirstEvaluations { get; set; }

        /// <summary>Kunjungan yang sudah batal atau ternyata belum tutup, sehingga tidak dinilai.</summary>
        public int Skipped { get; set; }

        public int Failed { get; set; }

        public Dictionary<CompletenessState, int> ByState { get; } = new();
    }

    public sealed record MedicalRecordCompletenessResult(
        MedicalRecordCompletenessStatus Status,
        MedicalRecordCompletenessDetailResponse? Detail,
        string Message);
}
