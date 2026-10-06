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
                GroupCode = x.GroupCode, SubtotalAmount = result.Total == null ? null : result.Subtotals.GetValueOrDefault(x.GroupCode),
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
        var calculation = await ReadCalculationAsync(invoice, ct);
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
            var sourceCalculation = sourceInvoice.Id == invoice.Id ? calculation : await ReadCalculationAsync(sourceInvoice, ct);
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
                unavailable |= sourceCalculation == null;
                var calculatedItem = calculatedItems.GetValueOrDefault(item.Id);
                var amount = calculatedItem?.NetAmount ?? 0;
                Add(group, new PatientBillingLineResponse
                {
                    Label = item.DescriptionSnapshot, Quantity = item.Quantity, ServiceDate = item.SourceOccurredAt,
                    LinkedEncounter = linkedEncounter
                }, amount);
                if (linkedEncounter != null) linkedTotal += amount;
            }
            await AddMissingTariffLinesAsync(sourceInvoice.EncounterId, linkedEncounter, Add, ct);
        }
        view.Groups = GroupOrder.Select(x => groups[x.Code]).Where(x => x.Lines.Count > 0).ToList();
        return new(view, calculation, totals, unavailable ? null : calculation!.TotalInvoiceAmount + linkedTotal);
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
}
