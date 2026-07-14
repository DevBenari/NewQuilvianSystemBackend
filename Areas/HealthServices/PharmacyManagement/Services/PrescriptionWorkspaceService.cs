using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionWorkspaceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InsuranceCoverageService _insuranceCoverageService;
        private readonly PrescriptionAggregateService _aggregateService;

        public PrescriptionWorkspaceService(
            ApplicationDbContext dbContext,
            InsuranceCoverageService insuranceCoverageService,
            PrescriptionAggregateService aggregateService)
        {
            _dbContext = dbContext;
            _insuranceCoverageService = insuranceCoverageService;
            _aggregateService = aggregateService;
        }

        public async Task<PrescriptionWorkspaceResponse?> GetAsync(
            Guid prescriptionId,
            CancellationToken cancellationToken = default)
        {
            var entity = await BuildWorkspaceQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, cancellationToken);

            return entity == null ? null : MapWorkspace(entity);
        }

        public async Task<PrescriptionWorkspaceResponse?> GetByConsultationAsync(
            Guid consultationId,
            CancellationToken cancellationToken = default)
        {
            var entity = await BuildWorkspaceQuery()
                .AsNoTracking()
                .Where(x => x.ConsultationId == consultationId && !x.IsDelete && !x.IsCancel)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            return entity == null ? null : MapWorkspace(entity);
        }

        public async Task<AutosavePrescriptionWorkspaceResponse> AutosaveAsync(
            Guid prescriptionId,
            AutosavePrescriptionWorkspaceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await _aggregateService.EnsureEditableAsync(prescriptionId, cancellationToken);

            var prescription = await _dbContext.Set<TrxPrescription>()
                .FirstAsync(x => x.Id == prescriptionId && !x.IsDelete, cancellationToken);

            if (request.ExpectedUpdatedAt.HasValue &&
                prescription.UpdateDateTime.HasValue &&
                prescription.UpdateDateTime.Value != request.ExpectedUpdatedAt.Value)
            {
                throw new PrescriptionAutosaveConflictException(
                    "Data resep telah berubah di sesi lain. Muat ulang workspace sebelum menyimpan kembali.");
            }

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            prescription.PrescriptionDateTime = request.PrescriptionDateTime ?? prescription.PrescriptionDateTime;
            prescription.ClinicalNote = NormalizeText(request.ClinicalNote);
            prescription.DoctorInstruction = NormalizeText(request.DoctorInstruction);
            prescription.UpdateDateTime = now;
            prescription.UpdateBy = actorUserId;

            await RemoveEntitiesAsync(prescriptionId, request, actorUserId, now, cancellationToken);

            var itemResults = new List<AutosaveEntityIdResponse>();
            foreach (var itemRequest in request.Items)
            {
                var item = await UpsertRegularItemAsync(
                    prescription,
                    itemRequest,
                    actorUserId,
                    now,
                    cancellationToken);

                itemResults.Add(new AutosaveEntityIdResponse
                {
                    RequestId = itemRequest.Id,
                    SavedId = item.Id
                });
            }

            var compoundResults = new List<AutosaveCompoundIdResponse>();
            foreach (var compoundRequest in request.Compounds)
            {
                var compound = await UpsertCompoundAsync(
                    prescription,
                    compoundRequest,
                    actorUserId,
                    now,
                    cancellationToken);

                var compoundResult = new AutosaveCompoundIdResponse
                {
                    RequestId = compoundRequest.Id,
                    SavedId = compound.Id
                };

                foreach (var itemRequest in compoundRequest.Items)
                {
                    var item = await UpsertCompoundItemAsync(
                        prescription,
                        compound,
                        itemRequest,
                        actorUserId,
                        now,
                        cancellationToken);

                    compoundResult.ItemIds.Add(new AutosaveEntityIdResponse
                    {
                        RequestId = itemRequest.Id,
                        SavedId = item.Id
                    });
                }

                compoundResults.Add(compoundResult);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var aggregate = await _aggregateService.RebuildAsync(
                prescriptionId,
                actorUserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new AutosavePrescriptionWorkspaceResponse
            {
                PrescriptionId = prescriptionId,
                SavedAt = now,
                UpdatedAt = now,
                Summary = new PrescriptionWorkspaceSummaryResponse
                {
                    RegularItemCount = aggregate.RegularItemCount,
                    CompoundCount = aggregate.CompoundCount,
                    CompoundIngredientCount = aggregate.CompoundIngredientCount,
                    TotalItemCount = aggregate.TotalItemCount,
                    TotalPrice = aggregate.TotalPrice,
                    CoveredAmount = aggregate.CoveredAmount,
                    PatientPayAmount = aggregate.PatientPayAmount,
                    IsNeedApproval = aggregate.IsNeedApproval,
                    IsApproved = aggregate.IsApproved
                },
                ItemIds = itemResults,
                CompoundIds = compoundResults
            };
        }

        private IQueryable<TrxPrescription> BuildWorkspaceQuery()
        {
            return _dbContext.Set<TrxPrescription>()
                .Include(x => x.Encounter)
                .Include(x => x.Consultation)
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .Include(x => x.Compounds.Where(c => !c.IsDelete && !c.IsCancel && c.IsActive))
                    .ThenInclude(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive));
        }

        private async Task RemoveEntitiesAsync(
            Guid prescriptionId,
            AutosavePrescriptionWorkspaceRequest request,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (request.RemovedItemIds.Count > 0)
            {
                var items = await _dbContext.Set<TrxPrescriptionItem>()
                    .Where(x => request.RemovedItemIds.Contains(x.Id) &&
                                x.PrescriptionId == prescriptionId &&
                                !x.IsDelete)
                    .ToListAsync(cancellationToken);

                foreach (var item in items)
                    SoftDelete(item, actorUserId, now);
            }

            if (request.RemovedCompoundItemIds.Count > 0)
            {
                var items = await _dbContext.Set<TrxPrescriptionCompoundItem>()
                    .Include(x => x.PrescriptionCompound)
                    .Where(x => request.RemovedCompoundItemIds.Contains(x.Id) &&
                                x.PrescriptionCompound != null &&
                                x.PrescriptionCompound.PrescriptionId == prescriptionId &&
                                !x.IsDelete)
                    .ToListAsync(cancellationToken);

                foreach (var item in items)
                    SoftDelete(item, actorUserId, now);
            }

            if (request.RemovedCompoundIds.Count > 0)
            {
                var compounds = await _dbContext.Set<TrxPrescriptionCompound>()
                    .Include(x => x.Items)
                    .Where(x => request.RemovedCompoundIds.Contains(x.Id) &&
                                x.PrescriptionId == prescriptionId &&
                                !x.IsDelete)
                    .ToListAsync(cancellationToken);

                foreach (var compound in compounds)
                {
                    foreach (var item in compound.Items.Where(x => !x.IsDelete))
                        SoftDelete(item, actorUserId, now);
                    SoftDelete(compound, actorUserId, now);
                }
            }
        }

        private async Task<TrxPrescriptionItem> UpsertRegularItemAsync(
            TrxPrescription prescription,
            AutosavePrescriptionItemRequest request,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var drug = await GetDrugAsync(request.DrugId, false, cancellationToken);
            var doseUnit = await GetMeasurementAsync(request.DoseUnitMeasurementId, cancellationToken);
            var dispenseUnit = await GetMeasurementAsync(request.DispenseUnitMeasurementId, cancellationToken);
            var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                prescription.EncounterId,
                drug.Id,
                request.Quantity,
                prescription.PrescriptionDateTime,
                cancellationToken);

            if (!coverage.IsValid)
                throw new InvalidOperationException(coverage.ErrorMessage ?? "Coverage obat tidak dapat dihitung.");

            TrxPrescriptionItem entity;
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                entity = await _dbContext.Set<TrxPrescriptionItem>()
                    .FirstOrDefaultAsync(x => x.Id == request.Id.Value &&
                                              x.PrescriptionId == prescription.Id &&
                                              !x.IsDelete,
                        cancellationToken)
                    ?? throw new InvalidOperationException("Item resep tidak ditemukan.");
            }
            else
            {
                entity = new TrxPrescriptionItem
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescription.Id,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsActive = true
                };
                _dbContext.Set<TrxPrescriptionItem>().Add(entity);
            }

            ApplyDrugSnapshot(entity, drug);
            entity.DrugId = drug.Id;
            entity.Dose = request.Dose;
            entity.DoseUnitMeasurementId = request.DoseUnitMeasurementId;
            entity.DoseUnitNameSnapshot = doseUnit?.MeasurementName;
            entity.DoseUnitSymbolSnapshot = doseUnit?.MeasurementSymbol;
            entity.FrequencyCode = NormalizeText(request.FrequencyCode);
            entity.FrequencyText = NormalizeText(request.FrequencyText);
            entity.FrequencyPerDay = request.FrequencyPerDay;
            entity.DurationValue = request.DurationValue;
            entity.DurationUnit = NormalizeText(request.DurationUnit);
            entity.IsAsNeeded = request.IsAsNeeded;
            entity.AdministrationTime = NormalizeText(request.AdministrationTime);
            entity.Signa = NormalizeText(request.Signa);
            entity.AdministrationInstruction = NormalizeText(request.AdministrationInstruction);
            entity.DoctorNote = NormalizeText(request.DoctorNote);
            entity.Quantity = request.Quantity;
            entity.DispenseUnitMeasurementId = request.DispenseUnitMeasurementId;
            entity.DispenseUnitNameSnapshot = dispenseUnit?.MeasurementName;
            entity.DispenseUnitSymbolSnapshot = dispenseUnit?.MeasurementSymbol;
            entity.SortOrder = request.SortOrder;
            ApplyCoverage(entity, coverage);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.IsDelete = false;
            entity.IsCancel = false;
            return entity;
        }

        private async Task<TrxPrescriptionCompound> UpsertCompoundAsync(
            TrxPrescription prescription,
            AutosavePrescriptionCompoundRequest request,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var packageUnit = await GetMeasurementAsync(request.PackageUnitMeasurementId, cancellationToken);
            var doseUnit = await GetMeasurementAsync(request.DoseUnitMeasurementId, cancellationToken);

            TrxPrescriptionCompound entity;
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                entity = await _dbContext.Set<TrxPrescriptionCompound>()
                    .FirstOrDefaultAsync(x => x.Id == request.Id.Value &&
                                              x.PrescriptionId == prescription.Id &&
                                              !x.IsDelete,
                        cancellationToken)
                    ?? throw new InvalidOperationException("Racikan tidak ditemukan.");
            }
            else
            {
                entity = new TrxPrescriptionCompound
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescription.Id,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsActive = true
                };
                _dbContext.Set<TrxPrescriptionCompound>().Add(entity);
            }

            entity.CompoundName = request.CompoundName.Trim();
            entity.CompoundForm = NormalizeText(request.CompoundForm);
            entity.TotalPackage = request.TotalPackage;
            entity.PackageUnitMeasurementId = request.PackageUnitMeasurementId;
            entity.PackageUnitNameSnapshot = packageUnit?.MeasurementName;
            entity.PackageUnitSymbolSnapshot = packageUnit?.MeasurementSymbol;
            entity.DosePerUse = request.DosePerUse;
            entity.DoseUnitMeasurementId = request.DoseUnitMeasurementId;
            entity.DoseUnitNameSnapshot = doseUnit?.MeasurementName;
            entity.DoseUnitSymbolSnapshot = doseUnit?.MeasurementSymbol;
            entity.FrequencyCode = NormalizeText(request.FrequencyCode);
            entity.FrequencyText = NormalizeText(request.FrequencyText);
            entity.FrequencyPerDay = request.FrequencyPerDay;
            entity.DurationValue = request.DurationValue;
            entity.DurationUnit = NormalizeText(request.DurationUnit);
            entity.IsAsNeeded = request.IsAsNeeded;
            entity.AdministrationTime = NormalizeText(request.AdministrationTime);
            entity.Signa = NormalizeText(request.Signa);
            entity.CompoundingInstruction = NormalizeText(request.CompoundingInstruction);
            entity.AdministrationInstruction = NormalizeText(request.AdministrationInstruction);
            entity.DoctorNote = NormalizeText(request.DoctorNote);
            entity.SortOrder = request.SortOrder;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.IsDelete = false;
            entity.IsCancel = false;
            return entity;
        }

        private async Task<TrxPrescriptionCompoundItem> UpsertCompoundItemAsync(
            TrxPrescription prescription,
            TrxPrescriptionCompound compound,
            AutosavePrescriptionCompoundItemRequest request,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var drug = await GetDrugAsync(request.DrugId, true, cancellationToken);
            var quantityUnit = await GetMeasurementAsync(request.QuantityUnitMeasurementId, cancellationToken);
            var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                prescription.EncounterId,
                drug.Id,
                request.TotalQuantity,
                prescription.PrescriptionDateTime,
                cancellationToken);

            if (!coverage.IsValid)
                throw new InvalidOperationException(coverage.ErrorMessage ?? "Coverage bahan racikan tidak dapat dihitung.");

            TrxPrescriptionCompoundItem entity;
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                entity = await _dbContext.Set<TrxPrescriptionCompoundItem>()
                    .FirstOrDefaultAsync(x => x.Id == request.Id.Value &&
                                              x.PrescriptionCompoundId == compound.Id &&
                                              !x.IsDelete,
                        cancellationToken)
                    ?? throw new InvalidOperationException("Bahan racikan tidak ditemukan.");
            }
            else
            {
                var duplicate = await _dbContext.Set<TrxPrescriptionCompoundItem>()
                    .AnyAsync(x => x.PrescriptionCompoundId == compound.Id &&
                                   x.DrugId == drug.Id && !x.IsDelete && !x.IsCancel,
                        cancellationToken);
                if (duplicate)
                    throw new InvalidOperationException($"Obat {drug.DrugName} sudah ada pada racikan ini.");

                entity = new TrxPrescriptionCompoundItem
                {
                    Id = Guid.NewGuid(),
                    PrescriptionCompoundId = compound.Id,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsActive = true
                };
                _dbContext.Set<TrxPrescriptionCompoundItem>().Add(entity);
            }

            ApplyDrugSnapshot(entity, drug);
            entity.DrugId = drug.Id;
            entity.AmountPerPackage = request.AmountPerPackage;
            entity.TotalQuantity = request.TotalQuantity;
            entity.QuantityUnitMeasurementId = request.QuantityUnitMeasurementId;
            entity.QuantityUnitNameSnapshot = quantityUnit?.MeasurementName;
            entity.QuantityUnitSymbolSnapshot = quantityUnit?.MeasurementSymbol;
            entity.IngredientInstruction = NormalizeText(request.IngredientInstruction);
            entity.SortOrder = request.SortOrder;
            ApplyCoverage(entity, coverage);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.IsDelete = false;
            entity.IsCancel = false;
            return entity;
        }

        private async Task<MstDrug> GetDrugAsync(
            Guid drugId,
            bool mustAllowCompound,
            CancellationToken cancellationToken)
        {
            var drug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Include(x => x.DrugCategory)
                .FirstOrDefaultAsync(x => x.Id == drugId &&
                                          !x.IsDelete &&
                                          x.IsActive &&
                                          x.IsPrescribable,
                    cancellationToken)
                ?? throw new InvalidOperationException("Obat tidak ditemukan atau tidak dapat diresepkan.");

            if (mustAllowCompound && !drug.IsCompoundIngredientAllowed)
                throw new InvalidOperationException($"Obat {drug.DrugName} tidak diizinkan sebagai bahan racikan.");

            return drug;
        }

        private async Task<MstMeasurement?> GetMeasurementAsync(
            Guid? measurementId,
            CancellationToken cancellationToken)
        {
            if (!measurementId.HasValue || measurementId.Value == Guid.Empty)
                return null;

            return await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == measurementId.Value &&
                                          !x.IsDelete &&
                                          x.IsActive &&
                                          x.IsForDrug,
                    cancellationToken)
                ?? throw new InvalidOperationException("Satuan obat tidak ditemukan atau tidak aktif.");
        }

        private static void ApplyDrugSnapshot(TrxPrescriptionItem entity, MstDrug drug)
        {
            entity.DrugCodeSnapshot = drug.DrugCode;
            entity.DrugNameSnapshot = drug.DrugName;
            entity.GenericNameSnapshot = drug.GenericName;
            entity.DrugCategoryNameSnapshot = drug.DrugCategory?.DrugCategoryName;
            entity.DrugFormSnapshot = drug.DrugForm;
            entity.StrengthSnapshot = drug.Strength;
            entity.RouteSnapshot = drug.Route;
            entity.IsFormularySnapshot = drug.IsFormulary;
            entity.IsGenericSnapshot = drug.IsGeneric;
            entity.IsAntibioticSnapshot = drug.IsAntibiotic;
            entity.IsNarcoticSnapshot = drug.IsNarcotic;
            entity.IsPsychotropicSnapshot = drug.IsPsychotropic;
            entity.IsHighAlertSnapshot = drug.IsHighAlert;
        }

        private static void ApplyDrugSnapshot(TrxPrescriptionCompoundItem entity, MstDrug drug)
        {
            entity.DrugCodeSnapshot = drug.DrugCode;
            entity.DrugNameSnapshot = drug.DrugName;
            entity.GenericNameSnapshot = drug.GenericName;
            entity.DrugCategoryNameSnapshot = drug.DrugCategory?.DrugCategoryName;
            entity.DrugFormSnapshot = drug.DrugForm;
            entity.StrengthSnapshot = drug.Strength;
            entity.RouteSnapshot = drug.Route;
            entity.IsFormularySnapshot = drug.IsFormulary;
            entity.IsGenericSnapshot = drug.IsGeneric;
            entity.IsAntibioticSnapshot = drug.IsAntibiotic;
            entity.IsNarcoticSnapshot = drug.IsNarcotic;
            entity.IsPsychotropicSnapshot = drug.IsPsychotropic;
            entity.IsHighAlertSnapshot = drug.IsHighAlert;
        }

        private static void ApplyCoverage(TrxPrescriptionItem entity, InsuranceCoverageResult coverage)
        {
            entity.TariffId = coverage.TariffId;
            entity.InsuranceTariffId = coverage.InsuranceTariffId;
            entity.InsuranceCoverageRuleId = coverage.InsuranceCoverageRuleId;
            entity.HospitalUnitPrice = coverage.HospitalUnitPrice;
            entity.ContractUnitPrice = coverage.ContractUnitPrice;
            entity.UnitPrice = coverage.UnitPrice;
            entity.TotalPrice = coverage.TotalPrice;
            entity.PricingSource = coverage.PricingSource;
            entity.IsCoverageApplicable = coverage.IsCoverageApplicable;
            entity.IsCoveredByInsurance = coverage.IsCovered;
            entity.CoverageStatus = coverage.CoverageStatus;
            entity.CoveragePercent = coverage.CoveragePercent;
            entity.CoveredAmount = coverage.CoveredAmount;
            entity.PatientPayAmount = coverage.PatientPayAmount;
            entity.CoPaymentAmount = coverage.CoPaymentAmount;
            entity.IsNeedApproval = coverage.IsNeedApproval;
            if (!entity.IsNeedApproval) entity.IsApproved = false;
            entity.IsNeedGuaranteeLetter = coverage.IsNeedGuaranteeLetter;
            entity.IsAllowExcessPaymentByPatient = coverage.IsAllowExcessPaymentByPatient;
            entity.CoverageNote = coverage.CoverageNote;
        }

        private static void ApplyCoverage(TrxPrescriptionCompoundItem entity, InsuranceCoverageResult coverage)
        {
            entity.TariffId = coverage.TariffId;
            entity.InsuranceTariffId = coverage.InsuranceTariffId;
            entity.InsuranceCoverageRuleId = coverage.InsuranceCoverageRuleId;
            entity.HospitalUnitPrice = coverage.HospitalUnitPrice;
            entity.ContractUnitPrice = coverage.ContractUnitPrice;
            entity.UnitPrice = coverage.UnitPrice;
            entity.TotalPrice = coverage.TotalPrice;
            entity.PricingSource = coverage.PricingSource;
            entity.IsCoverageApplicable = coverage.IsCoverageApplicable;
            entity.IsCoveredByInsurance = coverage.IsCovered;
            entity.CoverageStatus = coverage.CoverageStatus;
            entity.CoveragePercent = coverage.CoveragePercent;
            entity.CoveredAmount = coverage.CoveredAmount;
            entity.PatientPayAmount = coverage.PatientPayAmount;
            entity.CoPaymentAmount = coverage.CoPaymentAmount;
            entity.IsNeedApproval = coverage.IsNeedApproval;
            if (!entity.IsNeedApproval) entity.IsApproved = false;
            entity.IsNeedGuaranteeLetter = coverage.IsNeedGuaranteeLetter;
            entity.IsAllowExcessPaymentByPatient = coverage.IsAllowExcessPaymentByPatient;
            entity.CoverageNote = coverage.CoverageNote;
        }

        private static void SoftDelete(IdentityModel entity, Guid actorUserId, DateTime now)
        {
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
        }

        private static PrescriptionWorkspaceResponse MapWorkspace(TrxPrescription x)
        {
            return new PrescriptionWorkspaceResponse
            {
                PrescriptionId = x.Id,
                PrescriptionNumber = x.PrescriptionNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter?.EncounterNumber ?? string.Empty,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation?.ConsultationNumber ?? string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = x.Patient?.MedicalRecordNumber ?? string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor?.FullName ?? string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit?.ServiceUnitName ?? string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic?.ClinicName,
                PaymentType = x.PaymentTypeSnapshot,
                PaymentTypeName = x.PaymentTypeSnapshot.ToString(),
                PaymentSourceName = x.PaymentSourceNameSnapshot,
                InsuranceProviderName = x.InsuranceProviderNameSnapshot,
                BenefitPlanName = x.BenefitPlanNameSnapshot,
                PatientClassName = x.PatientClassNameSnapshot,
                PrescriptionStatus = x.PrescriptionStatus,
                PaymentStatus = x.PaymentStatus,
                FulfillmentStatus = x.FulfillmentStatus,
                PrescriptionDateTime = x.PrescriptionDateTime,
                ClinicalNote = x.ClinicalNote,
                DoctorInstruction = x.DoctorInstruction,
                CanEdit = x.PrescriptionStatus == PrescriptionStatus.Draft &&
                          x.PaymentStatus == PrescriptionPaymentStatus.NotBilled &&
                          !x.IsCancel,
                LastUpdatedAt = x.UpdateDateTime,
                Consultation = new PrescriptionConsultationContextResponse
                {
                    ChiefComplaint = x.Consultation?.ChiefComplaint,
                    DiagnosisText = x.Consultation?.DiagnosisText,
                    PrimaryDiagnosisText = x.Consultation?.PrimaryDiagnosisText,
                    SecondaryDiagnosisText = x.Consultation?.SecondaryDiagnosisText,
                    Subjective = x.Consultation?.Subjective,
                    Objective = x.Consultation?.Objective,
                    Assessment = x.Consultation?.Assessment,
                    Plan = x.Consultation?.Plan,
                    Weight = x.Consultation?.Weight,
                    Height = x.Consultation?.Height,
                    BMI = x.Consultation?.BMI
                },
                Summary = new PrescriptionWorkspaceSummaryResponse
                {
                    RegularItemCount = x.RegularItemCount,
                    CompoundCount = x.CompoundCount,
                    CompoundIngredientCount = x.CompoundIngredientCount,
                    TotalItemCount = x.TotalItemCount,
                    TotalPrice = x.TotalPrice,
                    CoveredAmount = x.CoveredAmount,
                    PatientPayAmount = x.PatientPayAmount,
                    IsNeedApproval = x.IsNeedApproval,
                    IsApproved = x.IsApproved
                },
                Items = x.Items.OrderBy(i => i.SortOrder).ThenBy(i => i.CreateDateTime)
                    .Select(MapItem).ToList(),
                Compounds = x.Compounds.OrderBy(c => c.SortOrder).ThenBy(c => c.CreateDateTime)
                    .Select(MapCompound).ToList()
            };
        }

        private static PrescriptionWorkspaceItemResponse MapItem(TrxPrescriptionItem x) => new()
        {
            Id = x.Id,
            DrugId = x.DrugId,
            DrugCode = x.DrugCodeSnapshot,
            DrugName = x.DrugNameSnapshot,
            GenericName = x.GenericNameSnapshot,
            DrugForm = x.DrugFormSnapshot,
            Strength = x.StrengthSnapshot,
            Dose = x.Dose,
            DoseUnitMeasurementId = x.DoseUnitMeasurementId,
            DoseUnitName = x.DoseUnitNameSnapshot,
            DoseUnitSymbol = x.DoseUnitSymbolSnapshot,
            FrequencyCode = x.FrequencyCode,
            FrequencyText = x.FrequencyText,
            FrequencyPerDay = x.FrequencyPerDay,
            DurationValue = x.DurationValue,
            DurationUnit = x.DurationUnit,
            IsAsNeeded = x.IsAsNeeded,
            AdministrationTime = x.AdministrationTime,
            Signa = x.Signa,
            AdministrationInstruction = x.AdministrationInstruction,
            DoctorNote = x.DoctorNote,
            Quantity = x.Quantity,
            DispenseUnitMeasurementId = x.DispenseUnitMeasurementId,
            DispenseUnitName = x.DispenseUnitNameSnapshot,
            DispenseUnitSymbol = x.DispenseUnitSymbolSnapshot,
            TariffId = x.TariffId,
            InsuranceTariffId = x.InsuranceTariffId,
            InsuranceCoverageRuleId = x.InsuranceCoverageRuleId,
            PricingQuantity = x.Quantity,
            HospitalUnitPrice = x.HospitalUnitPrice,
            ContractUnitPrice = x.ContractUnitPrice,
            UnitPrice = x.UnitPrice,
            TotalPrice = x.TotalPrice,
            HospitalTotalPrice = Math.Round(
                x.HospitalUnitPrice * x.Quantity,
                2,
                MidpointRounding.AwayFromZero),
            PricingSource = x.PricingSource,
            IsCoverageApplicable = x.IsCoverageApplicable,
            IsCovered = x.IsCoveredByInsurance,
            CoverageStatus = x.CoverageStatus,
            CoveragePercent = x.CoveragePercent,
            CoveredAmount = x.CoveredAmount,
            PatientPayAmount = x.PatientPayAmount,
            CoPaymentAmount = x.CoPaymentAmount,
            IsNeedApproval = x.IsNeedApproval,
            IsApproved = x.IsApproved,
            IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
            CoverageNote = x.CoverageNote,
            SortOrder = x.SortOrder
        };

        private static PrescriptionWorkspaceCompoundResponse MapCompound(TrxPrescriptionCompound x) => new()
        {
            Id = x.Id,
            CompoundName = x.CompoundName,
            CompoundForm = x.CompoundForm,
            TotalPackage = x.TotalPackage,
            PackageUnitMeasurementId = x.PackageUnitMeasurementId,
            PackageUnitName = x.PackageUnitNameSnapshot,
            PackageUnitSymbol = x.PackageUnitSymbolSnapshot,
            DosePerUse = x.DosePerUse,
            DoseUnitMeasurementId = x.DoseUnitMeasurementId,
            DoseUnitName = x.DoseUnitNameSnapshot,
            DoseUnitSymbol = x.DoseUnitSymbolSnapshot,
            FrequencyCode = x.FrequencyCode,
            FrequencyText = x.FrequencyText,
            FrequencyPerDay = x.FrequencyPerDay,
            DurationValue = x.DurationValue,
            DurationUnit = x.DurationUnit,
            IsAsNeeded = x.IsAsNeeded,
            AdministrationTime = x.AdministrationTime,
            Signa = x.Signa,
            CompoundingInstruction = x.CompoundingInstruction,
            AdministrationInstruction = x.AdministrationInstruction,
            DoctorNote = x.DoctorNote,
            IngredientCount = x.IngredientCount,
            TotalPrice = x.TotalPrice,
            CoveredAmount = x.CoveredAmount,
            PatientPayAmount = x.PatientPayAmount,
            IsNeedApproval = x.IsNeedApproval,
            IsApproved = x.IsApproved,
            IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
            SortOrder = x.SortOrder,
            Items = x.Items.OrderBy(i => i.SortOrder).ThenBy(i => i.CreateDateTime)
                .Select(MapCompoundItem).ToList()
        };

        private static PrescriptionWorkspaceCompoundItemResponse MapCompoundItem(TrxPrescriptionCompoundItem x) => new()
        {
            Id = x.Id,
            DrugId = x.DrugId,
            DrugCode = x.DrugCodeSnapshot,
            DrugName = x.DrugNameSnapshot,
            GenericName = x.GenericNameSnapshot,
            DrugForm = x.DrugFormSnapshot,
            Strength = x.StrengthSnapshot,
            AmountPerPackage = x.AmountPerPackage,
            TotalQuantity = x.TotalQuantity,
            QuantityUnitMeasurementId = x.QuantityUnitMeasurementId,
            QuantityUnitName = x.QuantityUnitNameSnapshot,
            QuantityUnitSymbol = x.QuantityUnitSymbolSnapshot,
            IngredientInstruction = x.IngredientInstruction,
            TariffId = x.TariffId,
            InsuranceTariffId = x.InsuranceTariffId,
            InsuranceCoverageRuleId = x.InsuranceCoverageRuleId,
            PricingQuantity = x.TotalQuantity,
            HospitalUnitPrice = x.HospitalUnitPrice,
            ContractUnitPrice = x.ContractUnitPrice,
            UnitPrice = x.UnitPrice,
            TotalPrice = x.TotalPrice,
            HospitalTotalPrice = Math.Round(
                x.HospitalUnitPrice * x.TotalQuantity,
                2,
                MidpointRounding.AwayFromZero),
            PricingSource = x.PricingSource,
            IsCoverageApplicable = x.IsCoverageApplicable,
            IsCovered = x.IsCoveredByInsurance,
            CoverageStatus = x.CoverageStatus,
            CoveragePercent = x.CoveragePercent,
            CoveredAmount = x.CoveredAmount,
            PatientPayAmount = x.PatientPayAmount,
            CoPaymentAmount = x.CoPaymentAmount,
            IsNeedApproval = x.IsNeedApproval,
            IsApproved = x.IsApproved,
            IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
            CoverageNote = x.CoverageNote,
            SortOrder = x.SortOrder
        };

        private static string? NormalizeText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public class PrescriptionAutosaveConflictException : Exception
    {
        public PrescriptionAutosaveConflictException(string message) : base(message) { }
    }
}
