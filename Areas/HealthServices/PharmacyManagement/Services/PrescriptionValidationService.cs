using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionValidationService
    {
        private readonly ApplicationDbContext _dbContext;

        public PrescriptionValidationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ConsultationFinalizationIssueResponse>> ValidateForConsultationAsync(
            Guid consultationId,
            CancellationToken cancellationToken = default)
        {
            var issues = new List<ConsultationFinalizationIssueResponse>();

            var prescriptions = await _dbContext.Set<TrxPrescription>()
                .AsNoTracking()
                .Include(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .Include(x => x.Compounds.Where(c => !c.IsDelete && !c.IsCancel && c.IsActive))
                    .ThenInclude(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .Where(x => x.ConsultationId == consultationId && !x.IsDelete && !x.IsCancel && x.IsActive)
                .ToListAsync(cancellationToken);

            if (prescriptions.Count > 1)
            {
                issues.Add(Issue("MULTIPLE_ACTIVE_PRESCRIPTIONS", ConsultationValidationSeverity.Error,
                    "Terdapat lebih dari satu resep aktif pada konsultasi.", "Prescription", "prescription"));
            }

            foreach (var prescription in prescriptions)
            {
                ValidateHeader(prescription, issues);
                ValidateRegularItems(prescription, issues);
                ValidateCompounds(prescription, issues);
            }

            return issues;
        }

        private static void ValidateHeader(TrxPrescription prescription, List<ConsultationFinalizationIssueResponse> issues)
        {
            if (prescription.PrescriptionStatus != PrescriptionStatus.Draft)
                return;

            if (prescription.PaymentStatus != PrescriptionPaymentStatus.NotBilled)
            {
                issues.Add(Issue("PRESCRIPTION_BILLING_ALREADY_STARTED", ConsultationValidationSeverity.Error,
                    "Resep tidak dapat difinalkan karena proses billing sudah dimulai.", "Prescription", "prescription",
                    "PaymentStatus", "Prescription", prescription.Id));
            }

            if (prescription.FulfillmentStatus != PrescriptionFulfillmentStatus.WaitingForClinicalFinalization)
            {
                issues.Add(Issue("INVALID_PRESCRIPTION_FULFILLMENT_STATUS", ConsultationValidationSeverity.Error,
                    "Status pemenuhan resep tidak valid untuk finalisasi konsultasi.", "Prescription", "prescription",
                    "FulfillmentStatus", "Prescription", prescription.Id));
            }

            if (prescription.TotalItemCount <= 0)
            {
                issues.Add(Issue("EMPTY_PRESCRIPTION", ConsultationValidationSeverity.Error,
                    "Resep belum memiliki obat umum atau bahan racikan.", "Prescription", "prescription",
                    null, "Prescription", prescription.Id));
            }

            if (prescription.IsNeedApproval && !prescription.IsApproved)
            {
                issues.Add(Issue("PRESCRIPTION_APPROVAL_REQUIRED", ConsultationValidationSeverity.Error,
                    "Masih terdapat item resep yang membutuhkan approval.", "Prescription", "prescription",
                    null, "Prescription", prescription.Id));
            }
        }

        private static void ValidateRegularItems(TrxPrescription prescription, List<ConsultationFinalizationIssueResponse> issues)
        {
            var items = prescription.Items.ToList();

            foreach (var item in items)
            {
                if (item.Dose <= 0)
                    issues.Add(ItemIssue("INVALID_DRUG_DOSE", ConsultationValidationSeverity.Error, item, "Dose", "Dosis obat harus lebih dari 0."));
                if (!item.DoseUnitMeasurementId.HasValue)
                    issues.Add(ItemIssue("MISSING_DRUG_DOSE_UNIT", ConsultationValidationSeverity.Error, item, "DoseUnitMeasurementId", "Satuan dosis obat wajib diisi."));
                if (item.Quantity <= 0)
                    issues.Add(ItemIssue("INVALID_DRUG_QUANTITY", ConsultationValidationSeverity.Error, item, "Quantity", "Jumlah obat harus lebih dari 0."));
                if (!item.DispenseUnitMeasurementId.HasValue)
                    issues.Add(ItemIssue("MISSING_DISPENSE_UNIT", ConsultationValidationSeverity.Error, item, "DispenseUnitMeasurementId", "Satuan penyerahan obat wajib diisi."));
                if (string.IsNullOrWhiteSpace(item.Signa) && string.IsNullOrWhiteSpace(item.FrequencyText))
                    issues.Add(ItemIssue("MISSING_DRUG_USAGE", ConsultationValidationSeverity.Error, item, "Signa", "Signa atau frekuensi penggunaan obat wajib diisi."));
                if (!item.TariffId.HasValue)
                    issues.Add(ItemIssue("MISSING_DRUG_TARIFF", ConsultationValidationSeverity.Error, item, "TariffId", "Tarif obat tidak ditemukan."));
                if (item.IsNeedApproval && !item.IsApproved)
                    issues.Add(ItemIssue("UNAPPROVED_DRUG", ConsultationValidationSeverity.Error, item, null, $"Obat {item.DrugNameSnapshot} membutuhkan approval."));

                AddDrugWarnings(item.Id, item.DrugNameSnapshot, item.IsHighAlertSnapshot, item.IsAntibioticSnapshot,
                    item.IsNarcoticSnapshot, item.IsPsychotropicSnapshot, item.CoverageStatus, item.PatientPayAmount,
                    item.IsNeedGuaranteeLetter, "PrescriptionItem", issues);
            }

            foreach (var duplicate in items.GroupBy(x => x.DrugId).Where(x => x.Count() > 1))
            {
                var names = string.Join(", ", duplicate.Select(x => x.DrugNameSnapshot).Distinct());
                issues.Add(Issue("DUPLICATE_DRUG", ConsultationValidationSeverity.Warning,
                    $"Obat yang sama tercatat lebih dari satu kali: {names}.", "Prescription", "prescription",
                    null, "PrescriptionItem", duplicate.First().Id));
            }
        }

        private static void ValidateCompounds(TrxPrescription prescription, List<ConsultationFinalizationIssueResponse> issues)
        {
            foreach (var compound in prescription.Compounds)
            {
                if (string.IsNullOrWhiteSpace(compound.CompoundName))
                    issues.Add(CompoundIssue("MISSING_COMPOUND_NAME", ConsultationValidationSeverity.Error, compound, "CompoundName", "Nama racikan wajib diisi."));
                if (compound.TotalPackage <= 0)
                    issues.Add(CompoundIssue("INVALID_COMPOUND_PACKAGE", ConsultationValidationSeverity.Error, compound, "TotalPackage", "Jumlah bungkus racikan harus lebih dari 0."));
                if (compound.DosePerUse <= 0)
                    issues.Add(CompoundIssue("INVALID_COMPOUND_DOSE", ConsultationValidationSeverity.Error, compound, "DosePerUse", "Dosis pemakaian racikan harus lebih dari 0."));
                if (string.IsNullOrWhiteSpace(compound.Signa) && string.IsNullOrWhiteSpace(compound.FrequencyText))
                    issues.Add(CompoundIssue("MISSING_COMPOUND_USAGE", ConsultationValidationSeverity.Error, compound, "Signa", "Signa atau frekuensi racikan wajib diisi."));

                var ingredients = compound.Items.ToList();
                if (ingredients.Count == 0)
                    issues.Add(CompoundIssue("EMPTY_COMPOUND", ConsultationValidationSeverity.Error, compound, null, $"Racikan {compound.CompoundName} belum memiliki bahan."));

                foreach (var item in ingredients)
                {
                    if (item.AmountPerPackage <= 0)
                        issues.Add(CompoundItemIssue("INVALID_INGREDIENT_AMOUNT", ConsultationValidationSeverity.Error, item, "AmountPerPackage", "Jumlah bahan per bungkus harus lebih dari 0."));
                    if (item.TotalQuantity <= 0)
                        issues.Add(CompoundItemIssue("INVALID_INGREDIENT_QUANTITY", ConsultationValidationSeverity.Error, item, "TotalQuantity", "Total jumlah bahan racikan harus lebih dari 0."));
                    if (!item.QuantityUnitMeasurementId.HasValue)
                        issues.Add(CompoundItemIssue("MISSING_INGREDIENT_UNIT", ConsultationValidationSeverity.Error, item, "QuantityUnitMeasurementId", "Satuan bahan racikan wajib diisi."));
                    if (!item.TariffId.HasValue)
                        issues.Add(CompoundItemIssue("MISSING_INGREDIENT_TARIFF", ConsultationValidationSeverity.Error, item, "TariffId", "Tarif bahan racikan tidak ditemukan."));
                    if (item.IsNeedApproval && !item.IsApproved)
                        issues.Add(CompoundItemIssue("UNAPPROVED_INGREDIENT", ConsultationValidationSeverity.Error, item, null, $"Bahan {item.DrugNameSnapshot} membutuhkan approval."));

                    AddDrugWarnings(item.Id, item.DrugNameSnapshot, item.IsHighAlertSnapshot, item.IsAntibioticSnapshot,
                        item.IsNarcoticSnapshot, item.IsPsychotropicSnapshot, item.CoverageStatus, item.PatientPayAmount,
                        item.IsNeedGuaranteeLetter, "PrescriptionCompoundItem", issues);
                }

                foreach (var duplicate in ingredients.GroupBy(x => x.DrugId).Where(x => x.Count() > 1))
                {
                    issues.Add(Issue("DUPLICATE_COMPOUND_INGREDIENT", ConsultationValidationSeverity.Warning,
                        $"Bahan yang sama muncul lebih dari satu kali pada racikan {compound.CompoundName}.",
                        "Prescription", "prescription", null, "PrescriptionCompound", compound.Id));
                }
            }
        }

        private static void AddDrugWarnings(Guid id, string name, bool highAlert, bool antibiotic, bool narcotic, bool psychotropic,
            string coverageStatus, decimal patientPay, bool guaranteeLetter, string entityType,
            List<ConsultationFinalizationIssueResponse> issues)
        {
            if (highAlert) issues.Add(Issue("HIGH_ALERT_DRUG", ConsultationValidationSeverity.Warning, $"{name} merupakan obat high alert.", "Prescription", "prescription", null, entityType, id));
            if (antibiotic) issues.Add(Issue("ANTIBIOTIC_DRUG", ConsultationValidationSeverity.Warning, $"Pastikan indikasi dan durasi antibiotik {name} sudah sesuai.", "Prescription", "prescription", null, entityType, id));
            if (narcotic) issues.Add(Issue("NARCOTIC_DRUG", ConsultationValidationSeverity.Warning, $"{name} merupakan obat narkotika.", "Prescription", "prescription", null, entityType, id));
            if (psychotropic) issues.Add(Issue("PSYCHOTROPIC_DRUG", ConsultationValidationSeverity.Warning, $"{name} merupakan obat psikotropika.", "Prescription", "prescription", null, entityType, id));
            if (coverageStatus.Equals("NotCovered", StringComparison.OrdinalIgnoreCase))
                issues.Add(Issue("DRUG_NOT_COVERED", ConsultationValidationSeverity.Information, $"{name} tidak dicover asuransi.", "Prescription", "prescription", null, entityType, id));
            else if (coverageStatus.Equals("PartiallyCovered", StringComparison.OrdinalIgnoreCase) || patientPay > 0)
                issues.Add(Issue("DRUG_PARTIALLY_COVERED", ConsultationValidationSeverity.Warning, $"{name} memiliki tanggungan pasien.", "Prescription", "prescription", null, entityType, id));
            if (guaranteeLetter)
                issues.Add(Issue("GUARANTEE_LETTER_REQUIRED", ConsultationValidationSeverity.Information, $"{name} membutuhkan surat jaminan pada proses billing.", "Prescription", "prescription", null, entityType, id));
        }

        private static ConsultationFinalizationIssueResponse ItemIssue(string code, ConsultationValidationSeverity severity, TrxPrescriptionItem item, string? field, string message)
            => Issue(code, severity, message, "Prescription", "prescription", field, "PrescriptionItem", item.Id);
        private static ConsultationFinalizationIssueResponse CompoundIssue(string code, ConsultationValidationSeverity severity, TrxPrescriptionCompound item, string? field, string message)
            => Issue(code, severity, message, "Prescription", "prescription", field, "PrescriptionCompound", item.Id);
        private static ConsultationFinalizationIssueResponse CompoundItemIssue(string code, ConsultationValidationSeverity severity, TrxPrescriptionCompoundItem item, string? field, string message)
            => Issue(code, severity, message, "Prescription", "prescription", field, "PrescriptionCompoundItem", item.Id);

        private static ConsultationFinalizationIssueResponse Issue(string code, ConsultationValidationSeverity severity, string message,
            string section, string tabKey, string? field = null, string? entityType = null, Guid? entityId = null)
        {
            return new ConsultationFinalizationIssueResponse
            {
                Code = code,
                Severity = severity,
                Section = section,
                TabKey = tabKey,
                Field = field,
                Message = message,
                EntityType = entityType,
                EntityId = entityId,
                IssueKey = $"{code}:{entityType ?? section}:{entityId?.ToString() ?? "general"}"
            };
        }
    }
}
