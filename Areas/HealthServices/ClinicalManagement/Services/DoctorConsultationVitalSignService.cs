using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Tautan catatan dokter ke deret tanda vital pasien — <c>BE-RWI-141</c>, keputusan pemilik
    /// K5 (30-09-2026) pada <c>rencana-kerja/soap/soap.md</c> Rev 2.1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dua jalan yang sah, dipilih eksplisit oleh pengirim.</b> (1) Dokter merujuk baris tanda
    /// vital yang sudah ada — biasanya ukuran perawat — lewat <c>SourceVitalSignId</c>; nilainya
    /// disalin sebagai snapshot catatan dokter. (2) Dokter mengukur sendiri
    /// (<c>IsDoctorMeasuredVitalSign</c>); pada catatan rawat inap nilainya ikut masuk deret tanda
    /// vital pasien sebagai satu baris bersumber <see cref="PatientVitalSignSource.DoctorConsultation"/>,
    /// sehingga perawat shift berikutnya melihatnya pada grafik yang sama.
    /// </para>
    /// <para>
    /// <b>Satu catatan, paling banyak satu baris buatan dokter.</b> Baris itu diperbarui selama
    /// catatan masih draf, dibatalkan bila catatannya dibatalkan atau dokter beralih ke data perawat,
    /// dan tidak dapat diubah lewat jalur perawat. Snapshot pada catatan dokter tetap disimpan:
    /// yang dilihat dokter saat menandatangani tidak ikut berubah bila deret dikoreksi kemudian.
    /// </para>
    /// <para>
    /// Contoh: Ns. Rina mencatat TD 100/60 pukul 06.10. dr. Yuhana memakai data itu pada SOAP
    /// pukul 07.40 → catatan menyimpan snapshot 100/60 dan menunjuk baris Ns. Rina. Esoknya dokter
    /// jaga mengukur sendiri TD 150/95 → lahir baris <c>VTD-00000001</c> bersumber dokter,
    /// menempel pada catatannya, dan tampil pada grafik perawat.
    /// </para>
    /// </remarks>
    public class DoctorConsultationVitalSignService
    {
        /// <summary>Deret nomor baris tanda vital buatan dokter (<c>QBE-CODE-005</c>).</summary>
        public const string DoctorVitalSignSequenceKey = "CLI_DOCTOR_VITAL_SIGN";

        /// <summary>
        /// Awalan nomor baris buatan dokter. Sengaja berbeda dari <c>VTS-</c> milik jalur perawat:
        /// jalur perawat masih menghitung nomornya dari data, sehingga berbagi awalan akan
        /// menerbitkan nomor kembar pada index unik <c>VitalSignRecordNumber</c>.
        /// </summary>
        public const string DoctorVitalSignNumberPrefix = "VTD";

        private const int DoctorVitalSignSequenceDigits = 8;

        public const string PenolakanDuaSumber =
            "Pilih salah satu sumber tanda vital: data yang sudah tercatat atau pengukuran dokter.";

        public const string PenolakanSumberTidakDitemukan =
            "Tanda vital yang dirujuk tidak ditemukan.";

        public const string PenolakanSumberPasienLain =
            "Tanda vital yang dirujuk bukan milik pasien atau perawatan ini.";

        public const string PenolakanSumberBatal =
            "Tanda vital yang dirujuk sudah dibatalkan atau ditandai salah catat.";

        public const string PenolakanUbahDariJalurPerawat =
            "Tanda vital ini dicatat dari SOAP dokter dan hanya dapat diubah lewat catatan dokter tersebut.";

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public DoctorConsultationVitalSignService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        /// <summary>
        /// Baris milik catatan dokter rawat inap: bersumber dokter, menempel pada catatan, dan
        /// berada pada episode. Jalur perawat menolak mengubah atau membatalkan baris seperti ini.
        /// </summary>
        public static bool IsDoctorConsultationVitalSign(TrxPatientVitalSign row) =>
            row.VitalSignSource == PatientVitalSignSource.DoctorConsultation &&
            row.ConsultationId.HasValue &&
            row.InpEpisodeId.HasValue;

        /// <summary>
        /// Memeriksa pilihan sumber tanda vital sebelum ada mutasi apa pun.
        /// </summary>
        /// <remarks>
        /// Catatan rawat inap hanya boleh merujuk baris pada episode yang sama; catatan lain hanya
        /// boleh merujuk baris pada kunjungan yang sama. Baris batal atau salah catat ditolak.
        /// </remarks>
        public async Task<VitalSignSourceResolution> ResolveAsync(
            Guid patientId,
            Guid encounterId,
            Guid? inpEpisodeId,
            Guid? sourceVitalSignId,
            bool isDoctorMeasured,
            CancellationToken cancellationToken = default)
        {
            var adaRujukan = sourceVitalSignId.HasValue && sourceVitalSignId.Value != Guid.Empty;

            if (adaRujukan && isDoctorMeasured)
                return VitalSignSourceResolution.Fail(PenolakanDuaSumber);

            if (!adaRujukan)
                return VitalSignSourceResolution.Ok(null);

            var row = await _dbContext.Set<TrxPatientVitalSign>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sourceVitalSignId!.Value && !x.IsDelete, cancellationToken);

            if (row == null)
                return VitalSignSourceResolution.Fail(PenolakanSumberTidakDitemukan);

            var konteksCocok = row.PatientId == patientId && (inpEpisodeId.HasValue
                ? row.InpEpisodeId == inpEpisodeId.Value
                : row.EncounterId == encounterId);

            if (!konteksCocok)
                return VitalSignSourceResolution.Fail(PenolakanSumberPasienLain);

            if (row.VitalSignStatus == PatientVitalSignStatus.Cancelled ||
                row.VitalSignStatus == PatientVitalSignStatus.EnteredInError)
            {
                return VitalSignSourceResolution.Fail(PenolakanSumberBatal);
            }

            return VitalSignSourceResolution.Ok(row);
        }

        /// <summary>
        /// Menyalin nilai baris rujukan ke snapshot catatan dokter dan menunjuk baris itu.
        /// </summary>
        public static void CopySnapshot(TrxDoctorConsultation consultation, TrxPatientVitalSign source)
        {
            consultation.BloodPressureSystolic = source.BloodPressureSystolic;
            consultation.BloodPressureDiastolic = source.BloodPressureDiastolic;
            consultation.PulseRate = source.PulseRate;
            consultation.RespiratoryRate = source.RespiratoryRate;
            consultation.Temperature = source.Temperature;
            consultation.OxygenSaturation = source.OxygenSaturation;
            consultation.Weight = source.Weight;
            consultation.Height = source.Height;
            consultation.BMI = source.BMI;
            consultation.SourceVitalSignId = source.Id;
            consultation.IsVitalSignCopiedFromAssessment =
                !(source.ConsultationId == consultation.Id && IsDoctorConsultationVitalSign(source));
        }

        /// <summary>
        /// Mencatat ukuran dokter ke deret tanda vital pasien (buat atau perbarui satu baris milik
        /// catatan ini). Pemanggil yang menyimpan perubahan.
        /// </summary>
        /// <remarks>
        /// Hanya catatan rawat inap. Nilai diambil dari snapshot catatan setelah permintaan
        /// diterapkan. Bila seluruh nilai kosong, baris milik catatan dibatalkan dan rujukan
        /// dilepas. Nomor baris baru diterbitkan <see cref="NumberSeriesAllocator"/>; nomor hangus
        /// bila transaksi pemanggil gagal (<c>INV-PLT-002</c>).
        /// </remarks>
        /// <exception cref="NumberSeriesAllocationException">Nomor baris tidak dapat diterbitkan.</exception>
        public async Task RecordDoctorMeasuredAsync(
            TrxDoctorConsultation consultation,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            consultation.IsVitalSignCopiedFromAssessment = false;

            if (!consultation.InpEpisodeId.HasValue)
            {
                consultation.SourceVitalSignId = null;
                return;
            }

            var own = await FindOwnRowAsync(consultation.Id, cancellationToken);

            if (!HasAnyVitalValue(consultation))
            {
                if (own != null)
                    CancelRow(own, actorUserId, now, "Pengukuran dokter dikosongkan pada catatan dokter.");

                consultation.SourceVitalSignId = null;
                return;
            }

            var calculated = PatientVitalSignCalculation.Calculate(
                consultation.Weight,
                consultation.Height,
                consultation.BloodPressureSystolic,
                consultation.BloodPressureDiastolic,
                consultation.RespiratoryRate,
                consultation.OxygenSaturation,
                consultation.Temperature,
                consultation.PulseRate,
                ConsciousnessStatus.Unknown,
                gcsEye: null,
                gcsVerbal: null,
                gcsMotor: null);

            var isNew = own == null;

            if (own == null)
            {
                own = new TrxPatientVitalSign
                {
                    Id = Guid.NewGuid(),
                    VitalSignRecordNumber = await AllocateNumberAsync(actorUserId, cancellationToken),
                    PatientId = consultation.PatientId,
                    EncounterId = consultation.EncounterId,
                    ConsultationId = consultation.Id,
                    InpEpisodeId = consultation.InpEpisodeId,
                    VitalSignSource = PatientVitalSignSource.DoctorConsultation,
                    VitalSignStatus = PatientVitalSignStatus.Recorded,
                    ObservedByUserId = actorUserId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<TrxPatientVitalSign>().Add(own);
            }
            else
            {
                own.UpdateDateTime = now;
                own.UpdateBy = actorUserId;
            }

            own.DoctorId = consultation.DoctorId;
            own.ServiceUnitId = consultation.ServiceUnitId;
            own.ClinicId = consultation.ClinicId;
            // Waktu pemeriksaan catatan adalah waktu pengukuran; visite 07.40 yang baru diketik
            // 11.00 tetap tercatat 07.40 pada deret.
            own.ObservationDateTime = consultation.ClinicalDateTime ?? (isNew ? now : own.ObservationDateTime);
            own.BloodPressureSystolic = consultation.BloodPressureSystolic;
            own.BloodPressureDiastolic = consultation.BloodPressureDiastolic;
            own.MeanArterialPressure = calculated.MeanArterialPressure;
            own.MapStatus = calculated.MapStatus;
            own.PulseRate = consultation.PulseRate;
            own.RespiratoryRate = consultation.RespiratoryRate;
            own.Temperature = consultation.Temperature;
            own.OxygenSaturation = consultation.OxygenSaturation;
            own.Weight = consultation.Weight;
            own.Height = consultation.Height;
            own.BMI = calculated.BMI;
            own.EarlyWarningScore = calculated.EarlyWarningScore;
            own.EwsRiskLevel = calculated.EwsRiskLevel;
            own.EwsMonitoringRecommendation = calculated.EwsMonitoringRecommendation;
            own.IsAbnormal = calculated.IsAbnormal;
            own.IsCritical = calculated.IsCritical;
            // Yang mengukur adalah dokter itu sendiri; memberi tahu dokter atas ukurannya sendiri
            // hanya menambah antrean pemberitahuan kosong pada layar perawat.
            own.NeedDoctorNotification = false;

            PatientVitalSignCalculation.Normalize(own);

            consultation.BMI = calculated.BMI;
            consultation.SourceVitalSignId = own.Id;
        }

        /// <summary>
        /// Membatalkan baris buatan dokter milik catatan ini, kecuali baris yang masih dirujuk.
        /// </summary>
        /// <remarks>
        /// Dipakai saat dokter beralih ke data perawat, dan saat catatannya dibatalkan. Baris tidak
        /// dihapus: deret tetap menunjukkan bahwa pengukuran itu pernah ada lalu ditarik.
        /// </remarks>
        public async Task CancelOwnRowsAsync(
            Guid consultationId,
            Guid? keepVitalSignId,
            Guid actorUserId,
            DateTime now,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var rows = await _dbContext.Set<TrxPatientVitalSign>()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    x.VitalSignSource == PatientVitalSignSource.DoctorConsultation &&
                    !x.IsDelete &&
                    x.VitalSignStatus != PatientVitalSignStatus.Cancelled &&
                    x.VitalSignStatus != PatientVitalSignStatus.EnteredInError)
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                if (keepVitalSignId.HasValue && row.Id == keepVitalSignId.Value)
                    continue;

                CancelRow(row, actorUserId, now, reason);
            }
        }

        /// <summary>
        /// Asal-usul baris tanda vital yang dirujuk catatan dokter, untuk lini masa SOAP.
        /// </summary>
        public async Task<Dictionary<Guid, VitalSignProvenance>> GetProvenanceAsync(
            IReadOnlyCollection<Guid> vitalSignIds,
            CancellationToken cancellationToken = default)
        {
            if (vitalSignIds.Count == 0)
                return new Dictionary<Guid, VitalSignProvenance>();

            return await _dbContext.Set<TrxPatientVitalSign>()
                .AsNoTracking()
                .Where(x => vitalSignIds.Contains(x.Id))
                .Select(x => new VitalSignProvenance
                {
                    Id = x.Id,
                    ObservationDateTime = x.ObservationDateTime,
                    ObservedByName = x.ObservedByUser != null ? x.ObservedByUser.DisplayName : null,
                    VitalSignSource = x.VitalSignSource,
                    ConsultationId = x.ConsultationId
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken);
        }

        private Task<TrxPatientVitalSign?> FindOwnRowAsync(Guid consultationId, CancellationToken cancellationToken)
        {
            return _dbContext.Set<TrxPatientVitalSign>()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    x.VitalSignSource == PatientVitalSignSource.DoctorConsultation &&
                    !x.IsDelete &&
                    x.VitalSignStatus != PatientVitalSignStatus.Cancelled &&
                    x.VitalSignStatus != PatientVitalSignStatus.EnteredInError)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static bool HasAnyVitalValue(TrxDoctorConsultation consultation) =>
            consultation.BloodPressureSystolic.HasValue ||
            consultation.BloodPressureDiastolic.HasValue ||
            consultation.PulseRate.HasValue ||
            consultation.RespiratoryRate.HasValue ||
            consultation.Temperature.HasValue ||
            consultation.OxygenSaturation.HasValue ||
            consultation.Weight.HasValue ||
            consultation.Height.HasValue;

        /// <summary>Sama dengan pembatalan jalur perawat, tanpa menghapus baris.</summary>
        private static void CancelRow(TrxPatientVitalSign row, Guid actorUserId, DateTime now, string reason)
        {
            row.VitalSignStatus = PatientVitalSignStatus.Cancelled;
            row.IsActive = false;
            row.NeedDoctorNotification = false;
            row.CancelledAt = now;
            row.CancelledByUserId = actorUserId;
            row.CancelReason = reason.Length > 250 ? reason[..250] : reason;
            row.IsCancel = true;
            row.CancelDateTime = now;
            row.CancelBy = actorUserId;
            row.UpdateDateTime = now;
            row.UpdateBy = actorUserId;
        }

        private Task<string> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
        {
            return _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: DoctorVitalSignSequenceKey,
                    Prefix: DoctorVitalSignNumberPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: DoctorVitalSignSequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
        }
    }

    /// <summary>Hasil pemeriksaan sumber tanda vital sebuah catatan dokter.</summary>
    public sealed class VitalSignSourceResolution
    {
        public bool IsValid { get; init; }

        public string? ErrorMessage { get; init; }

        /// <summary>Baris yang dirujuk; kosong bila permintaan tidak merujuk baris mana pun.</summary>
        public TrxPatientVitalSign? Source { get; init; }

        public static VitalSignSourceResolution Ok(TrxPatientVitalSign? source) =>
            new() { IsValid = true, Source = source };

        public static VitalSignSourceResolution Fail(string message) =>
            new() { IsValid = false, ErrorMessage = message };
    }

    /// <summary>Asal-usul satu baris tanda vital.</summary>
    public sealed class VitalSignProvenance
    {
        public Guid Id { get; set; }

        public DateTime ObservationDateTime { get; set; }

        public string? ObservedByName { get; set; }

        public PatientVitalSignSource VitalSignSource { get; set; }

        public Guid? ConsultationId { get; set; }
    }
}
