using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;

public partial class PatientBillingSummaryService
{
    private static readonly (string Code, string Label)[] GroupOrder =
    [
        ("ROOM", "Kamar Rawat Inap"), ("PROCEDURE", "Tindakan"), ("SUPPORT", "Penunjang Medis"),
        ("PHARMACY", "Obat & Alkes"), ("EQUIPMENT", "Pemakaian Alat"), ("SURGERY", "Operasi"),
        ("ADMINISTRATION", "Biaya Administrasi")
    ];

    public async Task<PatientBillingAmountResponse?> GetAmountsAsync(Guid episodeId, CancellationToken ct)
    {
        var result = await BuildBreakdownAsync(episodeId, ct);
        if (result == null) return null;
        var deposit = await _depositService.GetEpisodeDepositSummaryAsync(episodeId, ct);
        return new PatientBillingAmountResponse
        {
            EpisodeId = episodeId, EncounterId = result.View.EncounterId, InvoiceState = result.View.InvoiceState,
            CalculatedAt = result.View.CalculatedAt, RunningTotalAmount = result.Total,
            DepositReceivedAmount = deposit.TotalReceived, DepositRemainingAmount = deposit.AvailableBalance,
            DepositShortfallAmount = deposit.PolicyShortfallAmount
        };
    }

    public async Task<PatientBillingBreakdownResponse?> GetBreakdownAsync(Guid episodeId, CancellationToken ct) =>
        (await BuildBreakdownAsync(episodeId, ct))?.View;

    public async Task<PatientBillingBreakdownAmountResponse?> GetBreakdownAmountsAsync(Guid episodeId, CancellationToken ct)
    {
        var result = await BuildBreakdownAsync(episodeId, ct);
        if (result == null) return null;
        return new PatientBillingBreakdownAmountResponse
        {
            EpisodeId = episodeId, InvoiceState = result.View.InvoiceState, CalculatedAt = result.View.CalculatedAt,
            RunningTotalAmount = result.Total,
            Groups = result.View.Groups.Select(x => new PatientBillingGroupAmountResponse
            {
                GroupCode = x.GroupCode, SubtotalAmount = result.Subtotals.GetValueOrDefault(x.GroupCode),
                IncludesLinkedEncounter = x.Lines.Any(l => l.LinkedEncounter != null)
            }).ToList()
        };
    }

    private sealed record BreakdownResult(PatientBillingBreakdownResponse View, CalculationResponse? Calculation,
        Dictionary<string, decimal> Subtotals, decimal? Total);

    private async Task<BreakdownResult?> BuildBreakdownAsync(Guid episodeId, CancellationToken ct)
    {
        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .Where(x => x.Id == episodeId && !x.IsDelete).Select(x => new { x.EncounterId }).SingleOrDefaultAsync(ct);
        if (episode == null) return null;
        var view = new PatientBillingBreakdownResponse { EpisodeId = episodeId, EncounterId = episode.EncounterId };
        var totals = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.EncounterId == episode.EncounterId && !x.IsDelete, ct);
        if (invoice == null) return new(view, null, totals, null);
        view.InvoiceState = invoice.Status;
        var (calculation, _) = await ResolveCalculationAsync(invoice, ct);
        view.CalculatedAt = calculation?.CalculatedAt;
        var groups = GroupOrder.ToDictionary(x => x.Code, x => new PatientBillingGroupResponse { GroupCode = x.Code, Label = x.Label });
        void Add(string group, PatientBillingLineResponse line, decimal amount)
        {
            groups[group].Lines.Add(line);
            totals[group] = totals.GetValueOrDefault(group) + amount;
        }

        if (calculation != null)
        {
            var roomIds = calculation.Breakdown.RoomCharge.Segments.Select(x => x.RoomId).Distinct().ToList();
            var rooms = await _dbContext.Set<MstRoom>().AsNoTracking().Where(x => roomIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.RoomName, ct);
            foreach (var segment in calculation.Breakdown.RoomCharge.Segments)
                Add("ROOM", new PatientBillingLineResponse
                {
                    Label = rooms.GetValueOrDefault(segment.RoomId) ?? "Kamar Rawat Inap",
                    PeriodLabel = $"{segment.StartDateTime:dd/MM/yyyy HH:mm} – {(segment.EndDateTime?.ToString("dd/MM/yyyy HH:mm") ?? "berjalan")}",
                    Quantity = segment.ChargeUnits, UnitLabel = "unit tarif kamar",
                    LineStatus = segment.MissingTariff ? "TARIFF_NOT_FOUND" : "ACTIVE"
                }, segment.SegmentAmount);
            if (calculation.Breakdown.AdministrationFee.PolicyId.HasValue || calculation.AdministrationFeeAmount > 0)
                Add("ADMINISTRATION", new PatientBillingLineResponse
                {
                    Label = "Biaya Administrasi", Quantity = 1, ServiceDate = calculation.CalculatedAt
                }, calculation.AdministrationFeeAmount);
        }

        var linked = await _dbContext.BilInvoiceEncounterLinks.AsNoTracking()
            .Where(x => x.RanapInvoiceId == invoice.Id && !x.IsDelete)
            .Select(x => new LinkedBillingEncounterResponse
            {
                EncounterId = x.LinkedEncounterId,
                EncounterTypeName = x.LinkedEncounter.EncounterType == EncounterType.Outpatient ? "Poliklinik"
                    : x.LinkedEncounter.EncounterType == EncounterType.Emergency ? "IGD"
                    : x.LinkedEncounter.EncounterType.ToString(),
                ServiceUnitName = x.LinkedEncounter.ServiceUnit!.ServiceUnitName,
                VisitDate = x.LinkedEncounter.EncounterDate
            }).ToListAsync(ct);
        view.LinkedEncounters = linked;
        var encounterIds = linked.Select(x => x.EncounterId).Append(episode.EncounterId).ToList();
        var invoices = await _dbContext.BilInvoices.AsNoTracking().Where(x => encounterIds.Contains(x.EncounterId) && !x.IsDelete).ToListAsync(ct);
        decimal linkedTotal = 0;
        var unavailable = calculation == null;
        foreach (var sourceInvoice in invoices)
        {
            var linkedEncounter = linked.SingleOrDefault(x => x.EncounterId == sourceInvoice.EncounterId);
            var sourceCalculation = sourceInvoice.Id == invoice.Id ? calculation : (await ResolveCalculationAsync(sourceInvoice, ct)).Calculation;
            var calculatedItems = sourceCalculation?.Breakdown.Items.ToDictionary(x => x.InvoiceItemId) ?? [];
            var items = await _dbContext.BilInvoiceItems.AsNoTracking().Include(x => x.Category).Include(x => x.Tariff)
                .Where(x => x.InvoiceId == sourceInvoice.Id && !x.IsDelete && x.Status == BillingInvoiceItemStatuses.Active).ToListAsync(ct);
            var excludedIds = await _dbContext.BilInvoiceItemBillingDispositions.AsNoTracking()
                .Where(x => x.InvoiceItem!.InvoiceId == sourceInvoice.Id && x.IsActive && !x.IsDelete && x.Disposition == "EXCLUDED")
                .Select(x => x.InvoiceItemId).ToListAsync(ct);
            foreach (var item in items.Where(x => !excludedIds.Contains(x.Id)))
            {
                var group = MapGroup(item.SourceDomain, item.Category);
                if (linkedEncounter != null && group != "SURGERY") continue;
                unavailable |= (sourceCalculation == null && item.UnitPrice == 0);
                var calculatedItem = calculatedItems.GetValueOrDefault(item.Id);
                var amount = calculatedItem != null ? calculatedItem.NetAmount : (item.UnitPrice > 0 ? item.Quantity * item.UnitPrice : 0);
                var lineStatus = (calculatedItem == null && item.UnitPrice == 0) ? "TARIFF_NOT_FOUND" : "ACTIVE";
                Add(group, new PatientBillingLineResponse
                {
                    Label = item.DescriptionSnapshot, Quantity = item.Quantity, ServiceDate = item.SourceOccurredAt,
                    LinkedEncounter = linkedEncounter,
                    LineStatus = lineStatus
                }, amount);
                if (linkedEncounter != null) linkedTotal += amount;
            }
            await AddMissingTariffLinesAsync(sourceInvoice.EncounterId, linkedEncounter, Add, ct);
        }
        view.Groups = GroupOrder.Select(x => groups[x.Code]).Where(x => x.Lines.Count > 0).ToList();
        var total = calculation != null ? calculation.TotalInvoiceAmount + linkedTotal : (unavailable ? null : (decimal?)(totals.Values.Sum() + linkedTotal));
        return new(view, calculation, totals, total);
    }

    /// <summary>
    /// Rincian estimasi tagihan rawat inap berharga per item untuk pemegang izin PatientBillingSummary:ViewItemAmount (TASK BE-01 s.d. BE-09).
    /// Menggunakan live calculation preview pada invoice OPEN tanpa mengubah database, state invoice, atau row version.
    /// </summary>
    public async Task<PatientBillingEstimateResponse?> GetEstimateAsync(Guid episodeId, CancellationToken ct)
    {
        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .Where(x => x.Id == episodeId && !x.IsDelete)
            .Select(x => new { x.Id, x.EncounterId })
            .SingleOrDefaultAsync(ct);
        if (episode == null) return null;

        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.EncounterId == episode.EncounterId && !x.IsDelete, ct);

        if (invoice == null)
        {
            return new PatientBillingEstimateResponse
            {
                EpisodeId = episodeId,
                EncounterId = episode.EncounterId,
                InvoiceState = "NOT_FORMED",
                CalculationSource = "NOT_CALCULATED",
                IsEstimateComplete = false,
                WarningMessages = ["Belum ada invoice billing terbentuk untuk perawatan ini."]
            };
        }

        var (calculation, calculationSource) = await ResolveCalculationAsync(invoice, ct);

        var groups = GroupOrder.ToDictionary(
            x => x.Code,
            x => new PatientBillingEstimateGroupResponse { GroupCode = x.Code, Label = x.Label });

        var warningMessages = new List<string>();

        // 1. Biaya Kamar (ROOM) dari RoomChargeCalculationResponse engine Billing
        if (calculation != null && calculation.Breakdown.RoomCharge.Segments.Count > 0)
        {
            var roomIds = calculation.Breakdown.RoomCharge.Segments.Select(x => x.RoomId).Distinct().ToList();
            var rooms = await _dbContext.Set<MstRoom>().AsNoTracking()
                .Where(x => roomIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.RoomName, ct);

            foreach (var segment in calculation.Breakdown.RoomCharge.Segments)
            {
                var isMissingTariff = segment.MissingTariff;
                var roomLine = new PatientBillingEstimateLineResponse
                {
                    LineKey = $"ROOM_{segment.PlacementId}_{segment.StartDateTime:yyyyMMddHHmm}",
                    Label = rooms.GetValueOrDefault(segment.RoomId) ?? "Kamar Rawat Inap",
                    PeriodLabel = $"{segment.StartDateTime:dd/MM/yyyy HH:mm} – {(segment.EndDateTime?.ToString("dd/MM/yyyy HH:mm") ?? "berjalan")}",
                    Quantity = segment.ChargeUnits,
                    UnitLabel = "unit tarif kamar",
                    UnitPrice = isMissingTariff ? null : segment.UnitPrice,
                    GrossAmount = isMissingTariff ? null : segment.SegmentAmount,
                    ItemDiscount = 0m,
                    TaxAmount = 0m,
                    NetAmount = isMissingTariff ? null : segment.SegmentAmount,
                    PriceStatus = isMissingTariff ? "TARIFF_NOT_FOUND" : "PRICED",
                    LineStatus = isMissingTariff ? "TARIFF_NOT_FOUND" : "ACTIVE",
                    SourceDomain = "ROOM_CHARGE"
                };
                groups["ROOM"].Lines.Add(roomLine);
            }

            if (calculation.Breakdown.RoomCharge.Segments.Any(s => s.MissingTariff))
            {
                warningMessages.Add("Estimasi sementara — tarif kamar belum tersedia.");
            }
        }

        // 2. Biaya Administrasi (ADMINISTRATION)
        if (calculation != null && (calculation.Breakdown.AdministrationFee.PolicyId.HasValue || calculation.AdministrationFeeAmount > 0))
        {
            var adminLine = new PatientBillingEstimateLineResponse
            {
                LineKey = "ADMINISTRATION_FEE",
                Label = "Biaya Administrasi",
                PeriodLabel = null,
                Quantity = 1,
                UnitLabel = "transaksi",
                UnitPrice = calculation.AdministrationFeeAmount,
                GrossAmount = calculation.AdministrationFeeAmount,
                ItemDiscount = 0m,
                TaxAmount = 0m,
                NetAmount = calculation.AdministrationFeeAmount,
                PriceStatus = "PRICED",
                LineStatus = "ACTIVE",
                SourceDomain = "ADMINISTRATION",
                ServiceDate = calculation.CalculatedAt
            };
            groups["ADMINISTRATION"].Lines.Add(adminLine);
        }

        // 3. Kunjungan Tertaut (Linked Encounters)
        var linked = await _dbContext.BilInvoiceEncounterLinks.AsNoTracking()
            .Where(x => x.RanapInvoiceId == invoice.Id && !x.IsDelete)
            .Select(x => new LinkedBillingEncounterResponse
            {
                EncounterId = x.LinkedEncounterId,
                EncounterTypeName = x.LinkedEncounter.EncounterType == EncounterType.Outpatient ? "Poliklinik"
                    : x.LinkedEncounter.EncounterType == EncounterType.Emergency ? "IGD"
                    : x.LinkedEncounter.EncounterType.ToString(),
                ServiceUnitName = x.LinkedEncounter.ServiceUnit!.ServiceUnitName,
                VisitDate = x.LinkedEncounter.EncounterDate
            }).ToListAsync(ct);

        var encounterIds = linked.Select(x => x.EncounterId).Append(episode.EncounterId).ToList();
        var invoices = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => encounterIds.Contains(x.EncounterId) && !x.IsDelete)
            .ToListAsync(ct);

        var allActiveItems = new List<BilInvoiceItem>();

        foreach (var sourceInvoice in invoices)
        {
            var linkedEncounter = linked.SingleOrDefault(x => x.EncounterId == sourceInvoice.EncounterId);
            var sourceCalculation = sourceInvoice.Id == invoice.Id
                ? calculation
                : (await ResolveCalculationAsync(sourceInvoice, ct)).Calculation;
            var calculatedItems = sourceCalculation?.Breakdown.Items.ToDictionary(x => x.InvoiceItemId) ?? [];

            var items = await _dbContext.BilInvoiceItems.AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Tariff)
                .Where(x => x.InvoiceId == sourceInvoice.Id && !x.IsDelete && x.Status == BillingInvoiceItemStatuses.Active)
                .ToListAsync(ct);

            allActiveItems.AddRange(items);

            var excludedIds = await _dbContext.BilInvoiceItemBillingDispositions.AsNoTracking()
                .Where(x => x.InvoiceItem!.InvoiceId == sourceInvoice.Id && x.IsActive && !x.IsDelete && x.Disposition == "EXCLUDED")
                .Select(x => x.InvoiceItemId)
                .ToListAsync(ct);

            var excludedSet = new HashSet<Guid>(excludedIds);

            foreach (var item in items)
            {
                var group = MapGroup(item.SourceDomain, item.Category);
                if (linkedEncounter != null && group != "SURGERY") continue;

                var isExcluded = excludedSet.Contains(item.Id);
                var calculatedItem = calculatedItems.GetValueOrDefault(item.Id);

                decimal? unitPrice;
                decimal? grossAmount;
                decimal? itemDiscount;
                decimal? taxAmount;
                decimal? netAmount;
                string priceStatus;
                string lineStatus;

                if (isExcluded)
                {
                    unitPrice = item.UnitPrice > 0 ? item.UnitPrice : (decimal?)null;
                    grossAmount = item.UnitPrice > 0 ? item.Quantity * item.UnitPrice : (decimal?)null;
                    itemDiscount = 0m;
                    taxAmount = 0m;
                    netAmount = 0m;
                    priceStatus = "EXCLUDED";
                    lineStatus = "EXCLUDED";
                }
                else if (calculatedItem != null)
                {
                    grossAmount = calculatedItem.GrossAmount;
                    unitPrice = item.UnitPrice > 0 ? item.UnitPrice : (item.Quantity > 0 ? calculatedItem.GrossAmount / item.Quantity : 0m);
                    itemDiscount = calculatedItem.ItemDiscount;
                    taxAmount = calculatedItem.TaxAmount;
                    netAmount = calculatedItem.NetAmount;
                    priceStatus = (unitPrice > 0 || grossAmount > 0) ? "PRICED" : "TARIFF_NOT_FOUND";
                    lineStatus = priceStatus == "TARIFF_NOT_FOUND" ? "TARIFF_NOT_FOUND" : "ACTIVE";
                }
                else
                {
                    unitPrice = item.UnitPrice > 0 ? item.UnitPrice : (decimal?)null;
                    grossAmount = item.UnitPrice > 0 ? item.Quantity * item.UnitPrice : (decimal?)null;
                    itemDiscount = 0m;
                    taxAmount = 0m;
                    netAmount = grossAmount;
                    priceStatus = (unitPrice.HasValue && unitPrice.Value > 0) ? "PRICED" : "TARIFF_NOT_FOUND";
                    lineStatus = priceStatus == "TARIFF_NOT_FOUND" ? "TARIFF_NOT_FOUND" : "ACTIVE";
                }

                var unitLabel = item.Category?.IsPharmacy == true
                    ? "tablet / botol"
                    : (group is "PROCEDURE" or "SURGERY" || item.Category?.IsSurgery == true || item.Category?.IsProcedure == true ? "tindakan" : "unit");

                var line = new PatientBillingEstimateLineResponse
                {
                    LineKey = $"ITEM_{item.Id}",
                    InvoiceItemId = item.Id,
                    Label = item.DescriptionSnapshot,
                    UnitLabel = unitLabel,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    GrossAmount = grossAmount,
                    ItemDiscount = itemDiscount,
                    TaxAmount = taxAmount,
                    NetAmount = netAmount,
                    PriceStatus = priceStatus,
                    LineStatus = lineStatus,
                    SourceDomain = item.SourceDomain,
                    ServiceDate = item.SourceOccurredAt,
                    LinkedEncounter = linkedEncounter
                };

                groups[group].Lines.Add(line);
            }

            // Tambahkan baris fakta milestone klinis yang belum memiliki tarif
            await AddMissingTariffLinesToEstimateAsync(sourceInvoice.EncounterId, linkedEncounter, groups, ct);
        }

        // 4. Deteksi potensi double-count biaya kamar manual dan otomatis
        if (calculation != null && calculation.RoomChargeAmount > 0
            && allActiveItems.Any(BillingCalculationService.IsManualRoomChargeItem))
        {
            warningMessages.Add("Terdapat biaya kamar otomatis dan manual sekaligus pada tagihan.");
        }

        if (calculationSource == "LIVE_PREVIEW")
        {
            warningMessages.Add("Nilai merupakan estimasi berjalan (live preview) dan dapat berubah sesuai perkembangan pelayanan.");
        }

        // 5. Agregasi per grup
        foreach (var group in groups.Values)
        {
            var activeLines = group.Lines.Where(l => l.LineStatus != "EXCLUDED").ToList();
            group.ItemCount = activeLines.Count;
            group.UnpricedItemCount = activeLines.Count(l =>
                l.PriceStatus == "TARIFF_NOT_FOUND" ||
                l.PriceStatus == "PENDING_CALCULATION" ||
                !l.GrossAmount.HasValue);
            group.IsSubtotalComplete = (group.UnpricedItemCount == 0);
            group.IncludesLinkedEncounter = group.Lines.Any(l => l.LinkedEncounter != null);

            var pricedLines = activeLines.Where(l => l.GrossAmount.HasValue).ToList();
            if (pricedLines.Count > 0)
            {
                group.GrossSubtotal = pricedLines.Sum(l => l.GrossAmount!.Value);
            }
            else if (group.UnpricedItemCount > 0)
            {
                group.GrossSubtotal = null;
            }
            else
            {
                group.GrossSubtotal = 0m;
            }
        }

        var populatedGroups = GroupOrder
            .Select(x => groups[x.Code])
            .Where(x => x.Lines.Count > 0)
            .ToList();

        var totalUnpricedCount = populatedGroups.Sum(g => g.UnpricedItemCount);
        var isEstimateComplete = totalUnpricedCount == 0 && calculation != null;

        var totalGross = populatedGroups
            .Where(g => g.GrossSubtotal.HasValue)
            .Sum(g => g.GrossSubtotal!.Value);

        return new PatientBillingEstimateResponse
        {
            EpisodeId = episodeId,
            EncounterId = episode.EncounterId,
            InvoiceId = invoice.Id,
            InvoiceState = invoice.Status,
            CalculatedAt = calculation?.CalculatedAt,
            CalculationSource = calculationSource,
            IsEstimateComplete = isEstimateComplete,
            UnpricedItemCount = totalUnpricedCount,
            Groups = populatedGroups,
            LinkedEncounters = linked,
            TotalGrossAmount = totalGross,
            TotalDiscountAmount = calculation?.TotalDiscount,
            TotalTaxAmount = calculation?.TaxAmount,
            AdministrationFeeAmount = calculation?.AdministrationFeeAmount,
            RoomChargeAmount = calculation?.RoomChargeAmount,
            EstimatedInvoiceTotalAmount = calculation?.TotalInvoiceAmount ?? totalGross,
            PatientAmount = calculation?.PatientAmount,
            PrimaryAmount = calculation?.PrimaryAmount,
            IsTotalComplete = isEstimateComplete,
            WarningMessages = warningMessages
        };
    }

    /// <summary>
    /// Menentukan sumber kalkulasi tagihan (TASK BE-01, TASK BE-06):
    /// Untuk invoice OPEN, panggil PreviewCalculationAsync (live read-only preview tanpa write ke database).
    /// Untuk invoice non-OPEN atau fallback, baca versi kalkulasi yang tersimpan.
    /// </summary>
    private async Task<(CalculationResponse? Calculation, string CalculationSource)> ResolveCalculationAsync(
        BilInvoice invoice,
        CancellationToken ct)
    {
        if (invoice.Status == BillingInvoiceStatuses.Open)
        {
            try
            {
                var preview = await _calculationService.PreviewCalculationAsync(invoice.Id, Guid.Empty, ct);
                return (preview, "LIVE_PREVIEW");
            }
            catch (Exception)
            {
                // Bila preview gagal dijalankan (mis. invoice berubah secara bersamaan), fallback ke kalkulasi tersimpan
            }
        }

        var persisted = await ReadCalculationAsync(invoice, ct);
        return (persisted, persisted != null ? "PERSISTED" : "NOT_CALCULATED");
    }

    // Selalu memakai versi hitungan canonical yang tersimpan, tanpa hitung tarif kedua atau tulis saat GET.
    private async Task<CalculationResponse?> ReadCalculationAsync(BilInvoice invoice, CancellationToken ct)
    {
        var version = await _dbContext.BilCalculationVersions.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete)
            .SingleOrDefaultAsync(ct);
        return version == null ? null : BillingCalculationService.MapResponse(version, invoice.RowVersion);
    }

    private string MapGroup(string domain, MstTariffCategory? category)
    {
        if (domain == "EQUIPMENT_USAGE") return "EQUIPMENT";
        if (domain == "OPERATING_ROOM" || category?.IsSurgery == true) return "SURGERY";
        if (category?.IsRoomCharge == true) return "ROOM";
        if (category?.IsAdministrationFee == true) return "ADMINISTRATION";
        var configured = _configuration[$"PatientBillingSummary:GroupMapping:{domain}"];
        if (configured != null && GroupOrder.Any(x => x.Code == configured)) return configured;
        if (category?.IsPharmacy == true || domain is "PHARMACY" or "CONSUMABLE") return "PHARMACY";
        if (category?.IsLaboratory == true || category?.IsRadiology == true
            || domain is "LABORATORY" or "RADIOLOGY" or "NUTRITION" or "BLOOD_BANK" or "HEMODIALYSIS") return "SUPPORT";
        return "PROCEDURE";
    }

    private async Task AddMissingTariffLinesAsync(Guid encounterId, LinkedBillingEncounterResponse? linked,
        Action<string, PatientBillingLineResponse, decimal> add, CancellationToken ct)
    {
        var facts = await (from effect in _dbContext.Set<BilProcessingEffect>().AsNoTracking()
            join folio in _dbContext.Set<BilFolio>().AsNoTracking() on effect.FolioId equals folio.Id
            join fact in _dbContext.Set<CliClinicalMilestoneFact>().AsNoTracking()
                on new { effect.MilestoneFactId, effect.MilestoneFactVersion } equals new { fact.MilestoneFactId, fact.MilestoneFactVersion }
            where folio.EncounterId == encounterId && !folio.IsDelete && !effect.IsDelete && !fact.IsDelete
                && effect.InvoiceSyncErrorCode == BillingBridgeCodes.TariffNotFound
                && !effect.IsClinicalCancellation
                && !_dbContext.Set<BilProcessingEffect>().Any(later => later.MilestoneFactId == effect.MilestoneFactId
                    && later.SourceContext == effect.SourceContext && later.MilestoneFactVersion > effect.MilestoneFactVersion && !later.IsDelete)
            select new { effect.SourceContext, effect.OccurredAt, fact.Quantity, fact.Unit }).ToListAsync(ct);
        foreach (var fact in facts)
        {
            var domain = fact.SourceContext switch
            {
                "Prescription" => "PHARMACY", "Laboratory" => "LABORATORY", "Radiology" => "RADIOLOGY",
                "BloodBank" => "BLOOD_BANK", "OperatingRoom" => "OPERATING_ROOM",
                "EquipmentUsage" => "EQUIPMENT_USAGE", _ => fact.SourceContext.ToUpperInvariant()
            };
            var group = MapGroup(domain, null);
            if (linked != null && group != "SURGERY") continue;
            add(group, new PatientBillingLineResponse
            {
                Label = groupsLabel(group), ServiceDate = new DateTimeOffset(DateTime.SpecifyKind(fact.OccurredAt, DateTimeKind.Utc)),
                Quantity = fact.Quantity ?? 1, UnitLabel = fact.Unit ?? "unit", LineStatus = "TARIFF_NOT_FOUND", LinkedEncounter = linked
            }, 0);
        }
        static string groupsLabel(string code) => GroupOrder.First(x => x.Code == code).Label + " — tarif belum ada";
    }

    private async Task AddMissingTariffLinesToEstimateAsync(
        Guid encounterId,
        LinkedBillingEncounterResponse? linked,
        Dictionary<string, PatientBillingEstimateGroupResponse> groups,
        CancellationToken ct)
    {
        var facts = await (from effect in _dbContext.Set<BilProcessingEffect>().AsNoTracking()
            join folio in _dbContext.Set<BilFolio>().AsNoTracking() on effect.FolioId equals folio.Id
            join fact in _dbContext.Set<CliClinicalMilestoneFact>().AsNoTracking()
                on new { effect.MilestoneFactId, effect.MilestoneFactVersion } equals new { fact.MilestoneFactId, fact.MilestoneFactVersion }
            where folio.EncounterId == encounterId && !folio.IsDelete && !effect.IsDelete && !fact.IsDelete
                && effect.InvoiceSyncErrorCode == BillingBridgeCodes.TariffNotFound
                && !effect.IsClinicalCancellation
                && !_dbContext.Set<BilProcessingEffect>().Any(later => later.MilestoneFactId == effect.MilestoneFactId
                    && later.SourceContext == effect.SourceContext && later.MilestoneFactVersion > effect.MilestoneFactVersion && !later.IsDelete)
            select new { effect.MilestoneFactId, effect.SourceContext, effect.OccurredAt, fact.Quantity, fact.Unit }).ToListAsync(ct);

        foreach (var fact in facts)
        {
            var domain = fact.SourceContext switch
            {
                "Prescription" => "PHARMACY",
                "Laboratory" => "LABORATORY",
                "Radiology" => "RADIOLOGY",
                "BloodBank" => "BLOOD_BANK",
                "OperatingRoom" => "OPERATING_ROOM",
                "EquipmentUsage" => "EQUIPMENT_USAGE",
                _ => fact.SourceContext.ToUpperInvariant()
            };
            var group = MapGroup(domain, null);
            if (linked != null && group != "SURGERY") continue;

            var line = new PatientBillingEstimateLineResponse
            {
                LineKey = $"FACT_{fact.MilestoneFactId}_{fact.SourceContext}",
                Label = GroupOrder.First(x => x.Code == group).Label + " — tarif belum ada",
                ServiceDate = new DateTimeOffset(DateTime.SpecifyKind(fact.OccurredAt, DateTimeKind.Utc)),
                Quantity = fact.Quantity ?? 1,
                UnitLabel = fact.Unit ?? "unit",
                UnitPrice = null,
                GrossAmount = null,
                ItemDiscount = null,
                TaxAmount = null,
                NetAmount = null,
                PriceStatus = "TARIFF_NOT_FOUND",
                LineStatus = "TARIFF_NOT_FOUND",
                SourceDomain = domain,
                LinkedEncounter = linked
            };

            groups[group].Lines.Add(line);
        }
    }
}
