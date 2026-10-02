using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Menyusun lembar dokumen "Tagihan Penjamin Perusahaan" (Company Guarantor Invoice Document) satu invoice:
/// identitas pasien, blok perusahaan penjamin, identitas karyawan, rincian baris yang ditanggung penjamin
/// perusahaan beserta rupiahnya, keterangan rute penggantian biaya (reimbursement route), dan totalnya (MPY-DEC-006, MPY-DES-013, MPY-DES-014, CAP-38).
///
/// Murni baca (AsNoTracking), tidak pernah menulis, tidak memakai Idempotency-Key, dan TIDAK
/// menghitung ulang coverage sendiri - hanya membacakan hasil mesin kalkulasi yang sudah ada.
/// Penyaringan "hanya baris yang ditanggung penjamin perusahaan" dikerjakan di sini, bukan di
/// layar, supaya lembar yang tercetak selalu konsisten dengan yang diputuskan server.
/// </summary>
public sealed class BillingCompanyGuarantorInvoiceDocumentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingCalculationService _calculationService;

    public BillingCompanyGuarantorInvoiceDocumentService(
        ApplicationDbContext dbContext,
        BillingCalculationService calculationService)
    {
        _dbContext = dbContext;
        _calculationService = calculationService;
    }

    public async Task<CompanyGuarantorInvoiceDocumentResponse> GetDocumentAsync(
        Guid invoiceId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");

        // Invoice OPEN: angka segar, sama dengan yang dilihat kasir di Menu Pembayaran.
        // Invoice non-OPEN (FINAL/CLOSED/SETTLED_BY_WRITE_OFF): PreviewCalculationAsync MENOLAK
        // invoice non-OPEN, jadi rinciannya wajib dibaca dari versi kalkulasi yang sudah terkunci saat finalisasi.
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

        var response = new CompanyGuarantorInvoiceDocumentResponse
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
        var guarantor = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
            .Where(x => x.EncounterId == invoice.EncounterId && x.IsActive && !x.IsDelete)
            .FirstOrDefaultAsync(cancellationToken);

        response.PayerKind = guarantor is null
            ? CompanyGuarantorInvoicePayerKinds.Unknown
            : guarantor.PaymentType switch
            {
                EncounterPaymentType.Cash => CompanyGuarantorInvoicePayerKinds.Cash,
                EncounterPaymentType.CompanyGuarantor => CompanyGuarantorInvoicePayerKinds.CompanyGuarantor,
                EncounterPaymentType.Insurance => CompanyGuarantorInvoicePayerKinds.Insurance,
                _ => CompanyGuarantorInvoicePayerKinds.Unknown
            };

        switch (response.PayerKind)
        {
            case CompanyGuarantorInvoicePayerKinds.Cash:
                warnings.Add("Kunjungan ini dibayar mandiri, sehingga tidak ada Lembar Tagihan Perusahaan yang dapat diterbitkan.");
                break;
            case CompanyGuarantorInvoicePayerKinds.Insurance:
                warnings.Add("Penjamin kunjungan ini adalah perusahaan asuransi, bukan perusahaan tempat kerja. Gunakan dokumen Invoice Asuransi.");
                break;
            case CompanyGuarantorInvoicePayerKinds.Unknown:
                warnings.Add("Sumber pembayaran kunjungan ini belum tercatat. Lengkapi data penjamin di Registrasi terlebih dahulu.");
                break;
            case CompanyGuarantorInvoicePayerKinds.CompanyGuarantor:
                response.Payer = await LoadPayerAsync(guarantor!, cancellationToken);
                if (response.Payer is null)
                {
                    warnings.Add("Data perusahaan penjamin tidak ditemukan pada master. Hubungi admin master data.");
                }
                else
                {
                    response.ReimbursementRoute = await LoadReimbursementRouteAsync(response.Payer.CompanyGuarantorId, cancellationToken);
                }
                break;
        }

        if (response.PayerKind == CompanyGuarantorInvoicePayerKinds.CompanyGuarantor)
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
            {
                warnings.Add("Tidak ada item yang ditanggung penjamin perusahaan pada tagihan ini.");
            }
        }

        response.Totals = new CompanyGuarantorInvoiceTotalResponse
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

        response.IsPrintable = response.PayerKind == CompanyGuarantorInvoicePayerKinds.CompanyGuarantor
            && response.Payer is not null
            && response.Items.Count > 0;
        response.Warnings = warnings;
        return response;
    }

    private async Task<CompanyGuarantorInvoicePatientResponse?> LoadPatientAsync(
        Guid encounterId, CancellationToken cancellationToken)
    {
        var row = await (
            from encounter in _dbContext.RegPatientEncounters.AsNoTracking()
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

        return new CompanyGuarantorInvoicePatientResponse
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

    /// <summary>
    /// Membaca data perusahaan penjamin beserta identitas karyawan (MPY-DES-013).
    /// Yang sengaja TIDAK dibaca: aturan tanggungan komersial internal, instruksi persetujuan, dan kontak PIC.
    /// </summary>
    private async Task<CompanyGuarantorInvoicePayerResponse?> LoadPayerAsync(
        RegPatientEncounterGuarantor guarantor, CancellationToken cancellationToken)
    {
        Guid? companyGuarantorId = guarantor.CompanyGuarantorId;
        MstPatientCompanyGuarantor? patientCompanyGuarantor = null;

        if (guarantor.PatientCompanyGuarantorId.HasValue)
        {
            patientCompanyGuarantor = await _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == guarantor.PatientCompanyGuarantorId.Value && !x.IsDelete, cancellationToken);

            companyGuarantorId ??= patientCompanyGuarantor?.CompanyGuarantorId;
        }

        if (!companyGuarantorId.HasValue) return null;

        var company = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == companyGuarantorId.Value && !x.IsDelete, cancellationToken);
        if (company is null) return null;

        var employeeNumber = !string.IsNullOrWhiteSpace(guarantor.EmployeeNumberSnapshot)
            ? guarantor.EmployeeNumberSnapshot
            : patientCompanyGuarantor?.EmployeeNumber;

        var employeeName = !string.IsNullOrWhiteSpace(guarantor.EmployeeNameSnapshot)
            ? guarantor.EmployeeNameSnapshot
            : patientCompanyGuarantor?.EmployeeName;

        var planName = !string.IsNullOrWhiteSpace(guarantor.PlanNameSnapshot)
            ? guarantor.PlanNameSnapshot
            : patientCompanyGuarantor?.BenefitPlanName;

        var className = !string.IsNullOrWhiteSpace(guarantor.ClassNameSnapshot)
            ? guarantor.ClassNameSnapshot
            : patientCompanyGuarantor?.ClassName;

        var benefitPlanCode = !string.IsNullOrWhiteSpace(guarantor.BenefitPlanCodeSnapshot)
            ? guarantor.BenefitPlanCodeSnapshot
            : patientCompanyGuarantor?.BenefitPlanCode;

        var effectiveStartDate = guarantor.EffectiveStartDateSnapshot
            ?? patientCompanyGuarantor?.EffectiveStartDate;

        var effectiveEndDate = guarantor.EffectiveEndDateSnapshot
            ?? patientCompanyGuarantor?.EffectiveEndDate;

        return new CompanyGuarantorInvoicePayerResponse
        {
            CompanyGuarantorId = company.Id,
            CompanyGuarantorCode = company.CompanyGuarantorCode,
            CompanyGuarantorName = company.CompanyGuarantorName,
            CompanyGroupName = company.CompanyGroupName,
            GuarantorType = company.GuarantorType,
            ContractNumber = company.ContractNumber,
            OfficeAddress = company.OfficeAddress,
            BillingMethod = company.BillingMethod,
            EmployeeNumber = employeeNumber,
            EmployeeName = employeeName,
            PlanName = planName,
            ClassName = className,
            BenefitPlanCode = benefitPlanCode,
            EffectiveStartDate = effectiveStartDate,
            EffectiveEndDate = effectiveEndDate,
            IsEligible = guarantor.IsEligible,
            IsPolicyActive = guarantor.IsPolicyActive
        };
    }

    /// <summary>
    /// Membaca konfigurasi rute penggantian biaya perusahaan ke asuransi mitra (MPY-DES-014).
    /// </summary>
    private async Task<CompanyGuarantorInvoiceReimbursementRouteResponse> LoadReimbursementRouteAsync(
        Guid companyGuarantorId, CancellationToken cancellationToken)
    {
        var route = await _dbContext.MstCompanyGuarantorReimbursementRoutes.AsNoTracking()
            .Include(x => x.InsuranceProvider)
            .Where(x => x.CompanyGuarantorId == companyGuarantorId && x.IsActive && !x.IsDelete)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Priority)
            .FirstOrDefaultAsync(cancellationToken);

        if (route is null)
        {
            return new CompanyGuarantorInvoiceReimbursementRouteResponse
            {
                RouteType = "SELF",
                Description = "Menanggung sendiri (Tanpa asuransi mitra)"
            };
        }

        return new CompanyGuarantorInvoiceReimbursementRouteResponse
        {
            RouteType = route.RouteType,
            InsuranceProviderId = route.InsuranceProviderId,
            InsuranceProviderName = route.InsuranceProvider?.InsuranceProviderName,
            InsuranceProviderCode = route.InsuranceProvider?.InsuranceProviderCode,
            Description = !string.IsNullOrWhiteSpace(route.Description)
                ? route.Description
                : (route.RouteType == "INSURANCE_PROVIDER"
                    ? $"Penggantian biaya lewat asuransi mitra: {route.InsuranceProvider?.InsuranceProviderName ?? "-"}"
                    : "Menanggung sendiri")
        };
    }

    /// <summary>
    /// Menyaring baris biaya yang ditanggung penjamin perusahaan (CoveredAmount > 0).
    /// Mengikuti pola Invoice Asuransi (BKC-DEC-068, CAP-38).
    /// </summary>
    private async Task<List<CompanyGuarantorInvoiceItemResponse>> BuildItemsAsync(
        Guid invoiceId, CalculationResponse calculation, CancellationToken cancellationToken)
    {
        var breakdown = calculation.Breakdown;
        var items = new List<CompanyGuarantorInvoiceItemResponse>();

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

                if (!invoiceItems.TryGetValue(calcItem.InvoiceItemId, out var invoiceItem)) continue;

                var patientAmount =
                    (calcItem.NetAmount - calcItem.ItemPrimaryAmount - calcItem.ItemUnresolvedAmount) +
                    (calcItem.TaxAmount - calcItem.TaxPrimaryAmount - calcItem.TaxUnresolvedAmount);

                items.Add(new CompanyGuarantorInvoiceItemResponse
                {
                    Kind = CompanyGuarantorInvoiceItemKinds.Item,
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
            items.Add(new CompanyGuarantorInvoiceItemResponse
            {
                Kind = CompanyGuarantorInvoiceItemKinds.AdministrationFee,
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
            items.Add(new CompanyGuarantorInvoiceItemResponse
            {
                Kind = CompanyGuarantorInvoiceItemKinds.RoomCharge,
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
