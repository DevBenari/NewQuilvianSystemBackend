using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Globalization;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;

/// <summary>
/// Keputusan HRD yang dibaca dari dokumen penutupan benefit. Selalu terpisah dari status dokumen Finance.
/// </summary>
public sealed record HrdApprovalMetadata(
    string Status,
    Guid? ApproverUserId,
    DateTimeOffset? ActionDate,
    string? Notes,
    string? RejectionReason);

/// <summary>
/// Layanan persetujuan penutupan benefit RS internal oleh Human Resource Department (HRD).
/// HRD bertindak sebagai pihak yang berwenang memvalidasi kepesertaan, kelayakan, dan kecukupan plafon benefit karyawan/dokter
/// sebelum Finance menerbitkan pelunasan internal dan pemotongan saldo.
/// </summary>
public sealed class HrdBenefitSettlementApprovalService
{
    // Status persetujuan HRD (keputusan). Tidak ada nilai di sini yang berasal dari status dokumen Finance.
    public const string StatusPending = "MENUNGGU_PERSETUJUAN";
    public const string StatusApproved = "DISETUJUI";
    public const string StatusRejected = "DITOLAK";

    /// <summary>Dokumen sudah DITERBITKAN tetapi tidak ada bukti keputusan HRD pada catatan dokumen (data historis).</summary>
    public const string StatusNotVerifiable = "TIDAK_DAPAT_DIVERIFIKASI";

    /// <summary>Dokumen DIBATALKAN sebelum HRD sempat memutuskan.</summary>
    public const string StatusNotDecided = "TANPA_KEPUTUSAN";

    // Hanya dipakai sebagai nilai filter lama (status dokumen Finance); tidak pernah dikembalikan sebagai ApprovalStatus.
    public const string StatusPosted = "DITERBITKAN";
    public const string StatusCancelled = "DIBATALKAN";

    // Metadata keputusan HRD disimpan sebagai tag di kolom Notes (belum ada tabel approval terstruktur).
    private const string ApprovedTagPrefix = "[HRD_APPROVED:";
    private const string RejectedTagPrefix = "[HRD_REJECTED:";
    private const int MaxNotesLength = 500;
    private const string NotesSeparator = " | ";
    private const string CompactTimestampFormat = "yyyyMMdd'T'HHmmss'Z'";

    // Tag baru: [HRD_xxx:{guid}:{yyyyMMddTHHmmssZ}[:{teks ter-escape}]]
    // Tag lama: [HRD_xxx:{guid}:{ISO-8601 dengan titik dua}[:{teks}]] — tetap terbaca agar riwayat tidak hilang.
    private static readonly Regex DecisionTagRegex = new(
        @"\[HRD_(?<kind>APPROVED|REJECTED):(?<actor>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})"
        + @"(?::(?<ts>\d{8}T\d{6}Z|\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})))?"
        + @"(?::(?<text>[^\]]*))?\]",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RepeatedSeparatorRegex = new(
        @"(\s*\|\s*){2,}", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly ApplicationDbContext _dbContext;
    private readonly EmployeeBenefitBalanceService _balanceService;

    public HrdBenefitSettlementApprovalService(
        ApplicationDbContext dbContext,
        EmployeeBenefitBalanceService balanceService)
    {
        _dbContext = dbContext;
        _balanceService = balanceService;
    }

    public async Task<PagedResult<HrdBenefitSettlementApprovalItemResponse>> GetPagedAsync(
        HrdBenefitSettlementApprovalQuery query,
        CancellationToken cancellationToken)
    {
        var baseQuery = _dbContext.FinBenefitSettlements.AsNoTracking()
            .Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(query.PeriodCode))
        {
            baseQuery = baseQuery.Where(x => x.AccountingPeriodCode == query.PeriodCode.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.StartDate) && DateOnly.TryParse(query.StartDate.Trim(), out var parsedStart))
        {
            var startUtc = parsedStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            baseQuery = baseQuery.Where(x => x.CreateDateTime >= startUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.EndDate) && DateOnly.TryParse(query.EndDate.Trim(), out var parsedEnd))
        {
            var endUtc = parsedEnd.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            baseQuery = baseQuery.Where(x => x.CreateDateTime < endUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Pencarian di server pada seluruh data: nomor dokumen ATAU nama pemilik benefit
            // (profil tenaga kerja, karyawan lama, atau snapshot penjamin kunjungan).
            var pattern = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.SettlementNumber, pattern)
                || x.Items.Any(i => !i.IsDelete && _dbContext.FinReceivables.Any(r =>
                    r.Id == i.ReceivableId
                    && (_dbContext.MstWorkforceProfiles.Any(w =>
                            w.Id == r.BenefitOwnerWorkforceProfileId
                            && (EF.Functions.ILike(w.DisplayName, pattern)
                                || (w.Employee != null && EF.Functions.ILike(w.Employee.FullName, pattern))
                                || (w.Doctor != null && EF.Functions.ILike(w.Doctor.FullName, pattern))))
                        || _dbContext.MstEmployees.Any(e =>
                            e.Id == r.BenefitOwnerId && EF.Functions.ILike(e.FullName, pattern))
                        || _dbContext.BilInvoices.Any(inv =>
                            inv.Id == r.InvoiceId
                            && _dbContext.RegPatientEncounterGuarantors.Any(g =>
                                g.EncounterId == inv.EncounterId
                                && g.IsActive && !g.IsDelete
                                && g.EmployeeNameSnapshot != null
                                && EF.Functions.ILike(g.EmployeeNameSnapshot, pattern)))))));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "all", StringComparison.OrdinalIgnoreCase))
        {
            baseQuery = ApplyStatusFilter(baseQuery, NormalizeStatusFilter(query.Status));
        }

        var total = await baseQuery.CountAsync(cancellationToken);

        var descending = !string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var pageItems = await ApplySort(baseQuery, query.SortBy, descending)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var metas = pageItems.ToDictionary(
            x => x.Id,
            x => ParseApprovalMetadata(x.Notes, x.CancelReason, x.Status));

        var userIds = pageItems.Select(x => x.CreateBy)
            .Concat(metas.Values.Where(m => m.ApproverUserId.HasValue).Select(m => m.ApproverUserId!.Value));
        var userNames = await ResolveUserNamesAsync(userIds, cancellationToken);

        var debtorIds = pageItems.Select(x => x.DebtorReferenceId).Distinct().ToList();
        var debtorNames = debtorIds.Count > 0
            ? await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .Where(c => debtorIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CompanyGuarantorName, cancellationToken)
            : new Dictionary<Guid, string>();

        // Jumlah pemilik benefit per dokumen: aturan resolusi identitas SAMA dengan detail (penjamin kunjungan,
        // kartu piutang, lalu pemetaan karyawan lama ke profil tenaga kerja), bukan hitungan nama dan bukan angka paksa.
        var ownerTallies = await CountOwnersAsync(pageItems.Select(x => x.Id).ToList(), cancellationToken);

        var mapped = new List<HrdBenefitSettlementApprovalItemResponse>(pageItems.Count);
        foreach (var item in pageItems)
        {
            var meta = metas[item.Id];
            debtorNames.TryGetValue(item.DebtorReferenceId, out var debtorName);
            ownerTallies.TryGetValue(item.Id, out var tally);

            mapped.Add(new HrdBenefitSettlementApprovalItemResponse
            {
                Id = item.Id,
                SettlementNumber = item.SettlementNumber,
                CreatedDate = item.CreateDateTime,
                AccountingPeriodCode = item.AccountingPeriodCode,
                DebtorReferenceId = item.DebtorReferenceId,
                DebtorName = debtorName,
                TotalAmount = item.TotalAmount,
                ItemCount = item.ItemCount,
                OwnerCount = tally?.OwnerCount ?? 0,
                UnidentifiedOwnerCount = tally?.UnidentifiedCount ?? 0,
                DocumentStatus = item.Status,
                ApprovalStatus = meta.Status,
                CreatedByName = userNames.GetValueOrDefault(item.CreateBy),
                ApprovedByName = meta.ApproverUserId.HasValue ? userNames.GetValueOrDefault(meta.ApproverUserId.Value) : null,
                ApprovedAt = meta.ActionDate,
                RejectionReason = meta.RejectionReason,
                RowVersion = item.RowVersion
            });
        }

        return new PagedResult<HrdBenefitSettlementApprovalItemResponse>
        {
            Items = mapped,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = total,
            TotalPage = Math.Max(1, (int)Math.Ceiling((double)total / query.PageSize))
        };
    }

    public async Task<HrdBenefitSettlementApprovalDetailResponse> GetDetailAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var settlement = await _dbContext.FinBenefitSettlements.AsNoTracking()
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Dokumen penutupan benefit tidak ditemukan.");

        var approvalMeta = ParseApprovalMetadata(settlement.Notes, settlement.CancelReason, settlement.Status);
        var activeItems = settlement.Items.Where(x => !x.IsDelete).ToList();

        var userIds = new List<Guid> { settlement.CreateBy };
        if (approvalMeta.ApproverUserId.HasValue) userIds.Add(approvalMeta.ApproverUserId.Value);
        if (settlement.PostedBy.HasValue) userIds.Add(settlement.PostedBy.Value);
        var userNames = await ResolveUserNamesAsync(userIds, cancellationToken);

        var debtorName = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == settlement.DebtorReferenceId)
            .Select(x => x.CompanyGuarantorName)
            .FirstOrDefaultAsync(cancellationToken);

        var receivableIds = activeItems.Select(x => x.ReceivableId).Distinct().ToList();
        var receivables = receivableIds.Count > 0
            ? await _dbContext.FinReceivables.AsNoTracking()
                .Where(x => receivableIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, FinReceivable>();

        var receivableItems = receivableIds.Count > 0
            ? await _dbContext.FinReceivableItems.AsNoTracking()
                .Where(x => !x.IsDelete && receivableIds.Contains(x.ReceivableId))
                .ToListAsync(cancellationToken)
            : new List<FinReceivableItem>();

        var invoiceIds = receivables.Values.Where(r => r.InvoiceId.HasValue).Select(r => r.InvoiceId!.Value)
            .Concat(receivableItems.Where(i => i.InvoiceId.HasValue).Select(i => i.InvoiceId!.Value))
            .Distinct().ToList();

        var invoices = invoiceIds.Count > 0
            ? await _dbContext.BilInvoices.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, BilInvoice>();

        var encounterIds = invoices.Values.Select(x => x.EncounterId)
            .Concat(receivableItems.Where(i => i.EncounterId.HasValue).Select(i => i.EncounterId!.Value))
            .Where(x => x != Guid.Empty).Distinct().ToList();

        var encounters = encounterIds.Count > 0
            ? await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(x => encounterIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, RegPatientEncounter>();

        var patientIds = receivableItems.Where(i => i.PatientId.HasValue).Select(i => i.PatientId!.Value)
            .Concat(encounters.Values.Select(e => e.PatientId))
            .Where(p => p != Guid.Empty).Distinct().ToList();

        var patients = patientIds.Count > 0
            ? await _dbContext.MstPatients.AsNoTracking()
                .Where(x => patientIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, MstPatient>();

        // Penjamin benefit pada kunjungan: sumber identitas pemilik (dan tanggungan) yang sama dengan
        // yang dipakai Finance saat memotong plafon.
        var benefitGuarantors = new Dictionary<Guid, RegPatientEncounterGuarantor>();
        if (encounterIds.Count > 0)
        {
            var guarantorRows = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                .Include(g => g.CompanyGuarantor)
                .Where(g => encounterIds.Contains(g.EncounterId) && g.IsActive && !g.IsDelete && !g.IsCancel)
                .ToListAsync(cancellationToken);

            foreach (var group in guarantorRows.GroupBy(g => g.EncounterId))
            {
                var chosen = PickBenefitGuarantor(group);
                if (chosen != null) benefitGuarantors[group.Key] = chosen;
            }
        }

        // Identitas legacy (MstEmployee.Id) diterjemahkan ke profil tenaga kerja canonical.
        var legacyEmployeeIds = receivables.Values.Where(r => r.BenefitOwnerId.HasValue).Select(r => r.BenefitOwnerId!.Value)
            .Concat(benefitGuarantors.Values.Where(g => g.EmployeeIdSnapshot.HasValue).Select(g => g.EmployeeIdSnapshot!.Value))
            .Distinct().ToList();

        var legacyEmployeeProfiles = legacyEmployeeIds.Count > 0
            ? await _dbContext.MstEmployees.AsNoTracking()
                .Where(e => legacyEmployeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.WorkforceProfileId, cancellationToken)
            : new Dictionary<Guid, Guid>();

        var profileIds = new HashSet<Guid>(legacyEmployeeProfiles.Values);
        foreach (var r in receivables.Values)
        {
            if (r.BenefitOwnerWorkforceProfileId.HasValue) profileIds.Add(r.BenefitOwnerWorkforceProfileId.Value);
        }
        foreach (var g in benefitGuarantors.Values)
        {
            if (g.WorkforceProfileIdSnapshot.HasValue) profileIds.Add(g.WorkforceProfileIdSnapshot.Value);
        }

        var ownerProfiles = profileIds.Count > 0
            ? await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Include(w => w.Employee)
                .Include(w => w.Doctor)
                .Where(w => profileIds.Contains(w.Id) && !w.IsDelete)
                .ToDictionaryAsync(w => w.Id, cancellationToken)
            : new Dictionary<Guid, MstWorkforceProfile>();

        var balanceCache = new Dictionary<string, BenefitBalanceReadResult>();
        var built = new List<(HrdBenefitSettlementOwnerRowResponse Row, string? BalanceKey)>();
        var ownerKeys = new HashSet<Guid>();
        var missingReceivables = 0;
        var unidentifiedOwners = 0;

        foreach (var item in activeItems)
        {
            // Item tanpa kartu piutang yang sah TIDAK disembunyikan: dihitung dan dilaporkan sebagai masalah integritas.
            if (!receivables.TryGetValue(item.ReceivableId, out var receivable))
            {
                missingReceivables++;
                continue;
            }

            var rItem = receivableItems.FirstOrDefault(x => x.ReceivableId == item.ReceivableId);
            BilInvoice? inv = null;
            if (receivable.InvoiceId.HasValue) invoices.TryGetValue(receivable.InvoiceId.Value, out inv);
            else if (rItem?.InvoiceId.HasValue == true) invoices.TryGetValue(rItem.InvoiceId.Value, out inv);

            RegPatientEncounter? enc = null;
            if (inv != null && encounters.TryGetValue(inv.EncounterId, out var e1)) enc = e1;
            else if (rItem?.EncounterId.HasValue == true && encounters.TryGetValue(rItem.EncounterId.Value, out var e2)) enc = e2;

            MstPatient? patient = null;
            if (rItem?.PatientId.HasValue == true && patients.TryGetValue(rItem.PatientId.Value, out var p1)) patient = p1;
            else if (enc != null && patients.TryGetValue(enc.PatientId, out var p2)) patient = p2;

            RegPatientEncounterGuarantor? guarantor = null;
            if (enc != null) benefitGuarantors.TryGetValue(enc.Id, out guarantor);

            // Pasien TIDAK dipakai sebagai pemilik benefit: hanya penjamin kunjungan / kartu piutang.
            var profileId = guarantor?.WorkforceProfileIdSnapshot ?? receivable.BenefitOwnerWorkforceProfileId;
            var legacyEmployeeId = guarantor?.EmployeeIdSnapshot ?? receivable.BenefitOwnerId;
            var familyMemberId = guarantor?.FamilyMemberIdSnapshot;
            if (!profileId.HasValue && legacyEmployeeId.HasValue
                && legacyEmployeeProfiles.TryGetValue(legacyEmployeeId.Value, out var mappedProfileId))
            {
                profileId = mappedProfileId;
            }

            string? ownerName = null;
            string? staffNumber = null;
            string? ownerKind = null;
            if (profileId.HasValue && ownerProfiles.TryGetValue(profileId.Value, out var profile))
            {
                if (profile.Doctor != null)
                {
                    ownerName = profile.Doctor.FullName;
                    staffNumber = NullIfBlank(profile.Doctor.DoctorCode) ?? NullIfBlank(profile.Doctor.IdentityNumber);
                    ownerKind = "Dokter";
                }
                else if (profile.Employee != null)
                {
                    ownerName = profile.Employee.FullName;
                    staffNumber = NullIfBlank(profile.Employee.IdentityNumber);
                    ownerKind = "Karyawan";
                }
                else
                {
                    ownerName = profile.DisplayName;
                }
            }

            // Saldo SAAT INI dari HRD. Gagal baca/tidak layak => null, bukan 0.
            BenefitBalanceReadResult? balance = null;
            string? balanceNote = null;
            var hasIdentity = profileId.HasValue || legacyEmployeeId.HasValue;
            if (!hasIdentity)
            {
                balanceNote = "Pemilik benefit tidak dapat ditentukan dari kartu piutang maupun penjamin kunjungan.";
            }
            else
            {
                var asOfDate = (inv?.InvoiceDate ?? receivable.RecognizedAt).UtcDateTime;
                var cacheKey = $"{profileId}|{legacyEmployeeId}|{familyMemberId}|{asOfDate:yyyyMMdd}";
                if (balanceCache.TryGetValue(cacheKey, out var cachedBalance))
                {
                    balance = cachedBalance;
                }
                else
                {
                    try
                    {
                        balance = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
                        {
                            WorkforceProfileId = profileId,
                            EmployeeId = legacyEmployeeId ?? Guid.Empty,
                            FamilyMemberId = familyMemberId,
                            AsOfDate = asOfDate
                        }, cancellationToken);
                        balanceCache[cacheKey] = balance;
                    }
                    catch (InvalidOperationException)
                    {
                        balanceNote = "Saldo plafon tidak berhasil dibaca dari data HRD.";
                    }
                }

                if (balance != null && !balance.IsUsable)
                {
                    balanceNote = string.IsNullOrWhiteSpace(balance.Message)
                        ? $"Saldo plafon tidak dapat dipakai ({balance.ReasonCode})."
                        : balance.Message;
                }
            }

            BenefitBalanceReadResult? usable = balance is { IsUsable: true } ? balance : null;
            if (string.IsNullOrWhiteSpace(ownerName)) ownerName = NullIfBlank(usable?.OwnerName);
            if (string.IsNullOrWhiteSpace(ownerName)) ownerName = NullIfBlank(guarantor?.EmployeeNameSnapshot);

            // Kunci pemilik canonical (bukan nama) untuk menghitung jumlah pemilik.
            Guid? ownerKey = usable?.WorkforceProfileId ?? profileId ?? legacyEmployeeId;
            if (ownerKey.HasValue) ownerKeys.Add(ownerKey.Value);
            else unidentifiedOwners++;

            // Sumber saldo: orang yang dikenai saldo (pemilik, atau tanggungan dengan plafon terpisah).
            string? balanceKey = usable != null && !string.IsNullOrWhiteSpace(usable.BenefitPersonKind) && usable.BenefitPersonId.HasValue
                ? $"{usable.BenefitPersonKind}:{usable.BenefitPersonId.Value}"
                : null;

            var row = new HrdBenefitSettlementOwnerRowResponse
            {
                ReceivableId = item.ReceivableId,
                ReceivableNumber = receivable.ReceivableNumber,
                InvoiceNumber = inv?.InvoiceNumber,
                MedicalRecordNumber = patient?.MedicalRecordNumber,
                PatientName = patient?.FullName,
                PatientType = enc?.EncounterType.ToString(),
                ServiceDate = enc != null
                    ? (DateTimeOffset?)new DateTimeOffset(DateTime.SpecifyKind(enc.EncounterDate, DateTimeKind.Utc))
                    : null,
                Amount = item.Amount,
                BenefitOwnerWorkforceProfileId = usable?.WorkforceProfileId ?? profileId,
                BenefitOwnerName = ownerName,
                BenefitOwnerStaffNumber = staffNumber,
                BenefitOwnerKind = ownerKind ?? NullIfBlank(usable?.OwnerType),
                BenefitPlanName = NullIfBlank(usable?.PlanCode),
                RelationshipCode = NullIfBlank(usable?.RelationshipCode) ?? NullIfBlank(receivable.BenefitRelationship),
                IsOwnerIdentified = ownerKey.HasValue,
                IsBalanceAvailable = usable != null,
                BalanceNote = usable != null ? null : balanceNote,
                CoverageLimitAmount = usable?.LimitAmount,
                UsedAmount = usable?.UsedAmount,
                RemainingAmount = usable?.RemainingAmount,
                ProposedAmount = item.Amount,
                OwnerTotalProposedAmount = item.Amount
            };
            built.Add((row, balanceKey));
        }

        // Proyeksi sisa dihitung per SUMBER SALDO dari seluruh baris dokumen ini, bukan per baris terpisah.
        var totalsByBalance = built
            .Where(b => b.BalanceKey != null)
            .GroupBy(b => b.BalanceKey!)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.Row.Amount));

        // Finance memotong plafon saat menerbitkan (dan mengembalikannya saat membatalkan dokumen terbit). Pada dokumen
        // DITERBITKAN, saldo saat ini sudah memuat pemotongan dokumen ini, sehingga proyeksi tidak mengurangi lagi.
        var balanceAlreadyDeducted = settlement.Status == FinBenefitSettlementStatuses.Diterbitkan;

        foreach (var (row, balanceKey) in built)
        {
            if (balanceKey != null && row.RemainingAmount.HasValue)
            {
                var ownerTotal = totalsByBalance[balanceKey];
                row.OwnerTotalProposedAmount = ownerTotal;
                row.ProjectedRemainingAmount = balanceAlreadyDeducted
                    ? row.RemainingAmount.Value
                    : row.RemainingAmount.Value - ownerTotal;
                row.IsSufficient = row.ProjectedRemainingAmount >= 0m;
            }
        }

        var rows = built.Select(b => b.Row).ToList();

        var integrityMessages = new List<string>();
        if (activeItems.Count == 0)
        {
            integrityMessages.Add("Dokumen tidak memiliki item kartu piutang.");
        }
        if (missingReceivables > 0)
        {
            integrityMessages.Add($"{missingReceivables} dari {activeItems.Count} item merujuk kartu piutang yang tidak ditemukan atau sudah dihapus.");
        }
        if (activeItems.Count != settlement.ItemCount)
        {
            integrityMessages.Add($"Jumlah item tersimpan pada dokumen ({settlement.ItemCount}) berbeda dari jumlah item aktif ({activeItems.Count}).");
        }
        var itemsTotal = activeItems.Sum(x => x.Amount);
        if (itemsTotal != settlement.TotalAmount)
        {
            integrityMessages.Add($"Total dokumen ({settlement.TotalAmount:N0}) berbeda dari jumlah nominal item ({itemsTotal:N0}).");
        }
        if (unidentifiedOwners > 0)
        {
            integrityMessages.Add($"{unidentifiedOwners} kartu piutang belum teridentifikasi pemilik benefitnya.");
        }

        return new HrdBenefitSettlementApprovalDetailResponse
        {
            Id = settlement.Id,
            SettlementNumber = settlement.SettlementNumber,
            CreatedDate = settlement.CreateDateTime,
            AccountingPeriodCode = settlement.AccountingPeriodCode,
            DebtorReferenceId = settlement.DebtorReferenceId,
            DebtorName = debtorName,
            TotalAmount = settlement.TotalAmount,
            ItemCount = settlement.ItemCount,
            OwnerCount = ownerKeys.Count,
            DocumentStatus = settlement.Status,
            ApprovalStatus = approvalMeta.Status,
            CreatedByName = userNames.GetValueOrDefault(settlement.CreateBy),
            ApprovedByName = approvalMeta.ApproverUserId.HasValue ? userNames.GetValueOrDefault(approvalMeta.ApproverUserId.Value) : null,
            ApprovedAt = approvalMeta.ActionDate,
            RejectionReason = approvalMeta.RejectionReason,
            RowVersion = settlement.RowVersion,
            Notes = CleanNotes(settlement.Notes),
            ApprovalNotes = approvalMeta.Notes,
            PostedAt = settlement.PostedAt,
            PostedByName = settlement.PostedBy.HasValue ? userNames.GetValueOrDefault(settlement.PostedBy.Value) : null,
            CancelReason = settlement.Status == FinBenefitSettlementStatuses.Dibatalkan ? settlement.CancelReason : null,
            MappedItemCount = rows.Count,
            HasIntegrityIssue = integrityMessages.Count > 0,
            IntegrityMessages = integrityMessages,
            Items = rows
        };
    }

    public async Task<HrdBenefitSettlementApprovalDetailResponse> ApproveAsync(
        Guid id,
        HrdApproveSettlementRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        EnsureActor(actorUserId);
        if (request.RowVersion == Guid.Empty)
        {
            throw new ArgumentException("RowVersion wajib diisi. Muat ulang rincian permohonan sebelum menyetujui.");
        }

        var settlement = await _dbContext.FinBenefitSettlements
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Dokumen penutupan benefit tidak ditemukan.");

        if (settlement.Status != FinBenefitSettlementStatuses.Draf)
        {
            throw new InvalidOperationException($"Dokumen berstatus '{settlement.Status}' tidak dapat disetujui. Hanya dokumen berstatus DRAF yang dapat disetujui HRD.");
        }

        if (settlement.RowVersion != request.RowVersion)
        {
            throw new InvalidOperationException("Data permohonan telah diperbarui oleh pengguna lain. Silakan muat ulang halaman sebelum menyetujui.");
        }

        var approvalMeta = ParseApprovalMetadata(settlement.Notes, settlement.CancelReason, settlement.Status);
        if (approvalMeta.Status == StatusApproved)
        {
            throw new InvalidOperationException("Dokumen penutupan benefit ini sudah disetujui sebelumnya.");
        }
        if (approvalMeta.Status == StatusRejected)
        {
            throw new InvalidOperationException("Dokumen penutupan benefit ini telah ditolak dan tidak dapat langsung disetujui.");
        }

        // HRD tidak dapat memvalidasi dokumen yang rinciannya tidak lengkap/tidak konsisten.
        var preview = await GetDetailAsync(id, cancellationToken);
        if (preview.HasIntegrityIssue)
        {
            throw new InvalidOperationException(
                "Dokumen tidak dapat disetujui karena rinciannya belum lengkap: " + string.Join(" ", preview.IntegrityMessages));
        }

        // Kecukupan plafon divalidasi di server, bukan hanya ditampilkan. Saldo yang tidak terbaca (null) tidak
        // memblokir persetujuan; Finance tetap memvalidasi ulang saat menerbitkan dan memotong saldo.
        var insufficientRows = preview.Items.Where(x => x.IsSufficient == false).ToList();
        if (insufficientRows.Count > 0)
        {
            var owners = string.Join(", ", insufficientRows
                .Select(x => x.BenefitOwnerName ?? "pemilik tidak dikenal").Distinct().Take(3));
            throw new InvalidOperationException(
                $"Dokumen tidak dapat disetujui karena pengajuan melebihi sisa plafon ({insufficientRows.Count} rincian, antara lain: {owners}). Tolak dokumen atau minta Finance menyesuaikan pengajuan.");
        }

        var now = DateTimeOffset.UtcNow;
        var approvalTag = BuildDecisionTag(approved: true, actorUserId, now, request.Notes);
        if (approvalTag.Length > MaxNotesLength)
        {
            var room = MaxNotesLength - BuildDecisionTag(approved: true, actorUserId, now, null).Length;
            throw new ArgumentException(
                $"Catatan persetujuan terlalu panjang. Gunakan maksimal sekitar {room} karakter (karakter khusus dihitung lebih panjang).");
        }

        settlement.Notes = ComposeNotes(CleanNotes(settlement.Notes), approvalTag);
        settlement.UpdateBy = actorUserId;
        settlement.UpdateDateTime = DateTime.UtcNow;
        settlement.RowVersion = Guid.NewGuid();

        await SaveWithConcurrencyGuardAsync(cancellationToken);
        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<HrdBenefitSettlementApprovalDetailResponse> RejectAsync(
        Guid id,
        HrdRejectSettlementRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        EnsureActor(actorUserId);
        if (string.IsNullOrWhiteSpace(request.RejectReason))
        {
            throw new ArgumentException("Alasan penolakan persetujuan wajib diisi.");
        }
        if (request.RowVersion == Guid.Empty)
        {
            throw new ArgumentException("RowVersion wajib diisi. Muat ulang rincian permohonan sebelum menolak.");
        }

        var settlement = await _dbContext.FinBenefitSettlements
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Dokumen penutupan benefit tidak ditemukan.");

        if (settlement.Status != FinBenefitSettlementStatuses.Draf)
        {
            throw new InvalidOperationException($"Dokumen berstatus '{settlement.Status}' tidak dapat ditolak. Hanya dokumen berstatus DRAF yang dapat ditolak HRD.");
        }

        if (settlement.RowVersion != request.RowVersion)
        {
            throw new InvalidOperationException("Data permohonan telah diperbarui oleh pengguna lain. Silakan muat ulang halaman sebelum menolak.");
        }

        var approvalMeta = ParseApprovalMetadata(settlement.Notes, settlement.CancelReason, settlement.Status);
        if (approvalMeta.Status == StatusApproved)
        {
            throw new InvalidOperationException("Dokumen penutupan benefit ini sudah disetujui dan tidak dapat ditolak.");
        }
        if (approvalMeta.Status == StatusRejected)
        {
            throw new InvalidOperationException("Dokumen penutupan benefit ini sudah ditolak sebelumnya.");
        }

        // Alasan penuh disimpan di CancelReason (500 karakter); tag hanya memuat pelaku dan waktu sehingga tidak terpotong.
        var rejectTag = BuildDecisionTag(approved: false, actorUserId, DateTimeOffset.UtcNow, null);
        settlement.Notes = ComposeNotes(CleanNotes(settlement.Notes), rejectTag);

        var reason = request.RejectReason.Trim();
        settlement.CancelReason = reason.Length > MaxNotesLength ? reason[..MaxNotesLength] : reason;

        settlement.UpdateBy = actorUserId;
        settlement.UpdateDateTime = DateTime.UtcNow;
        settlement.RowVersion = Guid.NewGuid();

        await SaveWithConcurrencyGuardAsync(cancellationToken);
        return await GetDetailAsync(id, cancellationToken);
    }

    /// <summary>
    /// Membaca keputusan HRD dari catatan dokumen. Keputusan dibaca TERLEPAS dari status dokumen Finance,
    /// sehingga riwayat tetap ada setelah dokumen diterbitkan/dibatalkan. Dokumen DITERBITKAN tanpa bukti
    /// keputusan dilaporkan sebagai <see cref="StatusNotVerifiable"/>; approver tidak pernah disimpulkan.
    /// Parameter fallbackUserId/fallbackDate dipertahankan hanya demi kompatibilitas pemanggil dan tidak dipakai.
    /// </summary>
    public static HrdApprovalMetadata ParseApprovalMetadata(
        string? notes, string? cancelReason, string documentStatus, Guid? fallbackUserId = null, DateTime? fallbackDate = null)
    {
        var match = FindDecisionMatch(notes);
        if (match != null)
        {
            var approved = match.Groups["kind"].Value == "APPROVED";
            Guid? actor = Guid.TryParse(match.Groups["actor"].Value, out var parsedActor) ? parsedActor : null;
            var actionTime = ParseTagTimestamp(match.Groups["ts"].Value);
            var text = match.Groups["text"].Success ? NullIfBlank(UnescapeTagText(match.Groups["text"].Value)) : null;

            if (approved)
            {
                return new HrdApprovalMetadata(StatusApproved, actor, actionTime, text, null);
            }

            var reason = documentStatus == FinBenefitSettlementStatuses.Dibatalkan
                ? text
                : NullIfBlank(cancelReason) ?? text;
            return new HrdApprovalMetadata(StatusRejected, actor, actionTime, null, reason);
        }

        if (documentStatus == FinBenefitSettlementStatuses.Dibatalkan)
        {
            return new HrdApprovalMetadata(StatusNotDecided, null, null, null, null);
        }

        if (documentStatus == FinBenefitSettlementStatuses.Diterbitkan)
        {
            return new HrdApprovalMetadata(StatusNotVerifiable, null, null, null, null);
        }

        return new HrdApprovalMetadata(StatusPending, null, null, null, null);
    }

    /// <summary>
    /// Dipakai saat Finance mengganti catatan dokumen (mis. saat menerbitkan): tag keputusan HRD yang sudah ada
    /// dipertahankan, catatan Finance dibersihkan dari tag palsu dan dipotong bila perlu agar tag tidak terpotong.
    /// </summary>
    public static string? PreserveApprovalDecision(string? currentNotes, string? newNotes)
    {
        var sanitized = SanitizeUserNotes(newNotes);
        var tag = FindDecisionMatch(currentNotes)?.Value;
        if (tag == null) return sanitized;
        return ComposeNotes(NullIfBlank(sanitized?.Trim()), tag);
    }

    /// <summary>
    /// Catatan bebas dari pengguna tidak boleh memuat tag keputusan HRD; bila tidak, Finance dapat
    /// memalsukan persetujuan HRD lewat isi catatan.
    /// </summary>
    public static string? SanitizeUserNotes(string? notes)
    {
        if (string.IsNullOrEmpty(notes)) return notes;
        var sanitized = DecisionTagRegex.Replace(notes, string.Empty);
        return sanitized
            .Replace(ApprovedTagPrefix, "(HRD_APPROVED:", StringComparison.Ordinal)
            .Replace(RejectedTagPrefix, "(HRD_REJECTED:", StringComparison.Ordinal);
    }

    private static Match? FindDecisionMatch(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return null;

        Match? rejected = null;
        foreach (Match match in DecisionTagRegex.Matches(notes))
        {
            if (match.Groups["kind"].Value == "APPROVED") return match;
            rejected ??= match;
        }
        return rejected;
    }

    private static DateTimeOffset? ParseTagTimestamp(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (DateTimeOffset.TryParseExact(value, CompactTimestampFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var compact))
        {
            return compact;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var legacy)
            ? legacy
            : null;
    }

    private static string BuildDecisionTag(bool approved, Guid actorUserId, DateTimeOffset when, string? text)
    {
        var tag = $"{(approved ? ApprovedTagPrefix : RejectedTagPrefix)}{actorUserId:D}:{when.UtcDateTime.ToString(CompactTimestampFormat, CultureInfo.InvariantCulture)}";
        if (!string.IsNullOrWhiteSpace(text))
        {
            tag += ":" + EscapeTagText(text.Trim());
        }
        return tag + "]";
    }

    private static string EscapeTagText(string text) => text
        .Replace("%", "%25", StringComparison.Ordinal)
        .Replace("[", "%5B", StringComparison.Ordinal)
        .Replace("]", "%5D", StringComparison.Ordinal)
        .Replace("|", "%7C", StringComparison.Ordinal)
        .Replace("\r", " ", StringComparison.Ordinal)
        .Replace("\n", " ", StringComparison.Ordinal);

    private static string UnescapeTagText(string text) => text
        .Replace("%5B", "[", StringComparison.Ordinal)
        .Replace("%5D", "]", StringComparison.Ordinal)
        .Replace("%7C", "|", StringComparison.Ordinal)
        .Replace("%25", "%", StringComparison.Ordinal);

    /// <summary>Tag selalu utuh; bagian catatan lain yang dipotong bila total melebihi batas kolom Notes.</summary>
    private static string ComposeNotes(string? cleanExisting, string tag)
    {
        if (string.IsNullOrWhiteSpace(cleanExisting)) return tag;

        var room = MaxNotesLength - tag.Length - NotesSeparator.Length;
        if (room <= 0) return tag;

        var existing = cleanExisting.Length <= room ? cleanExisting : cleanExisting[..(room - 1)] + "…";
        return existing + NotesSeparator + tag;
    }

    private static string? CleanNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return null;

        var clean = DecisionTagRegex.Replace(notes, string.Empty);
        clean = RepeatedSeparatorRegex.Replace(clean, NotesSeparator).Trim().Trim('|').Trim();
        return string.IsNullOrWhiteSpace(clean) ? null : clean;
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static void EnsureActor(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Identitas pengguna yang melakukan tindakan tidak dapat ditentukan.");
        }
    }

    private async Task SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Data permohonan telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
        }
    }

    private sealed record OwnerTally(int OwnerCount, int UnidentifiedCount);

    /// <summary>Penjamin benefit pada satu kunjungan; dipakai bersama oleh daftar dan detail agar identitas pemilik sama.</summary>
    private static RegPatientEncounterGuarantor? PickBenefitGuarantor(IEnumerable<RegPatientEncounterGuarantor> guarantors) =>
        guarantors
            .OrderByDescending(g => g.IsPrimary)
            .ThenBy(g => g.Priority)
            .FirstOrDefault(g =>
                (g.CompanyGuarantor != null && g.CompanyGuarantor.IsEmployeeBenefitGuarantor)
                || g.EmployeeIdSnapshot.HasValue
                || g.WorkforceProfileIdSnapshot.HasValue);

    /// <summary>
    /// Menghitung pemilik benefit per dokumen untuk daftar. Kunci pemilik = profil tenaga kerja canonical, atau id karyawan
    /// lama bila belum dapat dipetakan; kartu piutang tanpa identitas dihitung terpisah sebagai belum teridentifikasi
    /// (tidak diubah menjadi 1 pemilik). Detail menambahkan satu langkah: profil yang dikembalikan layanan saldo.
    /// </summary>
    private async Task<Dictionary<Guid, OwnerTally>> CountOwnersAsync(
        List<Guid> settlementIds,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, OwnerTally>();
        if (settlementIds.Count == 0) return result;

        var itemRows = await (
            from item in _dbContext.FinBenefitSettlementItems.AsNoTracking()
            join r in _dbContext.FinReceivables.AsNoTracking() on item.ReceivableId equals r.Id
            where !item.IsDelete && !r.IsDelete && settlementIds.Contains(item.SettlementId)
            select new
            {
                item.SettlementId,
                ReceivableId = r.Id,
                r.InvoiceId,
                r.BenefitOwnerWorkforceProfileId,
                r.BenefitOwnerId
            }
        ).ToListAsync(cancellationToken);
        if (itemRows.Count == 0) return result;

        var receivableIds = itemRows.Select(x => x.ReceivableId).Distinct().ToList();
        var firstItemByReceivable = (await _dbContext.FinReceivableItems.AsNoTracking()
                .Where(x => !x.IsDelete && receivableIds.Contains(x.ReceivableId))
                .Select(x => new { x.ReceivableId, x.InvoiceId, x.EncounterId })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.ReceivableId)
            .ToDictionary(g => g.Key, g => g.First());

        var invoiceIds = itemRows.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value)
            .Concat(firstItemByReceivable.Values.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value))
            .Distinct().ToList();
        var invoiceEncounters = invoiceIds.Count > 0
            ? await _dbContext.BilInvoices.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, x => x.EncounterId, cancellationToken)
            : new Dictionary<Guid, Guid>();

        var encounterCandidates = invoiceEncounters.Values
            .Concat(firstItemByReceivable.Values.Where(x => x.EncounterId.HasValue).Select(x => x.EncounterId!.Value))
            .Where(x => x != Guid.Empty).Distinct().ToList();
        var existingEncounters = encounterCandidates.Count > 0
            ? (await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(x => encounterCandidates.Contains(x.Id) && !x.IsDelete)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken)).ToHashSet()
            : new HashSet<Guid>();

        var guarantorByEncounter = new Dictionary<Guid, RegPatientEncounterGuarantor>();
        if (existingEncounters.Count > 0)
        {
            var existingEncounterIds = existingEncounters.ToList();
            var guarantorRows = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                .Include(g => g.CompanyGuarantor)
                .Where(g => existingEncounterIds.Contains(g.EncounterId) && g.IsActive && !g.IsDelete && !g.IsCancel)
                .ToListAsync(cancellationToken);
            foreach (var group in guarantorRows.GroupBy(g => g.EncounterId))
            {
                var chosen = PickBenefitGuarantor(group);
                if (chosen != null) guarantorByEncounter[group.Key] = chosen;
            }
        }

        var legacyEmployeeIds = itemRows.Where(x => x.BenefitOwnerId.HasValue).Select(x => x.BenefitOwnerId!.Value)
            .Concat(guarantorByEncounter.Values.Where(g => g.EmployeeIdSnapshot.HasValue).Select(g => g.EmployeeIdSnapshot!.Value))
            .Distinct().ToList();
        var legacyEmployeeProfiles = legacyEmployeeIds.Count > 0
            ? await _dbContext.MstEmployees.AsNoTracking()
                .Where(e => legacyEmployeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.WorkforceProfileId, cancellationToken)
            : new Dictionary<Guid, Guid>();

        foreach (var group in itemRows.GroupBy(x => x.SettlementId))
        {
            var ownerKeys = new HashSet<Guid>();
            var unidentified = 0;

            foreach (var row in group)
            {
                firstItemByReceivable.TryGetValue(row.ReceivableId, out var firstItem);

                var invoiceId = row.InvoiceId ?? firstItem?.InvoiceId;
                Guid? encounterId = null;
                if (invoiceId.HasValue
                    && invoiceEncounters.TryGetValue(invoiceId.Value, out var invoiceEncounterId)
                    && existingEncounters.Contains(invoiceEncounterId))
                {
                    encounterId = invoiceEncounterId;
                }
                else if (firstItem?.EncounterId is { } itemEncounterId && existingEncounters.Contains(itemEncounterId))
                {
                    encounterId = itemEncounterId;
                }

                RegPatientEncounterGuarantor? guarantor = null;
                if (encounterId.HasValue) guarantorByEncounter.TryGetValue(encounterId.Value, out guarantor);

                var profileId = guarantor?.WorkforceProfileIdSnapshot ?? row.BenefitOwnerWorkforceProfileId;
                var legacyEmployeeId = guarantor?.EmployeeIdSnapshot ?? row.BenefitOwnerId;
                if (!profileId.HasValue && legacyEmployeeId.HasValue
                    && legacyEmployeeProfiles.TryGetValue(legacyEmployeeId.Value, out var mappedProfileId))
                {
                    profileId = mappedProfileId;
                }

                var ownerKey = profileId ?? legacyEmployeeId;
                if (ownerKey.HasValue) ownerKeys.Add(ownerKey.Value);
                else unidentified++;
            }

            result[group.Key] = new OwnerTally(ownerKeys.Count, unidentified);
        }

        return result;
    }

    private async Task<Dictionary<Guid, string>> ResolveUserNamesAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string>();

        return await _dbContext.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? "Staf", cancellationToken);
    }

    private static IQueryable<FinBenefitSettlement> ApplyStatusFilter(IQueryable<FinBenefitSettlement> source, string status)
    {
        switch (status)
        {
            case StatusApproved:
                return source.Where(x => x.Notes != null && x.Notes.Contains(ApprovedTagPrefix));
            case StatusRejected:
                return source.Where(x => x.Notes != null && x.Notes.Contains(RejectedTagPrefix) && !x.Notes.Contains(ApprovedTagPrefix));
            case StatusPending:
                return source.Where(x => x.Status == FinBenefitSettlementStatuses.Draf
                    && (x.Notes == null || (!x.Notes.Contains(ApprovedTagPrefix) && !x.Notes.Contains(RejectedTagPrefix))));
            case StatusNotVerifiable:
                return source.Where(x => x.Status == FinBenefitSettlementStatuses.Diterbitkan
                    && (x.Notes == null || (!x.Notes.Contains(ApprovedTagPrefix) && !x.Notes.Contains(RejectedTagPrefix))));
            case StatusNotDecided:
                return source.Where(x => x.Status == FinBenefitSettlementStatuses.Dibatalkan
                    && (x.Notes == null || (!x.Notes.Contains(ApprovedTagPrefix) && !x.Notes.Contains(RejectedTagPrefix))));
            case StatusPosted:
                return source.Where(x => x.Status == FinBenefitSettlementStatuses.Diterbitkan);
            case StatusCancelled:
                return source.Where(x => x.Status == FinBenefitSettlementStatuses.Dibatalkan);
            default:
                return source.Where(x => false);
        }
    }

    private static IOrderedQueryable<FinBenefitSettlement> ApplySort(
        IQueryable<FinBenefitSettlement> source, string? sortBy, bool descending)
    {
        var ordered = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "settlementnumber" => descending
                ? source.OrderByDescending(x => x.SettlementNumber)
                : source.OrderBy(x => x.SettlementNumber),
            "totalamount" => descending
                ? source.OrderByDescending(x => x.TotalAmount)
                : source.OrderBy(x => x.TotalAmount),
            _ => descending
                ? source.OrderByDescending(x => x.CreateDateTime)
                : source.OrderBy(x => x.CreateDateTime)
        };
        return ordered.ThenBy(x => x.Id);
    }

    private static string NormalizeStatusFilter(string filter) => filter.Trim().ToLowerInvariant() switch
    {
        "menunggu" or "pending" or "menunggu_persetujuan" => StatusPending,
        "disetujui" or "approved" => StatusApproved,
        "ditolak" or "rejected" => StatusRejected,
        "tidak_dapat_diverifikasi" or "not_verifiable" => StatusNotVerifiable,
        "tanpa_keputusan" or "not_decided" => StatusNotDecided,
        "dibatalkan" or "cancelled" => StatusCancelled,
        "diterbitkan" or "posted" => StatusPosted,
        _ => filter.Trim()
    };
}
