using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;
using P = QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Constants.WorkforceDashboardPanels;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    /// <summary>
    /// Dashboard SDM baca-saja (Paket B, kontrak v30 bagian 36.1; BE-HRD-458 s.d. BE-HRD-461).
    /// </summary>
    /// <remarks>
    /// <para>Panel 1–3 dibaca dari <c>MstEmployee</c> (pegawai yang sedang skorsing tetap aktif karena skorsing hanya
    /// menonaktifkan akun login, <c>HRD-ASM-93</c>). Panel 4 memakai <see cref="ContractRenewalBuckets"/> yang juga dipakai
    /// layar Kontrak (<c>HRD-DEC-245</c>). Panel 5 memakai radar lisensi dan radar sertifikat milik modul sumber, ditambah
    /// kueri baca SPK/RKK aktif (<c>INT-HR-116</c>, <c>INT-HR-117</c>).</para>
    /// <para>Daftar nama memakai aturan kelompok yang sama persis dengan ringkasan, sehingga jumlah baris daftar sama
    /// dengan angka panel. Respons tidak memuat gaji, NIK, data disiplin, atau data kesehatan; nomor lisensi hanya
    /// tampil 4 karakter terakhir.</para>
    /// </remarks>
    public class HrdWorkforceDashboardService : IHrdWorkforceDashboardService
    {
        public const string UnknownPanelOrBucketMessage = "Panel atau kelompok dashboard tidak dikenal.";
        public const string LimitExceededMessage = "Parameter halaman atau pencarian melebihi batas.";
        public const int MaxSearchLength = 100;
        public const int MaxPageSize = 100;

        private const string SpkActiveStatus = "Active";

        private readonly ApplicationDbContext _dbContext;
        private readonly ICredentialVerificationService _credentialVerificationService;
        private readonly ITrainingExpiryRadarService _trainingExpiryRadarService;

        public HrdWorkforceDashboardService(
            ApplicationDbContext dbContext,
            ICredentialVerificationService credentialVerificationService,
            ITrainingExpiryRadarService trainingExpiryRadarService)
        {
            _dbContext = dbContext;
            _credentialVerificationService = credentialVerificationService;
            _trainingExpiryRadarService = trainingExpiryRadarService;
        }

        // =============================================================================================
        // Ringkasan lima panel
        // =============================================================================================

        public async Task<WorkforceDashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var operationalDate = AppDateTimeHelper.OperationalDate();
            var (monthStart, nextMonth) = OperationalMonthRange(operationalDate);
            var employees = ActiveRecordEmployees();

            var response = new WorkforceDashboardSummaryResponse
            {
                OperationalDate = operationalDate,
                GeneratedAt = DateTime.UtcNow
            };

            // Panel 1 — ringkasan pegawai
            var activeCount = await employees.CountAsync(e => e.IsActive, cancellationToken);
            response.EmployeeOverview = new WorkforceDashboardEmployeeOverviewDto
            {
                ActiveCount = activeCount,
                NewThisMonthCount = await employees.CountAsync(e => e.JoinDate >= monthStart && e.JoinDate < nextMonth, cancellationToken),
                LeftThisMonthCount = await employees.CountAsync(
                    e => e.ResignDate.HasValue && e.ResignDate.Value >= monthStart && e.ResignDate.Value < nextMonth, cancellationToken)
            };

            // Panel 2 — distribusi profesi pegawai aktif
            response.ProfessionDistribution = await GetProfessionDistributionAsync(employees, cancellationToken);

            // Panel 3 — aktif / nonaktif
            response.ActiveStatus = new WorkforceDashboardActiveStatusDto
            {
                ActiveCount = activeCount,
                InactiveCount = await employees.CountAsync(e => !e.IsActive, cancellationToken)
            };

            // Panel 4 — kontrak berakhir (BE-HRD-459)
            response.ContractExpiry = await GetContractExpiryAsync(cancellationToken);

            // Panel 5 — dokumen kedaluwarsa (BE-HRD-460)
            response.DocumentExpiry = BuildDocumentExpiry(await LoadDocumentRowsAsync(operationalDate, cancellationToken));

            return response;
        }

        private async Task<List<WorkforceDashboardProfessionItemDto>> GetProfessionDistributionAsync(
            IQueryable<MstEmployee> employees,
            CancellationToken cancellationToken)
        {
            var groups = await employees
                .Where(e => e.IsActive)
                .GroupBy(e => e.ProfessionId)
                .Select(g => new { ProfessionId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var professionIds = groups.Where(g => g.ProfessionId.HasValue).Select(g => g.ProfessionId!.Value).ToList();
            var names = professionIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.MstProfessions
                    .AsNoTracking()
                    .Where(p => professionIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.ProfessionName, cancellationToken);

            return groups
                .Select(g => new WorkforceDashboardProfessionItemDto
                {
                    ProfessionId = g.ProfessionId,
                    ProfessionName = ProfessionLabel(g.ProfessionId, names),
                    ActiveCount = g.Count
                })
                .OrderByDescending(x => x.ActiveCount)
                .ThenBy(x => x.ProfessionName)
                .ToList();
        }

        private async Task<WorkforceDashboardContractExpiryDto> GetContractExpiryAsync(CancellationToken cancellationToken)
        {
            // Tanggal acuan dan seluruh tahapan berasal dari ContractRenewalBuckets, sama dengan layar Kontrak (HRD-DEC-245).
            var today = ContractRenewalBuckets.ReferenceDate();
            var contracts = _dbContext.WfpContractHistories.AsNoTracking();

            return new WorkforceDashboardContractExpiryDto
            {
                TotalWithin60Days = await contracts.CountAsync(ContractRenewalBuckets.ExpiringWithinWindow(today), cancellationToken),
                H60Count = await contracts.CountAsync(ContractRenewalBuckets.Stage(ContractRenewalBuckets.H60, today), cancellationToken),
                H14Count = await contracts.CountAsync(ContractRenewalBuckets.Stage(ContractRenewalBuckets.H14, today), cancellationToken),
                H7Count = await contracts.CountAsync(ContractRenewalBuckets.Stage(ContractRenewalBuckets.H7, today), cancellationToken),
                PendingDecisionCount = await _dbContext.HrdContractDecisions
                    .AsNoTracking()
                    .CountAsync(ContractRenewalBuckets.PendingDecisionPredicate(), cancellationToken),
                OverdueCount = await contracts.CountAsync(ContractRenewalBuckets.OverduePredicate(today), cancellationToken)
            };
        }

        private static WorkforceDashboardDocumentExpiryDto BuildDocumentExpiry(IReadOnlyCollection<DocumentRow> rows)
        {
            static int CountOf(IEnumerable<DocumentRow> source, string bucket) => source.Count(r => r.Bucket == bucket);

            return new WorkforceDashboardDocumentExpiryDto
            {
                ExpiredCount = CountOf(rows, P.BucketExpired),
                Within30Count = CountOf(rows, P.BucketWithin30),
                Days31To60Count = CountOf(rows, P.BucketDays31To60),
                Days61To90Count = CountOf(rows, P.BucketDays61To90),
                ByType = P.DocumentTypes
                    .Select(type =>
                    {
                        var ofType = rows.Where(r => r.DocumentType == type).ToList();
                        return new WorkforceDashboardDocumentTypeExpiryDto
                        {
                            DocumentType = type,
                            ExpiredCount = CountOf(ofType, P.BucketExpired),
                            Within30Count = CountOf(ofType, P.BucketWithin30),
                            Days31To60Count = CountOf(ofType, P.BucketDays31To60),
                            Days61To90Count = CountOf(ofType, P.BucketDays61To90)
                        };
                    })
                    .ToList()
            };
        }

        // =============================================================================================
        // Daftar nama (drill-down) — BE-HRD-461
        // =============================================================================================

        public async Task<WorkforceDashboardDrilldownResult> GetDrilldownAsync(
            WorkforceDashboardDrilldownQuery query,
            CancellationToken cancellationToken = default)
        {
            // VR-PKB-03: batas halaman dan pencarian.
            if (query.PageSize < 1 || query.PageSize > MaxPageSize || (query.Search?.Length ?? 0) > MaxSearchLength)
            {
                return WorkforceDashboardDrilldownResult.Invalid(LimitExceededMessage);
            }

            // VR-PKB-01: panel wajib dan dikenal.
            var panel = query.Panel?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(panel) || !P.Panels.Contains(panel))
            {
                return WorkforceDashboardDrilldownResult.Invalid(UnknownPanelOrBucketMessage);
            }

            // VR-PKB-02: kelompok sah untuk panelnya; professionId dan documentType hanya untuk panelnya masing-masing.
            var bucket = query.Bucket?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(bucket) || !P.IsValidBucket(panel, bucket))
            {
                return WorkforceDashboardDrilldownResult.Invalid(UnknownPanelOrBucketMessage);
            }

            if (query.ProfessionId.HasValue && panel != P.Profession)
            {
                return WorkforceDashboardDrilldownResult.Invalid(UnknownPanelOrBucketMessage);
            }

            var documentType = string.IsNullOrWhiteSpace(query.DocumentType) ? null : query.DocumentType.Trim().ToUpperInvariant();
            if (documentType != null && (panel != P.DocumentExpiry || !P.DocumentTypes.Contains(documentType)))
            {
                return WorkforceDashboardDrilldownResult.Invalid(UnknownPanelOrBucketMessage);
            }

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = query.PageSize;
            var pattern = HrCrossEmployeeLookup.LikePattern(query.Search);

            switch (panel)
            {
                case P.EmployeeOverview:
                case P.ActiveStatus:
                    return WorkforceDashboardDrilldownResult.Ok(
                        await GetEmployeeDrilldownAsync(panel, bucket, null, pattern, pageNumber, pageSize, cancellationToken));

                case P.Profession:
                {
                    Guid? professionId = null;
                    if (bucket != P.BucketUnassigned)
                    {
                        professionId = Guid.Parse(bucket);
                    }

                    if (query.ProfessionId.HasValue)
                    {
                        if (professionId.HasValue && professionId.Value != query.ProfessionId.Value)
                        {
                            return WorkforceDashboardDrilldownResult.Invalid(UnknownPanelOrBucketMessage);
                        }

                        professionId = query.ProfessionId.Value;
                    }

                    return WorkforceDashboardDrilldownResult.Ok(
                        await GetEmployeeDrilldownAsync(panel, bucket, professionId, pattern, pageNumber, pageSize, cancellationToken));
                }

                case P.ContractExpiry:
                    return WorkforceDashboardDrilldownResult.Ok(
                        await GetContractDrilldownAsync(bucket, pattern, pageNumber, pageSize, cancellationToken));

                default:
                    return WorkforceDashboardDrilldownResult.Ok(
                        await GetDocumentDrilldownAsync(bucket, documentType, query.Search, pageNumber, pageSize, cancellationToken));
            }
        }

        private async Task<PagedResult<WorkforceDashboardDrilldownItem>> GetEmployeeDrilldownAsync(
            string panel,
            string bucket,
            Guid? professionId,
            string? pattern,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var (monthStart, nextMonth) = OperationalMonthRange(AppDateTimeHelper.OperationalDate());
            var employees = ActiveRecordEmployees();
            string detailLabel;
            var useResignDate = false;

            if (panel == P.Profession)
            {
                employees = professionId.HasValue
                    ? employees.Where(e => e.IsActive && e.ProfessionId == professionId.Value)
                    : employees.Where(e => e.IsActive && e.ProfessionId == null);
                detailLabel = "Pegawai aktif";
            }
            else
            {
                switch (bucket)
                {
                    case P.BucketNewThisMonth:
                        employees = employees.Where(e => e.JoinDate >= monthStart && e.JoinDate < nextMonth);
                        detailLabel = "Masuk bulan ini";
                        break;
                    case P.BucketLeftThisMonth:
                        employees = employees.Where(e => e.ResignDate.HasValue && e.ResignDate.Value >= monthStart && e.ResignDate.Value < nextMonth);
                        detailLabel = "Keluar bulan ini";
                        useResignDate = true;
                        break;
                    case P.BucketInactive:
                        employees = employees.Where(e => !e.IsActive);
                        detailLabel = "Nonaktif";
                        useResignDate = true;
                        break;
                    default:
                        employees = employees.Where(e => e.IsActive);
                        detailLabel = "Pegawai aktif";
                        break;
                }
            }

            if (pattern != null)
            {
                employees = employees.Where(e => EF.Functions.ILike(e.FullName, pattern) || EF.Functions.ILike(e.EmployeeNumber, pattern));
            }

            var total = await employees.CountAsync(cancellationToken);
            var rows = await employees
                .OrderBy(e => e.FullName)
                .ThenBy(e => e.EmployeeNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.WorkforceProfileId,
                    e.EmployeeNumber,
                    e.FullName,
                    ProfessionName = e.Profession != null ? e.Profession.ProfessionName : null,
                    e.JoinDate,
                    e.ResignDate
                })
                .ToListAsync(cancellationToken);

            var items = rows.Select(r => new WorkforceDashboardDrilldownItem
            {
                WorkforceProfileId = r.WorkforceProfileId,
                EmployeeCode = r.EmployeeNumber,
                EmployeeName = r.FullName,
                ProfessionName = r.ProfessionName,
                DetailLabel = panel == P.Profession ? (r.ProfessionName ?? P.ProfessionUnassignedLabel) : detailLabel,
                ReferenceDate = useResignDate ? r.ResignDate : r.JoinDate,
                DaysRemaining = null
            }).ToList();

            return HrCrossEmployeeLookup.ToPaged(items, total, pageNumber, pageSize);
        }

        private async Task<PagedResult<WorkforceDashboardDrilldownItem>> GetContractDrilldownAsync(
            string bucket,
            string? pattern,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var today = ContractRenewalBuckets.ReferenceDate();
            var matchingProfileIds = pattern == null ? null : await SearchProfileIdsAsync(pattern, cancellationToken);

            if (bucket == ContractRenewalBuckets.PendingDecision)
            {
                var decisions = _dbContext.HrdContractDecisions.AsNoTracking().Where(ContractRenewalBuckets.PendingDecisionPredicate());
                if (matchingProfileIds != null)
                {
                    decisions = decisions.Where(d => matchingProfileIds.Contains(d.WorkforceProfileId));
                }

                var totalDecisions = await decisions.CountAsync(cancellationToken);
                var decisionRows = await decisions
                    .OrderBy(d => d.ContractHistory != null ? d.ContractHistory.EndDate : null)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new
                    {
                        d.WorkforceProfileId,
                        EndDate = d.ContractHistory != null ? d.ContractHistory.EndDate : null
                    })
                    .ToListAsync(cancellationToken);

                var decisionLabels = await LoadPersonLabelsAsync(decisionRows.Select(r => r.WorkforceProfileId), cancellationToken);
                var decisionItems = decisionRows.Select(r => ToItem(
                    decisionLabels,
                    r.WorkforceProfileId,
                    "Menunggu keputusan perpanjangan",
                    r.EndDate,
                    r.EndDate.HasValue ? (r.EndDate.Value.Date - today).Days : null)).ToList();

                return HrCrossEmployeeLookup.ToPaged(decisionItems, totalDecisions, pageNumber, pageSize);
            }

            var predicate = bucket switch
            {
                ContractRenewalBuckets.All => ContractRenewalBuckets.ExpiringWithinWindow(today),
                ContractRenewalBuckets.Overdue => ContractRenewalBuckets.OverduePredicate(today),
                _ => ContractRenewalBuckets.Stage(bucket, today)
            };

            var contracts = _dbContext.WfpContractHistories.AsNoTracking().Where(predicate);
            if (matchingProfileIds != null)
            {
                contracts = contracts.Where(c => matchingProfileIds.Contains(c.WorkforceProfileId));
            }

            var total = await contracts.CountAsync(cancellationToken);
            var rows = await contracts
                .OrderBy(c => c.EndDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.WorkforceProfileId,
                    c.EndDate,
                    ContractTypeName = c.ContractType != null ? c.ContractType.ContractTypeName : null
                })
                .ToListAsync(cancellationToken);

            var labels = await LoadPersonLabelsAsync(rows.Select(r => r.WorkforceProfileId), cancellationToken);
            var suffix = bucket == ContractRenewalBuckets.Overdue ? "lewat jatuh tempo" : "berakhir";
            var items = rows.Select(r => ToItem(
                labels,
                r.WorkforceProfileId,
                $"{(string.IsNullOrWhiteSpace(r.ContractTypeName) ? "Kontrak" : r.ContractTypeName)} {suffix}",
                r.EndDate,
                r.EndDate.HasValue ? (r.EndDate.Value.Date - today).Days : null)).ToList();

            return HrCrossEmployeeLookup.ToPaged(items, total, pageNumber, pageSize);
        }

        private async Task<PagedResult<WorkforceDashboardDrilldownItem>> GetDocumentDrilldownAsync(
            string bucket,
            string? documentType,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var rows = (await LoadDocumentRowsAsync(AppDateTimeHelper.OperationalDate(), cancellationToken))
                .Where(r => r.Bucket == bucket && (documentType == null || r.DocumentType == documentType))
                .ToList();

            var labels = await LoadPersonLabelsAsync(rows.Select(r => r.WorkforceProfileId), cancellationToken);
            var items = rows.Select(r =>
            {
                labels.TryGetValue(r.WorkforceProfileId, out var label);
                return new WorkforceDashboardDrilldownItem
                {
                    WorkforceProfileId = r.WorkforceProfileId,
                    EmployeeCode = label?.Code ?? r.SourceCode ?? string.Empty,
                    EmployeeName = label?.Name ?? r.SourceName ?? "Pegawai tidak ditemukan",
                    ProfessionName = label?.Profession ?? r.SourceProfession,
                    DetailLabel = r.DetailLabel,
                    ReferenceDate = r.ExpiryDate,
                    DaysRemaining = r.DaysRemaining
                };
            });

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                items = items.Where(i =>
                    i.EmployeeName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    i.EmployeeCode.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = items
                .OrderBy(i => i.DaysRemaining)
                .ThenBy(i => i.EmployeeName)
                .ToList();

            var page = filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return HrCrossEmployeeLookup.ToPaged(page, filtered.Count, pageNumber, pageSize);
        }

        // =============================================================================================
        // Sumber dokumen kedaluwarsa — BE-HRD-460 (kamus data v30 bagian 31.2)
        // =============================================================================================

        private sealed record DocumentRow(
            string DocumentType,
            string Bucket,
            Guid WorkforceProfileId,
            string? SourceCode,
            string? SourceName,
            string? SourceProfession,
            string DetailLabel,
            DateTime ExpiryDate,
            int DaysRemaining);

        /// <summary>
        /// Seluruh dokumen yang kedaluwarsa atau akan kedaluwarsa dalam 90 hari, sudah dikelompokkan. Ringkasan dan daftar
        /// nama memakai daftar yang sama, sehingga angka panel sama dengan jumlah baris daftar.
        /// </summary>
        private async Task<List<DocumentRow>> LoadDocumentRowsAsync(DateTime operationalDate, CancellationToken cancellationToken)
        {
            var rows = new List<DocumentRow>();

            // SIP/STR — radar lisensi milik CredentialingManagement (STR seumur hidup dan lisensi dicabut sudah dikecualikan).
            var licenses = await _credentialVerificationService.GetLicenseExpiringRadarListAsync(null, cancellationToken);
            foreach (var license in licenses)
            {
                var licenseBucket = MapLicenseRadarLevel(license.RadarLevel);
                if (licenseBucket == null)
                {
                    continue;
                }

                rows.Add(new DocumentRow(
                    P.DocumentSipStr,
                    licenseBucket,
                    license.WorkforceProfileId,
                    license.EmployeeCode,
                    license.EmployeeName,
                    null,
                    $"{license.LicenseType} {MaskReferenceNumber(license.LicenseNumber)}".Trim(),
                    license.ExpiredDate,
                    license.DaysRemaining));
            }

            // Sertifikat pelatihan wajib — radar milik LearningAndDevelopment.
            var certificateRadar = await _trainingExpiryRadarService.GetExpiringRadarSummaryAsync(cancellationToken);
            foreach (var certificate in certificateRadar.Items)
            {
                var certificateBucket = MapCertificateRadarZone(certificate.RadarZone);
                if (certificateBucket == null)
                {
                    continue;
                }

                rows.Add(new DocumentRow(
                    P.DocumentMandatoryCertificate,
                    certificateBucket,
                    certificate.WorkforceProfileId,
                    certificate.StaffCode,
                    certificate.StaffName,
                    certificate.Profession,
                    string.IsNullOrWhiteSpace(certificate.TrainingName) ? "Sertifikat pelatihan wajib" : certificate.TrainingName,
                    certificate.ExpiredDate,
                    certificate.DaysUntilExpiry));
            }

            // SPK/RKK aktif — belum ada radar di CredentialingManagement, sehingga dibaca langsung (Integration 26 butir 3).
            var referenceDate = DateTime.SpecifyKind(operationalDate.Date, DateTimeKind.Utc);
            var horizon = referenceDate.AddDays(90);
            var spkRows = await _dbContext.TrxSpkRkkIssuances
                .AsNoTracking()
                .Where(s => !s.IsDelete && s.IssuanceStatus == SpkActiveStatus && s.ValidUntil <= horizon)
                .Select(s => new { s.WorkforceProfileId, s.SpkNumber, s.ValidUntil })
                .ToListAsync(cancellationToken);

            foreach (var spk in spkRows)
            {
                var daysRemaining = (spk.ValidUntil.Date - referenceDate.Date).Days;
                var spkBucket = P.DocumentBucketFromDays(daysRemaining);
                if (spkBucket == null)
                {
                    continue;
                }

                rows.Add(new DocumentRow(
                    P.DocumentSpkRkk,
                    spkBucket,
                    spk.WorkforceProfileId,
                    null,
                    null,
                    null,
                    $"SPK/RKK {MaskReferenceNumber(spk.SpkNumber)}".Trim(),
                    spk.ValidUntil,
                    daysRemaining));
            }

            return rows;
        }

        /// <summary>Tingkat radar lisensi → kelompok dashboard (kamus data 31.2).</summary>
        private static string? MapLicenseRadarLevel(string? radarLevel) => radarLevel switch
        {
            "ExpiredH0" => P.BucketExpired,
            "H7" or "H30" => P.BucketWithin30,
            "H60" => P.BucketDays31To60,
            "H90" => P.BucketDays61To90,
            _ => null
        };

        /// <summary>Zona radar sertifikat → kelompok dashboard (kamus data 31.2).</summary>
        private static string? MapCertificateRadarZone(string? radarZone) => radarZone switch
        {
            "OVERDUE_H0" => P.BucketExpired,
            "RADAR_H30_CRITICAL" => P.BucketWithin30,
            "RADAR_H60" => P.BucketDays31To60,
            "RADAR_H90" => P.BucketDays61To90,
            _ => null
        };

        /// <summary>Nomor dokumen disamarkan: hanya 4 karakter terakhir yang tampil (permission-audit v30 bagian 34.2).</summary>
        internal static string MaskReferenceNumber(string? number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return string.Empty;
            }

            var trimmed = number.Trim();
            var lastFour = trimmed.Length <= 4 ? trimmed : trimmed[^4..];
            return "…" + lastFour;
        }

        // =============================================================================================
        // Pembantu
        // =============================================================================================

        private IQueryable<MstEmployee> ActiveRecordEmployees() =>
            _dbContext.MstEmployees.AsNoTracking().Where(e => !e.IsDelete);

        /// <summary>Awal bulan operasional dan awal bulan berikutnya, sebagai batas UTC untuk kolom tanggal.</summary>
        private static (DateTime MonthStart, DateTime NextMonth) OperationalMonthRange(DateTime operationalDate)
        {
            var monthStart = new DateTime(operationalDate.Year, operationalDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            return (monthStart, monthStart.AddMonths(1));
        }

        private static string ProfessionLabel(Guid? professionId, IReadOnlyDictionary<Guid, string> names)
        {
            if (!professionId.HasValue)
            {
                return P.ProfessionUnassignedLabel;
            }

            return names.TryGetValue(professionId.Value, out var name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : "Profesi tidak ditemukan";
        }

        private sealed record PersonLabel(string Code, string Name, string? Profession);

        /// <summary>Nomor, nama, dan profesi per <c>MstWorkforceProfile.Id</c>; dari <c>MstEmployee</c> bila ada, selain itu dari profil.</summary>
        private async Task<Dictionary<Guid, PersonLabel>> LoadPersonLabelsAsync(
            IEnumerable<Guid> workforceProfileIds,
            CancellationToken cancellationToken)
        {
            var ids = workforceProfileIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<Guid, PersonLabel>();
            }

            var employees = await _dbContext.MstEmployees
                .AsNoTracking()
                .Where(e => !e.IsDelete && ids.Contains(e.WorkforceProfileId))
                .Select(e => new
                {
                    e.WorkforceProfileId,
                    e.EmployeeNumber,
                    e.FullName,
                    e.IsActive,
                    ProfessionName = e.Profession != null ? e.Profession.ProfessionName : null
                })
                .ToListAsync(cancellationToken);

            var result = employees
                .GroupBy(e => e.WorkforceProfileId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var e = g.OrderByDescending(x => x.IsActive).First();
                        return new PersonLabel(e.EmployeeNumber, e.FullName, e.ProfessionName);
                    });

            var missing = ids.Where(id => !result.ContainsKey(id)).ToList();
            if (missing.Count > 0)
            {
                var profiles = await _dbContext.MstWorkforceProfiles
                    .AsNoTracking()
                    .Where(p => missing.Contains(p.Id))
                    .Select(p => new { p.Id, p.ProfileCode, p.DisplayName })
                    .ToListAsync(cancellationToken);

                foreach (var p in profiles)
                {
                    result[p.Id] = new PersonLabel(p.ProfileCode, p.DisplayName, null);
                }
            }

            return result;
        }

        /// <summary>Profil yang nama atau nomornya cocok dengan pencarian (dari <c>MstEmployee</c> dan <c>MstWorkforceProfile</c>).</summary>
        private async Task<List<Guid>> SearchProfileIdsAsync(string pattern, CancellationToken cancellationToken)
        {
            var fromEmployees = await _dbContext.MstEmployees
                .AsNoTracking()
                .Where(e => !e.IsDelete && (EF.Functions.ILike(e.FullName, pattern) || EF.Functions.ILike(e.EmployeeNumber, pattern)))
                .Select(e => e.WorkforceProfileId)
                .ToListAsync(cancellationToken);

            var fromProfiles = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(p => !p.IsDelete && (EF.Functions.ILike(p.DisplayName, pattern) || EF.Functions.ILike(p.ProfileCode, pattern)))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            return fromEmployees.Concat(fromProfiles).Distinct().ToList();
        }

        private static WorkforceDashboardDrilldownItem ToItem(
            IReadOnlyDictionary<Guid, PersonLabel> labels,
            Guid workforceProfileId,
            string detailLabel,
            DateTime? referenceDate,
            int? daysRemaining)
        {
            labels.TryGetValue(workforceProfileId, out var label);
            return new WorkforceDashboardDrilldownItem
            {
                WorkforceProfileId = workforceProfileId,
                EmployeeCode = label?.Code ?? string.Empty,
                EmployeeName = label?.Name ?? "Pegawai tidak ditemukan",
                ProfessionName = label?.Profession,
                DetailLabel = detailLabel,
                ReferenceDate = referenceDate,
                DaysRemaining = daysRemaining
            };
        }
    }
}
