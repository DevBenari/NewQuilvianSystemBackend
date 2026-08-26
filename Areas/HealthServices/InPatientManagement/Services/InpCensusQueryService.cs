using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Menyusun census, menghitung lama dirawat, dan menyusun daftar pantau. Service ini hanya
    /// membaca.
    /// </summary>
    /// <remarks>
    /// <b>Census tidak pernah disimpan sebagai tabel.</b> Ia selalu dihitung dari baris
    /// penempatan yang masih aktif. Menyimpannya akan melahirkan versi kedua yang harus
    /// disamakan terus-menerus, dan setiap kali keduanya berselisih, tidak ada cara mengetahui
    /// mana yang benar.
    ///
    /// <para>
    /// <b>Lama dirawat dihitung dari selisih tanggal, bukan selisih jam.</b> Tn. Budi masuk
    /// 21 September pukul 22:30 dan pulang 22 September pukul 06:00. Selisih jamnya hanya
    /// 7,5 jam, tetapi tanggalnya berbeda, sehingga lama dirawat tercatat <b>1 hari</b>,
    /// bukan 0 hari. Angkanya bertambah pada pergantian tanggal, bukan setiap genap 24 jam —
    /// <c>RWI-RULE-019</c>.
    /// </para>
    /// </remarks>
    public class InpCensusQueryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpSettingService _settingService;

        public InpCensusQueryService(
            ApplicationDbContext dbContext,
            InpSettingService settingService)
        {
            _dbContext = dbContext;
            _settingService = settingService;
        }

        // =====================================================================
        // BE-RWI-016 — Census dan lama dirawat
        // =====================================================================

        /// <summary>
        /// Menyusun daftar pasien yang sedang dirawat beserta lokasi, penanggung jawab, dan
        /// lama dirawatnya.
        /// </summary>
        /// <remarks>
        /// Census memuat episode <c>Admitted</c> dan <c>DischargePending</c> saja. Episode
        /// <c>DischargePending</c> yang kepergian fisiknya <b>sudah</b> dicatat tidak ikut,
        /// karena pasiennya memang sudah tidak berada di ruangan walaupun episodenya belum
        /// ditutup.
        /// </remarks>
        public async Task<CensusPagedResult> GetCensusAsync(
            CensusQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new CensusQuery();

            var (pageNumber, pageSize) = InpEpisodeService.NormalizePaging(
                query.PageNumber,
                query.PageSize);

            var filtered = BuildCensusQuery(query);

            var descending = string.Equals(
                query.SortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            filtered = (query.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "patientname" => descending
                    ? filtered.OrderByDescending(x => x.Episode!.Patient!.FullName)
                    : filtered.OrderBy(x => x.Episode!.Patient!.FullName),
                "admittedat" => descending
                    ? filtered.OrderByDescending(x => x.Episode!.AdmittedAt)
                    : filtered.OrderBy(x => x.Episode!.AdmittedAt),
                "roomname" => descending
                    ? filtered.OrderByDescending(x => x.Room!.RoomName)
                    : filtered.OrderBy(x => x.Room!.RoomName),
                _ => descending
                    ? filtered.OrderByDescending(x => x.Bed!.BedName)
                    : filtered.OrderBy(x => x.Bed!.BedName)
            };

            var totalData = await filtered.CountAsync(cancellationToken);

            var items = await filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CensusItemResponse
                {
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode!.EpisodeNumber,
                    PatientId = x.Episode.PatientId,
                    PatientName = x.Episode.Patient != null ? x.Episode.Patient.FullName : null,
                    MedicalRecordNumber = x.Episode.Patient != null
                        ? x.Episode.Patient.MedicalRecordNumber
                        : null,
                    EpisodeStatus = (int)x.Episode.EpisodeStatus,
                    BedId = x.BedId,
                    BedCode = x.Bed != null ? x.Bed.BedCode : null,
                    BedName = x.Bed != null ? x.Bed.BedName : null,
                    RoomId = x.RoomId,
                    RoomName = x.Room != null ? x.Room.RoomName : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    DoctorId = x.Episode.DoctorAssignments
                        .Where(d => d.EndDateTime == null && !d.IsDelete)
                        .OrderByDescending(d => d.SequenceNumber)
                        .Select(d => (Guid?)d.DoctorId)
                        .FirstOrDefault(),
                    DoctorName = x.Episode.DoctorAssignments
                        .Where(d => d.EndDateTime == null && !d.IsDelete)
                        .OrderByDescending(d => d.SequenceNumber)
                        .Select(d => d.Doctor != null ? d.Doctor.FullName : null)
                        .FirstOrDefault(),
                    NurseEmployeeId = x.Episode.NurseAssignments
                        .Where(n => n.EndDateTime == null && !n.IsDelete)
                        .OrderByDescending(n => n.SequenceNumber)
                        .Select(n => (Guid?)n.EmployeeId)
                        .FirstOrDefault(),
                    NurseName = x.Episode.NurseAssignments
                        .Where(n => n.EndDateTime == null && !n.IsDelete)
                        .OrderByDescending(n => n.SequenceNumber)
                        .Select(n => n.Employee != null ? n.Employee.FullName : null)
                        .FirstOrDefault(),
                    RequiresIsolation = x.Episode.RequiresIsolation,
                    // BE-RWI-031 — ibu dan bayi tampil sebagai dua baris; rujukan ke episode
                    // ibu ikut supaya layar dapat menjawab "bayi siapa", bukan sekadar
                    // "bayi mana".
                    MotherEpisodeId = x.Episode.MotherEpisodeId,
                    MotherEpisodeNumber = x.Episode.MotherEpisode != null
                        ? x.Episode.MotherEpisode.EpisodeNumber
                        : null,
                    MotherPatientName =
                        x.Episode.MotherEpisode != null && x.Episode.MotherEpisode.Patient != null
                            ? x.Episode.MotherEpisode.Patient.FullName
                            : null,
                    IsNewbornBed = x.Bed != null && x.Bed.IsForNewborn,
                    AdmittedAt = x.Episode.AdmittedAt,
                    PlacementStartDateTime = x.StartDateTime
                })
                .ToListAsync(cancellationToken);

            var today = DateTime.UtcNow;

            foreach (var item in items)
            {
                item.EpisodeStatusName = ((InpEpisodeStatus)item.EpisodeStatus).ToString();
                item.LengthOfStayDays = CalculateLengthOfStayDays(
                    item.AdmittedAt ?? item.PlacementStartDateTime,
                    today);
            }

            return new CensusPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>Menghitung jumlah pasien dirawat per unit layanan dan per kelas perawatan.</summary>
        public async Task<CensusSummaryResponse> GetCensusSummaryAsync(
            CensusQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new CensusQuery();

            var filtered = BuildCensusQuery(query);

            var byServiceUnit = await filtered
                .GroupBy(x => new
                {
                    x.ServiceUnitId,
                    Name = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null
                })
                .Select(g => new CensusSummaryGroupResponse
                {
                    Id = g.Key.ServiceUnitId,
                    Name = g.Key.Name,
                    Total = g.Count()
                })
                .ToListAsync(cancellationToken);

            var byPatientClass = await filtered
                .GroupBy(x => new
                {
                    x.PatientClassId,
                    Name = x.PatientClass != null ? x.PatientClass.PatientClassName : null
                })
                .Select(g => new CensusSummaryGroupResponse
                {
                    Id = g.Key.PatientClassId,
                    Name = g.Key.Name,
                    Total = g.Count()
                })
                .ToListAsync(cancellationToken);

            var totalPatient = await filtered.CountAsync(cancellationToken);

            var totalIsolation = await filtered
                .CountAsync(x => x.Episode!.RequiresIsolation, cancellationToken);

            return new CensusSummaryResponse
            {
                TotalPatient = totalPatient,
                TotalRequiringIsolation = totalIsolation,
                ByServiceUnit = byServiceUnit.OrderBy(x => x.Name).ToList(),
                ByPatientClass = byPatientClass.OrderBy(x => x.Name).ToList()
            };
        }

        /// <summary>Menyusun pilihan penyaring census beserta nilai bawaannya.</summary>
        public async Task<CensusFilterMetadataResponse> GetCensusFilterMetadataAsync(
            CancellationToken cancellationToken = default)
        {
            var serviceUnits = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ServiceUnitType == ServiceUnitType.Inpatient)
                .OrderBy(x => x.ServiceUnitName)
                .Select(x => new InpatientOptionResponse
                {
                    Value = x.Id.ToString(),
                    Label = x.ServiceUnitName
                })
                .ToListAsync(cancellationToken);

            var patientClasses = await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsForInpatient)
                .OrderBy(x => x.ClassLevel)
                .ThenBy(x => x.PatientClassName)
                .Select(x => new InpatientOptionResponse
                {
                    Value = x.Id.ToString(),
                    Label = x.PatientClassName
                })
                .ToListAsync(cancellationToken);

            return new CensusFilterMetadataResponse
            {
                DefaultFilter = new CensusDefaultFilterResponse(),
                SortOptions = new List<InpatientSortOptionResponse>
                {
                    new() { Value = "bedName", Label = "Nama tempat tidur" },
                    new() { Value = "roomName", Label = "Nama kamar" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "admittedAt", Label = "Waktu masuk" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ServiceUnitOptions = serviceUnits,
                PatientClassOptions = patientClasses,
                ResetButtonLabel = "Reset"
            };
        }

        /// <summary>
        /// Menghitung lama dirawat dalam hari, dari <b>selisih tanggal</b>, dengan hasil paling
        /// sedikit 1.
        /// </summary>
        /// <remarks>
        /// <b>Tiga kasus batas yang membedakan cara ini dari selisih jam.</b>
        ///
        /// <list type="number">
        /// <item><description>
        /// Masuk 21 September 22:30, dibaca 22 September 06:00. Selisih jam 7,5 — selisih
        /// tanggal 1. Hasilnya <b>1</b>.
        /// </description></item>
        /// <item><description>
        /// Masuk 21 September 06:00, dibaca 21 September 23:00. Selisih tanggal 0, tetapi
        /// hasilnya tetap <b>1</b> karena pasien memang dirawat hari itu.
        /// </description></item>
        /// <item><description>
        /// Masuk 21 September 06:00, dibaca 22 September 05:00. Selisih jam kurang dari 24 —
        /// selisih tanggal 1. Hasilnya <b>1</b>, dan menjadi 2 begitu tanggal berganti lagi,
        /// bukan setelah genap 48 jam.
        /// </description></item>
        /// </list>
        /// </remarks>
        public static int CalculateLengthOfStayDays(DateTime admittedAt, DateTime referenceTime)
        {
            var days = (referenceTime.Date - admittedAt.Date).Days;

            return days < 1 ? 1 : days;
        }

        // =====================================================================
        // BE-RWI-015 — Daftar pantau penempatan tidak sesuai kebutuhan isolasi
        // =====================================================================

        /// <summary>
        /// Menyusun daftar episode yang kebutuhan isolasinya tidak cocok dengan sifat tempat
        /// tidur yang sedang ditempatinya.
        /// </summary>
        /// <remarks>
        /// <b>Dua arah, dan keduanya sama pentingnya.</b> Pasien yang membutuhkan isolasi
        /// tetapi berada di tempat tidur biasa adalah risiko penularan; pasien yang tidak
        /// membutuhkan isolasi tetapi menempati tempat tidur isolasi adalah kapasitas isolasi
        /// yang terpakai sia-sia. Daftar ini memuat keduanya, dan membedakannya lewat kolom
        /// <c>MismatchKind</c>.
        ///
        /// <para>
        /// Daftar yang kosong mengembalikan daftar kosong, bukan galat. Kosong adalah keadaan
        /// yang normal dan justru yang diharapkan.
        /// </para>
        /// </remarks>
        public async Task<IsolationMismatchPagedResult> GetIsolationMismatchAsync(
            IsolationMismatchQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new IsolationMismatchQuery();

            var (pageNumber, pageSize) = InpEpisodeService.NormalizePaging(
                query.PageNumber,
                query.PageSize);

            IQueryable<InpBedPlacement> filtered = _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x =>
                    x.EndDateTime == null &&
                    !x.IsDelete &&
                    x.Episode != null &&
                    !x.Episode.IsDelete &&
                    (x.Episode.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     x.Episode.EpisodeStatus == InpEpisodeStatus.DischargePending) &&
                    x.Bed != null &&
                    x.Episode.RequiresIsolation != x.Bed.IsIsolationBed);

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            }

            if (query.RoomId.HasValue && query.RoomId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.RoomId == query.RoomId.Value);
            }

            var totalData = await filtered.CountAsync(cancellationToken);

            var items = await filtered
                .OrderBy(x => x.StartDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new IsolationMismatchItemResponse
                {
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode!.EpisodeNumber,
                    PatientId = x.Episode.PatientId,
                    PatientName = x.Episode.Patient != null ? x.Episode.Patient.FullName : null,
                    MedicalRecordNumber = x.Episode.Patient != null
                        ? x.Episode.Patient.MedicalRecordNumber
                        : null,
                    BedId = x.BedId,
                    BedCode = x.Bed != null ? x.Bed.BedCode : null,
                    BedName = x.Bed != null ? x.Bed.BedName : null,
                    RoomId = x.RoomId,
                    RoomName = x.Room != null ? x.Room.RoomName : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    RequiresIsolation = x.Episode.RequiresIsolation,
                    IsIsolationBed = x.Bed != null && x.Bed.IsIsolationBed,
                    PlacementStartDateTime = x.StartDateTime,
                    IsolationSetAt = x.Episode.IsolationSetAt
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                if (item.RequiresIsolation)
                {
                    item.MismatchKind = "NeedsIsolationBed";
                    item.MismatchMessage =
                        $"Pasien membutuhkan isolasi, tetapi sedang menempati " +
                        $"{item.BedName ?? "tempat tidur biasa"} yang bukan tempat tidur isolasi.";
                }
                else
                {
                    item.MismatchKind = "OccupiesIsolationBed";
                    item.MismatchMessage =
                        $"Tempat tidur isolasi {item.BedName ?? string.Empty} sedang ditempati " +
                        "pasien yang tidak membutuhkan isolasi.";
                }
            }

            return new IsolationMismatchPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        // =====================================================================
        // BE-RWI-018 — Episode aktif yang belum punya perawat penanggung jawab
        // =====================================================================

        /// <summary>
        /// Menyusun daftar episode aktif yang belum punya perawat penanggung jawab.
        /// </summary>
        /// <remarks>
        /// <b>Ini pasangan dari keputusan tidak menahan.</b> <c>RWI-DEC-032</c> memilih agar
        /// episode tetap berjalan tanpa perawat penanggung jawab; konsekuensinya, ketiadaan
        /// itu harus terlihat di suatu tempat, bukan menghilang begitu saja. Daftar inilah
        /// tempatnya.
        ///
        /// <para>
        /// <b>Endpoint-nya belum dibuka pada task ini.</b> <c>GET /monitoring/unassigned-nurse-episodes</c>
        /// milik <c>BE-RWI-029</c> beserta tiga daftar pantau lainnya. Method ini disediakan
        /// supaya acceptance criteria 3 milik <c>BE-RWI-018</c> dapat dibuktikan di tingkat
        /// service, dan supaya task berikutnya cukup memasang endpoint tanpa menulis ulang
        /// query-nya.
        /// </para>
        /// </remarks>
        public async Task<List<CensusItemResponse>> GetUnassignedNurseEpisodesAsync(
            Guid? serviceUnitId = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<InpEpisode> filtered = _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    (x.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     x.EpisodeStatus == InpEpisodeStatus.DischargePending) &&
                    !x.NurseAssignments.Any(n => n.EndDateTime == null && !n.IsDelete));

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            }

            var items = await filtered
                .OrderBy(x => x.AdmittedAt)
                .Select(x => new CensusItemResponse
                {
                    EpisodeId = x.Id,
                    EpisodeNumber = x.EpisodeNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    EpisodeStatus = (int)x.EpisodeStatus,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    RequiresIsolation = x.RequiresIsolation,
                    AdmittedAt = x.AdmittedAt,
                    BedId = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .Select(p => p.BedId)
                        .FirstOrDefault(),
                    BedName = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .Select(p => p.Bed != null ? p.Bed.BedName : null)
                        .FirstOrDefault(),
                    RoomName = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .Select(p => p.Room != null ? p.Room.RoomName : null)
                        .FirstOrDefault(),
                    PlacementStartDateTime = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .Select(p => p.StartDateTime)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var today = DateTime.UtcNow;

            foreach (var item in items)
            {
                item.EpisodeStatusName = ((InpEpisodeStatus)item.EpisodeStatus).ToString();
                item.LengthOfStayDays = CalculateLengthOfStayDays(
                    item.AdmittedAt ?? item.PlacementStartDateTime,
                    today);
            }

            return items;
        }

        // =====================================================================
        // BE-RWI-029 — Empat daftar pantau dan laporan selisih
        // =====================================================================

        /// <summary>
        /// Menyusun daftar episode yang sudah boleh pulang tetapi belum ditutup melewati
        /// ambang waktu.
        /// </summary>
        /// <remarks>
        /// <b>Daftar ini adalah pasangan wajib dari <c>RWI-RULE-010</c>.</b> Yang memutuskan
        /// pulang dan yang menutup episode adalah orang yang berbeda, sehingga episode dapat
        /// menggantung di <c>DischargePending</c> bila petugas admisi lalai. Tanpa daftar ini,
        /// satu-satunya cara menemukannya adalah menunggu ada yang mengeluh.
        ///
        /// <para>
        /// Ambangnya dibaca ulang dari <c>MstInpatientSetting.PendingClosureThresholdHours</c>
        /// setiap pembacaan, sehingga angka yang diubah admin berlaku pada pembacaan
        /// berikutnya tanpa aplikasi dinyalakan ulang.
        /// </para>
        /// </remarks>
        public async Task<PendingClosurePagedResult> GetPendingClosuresAsync(
            InpatientMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new InpatientMonitoringQuery();

            var setting = await _settingService.GetEffectiveSettingAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var cutoff = now.AddHours(-setting.PendingClosureThresholdHours);

            var (pageNumber, pageSize) = InpEpisodeService.NormalizePaging(
                query.PageNumber,
                query.PageSize);

            IQueryable<InpEpisode> filtered = _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.EpisodeStatus == InpEpisodeStatus.DischargePending &&
                    x.DischargeDecidedAt != null &&
                    x.DischargeDecidedAt <= cutoff);

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            }

            var totalData = await filtered.CountAsync(cancellationToken);

            var items = await filtered
                .OrderBy(x => x.DischargeDecidedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PendingClosureItemResponse
                {
                    EpisodeId = x.Id,
                    EpisodeNumber = x.EpisodeNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    BedName = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .Select(p => p.Bed != null ? p.Bed.BedName : null)
                        .FirstOrDefault(),
                    RoomName = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .Select(p => p.Room != null ? p.Room.RoomName : null)
                        .FirstOrDefault(),
                    DischargeDecidedAt = x.DischargeDecidedAt,
                    PhysicallyLeftAt = x.PhysicallyLeftAt,
                    IsBedStillHeld = x.BedPlacements
                        .Any(p => p.EndDateTime == null && !p.IsDelete)
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.ThresholdHours = setting.PendingClosureThresholdHours;
                item.PendingHours = item.DischargeDecidedAt.HasValue
                    ? (int)Math.Floor((now - item.DischargeDecidedAt.Value).TotalHours)
                    : 0;
            }

            return new PendingClosurePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Menyusun daftar episode yang ditutup menembus gerbang keuangan.
        /// </summary>
        /// <remarks>
        /// Setiap baris di sini adalah keputusan supervisor yang melewati satu syarat
        /// penutupan. Daftar yang panjang bukan berarti sistemnya rusak — ia berarti gerbang
        /// keuangan sedang tidak berfungsi sebagaimana dimaksud, dan itu perlu diketahui
        /// sebelum menjadi kebiasaan.
        /// </remarks>
        public async Task<OverrideClosurePagedResult> GetOverrideClosuresAsync(
            InpatientMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new InpatientMonitoringQuery();

            var (pageNumber, pageSize) = InpEpisodeService.NormalizePaging(
                query.PageNumber,
                query.PageSize);

            IQueryable<InpEpisode> filtered = _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsClosedWithoutFinancialClearance);

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            }

            var totalData = await filtered.CountAsync(cancellationToken);

            var items = await filtered
                .OrderByDescending(x => x.ClosedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OverrideClosureItemResponse
                {
                    EpisodeId = x.Id,
                    EpisodeNumber = x.EpisodeNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClosedAt = x.ClosedAt,
                    ClosedByUserId = x.UpdateBy,
                    ClosedWithoutClearanceReason = x.ClosedWithoutClearanceReason
                })
                .ToListAsync(cancellationToken);

            return new OverrideClosurePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Menyusun daftar episode aktif yang belum punya perawat penanggung jawab, bertingkat.
        /// </summary>
        public async Task<UnassignedNursePagedResult> GetUnassignedNurseEpisodesPagedAsync(
            InpatientMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new InpatientMonitoringQuery();

            var (pageNumber, pageSize) = InpEpisodeService.NormalizePaging(
                query.PageNumber,
                query.PageSize);

            var all = await GetUnassignedNurseEpisodesAsync(
                query.ServiceUnitId,
                cancellationToken);

            return new UnassignedNursePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = all.Count,
                TotalPage = (int)Math.Ceiling(all.Count / (double)pageSize),
                Items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        /// <summary>
        /// Menyusun laporan selisih antara salinan <c>MstBed.BedStatus</c> dan catatan
        /// penempatan beserta pemesanan.
        /// </summary>
        /// <remarks>
        /// <b>Ini satu-satunya pengawas atas satu-satunya arah tulis lintas modul.</b>
        /// <c>RWI-DEC-039</c> menurunkan <c>MstBed.BedStatus</c> menjadi salinan; sumber
        /// kebenarannya adalah <c>InpBedPlacement</c> dan <c>InpBedReservation</c>. Salinan
        /// dapat menyimpang karena banyak hal — perubahan langsung di database, kegagalan yang
        /// tidak tertangani, atau modul lain yang menulisnya tanpa sepengetahuan modul ini.
        ///
        /// <para>
        /// <b>Laporan ini hanya berguna bila ada yang membacanya.</b> Bila tidak pernah dibuka
        /// siapa pun, salinan akan menyimpang diam-diam sampai seorang pasien ditempatkan di
        /// tempat tidur yang sudah ada orangnya. Ini soal proses, bukan kode: perlu ada yang
        /// bertanggung jawab membacanya secara berkala.
        /// </para>
        ///
        /// <para>
        /// Keempat keadaan yang merupakan wewenang admin — <c>Cleaning</c>, <c>Maintenance</c>,
        /// <c>Blocked</c>, <c>Inactive</c> — <b>tidak</b> dihitung sebagai selisih, karena modul
        /// ini memang tidak pernah menuliskannya.
        /// </para>
        /// </remarks>
        public async Task<BedDriftPagedResult> GetBedDriftAsync(
            InpatientMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new InpatientMonitoringQuery();

            var (pageNumber, pageSize) = InpEpisodeService.NormalizePaging(
                query.PageNumber,
                query.PageSize);

            IQueryable<MstBed> bedQuery = _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Include(x => x.Room);

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                bedQuery = bedQuery.Where(x =>
                    x.Room != null && x.Room.ServiceUnitId == query.ServiceUnitId.Value);
            }

            var beds = await bedQuery
                .OrderBy(x => x.BedName)
                .Select(x => new
                {
                    x.Id,
                    x.BedCode,
                    x.BedName,
                    x.BedStatus,
                    x.RoomId,
                    RoomName = x.Room != null ? x.Room.RoomName : null,
                    ServiceUnitId = x.Room != null ? x.Room.ServiceUnitId : Guid.Empty,
                    ServiceUnitName = x.Room != null && x.Room.ServiceUnit != null
                        ? x.Room.ServiceUnit.ServiceUnitName
                        : null
                })
                .ToListAsync(cancellationToken);

            var bedIds = beds.Select(x => x.Id).ToList();

            var placements = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x => x.EndDateTime == null && !x.IsDelete && bedIds.Contains(x.BedId))
                .Select(x => new
                {
                    x.BedId,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null
                })
                .ToListAsync(cancellationToken);

            var reservations = await _dbContext.Set<InpBedReservation>()
                .AsNoTracking()
                .Where(x =>
                    x.ReservationStatus == InpBedReservationStatus.Active &&
                    !x.IsDelete &&
                    bedIds.Contains(x.BedId))
                .Select(x => new
                {
                    x.BedId,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null
                })
                .ToListAsync(cancellationToken);

            var drift = new List<BedDriftItemResponse>();

            foreach (var bed in beds)
            {
                // Keadaan yang merupakan wewenang admin dilewati: modul ini tidak pernah
                // menuliskannya, sehingga selisih di sana bukan tanggung jawabnya.
                if (bed.BedStatus is BedStatus.Cleaning
                    or BedStatus.Maintenance
                    or BedStatus.Blocked
                    or BedStatus.Inactive)
                {
                    continue;
                }

                var placement = placements.FirstOrDefault(x => x.BedId == bed.Id);
                var reservation = reservations.FirstOrDefault(x => x.BedId == bed.Id);

                var expected = placement != null
                    ? BedStatus.Occupied
                    : reservation != null
                        ? BedStatus.Reserved
                        : BedStatus.Available;

                if (bed.BedStatus == expected)
                {
                    continue;
                }

                drift.Add(new BedDriftItemResponse
                {
                    BedId = bed.Id,
                    BedCode = bed.BedCode,
                    BedName = bed.BedName,
                    RoomId = bed.RoomId,
                    RoomName = bed.RoomName,
                    ServiceUnitId = bed.ServiceUnitId,
                    ServiceUnitName = bed.ServiceUnitName,
                    CopiedStatus = (int)bed.BedStatus,
                    CopiedStatusName = bed.BedStatus.ToString(),
                    ExpectedStatus = (int)expected,
                    ExpectedStatusName = expected.ToString(),
                    HasActivePlacement = placement != null,
                    HasActiveReservation = reservation != null,
                    HoldingEpisodeNumber = placement?.EpisodeNumber ?? reservation?.EpisodeNumber,
                    DriftMessage =
                        $"Salinan status tempat tidur {bed.BedName} berbunyi " +
                        $"{bed.BedStatus}, sedangkan catatan penempatan menunjukkan {expected}."
                });
            }

            var totalData = drift.Count;

            return new BedDriftPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = drift.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Menyusun query census: baris penempatan yang masih aktif milik episode yang
        /// benar-benar sedang dirawat.
        /// </summary>
        /// <remarks>
        /// Pasien yang kepergian fisiknya sudah dicatat tidak muncul, karena baris
        /// penempatannya sudah ditutup pada saat kepergian itu dicatat. Kolom
        /// <c>PhysicallyLeftAt</c> ikut diperiksa sebagai penjaga kedua, supaya census tetap
        /// benar walaupun ada baris penempatan yang tertinggal terbuka karena kejadian lama.
        /// </remarks>
        private IQueryable<InpBedPlacement> BuildCensusQuery(CensusQuery query)
        {
            IQueryable<InpBedPlacement> filtered = _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x =>
                    x.EndDateTime == null &&
                    !x.IsDelete &&
                    x.Episode != null &&
                    !x.Episode.IsDelete &&
                    x.Episode.PhysicallyLeftAt == null &&
                    (x.Episode.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     x.Episode.EpisodeStatus == InpEpisodeStatus.DischargePending));

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();

                filtered = filtered.Where(x =>
                    x.Episode!.EpisodeNumber.ToLower().Contains(keyword) ||
                    (x.Episode.Patient != null &&
                     x.Episode.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Episode.Patient != null &&
                     x.Episode.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Bed != null && x.Bed.BedName.ToLower().Contains(keyword)));
            }

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            }

            if (query.RoomId.HasValue && query.RoomId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.RoomId == query.RoomId.Value);
            }

            if (query.PatientClassId.HasValue && query.PatientClassId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.PatientClassId == query.PatientClassId.Value);
            }

            if (query.DoctorId.HasValue && query.DoctorId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x =>
                    x.Episode!.DoctorAssignments.Any(d =>
                        d.EndDateTime == null &&
                        !d.IsDelete &&
                        d.DoctorId == query.DoctorId.Value));
            }

            if (query.RequiresIsolation.HasValue)
            {
                filtered = filtered.Where(x =>
                    x.Episode!.RequiresIsolation == query.RequiresIsolation.Value);
            }

            return filtered;
        }
    }
}
