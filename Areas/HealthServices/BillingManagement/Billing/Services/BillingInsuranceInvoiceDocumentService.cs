using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Menyusun lembar dokumen "Invoice Asuransi" satu invoice: identitas pasien, blok perusahaan
/// asuransi, baris yang ditanggung asuransi beserta rupiahnya, dan totalnya (BKC-DEC-065-069).
///
/// Murni baca (AsNoTracking), tidak pernah menulis, tidak memakai Idempotency-Key, dan TIDAK
/// menghitung ulang coverage sendiri - hanya membacakan hasil mesin kalkulasi yang sudah ada.
/// Penyaringan "hanya baris yang ditanggung asuransi" (BKC-DEC-068) dikerjakan di sini, bukan di
/// layar, supaya lembar yang tercetak selalu konsisten dengan yang diputuskan server.
/// </summary>
public sealed class BillingInsuranceInvoiceDocumentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingCalculationService _calculationService;

    public BillingInsuranceInvoiceDocumentService(
        ApplicationDbContext dbContext,
        BillingCalculationService calculationService)
    {
        _dbContext = dbContext;
        _calculationService = calculationService;
    }

    public async Task<InsuranceInvoiceDocumentResponse> GetDocumentAsync(
        Guid invoiceId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");

        // Invoice OPEN: angka segar, sama dengan yang dilihat kasir di Menu Pembayaran.
        // Invoice non-OPEN (FINAL/CLOSED/SETTLED_BY_WRITE_OFF): PreviewCalculationAsync MENOLAK
        // invoice non-OPEN ("Hanya invoice OPEN yang dapat dihitung ulang."), jadi rinciannya
        // wajib dibaca dari versi kalkulasi yang sudah terkunci saat finalisasi.
        var isFromLockedSnapshot = invoice.Status != BillingInvoiceStatuses.Open;
        var calculation = isFromLockedSnapshot
            ? BillingCalculationService.MapResponse(
                await _dbContext.BilCalculationVersions.AsNoTracking()
                    .SingleOrDefaultAsync(
                        x => x.InvoiceId == invoiceId && x.VersionNo == invoice.CurrentCalculationVersion,
                        cancellationToken)
                    ?? throw new KeyNotFoundException("Versi kalkulasi invoice tidak ditemukan."),
                invoice.RowVersion)
            : await _calculationService.PreviewCalculationAsync(invoiceId, actorUserId, cancellationToken);

        var response = new InsuranceInvoiceDocumentResponse
        {
            InvoiceId = invoice.Id,
            DocumentNumber = invoice.InvoiceNumber,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceStatus = invoice.Status,
            ServiceType = invoice.ServiceType,
            InvoiceDate = invoice.InvoiceDate,
            IsFromLockedSnapshot = isFromLockedSnapshot,
            IsPerItemBreakdownAvailable = calculation.Breakdown.Coverage.IsPerItemAllocationAvailable,
            CalculationVersionNo = calculation.VersionNo,
            CalculationContractVersion = calculation.Breakdown.ContractVersion,
            CalculatedAt = calculation.CalculatedAt,
            Patient = await LoadPatientAsync(invoice.EncounterId, cancellationToken)
        };

        var warnings = new List<string>();
        var guarantor = await _dbContext.TrxPatientEncounterGuarantors.AsNoTracking()
            .Where(x => x.EncounterId == invoice.EncounterId && x.IsActive && !x.IsDelete)
            .FirstOrDefaultAsync(cancellationToken);

        response.PayerKind = guarantor is null
            ? InsuranceInvoicePayerKinds.Unknown
            : guarantor.PaymentType switch
            {
                EncounterPaymentType.Cash => InsuranceInvoicePayerKinds.Cash,
                EncounterPaymentType.CompanyGuarantor => InsuranceInvoicePayerKinds.CompanyGuarantor,
                EncounterPaymentType.Insurance => InsuranceInvoicePayerKinds.Insurance,
                _ => InsuranceInvoicePayerKinds.Unknown
            };

        switch (response.PayerKind)
        {
            case InsuranceInvoicePayerKinds.Cash:
                warnings.Add("Kunjungan ini dibayar mandiri, sehingga tidak ada Invoice Asuransi yang dapat diterbitkan.");
                break;
            case InsuranceInvoicePayerKinds.CompanyGuarantor:
                warnings.Add("Penjamin kunjungan ini adalah perusahaan tempat kerja, bukan perusahaan asuransi. Dokumen ini belum mendukung penjamin perusahaan.");
                break;
            case InsuranceInvoicePayerKinds.Unknown:
                warnings.Add("Sumber pembayaran kunjungan ini belum tercatat. Lengkapi data penjamin di Registrasi terlebih dahulu.");
                break;
            case InsuranceInvoicePayerKinds.Insurance:
                response.Payer = await LoadPayerAsync(guarantor!, cancellationToken);
                if (response.Payer is null)
                    warnings.Add("Data perusahaan asuransi tidak ditemukan pada master. Hubungi admin master data.");
                break;
        }

        if (response.PayerKind == InsuranceInvoicePayerKinds.Insurance)
        {
            if (response.IsPerItemBreakdownAvailable)
            {
                response.Items = await BuildItemsAsync(invoice.Id, calculation, cancellationToken);
            }
            else
            {
                warnings.Add("Rincian per item tidak tersedia untuk tagihan yang difinalkan sebelum pembaruan sistem ini. Total tanggungan penjamin tetap sah.");
            }

            if (response.Items.Count == 0 && response.Payer is not null)
                warnings.Add("Tidak ada item yang ditanggung asuransi pada tagihan ini.");
        }

        // TotalCoveredAmount dan PrimaryAmount sengaja bersumber dari field TOP-LEVEL
        // CalculationResponse (bukan dari .Breakdown.Coverage) - keduanya diisi langsung dari
        // kolom relasional BilCalculationVersion baik pada jalur segar (CalculateAsync menyimpannya
        // sebelum serialize) maupun jalur snapshot lama (MapResponse membacanya dari kolom, bukan
        // dari JSON). EligibleAmount tidak punya kolom relasional, sehingga tetap dibaca dari
        // Breakdown.Coverage - field itu sudah ada sejak kontrak baseline dan selalu berhasil
        // dideserialisasi walau untuk snapshot lama sebelum rincian per baris ada.
        response.Totals = new InsuranceInvoiceTotalResponse
        {
            EligibleAmount = calculation.Breakdown.Coverage.EligibleAmount,
            CoveredNetAmount = response.Items.Sum(x => x.CoveredNetAmount),
            CoveredTaxAmount = response.Items.Sum(x => x.CoveredTaxAmount),
            TotalCoveredAmount = calculation.PrimaryAmount,
            PrimaryAmount = calculation.PrimaryAmount,
            ExcessAmount = calculation.ExcessAmount,
            UnresolvedCoverageAmount = calculation.UnresolvedCoverageAmount,
            PatientAmount = calculation.PatientAmount
        };

        response.IsPrintable = response.PayerKind == InsuranceInvoicePayerKinds.Insurance
            && response.Payer is not null
            && response.Items.Count > 0;
        response.Warnings = warnings;
        return response;
    }

    private async Task<InsuranceInvoicePatientResponse?> LoadPatientAsync(
        Guid encounterId, CancellationToken cancellationToken)
    {
        var row = await (
            from encounter in _dbContext.TrxPatientEncounters.AsNoTracking()
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id
            where encounter.Id == encounterId && !encounter.IsDelete
            select new { encounter, patient })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null) return null;

        var roomName = row.encounter.RoomId.HasValue
            ? await _dbContext.MstRooms.AsNoTracking()
                .Where(x => x.Id == row.encounter.RoomId.Value)
                .Select(x => (string?)x.RoomName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        var serviceUnitName = await _dbContext.MstServiceUnits.AsNoTracking()
            .Where(x => x.Id == row.encounter.ServiceUnitId)
            .Select(x => (string?)x.ServiceUnitName)
            .FirstOrDefaultAsync(cancellationToken);
        var patientClassName = row.encounter.PatientClassId.HasValue
            ? await _dbContext.MstPatientClasses.AsNoTracking()
                .Where(x => x.Id == row.encounter.PatientClassId.Value)
                .Select(x => (string?)x.PatientClassName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new InsuranceInvoicePatientResponse
        {
            MedicalRecordNumber = row.patient.MedicalRecordNumber,
            FullName = row.patient.FullName,
            Gender = row.patient.Gender?.ToString(),
            AgeText = row.encounter.AgeTextAtEncounter,
            EncounterNumber = row.encounter.EncounterNumber,
            EncounterDate = row.encounter.EncounterDate,
            EncounterType = row.encounter.EncounterType.ToString(),
            ServiceUnitName = serviceUnitName,
            RoomName = roomName,
            PatientClassName = patientClassName
        };
    }

    // Yang sengaja TIDAK dibaca ke sini: RuleCode/RuleName/ApprovalInstruction/BillingInstruction
    // (kesepakatan komersial RS-asuransi) dan CardNumberSnapshot (nomor kartu asuransi). Lihat
    // 02-backend-architecture.md § Yang sengaja tidak dibuat.
    private async Task<InsuranceInvoicePayerResponse?> LoadPayerAsync(
        TrxPatientEncounterGuarantor guarantor, CancellationToken cancellationToken)
    {
        if (!guarantor.InsuranceProviderId.HasValue) return null;
        var provider = await _dbContext.MstInsuranceProviders.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == guarantor.InsuranceProviderId.Value && !x.IsDelete, cancellationToken);
        if (provider is null) return null;

        return new InsuranceInvoicePayerResponse
        {
            InsuranceProviderName = provider.InsuranceProviderName,
            InsuranceGroupName = provider.InsuranceGroupName,
            ProviderType = provider.ProviderType,
            ClaimMethod = provider.ClaimMethod,
            ContractNumber = provider.ContractNumber,
            OfficeAddress = provider.OfficeAddress,
            PolicyNumber = guarantor.PolicyNumberSnapshot,
            MemberNumber = guarantor.MemberNumberSnapshot,
            PlanName = guarantor.PlanNameSnapshot,
            ClassName = guarantor.ClassNameSnapshot,
            BenefitPlanCode = guarantor.BenefitPlanCodeSnapshot,
            EffectiveStartDate = guarantor.EffectiveStartDateSnapshot,
            EffectiveEndDate = guarantor.EffectiveEndDateSnapshot,
            IsEligible = guarantor.IsEligible,
            IsPolicyActive = guarantor.IsPolicyActive
        };
    }

    // BKC-DEC-068: hanya baris ber-CoveredAmount > 0 yang tampil. Sumber angkanya PrimaryAmount
    // hasil waterfall coverage sesungguhnya (bukan UnresolvedAmount) - itulah rupiah yang BENAR-
    // BENAR ditanggung penjamin, bukan yang masih menggantung. AdministrationFee dan RoomCharge
    // bukan BilInvoiceItem, sehingga tidak punya InvoiceItemId dan tidak lewat dictionary lookup.
    //
    // CalculationItemResponse TIDAK menyimpan Description/CategoryName/Quantity/UnitPrice (hanya
    // CategoryCode) - keempatnya dibaca langsung dari BilInvoiceItem + Category, sumber yang sama
    // dipakai Kwitansi dan Struk Pasien.
    private async Task<List<InsuranceInvoiceItemResponse>> BuildItemsAsync(
        Guid invoiceId, CalculationResponse calculation, CancellationToken cancellationToken)
    {
        var breakdown = calculation.Breakdown;
        var items = new List<InsuranceInvoiceItemResponse>();

        if (breakdown.Items.Count > 0)
        {
            var invoiceItems = await _dbContext.BilInvoiceItems.AsNoTracking()
                .Include(x => x.Category)
                .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            foreach (var calcItem in breakdown.Items)
            {
                var coveredNet = calcItem.ItemPrimaryAmount;
                var coveredTax = calcItem.TaxPrimaryAmount;
                var coveredAmount = coveredNet + coveredTax;
                if (coveredAmount <= 0) continue;
                // Item hanya masuk breakdown bila aktif saat dihitung, sehingga baris ini selalu
                // ditemukan pada invoice yang belum berubah; dilewati (bukan dilempar galat) bila
                // tidak ditemukan, supaya satu item bermasalah tidak menggagalkan seluruh dokumen.
                if (!invoiceItems.TryGetValue(calcItem.InvoiceItemId, out var invoiceItem)) continue;

                var patientAmount =
                    (calcItem.NetAmount - calcItem.ItemPrimaryAmount - calcItem.ItemUnresolvedAmount) +
                    (calcItem.TaxAmount - calcItem.TaxPrimaryAmount - calcItem.TaxUnresolvedAmount);

                items.Add(new InsuranceInvoiceItemResponse
                {
                    Kind = InsuranceInvoiceItemKinds.Item,
                    InvoiceItemId = invoiceItem.Id,
                    Description = invoiceItem.DescriptionSnapshot,
                    CategoryCode = invoiceItem.Category.TariffCategoryCode,
                    CategoryName = invoiceItem.Category.TariffCategoryName,
                    Quantity = invoiceItem.Quantity,
                    UnitPrice = invoiceItem.UnitPrice,
                    GrossAmount = calcItem.GrossAmount,
                    ItemDiscount = calcItem.ItemDiscount,
                    NetAmount = calcItem.NetAmount,
                    TaxAmount = calcItem.TaxAmount,
                    CoveredNetAmount = coveredNet,
                    CoveredTaxAmount = coveredTax,
                    CoveredAmount = coveredAmount,
                    PatientAmount = patientAmount
                });
            }
        }

        var administrationFee = breakdown.AdministrationFee;
        if (administrationFee.PrimaryAmount > 0)
        {
            items.Add(new InsuranceInvoiceItemResponse
            {
                Kind = InsuranceInvoiceItemKinds.AdministrationFee,
                Description = "Biaya Administrasi",
                CategoryCode = administrationFee.PolicyCode,
                Quantity = 1,
                UnitPrice = administrationFee.AppliedAmount,
                GrossAmount = administrationFee.AppliedAmount,
                NetAmount = administrationFee.AppliedAmount,
                CoveredNetAmount = administrationFee.PrimaryAmount,
                CoveredAmount = administrationFee.PrimaryAmount,
                PatientAmount = administrationFee.AppliedAmount - administrationFee.PrimaryAmount - administrationFee.UnresolvedAmount
            });
        }

        var roomCharge = breakdown.RoomCharge;
        if (roomCharge.PrimaryAmount > 0)
        {
            items.Add(new InsuranceInvoiceItemResponse
            {
                Kind = InsuranceInvoiceItemKinds.RoomCharge,
                Description = "Biaya Kamar",
                CategoryCode = roomCharge.PolicyCode,
                Quantity = 1,
                UnitPrice = roomCharge.AppliedAmount,
                GrossAmount = roomCharge.AppliedAmount,
                NetAmount = roomCharge.AppliedAmount,
                CoveredNetAmount = roomCharge.PrimaryAmount,
                CoveredAmount = roomCharge.PrimaryAmount,
                PatientAmount = roomCharge.AppliedAmount - roomCharge.PrimaryAmount - roomCharge.UnresolvedAmount
            });
        }

        return items;
    }
}
