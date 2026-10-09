using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>Keadaan satu sumber data Workspace PPRI.</summary>
    public enum InpAdmissionSourceState
    {
        /// <summary>Terbaca.</summary>
        Available = 1,

        /// <summary>Gagal dibaca; tidak pernah diganti nilai tebakan.</summary>
        Failed = 2,

        /// <summary>Service pemiliknya belum ada karena gerbang persetujuannya belum turun.</summary>
        NotYetAvailable = 3
    }

    /// <summary>Hasil baca satu sumber beserta keadaannya.</summary>
    public sealed class InpAdmissionSourceResult<T>
    {
        public InpAdmissionSourceState State { get; private init; }

        public T? Value { get; private init; }

        public string? Reason { get; private init; }

        public bool IsAvailable => State == InpAdmissionSourceState.Available;

        public static InpAdmissionSourceResult<T> Ok(T value)
            => new() { State = InpAdmissionSourceState.Available, Value = value };

        public static InpAdmissionSourceResult<T> Failed(string reason)
            => new() { State = InpAdmissionSourceState.Failed, Reason = reason };

        public static InpAdmissionSourceResult<T> NotYetAvailable(string reason)
            => new() { State = InpAdmissionSourceState.NotYetAvailable, Reason = reason };
    }

    /// <summary>
    /// <b>Satu-satunya pintu baca</b> Workspace PPRI ke data modul lain — <c>BE-RWI-193</c>,
    /// <c>INV-RWA-14</c>, <c>02-backend-architecture.md</c> 13.6, integration 10.
    /// </summary>
    /// <remarks>
    /// Seluruh bacaan modul lain lewat service pemiliknya, dalam proses yang sama: identitas pasien
    /// (<c>PatientManagement</c>), alergi, penjamin, dan surat pengantar (<c>ClinicalManagement</c>),
    /// deposit (Billing, lewat adapter Rawat Inap yang sudah ada), kasus OK, dan profil rumah sakit
    /// (HR). Rawat Inap tidak menulis satu pun query ke tabel master modul lain.
    ///
    /// <para>
    /// <b>Gagal tidak pernah menjadi 500.</b> Setiap kegagalan menghasilkan
    /// <see cref="InpAdmissionSourceState.Failed"/> beserta alasannya. Dua sumber yang gerbangnya
    /// belum turun — dokter perujuk luar (<c>RWI-OQ-128</c>) dan tarif kamar harian
    /// (<c>RWI-OQ-129</c>) — menghasilkan <see cref="InpAdmissionSourceState.NotYetAvailable"/>,
    /// sehingga IPD mencetak garis kosong dan "lihat kasir" (<c>RWI-AC-386</c>, <c>387</c>).
    /// </para>
    /// </remarks>
    public sealed class InpAdmissionSourceReader
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PatientProfileQueryService _patientProfiles;
        private readonly PatientAllergyQueryService _patientAllergies;
        private readonly EncounterInsuranceService _encounterInsurance;
        private readonly DoctorCertificateService _doctorCertificates;
        private readonly IInpBillingDepositAdapter _depositAdapter;
        private readonly OperatingRoomCaseService _operatingRoomCases;
        private readonly HospitalSiteProfileQueryService _hospitalSites;
        private readonly InpSettingService _settings;
        private readonly ILogger<InpAdmissionSourceReader> _logger;

        public InpAdmissionSourceReader(
            ApplicationDbContext dbContext,
            PatientProfileQueryService patientProfiles,
            PatientAllergyQueryService patientAllergies,
            EncounterInsuranceService encounterInsurance,
            DoctorCertificateService doctorCertificates,
            IInpBillingDepositAdapter depositAdapter,
            OperatingRoomCaseService operatingRoomCases,
            HospitalSiteProfileQueryService hospitalSites,
            InpSettingService settings,
            ILogger<InpAdmissionSourceReader> logger)
        {
            _dbContext = dbContext;
            _patientProfiles = patientProfiles;
            _patientAllergies = patientAllergies;
            _encounterInsurance = encounterInsurance;
            _doctorCertificates = doctorCertificates;
            _depositAdapter = depositAdapter;
            _operatingRoomCases = operatingRoomCases;
            _hospitalSites = hospitalSites;
            _settings = settings;
            _logger = logger;
        }

        // ------------------------------------------------------------------
        // Data milik Rawat Inap sendiri
        // ------------------------------------------------------------------

        /// <summary>
        /// Episode beserta penempatan, DPJP, perawat penanggung jawab, dan petugas yang
        /// mengonfirmasi admisi — milik <c>InPatientManagement</c> sendiri.
        /// </summary>
        public async Task<InpAdmissionSourceResult<InpAdmissionEpisodeContext>> GetEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var episode = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .Where(x => x.Id == episodeId && !x.IsDelete)
                    .Select(x => new
                    {
                        x.Id,
                        x.EpisodeNumber,
                        x.EncounterId,
                        EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                        x.PatientId,
                        x.PatientClassId,
                        PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                        x.EpisodeStatus,
                        x.AdmittedAt,
                        x.RequiresIsolation,
                        x.ServiceUnitId,
                        ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                        AttendingDoctorName = x.DoctorAssignments
                            .Where(d => d.AssignmentRole == InpDoctorAssignmentRole.Dpjp && d.EndDateTime == null && !d.IsDelete)
                            .OrderByDescending(d => d.SequenceNumber)
                            .Select(d => d.Doctor != null ? d.Doctor.FullName : null)
                            .FirstOrDefault(),
                        FloorNurseName = x.NurseAssignments
                            .Where(n => n.EndDateTime == null && !n.IsDelete)
                            .OrderByDescending(n => n.SequenceNumber)
                            .Select(n => n.Employee != null ? n.Employee.FullName : null)
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (episode == null)
                {
                    return InpAdmissionSourceResult<InpAdmissionEpisodeContext>.Failed("Episode rawat inap tidak ditemukan.");
                }

                var placements = await _dbContext.Set<InpBedPlacement>()
                    .AsNoTracking()
                    .Where(p => p.EpisodeId == episodeId && !p.IsDelete && !p.IsSuperseded &&
                        p.SupersededByCorrectionId == null)
                    .OrderBy(p => p.StartDateTime)
                    .Select(p => new InpAdmissionPlacementInfo
                    {
                        PlacementId = p.Id,
                        StartDateTime = p.StartDateTime,
                        EndDateTime = p.EndDateTime,
                        IsActive = p.IsActive,
                        ServiceUnitName = p.ServiceUnit != null ? p.ServiceUnit.ServiceUnitName : null,
                        RoomName = p.Room != null ? p.Room.RoomName : null,
                        BedName = p.Bed != null ? p.Bed.BedName : null,
                        PatientClassName = p.PatientClass != null ? p.PatientClass.PatientClassName : null,
                        IsIsolationRoom = p.Room != null && p.Room.IsIsolationRoom,
                        IsIntensiveCareRoom = p.Room != null && p.Room.IsIntensiveCare,
                        IsIsolationBed = p.Bed != null && p.Bed.IsIsolationBed,
                        IsIntensiveCareBed = p.Bed != null && p.Bed.IsIntensiveCareBed
                    })
                    .ToListAsync(cancellationToken);

                // Petugas yang mengonfirmasi admisi = pelaku perpindahan pertama Draft → Admitted.
                var confirmedByUserId = await _dbContext.Set<InpStatusHistory>()
                    .AsNoTracking()
                    .Where(h => h.EpisodeId == episodeId && !h.IsDelete &&
                        h.FromStatus == InpEpisodeStatus.Draft && h.ToStatus == InpEpisodeStatus.Admitted)
                    .OrderBy(h => h.ChangedAt)
                    .Select(h => h.ChangedByUserId)
                    .FirstOrDefaultAsync(cancellationToken);

                string? confirmedByName = null;

                if (confirmedByUserId.HasValue)
                {
                    confirmedByName = await _dbContext.Users
                        .AsNoTracking()
                        .Where(u => u.Id == confirmedByUserId.Value)
                        .Select(u => u.DisplayName)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                var current = placements
                    .Where(p => p.EndDateTime == null && p.IsActive)
                    .OrderByDescending(p => p.StartDateTime)
                    .FirstOrDefault();

                return InpAdmissionSourceResult<InpAdmissionEpisodeContext>.Ok(new InpAdmissionEpisodeContext
                {
                    EpisodeId = episode.Id,
                    EpisodeNumber = episode.EpisodeNumber,
                    EncounterId = episode.EncounterId,
                    EncounterNumber = episode.EncounterNumber,
                    PatientId = episode.PatientId,
                    PatientClassId = episode.PatientClassId,
                    PatientClassName = episode.PatientClassName,
                    Status = episode.EpisodeStatus,
                    AdmittedAt = episode.AdmittedAt,
                    RequiresIsolation = episode.RequiresIsolation,
                    ServiceUnitId = episode.ServiceUnitId,
                    EpisodeServiceUnitName = episode.ServiceUnitName,
                    AttendingDoctorName = episode.AttendingDoctorName,
                    FloorNurseName = episode.FloorNurseName,
                    AdmissionConfirmedByName = string.IsNullOrWhiteSpace(confirmedByName) ? null : confirmedByName,
                    CurrentPlacement = current,
                    Placements = placements
                });
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca episode {EpisodeId}.", episodeId);
                return InpAdmissionSourceResult<InpAdmissionEpisodeContext>.Failed("Data episode tidak dapat dimuat.");
            }
        }

        /// <summary>Pengaturan Rawat Inap yang berlaku; kegagalan jatuh ke nilai bawaan tanpa kode formulir.</summary>
        public async Task<InpatientSettingValues> GetSettingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _settings.GetEffectiveSettingAsync(cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca pengaturan Rawat Inap.");
                return InpatientSettingValues.Defaults;
            }
        }

        /// <summary>Nama tampilan dan jabatan utama akun — identitas platform (13.6, pola <c>InpDischargeService.Departure</c>).</summary>
        public async Task<Dictionary<Guid, InpAdmissionUserInfo>> GetUsersAsync(
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            var ids = userIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (ids.Count == 0)
            {
                return new Dictionary<Guid, InpAdmissionUserInfo>();
            }

            try
            {
                return await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => ids.Contains(u.Id))
                    .Select(u => new InpAdmissionUserInfo(
                        u.Id,
                        u.DisplayName,
                        u.PrimaryPosition != null ? u.PrimaryPosition.PositionName : null))
                    .ToDictionaryAsync(x => x.UserId, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca nama akun.");
                return new Dictionary<Guid, InpAdmissionUserInfo>();
            }
        }

        // ------------------------------------------------------------------
        // INT-RWA-01, 02 — PatientManagement
        // ------------------------------------------------------------------

        public async Task<InpAdmissionSourceResult<PatientIdentityProfile>> GetPatientIdentityAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var identity = await _patientProfiles.GetIdentityAsync(patientId, cancellationToken);

                return identity == null
                    ? InpAdmissionSourceResult<PatientIdentityProfile>.Failed("Data pasien tidak ditemukan.")
                    : InpAdmissionSourceResult<PatientIdentityProfile>.Ok(identity);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca identitas pasien {PatientId}.", patientId);
                return InpAdmissionSourceResult<PatientIdentityProfile>.Failed("Data pasien tidak dapat dimuat.");
            }
        }

        public async Task<InpAdmissionSourceResult<IReadOnlyList<PatientPartyCandidate>>> GetPartyCandidatesAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return InpAdmissionSourceResult<IReadOnlyList<PatientPartyCandidate>>.Ok(
                    await _patientProfiles.GetPartyCandidatesAsync(patientId, cancellationToken));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca relasi dan kontak darurat pasien {PatientId}.", patientId);
                return InpAdmissionSourceResult<IReadOnlyList<PatientPartyCandidate>>.Failed(
                    "Data wali dan kontak darurat tidak dapat dimuat.");
            }
        }

        // ------------------------------------------------------------------
        // INT-RWA-03, 04, 05 — ClinicalManagement
        // ------------------------------------------------------------------

        public async Task<InpAdmissionSourceResult<List<PatientAllergyAlertResponse>>> GetActiveAllergiesAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return InpAdmissionSourceResult<List<PatientAllergyAlertResponse>>.Ok(
                    await _patientAllergies.GetActiveAlertsAsync(patientId, cancellationToken));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca alergi aktif pasien {PatientId}.", patientId);
                return InpAdmissionSourceResult<List<PatientAllergyAlertResponse>>.Failed("Alergi tidak dapat dimuat.");
            }
        }

        /// <summary>
        /// Penjamin utama kunjungan. Konteks yang tidak sah (misalnya polis kedaluwarsa) diperlakukan
        /// gagal dibaca, sehingga aturan Selisih Biaya "tidak dapat dihitung", bukan ditebak.
        /// </summary>
        public async Task<InpAdmissionSourceResult<EncounterInsuranceContext>> GetGuarantorAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var context = await _encounterInsurance.GetContextAsync(encounterId, null, cancellationToken);

                return context.IsValid
                    ? InpAdmissionSourceResult<EncounterInsuranceContext>.Ok(context)
                    : InpAdmissionSourceResult<EncounterInsuranceContext>.Failed(
                        context.ErrorMessage ?? "Data penjamin kunjungan tidak dapat dibaca.");
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca penjamin kunjungan {EncounterId}.", encounterId);
                return InpAdmissionSourceResult<EncounterInsuranceContext>.Failed("Data penjamin kunjungan tidak dapat dibaca.");
            }
        }

        /// <summary>Surat Pengantar Rawat Inap terbit terbaru pada kunjungan; kosong bila tidak ada.</summary>
        public async Task<InpAdmissionSourceResult<InpatientReferralLetterSummary?>> GetInpatientReferralLetterAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return InpAdmissionSourceResult<InpatientReferralLetterSummary?>.Ok(
                    await _doctorCertificates.GetLatestIssuedInpatientReferralAsync(encounterId, cancellationToken));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca surat pengantar kunjungan {EncounterId}.", encounterId);
                return InpAdmissionSourceResult<InpatientReferralLetterSummary?>.Failed("Surat pengantar tidak dapat dimuat.");
            }
        }

        // ------------------------------------------------------------------
        // INT-RWA-06 — RegistrationManagement (gerbang RWI-OQ-128)
        // ------------------------------------------------------------------

        /// <summary>
        /// Dokter perujuk luar kunjungan. <b>Belum tersedia</b> sampai pemilik
        /// <c>RegistrationManagement</c> menyetujui service bacanya (<c>RWI-OQ-128</c>,
        /// <c>BE-RWI-190</c>); selama itu IPD mencetak garis kosong (<c>RWI-AC-386</c>).
        /// </summary>
        public Task<InpAdmissionSourceResult<InpAdmissionExternalReferral?>> GetExternalReferralAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(InpAdmissionSourceResult<InpAdmissionExternalReferral?>.NotYetAvailable(
                "Service baca dokter perujuk luar menunggu persetujuan pemilik Registration (RWI-OQ-128)."));

        // ------------------------------------------------------------------
        // INT-RWA-07, 08 — Billing
        // ------------------------------------------------------------------

        /// <summary>Ringkasan deposit episode dari Billing lewat adapter Rawat Inap yang sudah ada.</summary>
        public async Task<InpAdmissionSourceResult<InpEpisodeDepositSummaryDto>> GetDepositSummaryAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var summary = await _depositAdapter.GetDepositSummaryAsync(episodeId, cancellationToken);

                return summary.IsDataAvailable
                    ? InpAdmissionSourceResult<InpEpisodeDepositSummaryDto>.Ok(summary)
                    : InpAdmissionSourceResult<InpEpisodeDepositSummaryDto>.Failed(
                        "Data deposit tidak dapat dibaca dari kasir.");
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca deposit episode {EpisodeId}.", episodeId);
                return InpAdmissionSourceResult<InpEpisodeDepositSummaryDto>.Failed("Data deposit tidak dapat dibaca dari kasir.");
            }
        }

        /// <summary>
        /// Tarif kamar per hari menurut unit, kelas, dan penjamin. <b>Belum tersedia</b> sampai
        /// Billing menyetujui method bacanya (<c>RWI-OQ-129</c>, <c>BE-RWI-191</c>); selama itu IPD
        /// menulis "lihat kasir" (<c>RWI-AC-387</c>). Rawat Inap tidak pernah menghitung tarif sendiri.
        /// </summary>
        public Task<InpAdmissionSourceResult<decimal?>> GetDailyRoomRateAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(InpAdmissionSourceResult<decimal?>.NotYetAvailable(
                "Method baca tarif kamar harian menunggu persetujuan Billing (RWI-OQ-129)."));

        // ------------------------------------------------------------------
        // INT-RWA-11 — Kamar Operasi
        // ------------------------------------------------------------------

        /// <summary>
        /// Status kasus OK pada kunjungan episode. Dipakai aturan wajib Estimasi Biaya
        /// (<c>RWI-DEC-250</c>) begitu <c>EPIC-RWA-09</c> dikirim.
        /// </summary>
        public async Task<InpAdmissionSourceResult<IReadOnlyList<OprCaseStatus>>> GetOperatingRoomCaseStatusesAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var page = await _operatingRoomCases.GetPagedAsync(
                    new OprCasePagedQuery { EncounterId = encounterId, PageNumber = 1, PageSize = 100 },
                    cancellationToken);

                return InpAdmissionSourceResult<IReadOnlyList<OprCaseStatus>>.Ok(
                    page.Items.Select(x => x.Status).ToList());
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca kasus OK kunjungan {EncounterId}.", encounterId);
                return InpAdmissionSourceResult<IReadOnlyList<OprCaseStatus>>.Failed("Data kasus kamar operasi tidak dapat dimuat.");
            }
        }

        // ------------------------------------------------------------------
        // INT-RWA-12 — HR Master Data
        // ------------------------------------------------------------------

        /// <summary>
        /// Profil situs rumah sakit utama. Profil yang tidak tersedia (nol atau lebih dari satu situs
        /// utama) diperlakukan gagal; kop dicetak tanpa identitas, tidak pernah nilai bawaan.
        /// </summary>
        public async Task<InpAdmissionSourceResult<HospitalSiteProfile>> GetHospitalProfileAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var profile = await _hospitalSites.GetMainSiteProfileAsync(cancellationToken);

                return profile.IsAvailable
                    ? InpAdmissionSourceResult<HospitalSiteProfile>.Ok(profile)
                    : InpAdmissionSourceResult<HospitalSiteProfile>.Failed(
                        profile.UnavailableReason ?? "Profil rumah sakit tidak tersedia.");
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Workspace PPRI gagal membaca profil rumah sakit.");
                return InpAdmissionSourceResult<HospitalSiteProfile>.Failed("Profil rumah sakit tidak dapat dimuat.");
            }
        }

        /// <summary>Zona waktu rumah sakit; kegagalan profil jatuh ke <c>Asia/Jakarta</c>.</summary>
        public static TimeZoneInfo ResolveTimeZone(InpAdmissionSourceResult<HospitalSiteProfile> profile)
            => InpAdmissionText.ResolveTimeZone(profile.Value?.TimeZoneId);
    }

    /// <summary>Episode dan lokasinya menurut data Rawat Inap sendiri.</summary>
    public sealed class InpAdmissionEpisodeContext
    {
        public Guid EpisodeId { get; init; }

        public string EpisodeNumber { get; init; } = string.Empty;

        public Guid EncounterId { get; init; }

        public string? EncounterNumber { get; init; }

        public Guid PatientId { get; init; }

        public Guid PatientClassId { get; init; }

        public string? PatientClassName { get; init; }

        public InpEpisodeStatus Status { get; init; }

        public DateTime? AdmittedAt { get; init; }

        public bool RequiresIsolation { get; init; }

        public Guid ServiceUnitId { get; init; }

        public string? EpisodeServiceUnitName { get; init; }

        /// <summary>DPJP aktif.</summary>
        public string? AttendingDoctorName { get; init; }

        /// <summary>Perawat penanggung jawab aktif.</summary>
        public string? FloorNurseName { get; init; }

        /// <summary>Petugas yang mengonfirmasi admisi (IPD "Administrasi Rawat Inap").</summary>
        public string? AdmissionConfirmedByName { get; init; }

        /// <summary>Penempatan berjalan; kosong bila pasien belum atau tidak lagi menempati bed.</summary>
        public InpAdmissionPlacementInfo? CurrentPlacement { get; init; }

        /// <summary>Seluruh penempatan yang sah, urut waktu mulai — riwayat pindah ruangan IPD.</summary>
        public IReadOnlyList<InpAdmissionPlacementInfo> Placements { get; init; } = Array.Empty<InpAdmissionPlacementInfo>();

        /// <summary>Unit layanan untuk tampilan: unit penempatan berjalan, atau unit episode.</summary>
        public string? DisplayServiceUnitName => CurrentPlacement?.ServiceUnitName ?? EpisodeServiceUnitName;

        /// <summary>Kelas untuk tampilan: kelas episode, atau kelas penempatan berjalan.</summary>
        public string? DisplayPatientClassName => PatientClassName ?? CurrentPlacement?.PatientClassName;
    }

    /// <summary>Satu penempatan bed beserta penanda isolasi dan intensif kamar/bed-nya.</summary>
    public sealed class InpAdmissionPlacementInfo
    {
        public Guid PlacementId { get; init; }

        public DateTime StartDateTime { get; init; }

        public DateTime? EndDateTime { get; init; }

        public bool IsActive { get; init; }

        public string? ServiceUnitName { get; init; }

        public string? RoomName { get; init; }

        public string? BedName { get; init; }

        public string? PatientClassName { get; init; }

        public bool IsIsolationRoom { get; init; }

        public bool IsIntensiveCareRoom { get; init; }

        public bool IsIsolationBed { get; init; }

        public bool IsIntensiveCareBed { get; init; }
    }

    /// <summary>Nama tampilan dan jabatan utama akun pada saat dibaca.</summary>
    public sealed record InpAdmissionUserInfo(Guid UserId, string DisplayName, string? PositionName);

    /// <summary>Dokter perujuk luar kunjungan — terisi sesudah <c>BE-RWI-190</c>.</summary>
    public sealed record InpAdmissionExternalReferral(string DoctorName, string? InstitutionName);
}
