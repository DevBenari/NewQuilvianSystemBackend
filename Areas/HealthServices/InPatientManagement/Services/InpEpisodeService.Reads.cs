using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Bagian baca <see cref="InpEpisodeService"/> — daftar, ringkasan, dan metadata penyaring
    /// layar episode. Diisi task <c>BE-RWI-009</c>.
    /// </summary>
    /// <remarks>
    /// Dipisahkan sebagai partial class mengikuti pola <c>WorkflowService.ActionsV2.cs</c> yang
    /// sudah ada di repository ini. Alasannya sama: perilaku tulis dan perilaku baca punya
    /// alasan berubah yang berbeda, dan menggabungkannya membuat satu berkas yang tidak lagi
    /// dapat dibaca utuh.
    ///
    /// <para>
    /// <b>Tiga batas yang mengikat seluruh method di sini.</b> Pertama, setiap query memakai
    /// <c>AsNoTracking</c> dan projection langsung ke DTO. Kedua, kolom sensitif —
    /// <c>Notes</c> dan <c>IsolationNote</c> — tidak pernah ikut pada daftar mana pun; ia
    /// hanya muncul pada detail. Ketiga, lokasi pasien selalu dibaca dari
    /// <c>InpBedPlacement</c>, tidak pernah dari kolom pada episode.
    /// </para>
    /// </remarks>
    public partial class InpEpisodeService
    {
        /// <summary>
        /// Membaca daftar episode yang sudah disaring dan diurutkan.
        /// </summary>
        /// <remarks>
        /// <b>Kedaluwarsa dijalankan lebih dulu.</b> Episode <c>Draft</c> yang telantar
        /// dibatalkan pada saat dibaca, bukan oleh program penjadwal. Bila daftar ini membaca
        /// tabel tanpa menjalankan perhitungan itu, layar akan menampilkan admisi yang
        /// sesungguhnya sudah gugur — dan petugas akan mencoba melanjutkannya lalu ditolak
        /// tanpa mengerti kenapa.
        /// </remarks>
        public async Task<InpatientEpisodePagedResult> GetEpisodeListAsync(
            InpatientEpisodeListQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new InpatientEpisodeListQuery();

            await ExpireDueDraftEpisodesAsync(cancellationToken);

            var (pageNumber, pageSize) = NormalizePaging(query.PageNumber, query.PageSize);

            var filtered = BuildFilteredEpisodeQuery(query);

            var descending = !string.Equals(
                query.SortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase);

            filtered = (query.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "episodenumber" => descending
                    ? filtered.OrderByDescending(x => x.EpisodeNumber)
                    : filtered.OrderBy(x => x.EpisodeNumber),
                "admittedat" => descending
                    ? filtered.OrderByDescending(x => x.AdmittedAt)
                    : filtered.OrderBy(x => x.AdmittedAt),
                "episodestatus" => descending
                    ? filtered.OrderByDescending(x => x.EpisodeStatus)
                    : filtered.OrderBy(x => x.EpisodeStatus),
                "patientname" => descending
                    ? filtered.OrderByDescending(x => x.Patient!.FullName)
                    : filtered.OrderBy(x => x.Patient!.FullName),
                _ => descending
                    ? filtered.OrderByDescending(x => x.CreateDateTime)
                    : filtered.OrderBy(x => x.CreateDateTime)
            };

            var totalData = await filtered.CountAsync(cancellationToken);

            var items = await filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InpatientEpisodeListItemResponse
                {
                    Id = x.Id,
                    EpisodeNumber = x.EpisodeNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    EpisodeStatus = (int)x.EpisodeStatus,
                    AdmittedAt = x.AdmittedAt,
                    DischargeDecidedAt = x.DischargeDecidedAt,
                    PhysicallyLeftAt = x.PhysicallyLeftAt,
                    ClosedAt = x.ClosedAt,
                    RequiresIsolation = x.RequiresIsolation,
                    ActiveDoctorName = x.DoctorAssignments
                        .Where(d => d.EndDateTime == null && !d.IsDelete)
                        .OrderByDescending(d => d.SequenceNumber)
                        .Select(d => d.Doctor != null ? d.Doctor.FullName : null)
                        .FirstOrDefault(),
                    ActiveNurseName = x.NurseAssignments
                        .Where(n => n.EndDateTime == null && !n.IsDelete)
                        .OrderByDescending(n => n.SequenceNumber)
                        .Select(n => n.Employee != null ? n.Employee.FullName : null)
                        .FirstOrDefault(),
                    CurrentBedName = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .OrderByDescending(p => p.SequenceNumber)
                        .Select(p => p.Bed != null ? p.Bed.BedName : null)
                        .FirstOrDefault(),
                    CurrentRoomName = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .OrderByDescending(p => p.SequenceNumber)
                        .Select(p => p.Room != null ? p.Room.RoomName : null)
                        .FirstOrDefault(),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.EpisodeStatusName = ((InpEpisodeStatus)item.EpisodeStatus).ToString();
            }

            return new InpatientEpisodePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Menghitung jumlah episode per status memakai penyaring yang sama dengan daftar.
        /// </summary>
        /// <remarks>
        /// Kelima status selalu dikembalikan walaupun jumlahnya nol, supaya layar tidak perlu
        /// menebak status mana yang hilang dari jawaban lalu menganggapnya nol sendiri.
        /// </remarks>
        public async Task<InpatientEpisodeSummaryResponse> GetEpisodeSummaryAsync(
            InpatientEpisodeListQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new InpatientEpisodeListQuery();

            await ExpireDueDraftEpisodesAsync(cancellationToken);

            var grouped = await BuildFilteredEpisodeQuery(query)
                .GroupBy(x => x.EpisodeStatus)
                .Select(g => new { Status = g.Key, Total = g.Count() })
                .ToListAsync(cancellationToken);

            var byStatus = Enum.GetValues<InpEpisodeStatus>()
                .Select(status => new InpatientEpisodeStatusCountResponse
                {
                    EpisodeStatus = (int)status,
                    EpisodeStatusName = status.ToString(),
                    Total = grouped.FirstOrDefault(g => g.Status == status)?.Total ?? 0
                })
                .ToList();

            return new InpatientEpisodeSummaryResponse
            {
                TotalAll = byStatus.Sum(x => x.Total),
                ByStatus = byStatus
            };
        }

        /// <summary>
        /// Menyusun pilihan penyaring layar daftar episode beserta nilai bawaannya.
        /// </summary>
        /// <remarks>
        /// Unit layanan yang ditawarkan hanya yang bertipe <c>Inpatient</c>, dan kelas
        /// perawatan hanya yang bertanda <c>IsForInpatient</c>. Menawarkan seluruh master
        /// akan membuat petugas menyaring memakai unit poliklinik lalu menerima daftar kosong
        /// tanpa penjelasan.
        /// </remarks>
        public async Task<InpatientEpisodeFilterMetadataResponse> GetFilterMetadataAsync(
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

            return new InpatientEpisodeFilterMetadataResponse
            {
                DefaultFilter = new InpatientEpisodeDefaultFilterResponse(),
                SortOptions = new List<InpatientSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Waktu admisi dibuka" },
                    new() { Value = "episodeNumber", Label = "Nomor episode" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "admittedAt", Label = "Waktu pasien ditempatkan" },
                    new() { Value = "episodeStatus", Label = "Status episode" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EpisodeStatusOptions = BuildEpisodeStatusOptions(),
                ServiceUnitOptions = serviceUnits,
                PatientClassOptions = patientClasses,
                ResetButtonLabel = "Reset"
            };
        }

        /// <summary>
        /// Membaca riwayat perpindahan status satu episode, urut nomor urut.
        /// </summary>
        /// <remarks>
        /// <b>Hanya pembacaan.</b> Tidak ada endpoint yang dapat mengubah maupun menghapus
        /// baris riwayat, dan ketiadaan itu disengaja — api contract bagian 8 dan
        /// <c>RWI-RULE-031</c> aturan 5.
        ///
        /// <para>
        /// <b>Kolom pelaku kosong untuk perubahan yang dihitung sistem.</b> Episode
        /// <c>Draft</c> yang gugur sendiri dicatat sebagai tindakan sistem, bukan atas nama
        /// pengguna yang kebetulan membuka layar saat perhitungan itu berjalan. Ini masalah
        /// keadilan, bukan teknis: laporan pengecualian yang menuduh orang yang tidak melakukan
        /// apa-apa lebih buruk daripada laporan yang tidak menyebut siapa pun.
        /// </para>
        ///
        /// <para>
        /// Riwayat tetap terbaca setelah episode <c>Closed</c>. Tidak ada penyaringan status di
        /// sini sama sekali — justru episode yang sudah ditutup yang paling sering ditelusuri
        /// auditor.
        /// </para>
        /// </remarks>
        public async Task<List<InpatientStatusHistoryResponse>> GetStatusHistoryAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var rows = await _dbContext.Set<InpStatusHistory>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.SequenceNumber)
                .Select(x => new InpatientStatusHistoryResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    SequenceNumber = x.SequenceNumber,
                    FromStatus = x.FromStatus != null ? (int)x.FromStatus : null,
                    ToStatus = (int)x.ToStatus,
                    ActionType = x.ActionType,
                    ActorType = (int)x.ActorType,
                    ChangedByUserId = x.ChangedByUserId,
                    ChangedAt = x.ChangedAt,
                    Reason = x.Reason
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                row.FromStatusName = row.FromStatus.HasValue
                    ? ((InpEpisodeStatus)row.FromStatus.Value).ToString()
                    : null;
                row.ToStatusName = ((InpEpisodeStatus)row.ToStatus).ToString();
                row.ActorTypeName = ((InpStatusChangeActorType)row.ActorType).ToString();
            }

            return rows;
        }

        /// <summary>
        /// Menggugurkan seluruh episode <c>Draft</c> yang sudah melewati batas waktu, lalu
        /// mengembalikan jumlahnya.
        /// </summary>
        /// <remarks>
        /// <b>Kenapa ini ada di jalur baca.</b> Modul ini sengaja tidak memakai program
        /// penjadwal (<c>RWI-DEC-030</c>). Konsekuensinya, baris <c>Draft</c> basi tetap ada
        /// di tabel sampai seseorang membacanya. Endpoint daftar dan ringkasan adalah pembaca
        /// yang paling sering dipanggil, sehingga di sinilah perhitungan itu dijalankan.
        ///
        /// <para>
        /// Setiap episode digugurkan lewat jalur yang sama dengan pembatalan biasa, sehingga
        /// pemesanan dilepas, kunjungan yang lahir bersama episode ikut dibatalkan, dan satu
        /// baris riwayat lahir bertanda dilakukan sistem. Tidak ada jalan pintas yang hanya
        /// menyetel kolom status.
        /// </para>
        /// </remarks>
        public async Task<int> ExpireDueDraftEpisodesAsync(
            CancellationToken cancellationToken = default)
        {
            var setting = await _settingService.GetEffectiveSettingAsync(cancellationToken);
            var cutoff = DateTime.UtcNow.AddHours(-setting.DraftEpisodeExpiryHours);

            var dueIds = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x =>
                    x.EpisodeStatus == InpEpisodeStatus.Draft &&
                    !x.IsDelete &&
                    (x.UpdateDateTime ?? x.CreateDateTime) <= cutoff)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (dueIds.Count == 0)
            {
                return 0;
            }

            var expired = 0;

            foreach (var id in dueIds)
            {
                var episode = await LoadEpisodeForWriteAsync(id, cancellationToken);

                if (episode == null)
                {
                    continue;
                }

                if (await ExpireDraftIfDueAsync(episode, cancellationToken))
                {
                    expired++;
                }
            }

            return expired;
        }

        /// <summary>
        /// Membaca satu episode beserta seluruh isian detailnya, sesudah perhitungan
        /// kedaluwarsa dijalankan pada episode itu.
        /// </summary>
        public async Task<InpatientEpisodeDetailResponse?> GetEpisodeDetailAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await LoadEpisodeForWriteAsync(episodeId, cancellationToken);

            if (episode == null)
            {
                return null;
            }

            await ExpireDraftIfDueAsync(episode, cancellationToken);

            return await GetDetailResponseAsync(episodeId, null, cancellationToken);
        }

        // =====================================================================
        // Pembantu bagian baca
        // =====================================================================

        private IQueryable<InpEpisode> BuildFilteredEpisodeQuery(InpatientEpisodeListQuery query)
        {
            IQueryable<InpEpisode> filtered = _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();

                filtered = filtered.Where(x =>
                    x.EpisodeNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            }

            if (query.PatientClassId.HasValue && query.PatientClassId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.PatientClassId == query.PatientClassId.Value);
            }

            if (query.PatientId.HasValue && query.PatientId.Value != Guid.Empty)
            {
                filtered = filtered.Where(x => x.PatientId == query.PatientId.Value);
            }

            if (query.EpisodeStatus.HasValue &&
                Enum.IsDefined(typeof(InpEpisodeStatus), query.EpisodeStatus.Value))
            {
                var status = (InpEpisodeStatus)query.EpisodeStatus.Value;
                filtered = filtered.Where(x => x.EpisodeStatus == status);
            }

            if (query.StartDate.HasValue)
            {
                filtered = filtered.Where(x => x.CreateDateTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                var exclusiveEnd = query.EndDate.Value.Date.AddDays(1);
                filtered = filtered.Where(x => x.CreateDateTime < exclusiveEnd);
            }

            if (query.RequiresIsolation.HasValue)
            {
                filtered = filtered.Where(x => x.RequiresIsolation == query.RequiresIsolation.Value);
            }

            return filtered;
        }

        private static List<InpatientOptionResponse> BuildEpisodeStatusOptions()
        {
            return new List<InpatientOptionResponse>
            {
                new() { Value = ((int)InpEpisodeStatus.Draft).ToString(), Label = "Sedang disiapkan" },
                new() { Value = ((int)InpEpisodeStatus.Admitted).ToString(), Label = "Sedang dirawat" },
                new() { Value = ((int)InpEpisodeStatus.DischargePending).ToString(), Label = "Boleh pulang" },
                new() { Value = ((int)InpEpisodeStatus.Closed).ToString(), Label = "Sudah ditutup" },
                new() { Value = ((int)InpEpisodeStatus.Cancelled).ToString(), Label = "Dibatalkan" }
            };
        }

        internal static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);

            return (pageNumber, pageSize);
        }
    }
}
