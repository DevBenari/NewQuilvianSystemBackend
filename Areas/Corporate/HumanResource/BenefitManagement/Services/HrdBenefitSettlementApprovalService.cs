using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;

/// <summary>
/// Layanan persetujuan penutupan benefit RS internal oleh Human Resource Department (HRD).
/// HRD bertindak sebagai pihak yang berwenang memvalidasi kepesertaan, kelayakan, dan kecukupan plafon benefit karyawan/dokter
/// sebelum Finance menerbitkan pelunasan internal dan pemotongan saldo.
/// </summary>
public sealed class HrdBenefitSettlementApprovalService
{
    public const string StatusPending = "MENUNGGU_PERSETUJUAN";
    public const string StatusApproved = "DISETUJUI";
    public const string StatusRejected = "DITOLAK";
    public const string StatusCancelled = "DIBATALKAN";
    public const string StatusPosted = "DITERBITKAN";

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
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.SettlementNumber, $"%{search}%"));
        }

        var allItems = await baseQuery
            .OrderByDescending(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        // Petakan status persetujuan HRD
        var mappedList = new List<HrdBenefitSettlementApprovalItemResponse>();
        var userIds = allItems.Select(x => x.CreateBy).Where(id => id.HasValue).Select(id => id!.Value)
            .Concat(allItems.Select(x => x.UpdateBy).Where(id => id.HasValue).Select(id => id!.Value))
            .Distinct()
            .ToList();

        var userNames = userIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? "Staf", cancellationToken)
            : new Dictionary<Guid, string>();

        var debtorIds = allItems.Select(x => x.DebtorReferenceId).Distinct().ToList();
        var debtorNames = debtorIds.Count > 0
            ? await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .Where(c => debtorIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CompanyGuarantorName, cancellationToken)
            : new Dictionary<Guid, string>();

        // Ambil jumlah pemilik benefit per settlement
        var settlementIds = allItems.Select(x => x.Id).ToList();
        var ownerCountsBySettlement = new Dictionary<Guid, int>();
        if (settlementIds.Count > 0)
        {
            var ownerGroups = await (
                from item in _dbContext.FinBenefitSettlementItems.AsNoTracking()
                join r in _dbContext.FinReceivables.AsNoTracking() on item.ReceivableId equals r.Id
                where !item.IsDelete && settlementIds.Contains(item.SettlementId)
                select new
                {
                    item.SettlementId,
                    OwnerId = r.BenefitOwnerWorkforceProfileId ?? r.BenefitOwnerId ?? r.Id
                }
            ).Distinct().GroupBy(x => x.SettlementId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(cancellationToken);

            foreach (var og in ownerGroups)
            {
                ownerCountsBySettlement[og.Key] = og.Count;
            }
        }

        foreach (var item in allItems)
        {
            var approvalMeta = ParseApprovalMetadata(item.Notes, item.CancelReason, item.Status, item.UpdateBy, item.UpdateDateTime);

            // Filter status
            if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "all", StringComparison.OrdinalIgnoreCase))
            {
                var filterStatus = NormalizeStatusFilter(query.Status);
                if (!string.Equals(approvalMeta.Status, filterStatus, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            debtorNames.TryGetValue(item.DebtorReferenceId, out var debtorName);
            var createdByName = item.CreateBy.HasValue && userNames.TryGetValue(item.CreateBy.Value, out var cName) ? cName : "Finance";
            var approvedByName = approvalMeta.ApproverUserId.HasValue && userNames.TryGetValue(approvalMeta.ApproverUserId.Value, out var aName) ? aName : null;

            ownerCountsBySettlement.TryGetValue(item.Id, out var ownerCount);

            mappedList.Add(new HrdBenefitSettlementApprovalItemResponse
            {
                Id = item.Id,
                SettlementNumber = item.SettlementNumber,
                CreatedDate = item.CreateDateTime,
                AccountingPeriodCode = item.AccountingPeriodCode,
                DebtorReferenceId = item.DebtorReferenceId,
                DebtorName = debtorName ?? "RS Benefit Internal",
                TotalAmount = item.TotalAmount,
                ItemCount = item.ItemCount,
                OwnerCount = Math.Max(1, ownerCount),
                DocumentStatus = item.Status,
                ApprovalStatus = approvalMeta.Status,
                CreatedByName = createdByName,
                ApprovedByName = approvedByName,
                ApprovedAt = approvalMeta.ActionDate,
                RejectionReason = approvalMeta.RejectionReason,
                RowVersion = item.RowVersion
            });
        }

        var total = mappedList.Count;
        var pagedItems = mappedList
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PagedResult<HrdBenefitSettlementApprovalItemResponse>
        {
            Items = pagedItems,
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

        var approvalMeta = ParseApprovalMetadata(settlement.Notes, settlement.CancelReason, settlement.Status, settlement.UpdateBy, settlement.UpdateDateTime);

        var debtorName = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == settlement.DebtorReferenceId)
            .Select(x => x.CompanyGuarantorName)
            .FirstOrDefaultAsync(cancellationToken) ?? "RS Benefit Internal";

        var createdByName = settlement.CreateBy.HasValue
            ? await _dbContext.Users.AsNoTracking().Where(u => u.Id == settlement.CreateBy.Value).Select(u => u.UserName ?? u.Email).FirstOrDefaultAsync(cancellationToken)
            : "Finance";

        var approvedByName = approvalMeta.ApproverUserId.HasValue
            ? await _dbContext.Users.AsNoTracking().Where(u => u.Id == approvalMeta.ApproverUserId.Value).Select(u => u.UserName ?? u.Email).FirstOrDefaultAsync(cancellationToken)
            : null;

        var receivableIds = settlement.Items.Select(x => x.ReceivableId).Distinct().ToList();
        var receivables = await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => receivableIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var receivableItems = await _dbContext.FinReceivableItems.AsNoTracking()
            .Where(x => !x.IsDelete && receivableIds.Contains(x.ReceivableId))
            .ToListAsync(cancellationToken);

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

        // Resolusi data pemilik benefit (WorkforceProfile / Employee / Doctor)
        var ownerProfileIds = receivables.Values
            .Where(r => r.BenefitOwnerWorkforceProfileId.HasValue)
            .Select(r => r.BenefitOwnerWorkforceProfileId!.Value)
            .Distinct().ToList();

        var ownerProfiles = ownerProfileIds.Count > 0
            ? await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Include(w => w.Employee)
                .Include(w => w.Doctor)
                .Where(w => ownerProfileIds.Contains(w.Id) && !w.IsDelete)
                .ToDictionaryAsync(w => w.Id, cancellationToken)
            : new Dictionary<Guid, QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile>();

        // Cache pembacaan saldo plafon per orang
        var balanceCache = new Dictionary<Guid, BenefitBalanceReadResult>();

        var rows = new List<HrdBenefitSettlementOwnerRowResponse>();
        var uniqueOwners = new HashSet<string>();

        foreach (var item in settlement.Items)
        {
            receivables.TryGetValue(item.ReceivableId, out var receivable);
            if (receivable == null) continue;

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

            // Resolusi data profil pemilik
            string ownerName = receivable.BenefitOwnerNameSnapshot ?? "-";
            string staffNumber = "-";
            string ownerKind = "Pegawai";
            string planName = "Benefit Medical RS";

            if (receivable.BenefitOwnerWorkforceProfileId.HasValue && ownerProfiles.TryGetValue(receivable.BenefitOwnerWorkforceProfileId.Value, out var profile))
            {
                if (profile.Doctor != null)
                {
                    ownerName = profile.Doctor.FullName;
                    staffNumber = profile.Doctor.DoctorCode ?? profile.Doctor.IdentityNumber ?? "-";
                    ownerKind = "Dokter";
                }
                else if (profile.Employee != null)
                {
                    ownerName = profile.Employee.FullName;
                    staffNumber = profile.Employee.IdentityNumber ?? "-";
                    ownerKind = "Karyawan";
                }
                else
                {
                    ownerName = profile.DisplayName;
                }
            }

            uniqueOwners.Add(ownerName);

            // Baca plafon saat ini via balance service
            decimal limit = 0m;
            decimal used = 0m;
            decimal remaining = 0m;

            var cacheKey = receivable.BenefitOwnerWorkforceProfileId ?? receivable.BenefitOwnerId ?? receivable.Id;
            if (balanceCache.TryGetValue(cacheKey, out var cachedBalance))
            {
                limit = cachedBalance.LimitAmount;
                used = cachedBalance.UsedAmount;
                remaining = cachedBalance.RemainingAmount;
                if (!string.IsNullOrWhiteSpace(cachedBalance.PlanCode)) planName = cachedBalance.PlanCode;
            }
            else
            {
                var asOfDate = (inv?.InvoiceDate ?? receivable.RecognizedAt).UtcDateTime;
                var balanceResult = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
                {
                    WorkforceProfileId = receivable.BenefitOwnerWorkforceProfileId,
                    EmployeeId = receivable.BenefitOwnerId ?? Guid.Empty,
                    AsOfDate = asOfDate
                }, cancellationToken);

                if (balanceResult.IsUsable)
                {
                    limit = balanceResult.LimitAmount;
                    used = balanceResult.UsedAmount;
                    remaining = balanceResult.RemainingAmount;
                    if (!string.IsNullOrWhiteSpace(balanceResult.PlanCode)) planName = balanceResult.PlanCode;
                    balanceCache[cacheKey] = balanceResult;
                }
            }

            var projected = remaining - item.Amount;
            var isSufficient = projected >= 0;

            rows.Add(new HrdBenefitSettlementOwnerRowResponse
            {
                ReceivableId = item.ReceivableId,
                ReceivableNumber = receivable.ReceivableNumber,
                InvoiceNumber = inv?.InvoiceNumber ?? "-",
                MedicalRecordNumber = patient?.MedicalRecordNumber ?? "-",
                PatientName = patient?.FullName ?? "-",
                PatientType = enc?.EncounterType.ToString() ?? "Outpatient",
                ServiceDate = enc?.EncounterDate.HasValue == true
                    ? new DateTimeOffset(DateTime.SpecifyKind(enc.EncounterDate.Value, DateTimeKind.Utc))
                    : receivable.RecognizedAt,
                Amount = item.Amount,
                BenefitOwnerWorkforceProfileId = receivable.BenefitOwnerWorkforceProfileId,
                BenefitOwnerName = ownerName,
                BenefitOwnerStaffNumber = staffNumber,
                BenefitOwnerKind = ownerKind,
                BenefitPlanName = planName,
                RelationshipCode = receivable.BenefitRelationship ?? "SELF",
                CoverageLimitAmount = limit,
                UsedAmount = used,
                RemainingAmount = remaining,
                ProposedAmount = item.Amount,
                ProjectedRemainingAmount = projected,
                IsSufficient = isSufficient
            });
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
            OwnerCount = Math.Max(1, uniqueOwners.Count),
            DocumentStatus = settlement.Status,
            ApprovalStatus = approvalMeta.Status,
            CreatedByName = createdByName,
            ApprovedByName = approvedByName,
            ApprovedAt = approvalMeta.ActionDate,
            RejectionReason = approvalMeta.RejectionReason,
            RowVersion = settlement.RowVersion,
            Notes = CleanNotes(settlement.Notes),
            ApprovalNotes = approvalMeta.Notes,
            Items = rows
        };
    }

    public async Task<HrdBenefitSettlementApprovalDetailResponse> ApproveAsync(
        Guid id,
        HrdApproveSettlementRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
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

        var approvalMeta = ParseApprovalMetadata(settlement.Notes, settlement.CancelReason, settlement.Status, settlement.UpdateBy, settlement.UpdateDateTime);
        if (approvalMeta.Status == StatusApproved)
        {
            throw new InvalidOperationException("Dokumen penutupan benefit ini sudah disetujui sebelumnya.");
        }
        if (approvalMeta.Status == StatusRejected)
        {
            throw new InvalidOperationException("Dokumen penutupan benefit ini telah ditolak dan tidak dapat langsung disetujui.");
        }

        // Simpan tanda approval HRD di kolom Notes secara presisten
        var cleanExistingNotes = CleanNotes(settlement.Notes);
        var approvalTag = $"[HRD_APPROVED:{actorUserId:D}:{DateTimeOffset.UtcNow:O}:{request.Notes?.Trim()}]";
        settlement.Notes = string.IsNullOrWhiteSpace(cleanExistingNotes)
            ? approvalTag
            : $"{cleanExistingNotes} | {approvalTag}";

        if (settlement.Notes.Length > 500)
        {
            settlement.Notes = settlement.Notes[..500];
        }

        settlement.UpdateBy = actorUserId;
        settlement.UpdateDateTime = DateTime.UtcNow;
        settlement.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<HrdBenefitSettlementApprovalDetailResponse> RejectAsync(
        Guid id,
        HrdRejectSettlementRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RejectReason))
        {
            throw new ArgumentException("Alasan penolakan persetujuan wajib diisi.", nameof(request));
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

        var approvalMeta = ParseApprovalMetadata(settlement.Notes, settlement.CancelReason, settlement.Status, settlement.UpdateBy, settlement.UpdateDateTime);
        if (approvalMeta.Status == StatusApproved)
        {
            throw new InvalidOperationException("Dokumen penutupan benefit ini sudah disetujui dan tidak dapat ditolak.");
        }

        var cleanExistingNotes = CleanNotes(settlement.Notes);
        var rejectTag = $"[HRD_REJECTED:{actorUserId:D}:{DateTimeOffset.UtcNow:O}:{request.RejectReason.Trim()}]";
        settlement.Notes = string.IsNullOrWhiteSpace(cleanExistingNotes)
            ? rejectTag
            : $"{cleanExistingNotes} | {rejectTag}";

        if (settlement.Notes.Length > 500)
        {
            settlement.Notes = settlement.Notes[..500];
        }

        settlement.CancelReason = request.RejectReason.Trim();
        if (settlement.CancelReason.Length > 500)
        {
            settlement.CancelReason = settlement.CancelReason[..500];
        }

        settlement.UpdateBy = actorUserId;
        settlement.UpdateDateTime = DateTime.UtcNow;
        settlement.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetDetailAsync(id, cancellationToken);
    }

    // Helper parser tanda status persetujuan
    public static (string Status, Guid? ApproverUserId, DateTimeOffset? ActionDate, string? Notes, string? RejectionReason)
        ParseApprovalMetadata(string? notes, string? cancelReason, string documentStatus, Guid? fallbackUserId, DateTime? fallbackDate)
    {
        if (documentStatus == FinBenefitSettlementStatuses.Dibatalkan)
        {
            return (StatusCancelled, fallbackUserId, fallbackDate.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(fallbackDate.Value, DateTimeKind.Utc)) : null, null, cancelReason);
        }

        if (documentStatus == FinBenefitSettlementStatuses.Diterbitkan)
        {
            return (StatusPosted, fallbackUserId, fallbackDate.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(fallbackDate.Value, DateTimeKind.Utc)) : null, null, null);
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            if (notes.Contains("[HRD_APPROVED:"))
            {
                var tagStart = notes.IndexOf("[HRD_APPROVED:", StringComparison.Ordinal);
                var tagEnd = notes.IndexOf(']', tagStart);
                if (tagEnd > tagStart)
                {
                    var content = notes.Substring(tagStart + "[HRD_APPROVED:".Length, tagEnd - tagStart - "[HRD_APPROVED:".Length);
                    var parts = content.Split(':');
                    Guid? approverId = parts.Length > 0 && Guid.TryParse(parts[0], out var g) ? g : fallbackUserId;
                    DateTimeOffset? actionTime = parts.Length > 1 && DateTimeOffset.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) ? dt : null;
                    string? appNotes = parts.Length > 2 ? parts[2] : null;
                    return (StatusApproved, approverId, actionTime, appNotes, null);
                }
                return (StatusApproved, fallbackUserId, null, null, null);
            }

            if (notes.Contains("[HRD_REJECTED:"))
            {
                var tagStart = notes.IndexOf("[HRD_REJECTED:", StringComparison.Ordinal);
                var tagEnd = notes.IndexOf(']', tagStart);
                if (tagEnd > tagStart)
                {
                    var content = notes.Substring(tagStart + "[HRD_REJECTED:".Length, tagEnd - tagStart - "[HRD_REJECTED:".Length);
                    var parts = content.Split(':');
                    Guid? rejectorId = parts.Length > 0 && Guid.TryParse(parts[0], out var g) ? g : fallbackUserId;
                    DateTimeOffset? actionTime = parts.Length > 1 && DateTimeOffset.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) ? dt : null;
                    string? rejReason = parts.Length > 2 ? parts[2] : cancelReason;
                    return (StatusRejected, rejectorId, actionTime, null, rejReason ?? cancelReason);
                }
                return (StatusRejected, fallbackUserId, null, null, cancelReason);
            }
        }

        return (StatusPending, null, null, null, null);
    }

    private static string? CleanNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return null;
        var clean = notes;
        var appIdx = clean.IndexOf("[HRD_APPROVED:", StringComparison.Ordinal);
        if (appIdx >= 0)
        {
            var endIdx = clean.IndexOf(']', appIdx);
            if (endIdx >= 0) clean = clean.Remove(appIdx, endIdx - appIdx + 1);
        }
        var rejIdx = clean.IndexOf("[HRD_REJECTED:", StringComparison.Ordinal);
        if (rejIdx >= 0)
        {
            var endIdx = clean.IndexOf(']', rejIdx);
            if (endIdx >= 0) clean = clean.Remove(rejIdx, endIdx - rejIdx + 1);
        }
        clean = clean.Replace("|", "").Trim();
        return string.IsNullOrWhiteSpace(clean) ? null : clean;
    }

    private static string NormalizeStatusFilter(string filter) => filter.Trim().ToLowerInvariant() switch
    {
        "menunggu" or "pending" or "menunggu_persetujuan" => StatusPending,
        "disetujui" or "approved" => StatusApproved,
        "ditolak" or "rejected" => StatusRejected,
        "dibatalkan" or "cancelled" => StatusCancelled,
        "diterbitkan" or "posted" => StatusPosted,
        _ => filter.Trim()
    };
}
