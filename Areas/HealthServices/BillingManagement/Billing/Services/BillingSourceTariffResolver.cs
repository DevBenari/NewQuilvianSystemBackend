using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Menetapkan harga item invoice dari katalog tarif untuk fakta pelayanan Rawat Jalan
/// (<c>RJ-E2E-DEC-006</c>, <c>02-backend-architecture.md</c> V2.7.4).
///
/// Jumlah berasal dari fakta klinis (kebenaran klinis); harga berasal dari <see cref="MstTariff"/>
/// yang berlaku pada waktu pelayanan (kebenaran finansial). Harga yang dikirim modul klinis di
/// <c>TariffSnapshot</c> sengaja tidak dibaca. Bila tarif tidak ditemukan, resolver tidak pernah
/// mengarang Rp0 — hasilnya penolakan yang diteruskan ke antrean rekonsiliasi.
/// </summary>
public sealed class BillingSourceTariffResolver
{
    private readonly ApplicationDbContext _dbContext;

    public BillingSourceTariffResolver(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<BillingTariffResolution> ResolveAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        return request.SourceDomain switch
        {
            BillingBridgeSourceDomains.Procedure => await ResolveProcedureAsync(request, cancellationToken),
            BillingBridgeSourceDomains.Laboratory => await ResolveLaboratoryAsync(request, cancellationToken),
            BillingBridgeSourceDomains.Radiology => await ResolveRadiologyAsync(request, cancellationToken),
            BillingBridgeSourceDomains.Consultation => await ResolveConsultationAsync(request, cancellationToken),
            BillingBridgeSourceDomains.Pharmacy => await ResolvePharmacyAsync(request, cancellationToken),
            BillingBridgeSourceDomains.OperatingRoom => await ResolveOperatingRoomAsync(request, cancellationToken),
            BillingBridgeSourceDomains.EquipmentUsage => await ResolveEquipmentUsageAsync(request, cancellationToken),
            BillingBridgeSourceDomains.BloodBank => await ResolveBloodBankAsync(request, cancellationToken),
            BillingBridgeSourceDomains.Consumable => await ResolveConsumableAsync(request, cancellationToken),
            _ => BillingTariffResolution.Pending(
                BillingBridgeCodes.SourcePendingSupport,
                $"Penetapan tarif untuk {request.SourceDomain} belum tersedia pada jembatan.")
        };
    }

    private async Task<BillingTariffResolution> ResolveEquipmentUsageAsync(BillingTariffResolutionRequest request, CancellationToken cancellationToken)
    {
        var usage = await _dbContext.Set<CliEquipmentUsage>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.SourceAggregateId && !x.IsDelete, cancellationToken);
        if (usage == null) return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Pemakaian alat tidak ditemukan.");
        var candidates = await EffectiveTariffs(request.OccurredAt)
            .Where(x => x.MedicalEquipmentId == usage.MedicalEquipmentId
                && (x.PatientClassId == null || x.PatientClassId == request.PatientClassId)
                && (x.ClinicId == null || x.ClinicId == request.EncounterClinicId))
            .ToListAsync(cancellationToken);
        var tariff = candidates.OrderByDescending(x => (x.PatientClassId != null ? 2 : 0) + (x.ClinicId != null ? 1 : 0))
            .ThenBy(x => x.TariffCode, StringComparer.Ordinal).FirstOrDefault();
        return Build(tariff, request.FactQuantity ?? usage.BilledUnits ?? 0);
    }

    /// <summary>
    /// Tindakan Bank Darah (<c>BE-BKC-FIX-018</c>, K-06, <c>DEC-BD-016</c>): satu baris per tindakan
    /// selesai, berapa pun kantongnya. Harga dibaca ulang dari <see cref="MstTariff"/> yang berlaku
    /// pada waktu tindakan — rujukan tarif yang dibekukan Bank Darah dipakai lebih dulu, lalu tarif
    /// tindakan rujukannya — bukan dari <c>TariffAmountSnapshot</c> klinis, sama seperti sumber lain.
    /// </summary>
    private async Task<BillingTariffResolution> ResolveBloodBankAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SourceItemId is not { } procedureId || procedureId == Guid.Empty)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Tindakan Bank Darah tidak dikenali.");

        var procedure = await _dbContext.Set<BbkBloodBankProcedure>().AsNoTracking()
            .Where(x => x.Id == procedureId && x.BloodOrderId == request.SourceAggregateId && !x.IsDelete)
            .Select(x => new { x.TariffId, x.ProcedureRefId, x.PatientClassId })
            .FirstOrDefaultAsync(cancellationToken);
        if (procedure is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Tindakan Bank Darah sumber tidak ditemukan.");

        var patientClassId = procedure.PatientClassId != Guid.Empty ? procedure.PatientClassId : request.PatientClassId;
        var tariff = await FindTariffAsync(
            procedure.TariffId, procedure.ProcedureRefId, request.EncounterClinicId,
            patientClassId, request.OccurredAt, cancellationToken);
        return Build(tariff, 1m);
    }

    /// <summary>
    /// Pemakaian obat/BMHP tanpa resep (<c>BE-BKC-FIX-017</c>, K-04): satu baris per butir pemakaian,
    /// jumlah dari fakta (jumlah yang benar-benar dicatat, atau sisa setelah retur), harga dari tarif
    /// obat/alkes yang berlaku pada waktu pemakaian. Pemakaian yang menempel pada resep tidak
    /// pernah ditagih dari sini — penyerahan resep sudah ditagih lewat sumber resep.
    /// </summary>
    private async Task<BillingTariffResolution> ResolveConsumableAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SourceItemId is not { } itemId || itemId == Guid.Empty)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Butir pemakaian tidak dikenali.");

        var item = await _dbContext.PhmDrugUsageItems.AsNoTracking()
            .Where(x => x.Id == itemId && x.DrugUsageId == request.SourceAggregateId && !x.IsDelete)
            .Select(x => new { x.DrugId, x.Quantity, x.DrugUsage!.PrescriptionId, x.DrugUsage.Status })
            .FirstOrDefaultAsync(cancellationToken);
        if (item is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Butir pemakaian obat sumber tidak ditemukan.");
        if (item.PrescriptionId is not null)
            return BillingTariffResolution.NotBillable(BillingBridgeCodes.NotBillable,
                "Pemakaian ini adalah penyerahan resep; tagihannya mengikuti resep.");
        if (item.Status is not (DrugUsageStatus.NotBilled or DrugUsageStatus.Billed))
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceRejected,
                "Pemakaian belum dicatat; draft tidak ditagihkan.");

        var tariff = await FindDrugTariffAsync(
            null, item.DrugId, request.EncounterClinicId, request.PatientClassId,
            request.OccurredAt, cancellationToken);
        if (tariff is null)
            return BillingTariffResolution.Rejected(
                BillingBridgeCodes.TariffNotFound,
                "Tarif obat/alkes pemakaian ini belum tersedia pada tanggal pemakaian. Lengkapi tarif, lalu kirim ulang.");

        return Build(tariff, request.FactQuantity is > 0 ? request.FactQuantity.Value : item.Quantity);
    }

    private async Task<BillingTariffResolution> ResolveProcedureAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        var procedure = await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
            .Where(x => x.Id == request.SourceAggregateId)
            .Select(x => new { x.TariffId, x.ProcedureId, x.ClinicId, x.Quantity, x.IsFreeOfCharge, x.IsBillable })
            .FirstOrDefaultAsync(cancellationToken);
        if (procedure is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Tindakan sumber tidak ditemukan.");

        // Tindakan gratis atau yang ditandai tidak ditagihkan memang tidak masuk invoice.
        if (procedure.IsFreeOfCharge || !procedure.IsBillable)
            return BillingTariffResolution.NotBillable(BillingBridgeCodes.NotBillable, "Tindakan ditandai gratis atau tidak ditagihkan.");

        var quantity = request.FactQuantity is > 0 ? request.FactQuantity.Value : procedure.Quantity;
        var tariff = await FindTariffAsync(
            procedure.TariffId, procedure.ProcedureId, procedure.ClinicId ?? request.EncounterClinicId,
            request.PatientClassId, request.OccurredAt, cancellationToken);
        return Build(tariff, quantity);
    }

    private async Task<BillingTariffResolution> ResolveLaboratoryAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        var examination = await _dbContext.Set<LabExamination>().AsNoTracking()
            .Where(x => x.Id == request.SourceItemId)
            .Select(x => new { x.TariffId, x.ProcedureId })
            .FirstOrDefaultAsync(cancellationToken);

        Guid? procedureId = examination?.ProcedureId;
        Guid? tariffId = examination?.TariffId;

        if (examination is null)
        {
            var orderedProc = await _dbContext.Set<LabOrderedProcedure>().AsNoTracking()
                .Where(x => x.Id == request.SourceItemId || (x.LabOrderId == request.SourceAggregateId && x.ProcedureId == request.SourceItemId))
                .Select(x => new { x.ProcedureId })
                .FirstOrDefaultAsync(cancellationToken);

            if (orderedProc != null)
            {
                procedureId = orderedProc.ProcedureId;
            }
            else
            {
                var order = await _dbContext.Set<LabOrder>().AsNoTracking()
                    .Where(x => x.Id == request.SourceAggregateId || x.Id == request.SourceItemId)
                    .Select(x => new { x.ProcedureId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (order != null)
                {
                    procedureId = order.ProcedureId;
                }
            }
        }

        if (procedureId is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Pemeriksaan laboratorium sumber tidak ditemukan.");

        var tariff = await FindTariffAsync(
            tariffId, procedureId.Value, request.EncounterClinicId,
            request.PatientClassId, request.OccurredAt, cancellationToken);
        return Build(tariff, 1m);
    }

    private async Task<BillingTariffResolution> ResolveRadiologyAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        var study = await _dbContext.Set<RadStudy>().AsNoTracking()
            .Where(x => x.Id == request.SourceItemId)
            .Select(x => new { x.RadOrderId, x.RepeatCause })
            .FirstOrDefaultAsync(cancellationToken);

        Guid? procedureId = null;
        if (study != null)
        {
            // RJ-BIL-GATE-DEC-004: pengulangan karena kesalahan rumah sakit tidak menambah tagihan pasien.
            if (study.RepeatCause == RadRepeatCause.InternalHospitalError)
                return BillingTariffResolution.NotBillable(
                    BillingBridgeCodes.RepeatInternalError,
                    "Pengulangan pemeriksaan karena kesalahan rumah sakit tidak ditagihkan.");

            procedureId = await _dbContext.Set<RadOrder>().AsNoTracking()
                .Where(x => x.Id == study.RadOrderId)
                .Select(x => (Guid?)x.ProcedureId)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            procedureId = await _dbContext.Set<RadOrder>().AsNoTracking()
                .Where(x => x.Id == request.SourceAggregateId || x.Id == request.SourceItemId)
                .Select(x => (Guid?)x.ProcedureId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (procedureId is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Order atau study radiologi sumber tidak ditemukan.");

        var tariff = await FindTariffAsync(
            null, procedureId.Value, request.EncounterClinicId, request.PatientClassId,
            request.OccurredAt, cancellationToken);
        return Build(tariff, 1m);
    }

    /// <summary>
    /// Jasa konsultasi — urutan <c>RJ-E2E-DEC-012</c> dilengkapi <c>RJ-E2E-DEC-023</c>:
    /// (1) <see cref="MstDoctorServiceRule"/> aktif dan berlaku untuk dokter, klinik, dan kelas
    /// pasien yang memiliki <c>TariffId</c> tarif konsultasi; (1b) tindakan konsultasi rule itu
    /// (<c>ProcedureId</c>) → tarif konsultasi tindakan tersebut untuk kelas pasien; (2) <see cref="MstTariff"/> ber-<c>IsConsultationFee</c>
    /// untuk klinik + kelas pasien; (3) ber-<c>IsConsultationFee</c> untuk klinik saja.
    /// <c>MstPatientClass.DefaultConsultationFee</c> sengaja tidak dipakai karena bukan tarif katalog.
    /// </summary>
    private async Task<BillingTariffResolution> ResolveConsultationAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Set<TrxDoctorConsultation>().AsNoTracking()
            .Where(x => x.Id == request.SourceAggregateId)
            .Select(x => new { x.DoctorId, x.ClinicId })
            .FirstOrDefaultAsync(cancellationToken);
        if (consultation is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Konsultasi sumber tidak ditemukan.");

        var clinicId = consultation.ClinicId ?? request.EncounterClinicId;
        var at = request.OccurredAt;

        // (1) Aturan layanan dokter. Rule ber-klinik/kelas kosong berlaku umum untuk dokter itu;
        // yang paling spesifik menang.
        var ruleTariffIds = await _dbContext.Set<MstDoctorServiceRule>().AsNoTracking()
            .Where(x => x.DoctorId == consultation.DoctorId
                && (x.TariffId != null || x.ProcedureId != null)
                && x.IsActive && !x.IsDelete && !x.IsCancel
                && x.RuleStatus == DoctorServiceRuleStatus.Active
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= at)
                && (x.EffectiveEndDate == null || at < x.EffectiveEndDate)
                && (x.ClinicId == null || x.ClinicId == clinicId)
                && (x.PatientClassId == null || x.PatientClassId == request.PatientClassId))
            .Select(x => new { x.TariffId, x.ProcedureId, Score = (x.ClinicId != null ? 2 : 0) + (x.PatientClassId != null ? 1 : 0), x.RuleCode })
            .ToListAsync(cancellationToken);

        foreach (var rule in ruleTariffIds.OrderByDescending(x => x.Score).ThenBy(x => x.RuleCode, StringComparer.Ordinal))
        {
            // Tarif yang dipakai wajib tarif konsultasi — rule yang salah menunjuk tarif lain
            // (mis. pemeriksaan) tidak boleh menagihkan harga itu sebagai jasa konsultasi.
            if (rule.TariffId is { } ruleTariffId)
            {
                var ruleTariff = await EffectiveTariffs(at)
                    .FirstOrDefaultAsync(x => x.Id == ruleTariffId && x.IsConsultationFee, cancellationToken);
                if (ruleTariff is not null)
                    return Build(ruleTariff, 1m);
            }

            // (1b) RJ-E2E-DEC-023: tarif konsultasi di master ditautkan ke tindakan konsultasi +
            // kelas pasien. Rule layanan dokter menunjuk tindakan itu lewat ProcedureId.
            if (rule.ProcedureId is { } ruleProcedureId)
            {
                var procedureTariffs = await EffectiveTariffs(at)
                    .Where(x => x.IsConsultationFee && x.ProcedureId == ruleProcedureId
                        && (x.PatientClassId == null || x.PatientClassId == request.PatientClassId))
                    .ToListAsync(cancellationToken);
                var procedureTariff = procedureTariffs
                    .OrderByDescending(x => x.PatientClassId != null ? 1 : 0)
                    .ThenBy(x => x.TariffCode, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (procedureTariff is not null)
                    return Build(procedureTariff, 1m);
            }
        }

        // (2) dan (3) Tarif konsultasi klinik: dengan kelas pasien lebih dulu, lalu klinik saja.
        if (clinicId is not null)
        {
            var clinicTariffs = await EffectiveTariffs(at)
                .Where(x => x.IsConsultationFee && x.ClinicId == clinicId
                    && (x.PatientClassId == null || x.PatientClassId == request.PatientClassId))
                .ToListAsync(cancellationToken);
            var clinicTariff = clinicTariffs
                .OrderByDescending(x => x.PatientClassId != null ? 1 : 0)
                .ThenBy(x => x.TariffCode, StringComparer.Ordinal)
                .FirstOrDefault();
            if (clinicTariff is not null)
                return Build(clinicTariff, 1m);
        }

        return Build(null, 1m);
    }

    /// <summary>
    /// Resep — satu item invoice per resep (clearance farmasi membaca satu item <c>PHARMACY</c>
    /// per resep). Harganya Σ(jumlah × <c>NormalPrice</c>) per item obat; tarif item dicari dari
    /// (1) <c>PhmPrescriptionItem.TariffId</c>, lalu (2) <see cref="MstTariff.DrugId"/> = obat.
    /// Tahap 1 memakai jumlah yang diresepkan untuk item yang tidak dihentikan; tahap 2 memakai
    /// jumlah yang benar-benar diserahkan (<see cref="BillingTariffResolutionRequest.DispensedQuantities"/>).
    /// Satu obat tanpa tarif membuat seluruh resep masuk antrean — tidak pernah ditagih sebagian.
    /// <para>
    /// Racikan (<c>BE-BKC-FIX-015</c>, keputusan pemilik K-02, 12 Oktober 2026): setiap bahan
    /// racikan aktif ditagih <c>PricingQuantity</c> × tarif obat bahan itu — nilai yang sama dengan
    /// yang selama ini ditagih jalur langsung lewat <c>TotalPrice</c> resep. Jasa racik belum
    /// ditagih. Penyerahan per bahan belum dicatat Farmasi, sehingga bahan racikan ikut ditagih
    /// pada tahap yang sedang berlaku (tahap 1 rawat jalan/IGD; tahap 2 rawat inap, yaitu saat
    /// resepnya diserahkan) dengan jumlah yang diresepkan. Retur bahan racikan memakai adjustment
    /// manual.
    /// </para>
    /// </summary>
    private async Task<BillingTariffResolution> ResolvePharmacyAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        var prescription = await _dbContext.PhmPrescriptions.AsNoTracking()
            .Where(x => x.Id == request.SourceAggregateId)
            .Select(x => new { x.PrescriptionNumber })
            .FirstOrDefaultAsync(cancellationToken);
        if (prescription is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Resep sumber tidak ditemukan.");

        var compoundIngredients = await (
                from ingredient in _dbContext.PhmPrescriptionCompoundItems.AsNoTracking()
                join compound in _dbContext.PhmPrescriptionCompounds.AsNoTracking()
                    on ingredient.PrescriptionCompoundId equals compound.Id
                where compound.PrescriptionId == request.SourceAggregateId
                    && compound.IsActive && !compound.IsDelete && !compound.IsCancel
                    && ingredient.IsActive && !ingredient.IsDelete && !ingredient.IsCancel
                orderby compound.CompoundName, ingredient.DrugNameSnapshot
                select new { ingredient.DrugId, ingredient.TariffId, ingredient.PricingQuantity, ingredient.DrugNameSnapshot, compound.CompoundName })
            .ToListAsync(cancellationToken);

        var items = await _dbContext.PhmPrescriptionItems.AsNoTracking()
            .Where(x => x.PrescriptionId == request.SourceAggregateId && !x.IsDelete)
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.DrugId, x.TariffId, x.Quantity, x.IsActive, x.IsStopped, x.DrugNameSnapshot })
            .ToListAsync(cancellationToken);

        var dispensed = request.DispensedQuantities;
        var total = 0m;
        MstTariff? firstTariff = null;
        foreach (var item in items)
        {
            var quantity = dispensed is null
                ? (item.IsActive && !item.IsStopped ? item.Quantity : 0m)
                : dispensed.GetValueOrDefault(item.Id);
            if (quantity <= 0) continue;

            var tariff = await FindDrugTariffAsync(
                item.TariffId, item.DrugId, request.EncounterClinicId, request.PatientClassId,
                request.OccurredAt, cancellationToken);
            if (tariff is null)
                return BillingTariffResolution.Rejected(
                    BillingBridgeCodes.TariffNotFound,
                    Truncate($"Tarif obat {item.DrugNameSnapshot} belum tersedia pada tanggal pelayanan. " +
                        "Seluruh resep ditahan; lengkapi tarif, lalu kirim ulang.", 1000));

            firstTariff ??= tariff;
            total += quantity * tariff.NormalPrice;
        }

        foreach (var ingredient in compoundIngredients)
        {
            if (ingredient.PricingQuantity <= 0) continue;

            var tariff = await FindDrugTariffAsync(
                ingredient.TariffId, ingredient.DrugId, request.EncounterClinicId, request.PatientClassId,
                request.OccurredAt, cancellationToken);
            if (tariff is null)
                return BillingTariffResolution.Rejected(
                    BillingBridgeCodes.TariffNotFound,
                    Truncate($"Tarif bahan racikan {ingredient.DrugNameSnapshot} ({ingredient.CompoundName}) belum tersedia " +
                        "pada tanggal pelayanan. Seluruh resep ditahan; lengkapi tarif, lalu kirim ulang.", 1000));

            firstTariff ??= tariff;
            total += ingredient.PricingQuantity * tariff.NormalPrice;
        }

        if (firstTariff is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceRejected, "Resep tidak memuat obat yang dapat ditagih.");

        return BillingTariffResolution.Resolved(
            null,
            firstTariff.TariffCategoryId,
            Truncate($"Resep {prescription.PrescriptionNumber}", 250),
            1m,
            decimal.Round(total, 2, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Komponen biaya Kamar Operasi (<c>BE-RWI-179</c>, <c>RWI-DEC-196</c>, backend 12.7). Butirnya
    /// dikenali dari <c>SourceItemId</c>:
    /// <list type="bullet">
    /// <item><description>catatan anestesi → tarif ber-<c>SurgeryComponentType = AnesthesiaService</c>;</description></item>
    /// <item><description>catatan operasi → tarif ber-<c>SurgeryComponentType = OperatingRoomRent</c>;</description></item>
    /// <item><description>pemakaian bahan → tarif ber-<c>DrugId</c> = item bahan.</description></item>
    /// </list>
    /// Jumlah fakta untuk anestesi dan sewa kamar adalah durasi dalam menit; tarif <c>PerHour</c>
    /// mengubahnya menjadi jam menurut <c>ChargeRounding</c> (dibulatkan ke atas, atau proporsional
    /// dua desimal). Contoh: operasi 95 menit, sewa kamar Rp500.000/jam bulat ke atas → 2 jam →
    /// Rp1.000.000; proporsional → 1,58 jam → Rp790.000. Tanpa tarif → "tarif belum ada"
    /// (<c>TARIFF_NOT_FOUND</c>) dan invoice tidak dapat difinalkan (<c>BIL-FIN-021</c>).
    /// </summary>
    private async Task<BillingTariffResolution> ResolveOperatingRoomAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SourceItemId is not { } itemId || itemId == Guid.Empty)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Komponen biaya operasi tidak dikenali.");

        var at = request.OccurredAt;

        var isAnesthesia = await _dbContext.Set<OprAnesthesiaRecord>().AsNoTracking()
            .AnyAsync(x => x.Id == itemId && x.OprCaseId == request.SourceAggregateId, cancellationToken);
        var isRoomRent = !isAnesthesia && await _dbContext.Set<OprExecutionRecord>().AsNoTracking()
            .AnyAsync(x => x.Id == itemId && x.OprCaseId == request.SourceAggregateId, cancellationToken);

        if (isAnesthesia || isRoomRent)
        {
            var componentType = isAnesthesia
                ? MstSurgeryComponentType.AnesthesiaService
                : MstSurgeryComponentType.OperatingRoomRent;
            var candidates = await EffectiveTariffs(at)
                .Where(x => x.SurgeryComponentType == componentType
                    && (x.ClinicId == null || x.ClinicId == request.EncounterClinicId)
                    && (x.PatientClassId == null || x.PatientClassId == request.PatientClassId))
                .ToListAsync(cancellationToken);
            var tariff = candidates
                .OrderByDescending(x => (x.ClinicId != null ? 2 : 0) + (x.PatientClassId != null ? 1 : 0))
                .ThenBy(x => x.TariffCode, StringComparer.Ordinal)
                .FirstOrDefault();
            if (tariff is null)
                return BillingTariffResolution.Rejected(
                    BillingBridgeCodes.TariffNotFound,
                    isAnesthesia
                        ? "Tarif jasa anestesi belum ada untuk kelas pasien ini. Lengkapi tarif komponen operasi, lalu kirim ulang."
                        : "Tarif sewa kamar operasi belum ada untuk kelas pasien ini. Lengkapi tarif komponen operasi, lalu kirim ulang.");

            if (tariff.ChargeBasis != MstTariffChargeBasis.PerHour)
                return Build(tariff, 1m);

            var minutes = request.FactQuantity ?? 0m;
            if (minutes <= 0)
                return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceRejected,
                    "Durasi operasi tidak tersedia, sehingga tarif per jam tidak dapat dihitung.");

            var hours = minutes / 60m;
            var billedHours = tariff.ChargeRounding == MstEquipmentRoundingRule.Proportional
                ? decimal.Round(hours, 2, MidpointRounding.AwayFromZero)
                : decimal.Ceiling(hours);
            return Build(tariff, billedHours <= 0 ? 0.01m : billedHours);
        }

        var usage = await _dbContext.Set<OprMaterialUsage>().AsNoTracking()
            .Where(x => x.Id == itemId && x.OprCaseId == request.SourceAggregateId && !x.IsDelete)
            .Select(x => new { x.ExternalItemId, x.Quantity, x.Outcome })
            .FirstOrDefaultAsync(cancellationToken);
        if (usage is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Komponen biaya operasi sumber tidak ditemukan.");
        if (usage.Outcome != OprMaterialOutcome.Used)
            return BillingTariffResolution.NotBillable(BillingBridgeCodes.NotBillable, "Bahan yang tidak terpakai tidak ditagihkan.");

        var drugTariff = await FindDrugTariffAsync(
            null, usage.ExternalItemId, request.EncounterClinicId, request.PatientClassId, at, cancellationToken);
        if (drugTariff is null)
            return BillingTariffResolution.Rejected(
                BillingBridgeCodes.TariffNotFound,
                "Tarif bahan atau implan operasi belum ada pada tanggal pelayanan. Lengkapi tarif, lalu kirim ulang.");

        var quantity = request.FactQuantity is > 0 ? request.FactQuantity.Value : usage.Quantity;
        return Build(drugTariff, quantity);
    }

    private async Task<MstTariff?> FindDrugTariffAsync(
        Guid? directTariffId,
        Guid drugId,
        Guid? clinicId,
        Guid? patientClassId,
        DateTime occurredAt,
        CancellationToken cancellationToken)
    {
        var effective = EffectiveTariffs(occurredAt);

        if (directTariffId is { } tariffId && tariffId != Guid.Empty)
        {
            var direct = await effective.FirstOrDefaultAsync(x => x.Id == tariffId, cancellationToken);
            if (direct is not null) return direct;
        }

        var candidates = await effective
            .Where(x => x.DrugId == drugId
                && (x.ClinicId == null || x.ClinicId == clinicId)
                && (x.PatientClassId == null || x.PatientClassId == patientClassId))
            .ToListAsync(cancellationToken);

        return candidates
            .OrderByDescending(x => (x.ClinicId != null ? 2 : 0) + (x.PatientClassId != null ? 1 : 0))
            .ThenBy(x => x.TariffCode, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

    /// <summary>
    /// Tarif langsung (bila sumber sudah menyimpannya) menang; bila tidak ada atau tidak berlaku,
    /// dicari berdasarkan tindakan dengan urutan paling spesifik: klinik dan kelas pasien, lalu
    /// salah satunya, lalu umum. Seri diputus dengan kode tarif supaya hasilnya deterministik.
    /// </summary>
    private async Task<MstTariff?> FindTariffAsync(
        Guid? directTariffId,
        Guid procedureId,
        Guid? clinicId,
        Guid? patientClassId,
        DateTime occurredAt,
        CancellationToken cancellationToken)
    {
        var effective = EffectiveTariffs(occurredAt);

        if (directTariffId is { } tariffId && tariffId != Guid.Empty)
        {
            var direct = await effective.FirstOrDefaultAsync(x => x.Id == tariffId, cancellationToken);
            if (direct is not null) return direct;
        }

        var candidates = await effective
            .Where(x => x.ProcedureId == procedureId
                && (x.ClinicId == null || x.ClinicId == clinicId)
                && (x.PatientClassId == null || x.PatientClassId == patientClassId))
            .ToListAsync(cancellationToken);

        return candidates
            .OrderByDescending(x => (x.ClinicId != null ? 2 : 0) + (x.PatientClassId != null ? 1 : 0))
            .ThenBy(x => x.TariffCode, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private IQueryable<MstTariff> EffectiveTariffs(DateTime occurredAt) =>
        _dbContext.Set<MstTariff>().AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= occurredAt)
                && (x.EffectiveEndDate == null || occurredAt < x.EffectiveEndDate));

    private static BillingTariffResolution Build(MstTariff? tariff, decimal quantity)
    {
        if (tariff is null)
            return BillingTariffResolution.Rejected(
                BillingBridgeCodes.TariffNotFound,
                "Tarif untuk pelayanan ini belum tersedia pada tanggal pelayanan. Lengkapi tarif, lalu kirim ulang.");
        if (quantity <= 0)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceRejected, "Jumlah pelayanan tidak sah.");

        var description = tariff.TariffName.Trim();
        return BillingTariffResolution.Resolved(
            tariff.Id,
            tariff.TariffCategoryId,
            description.Length > 250 ? description[..250] : description,
            quantity,
            tariff.NormalPrice);
    }
}

public sealed record BillingTariffResolutionRequest(
    string SourceDomain,
    Guid SourceAggregateId,
    Guid? SourceItemId,
    decimal? FactQuantity,
    Guid? EncounterClinicId,
    Guid? PatientClassId,
    DateTime OccurredAt,
    IReadOnlyDictionary<Guid, decimal>? DispensedQuantities = null);

public enum BillingTariffResolutionKind
{
    Resolved = 1,
    NotBillable = 2,
    Rejected = 3,
    Pending = 4
}

public sealed record BillingTariffResolution(
    BillingTariffResolutionKind Kind,
    Guid? TariffId,
    Guid CategoryId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    string? Code,
    string? Message)
{
    public static BillingTariffResolution Resolved(Guid? tariffId, Guid categoryId, string description, decimal quantity, decimal unitPrice) =>
        new(BillingTariffResolutionKind.Resolved, tariffId, categoryId, description, quantity, unitPrice, null, null);

    public static BillingTariffResolution NotBillable(string code, string message) =>
        new(BillingTariffResolutionKind.NotBillable, null, Guid.Empty, string.Empty, 0, 0, code, message);

    public static BillingTariffResolution Rejected(string code, string message) =>
        new(BillingTariffResolutionKind.Rejected, null, Guid.Empty, string.Empty, 0, 0, code, message);

    public static BillingTariffResolution Pending(string code, string message) =>
        new(BillingTariffResolutionKind.Pending, null, Guid.Empty, string.Empty, 0, 0, code, message);
}
