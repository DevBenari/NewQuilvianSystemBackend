using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionTemplateService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InsuranceCoverageService _coverageService;
        private readonly PrescriptionAggregateService _aggregateService;

        public PrescriptionTemplateService(
            ApplicationDbContext dbContext,
            InsuranceCoverageService coverageService,
            PrescriptionAggregateService aggregateService)
        {
            _dbContext = dbContext;
            _coverageService = coverageService;
            _aggregateService = aggregateService;
        }

        public async Task<MstPrescriptionTemplate> CreateAsync(
            CreatePrescriptionTemplateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await ValidateDoctorAsync(request.OwnerDoctorId, cancellationToken);
            await ValidateTemplateContentAsync(request.Items, request.Compounds, cancellationToken);

            var now = DateTime.UtcNow;
            var entity = new MstPrescriptionTemplate
            {
                Id = Guid.NewGuid(),
                TemplateCode = await GenerateCodeAsync(now, cancellationToken),
                TemplateName = request.TemplateName.Trim(),
                TemplateCategory = Normalize(request.TemplateCategory),
                Description = Normalize(request.Description),
                OwnerDoctorId = request.OwnerDoctorId,
                IsShared = request.IsShared,
                IsFavorite = request.IsFavorite,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            ApplyGraph(entity, request.Items, request.Compounds, actorUserId, now);
            RebuildCounts(entity);
            _dbContext.Set<MstPrescriptionTemplate>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<MstPrescriptionTemplate> UpdateAsync(
            Guid id,
            UpdatePrescriptionTemplateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstPrescriptionTemplate>()
                .Include(x => x.Items)
                .Include(x => x.Compounds).ThenInclude(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Template resep tidak ditemukan.");

            await ValidateDoctorAsync(request.OwnerDoctorId, cancellationToken);
            await ValidateTemplateContentAsync(request.Items, request.Compounds, cancellationToken);

            var now = DateTime.UtcNow;
            entity.TemplateName = request.TemplateName.Trim();
            entity.TemplateCategory = Normalize(request.TemplateCategory);
            entity.Description = Normalize(request.Description);
            entity.OwnerDoctorId = request.OwnerDoctorId;
            entity.IsShared = request.IsShared;
            entity.IsFavorite = request.IsFavorite;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            foreach (var item in entity.Items.Where(x => !x.IsDelete)) SoftDelete(item, actorUserId, now);
            foreach (var compound in entity.Compounds.Where(x => !x.IsDelete))
            {
                foreach (var item in compound.Items.Where(x => !x.IsDelete)) SoftDelete(item, actorUserId, now);
                SoftDelete(compound, actorUserId, now);
            }

            ApplyGraph(entity, request.Items, request.Compounds, actorUserId, now);
            RebuildCounts(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<MstPrescriptionTemplate> CreateFromPrescriptionAsync(
            CreateTemplateFromPrescriptionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var prescription = await _dbContext.Set<TrxPrescription>()
                .Include(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .Include(x => x.Compounds.Where(c => !c.IsDelete && !c.IsCancel && c.IsActive))
                    .ThenInclude(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .FirstOrDefaultAsync(x => x.Id == request.PrescriptionId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Resep tidak ditemukan.");

            var dto = new CreatePrescriptionTemplateRequest
            {
                TemplateName = request.TemplateName,
                TemplateCategory = request.TemplateCategory,
                Description = request.Description,
                OwnerDoctorId = prescription.DoctorId,
                IsShared = request.IsShared,
                IsFavorite = request.IsFavorite,
                Items = prescription.Items.OrderBy(x => x.SortOrder).Select(x => new PrescriptionTemplateItemRequest
                {
                    DrugId = x.DrugId,
                    Dose = x.Dose,
                    DoseUnitMeasurementId = x.DoseUnitMeasurementId,
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
                    SortOrder = x.SortOrder
                }).ToList(),
                Compounds = prescription.Compounds.OrderBy(x => x.SortOrder).Select(x => new PrescriptionTemplateCompoundRequest
                {
                    CompoundName = x.CompoundName,
                    CompoundForm = x.CompoundForm,
                    TotalPackage = x.TotalPackage,
                    PackageUnitMeasurementId = x.PackageUnitMeasurementId,
                    DosePerUse = x.DosePerUse,
                    DoseUnitMeasurementId = x.DoseUnitMeasurementId,
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
                    SortOrder = x.SortOrder,
                    Items = x.Items.OrderBy(i => i.SortOrder).Select(i => new PrescriptionTemplateCompoundItemRequest
                    {
                        DrugId = i.DrugId,
                        AmountPerPackage = i.AmountPerPackage,
                        TotalQuantity = i.TotalQuantity,
                        QuantityUnitMeasurementId = i.QuantityUnitMeasurementId,
                        IngredientInstruction = i.IngredientInstruction,
                        SortOrder = i.SortOrder
                    }).ToList()
                }).ToList()
            };

            return await CreateAsync(dto, actorUserId, cancellationToken);
        }

        public async Task<ApplyPrescriptionTemplateResponse> ApplyAsync(
            Guid templateId,
            ApplyPrescriptionTemplateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await _aggregateService.EnsureEditableAsync(request.PrescriptionId, cancellationToken);

            var template = await _dbContext.Set<MstPrescriptionTemplate>()
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.Drug)
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.DoseUnitMeasurement)
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.DispenseUnitMeasurement)
                .Include(x => x.Compounds.Where(c => !c.IsDelete && c.IsActive)).ThenInclude(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.Drug)
                .FirstOrDefaultAsync(x => x.Id == templateId && x.IsActive && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Template resep tidak ditemukan atau tidak aktif.");

            var prescription = await _dbContext.Set<TrxPrescription>()
                .FirstAsync(x => x.Id == request.PrescriptionId && !x.IsDelete, cancellationToken);

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (request.ReplaceExisting)
                await SoftDeletePrescriptionContentAsync(prescription.Id, actorUserId, now, cancellationToken);

            foreach (var source in template.Items.OrderBy(x => x.SortOrder))
            {
                var drug = source.Drug ?? throw new InvalidOperationException("Obat pada template tidak ditemukan.");
                var coverage = await _coverageService.ResolveDrugAsync(prescription.EncounterId, drug.Id, source.Quantity, prescription.PrescriptionDateTime, cancellationToken);
                if (!coverage.IsValid) throw new InvalidOperationException(coverage.ErrorMessage ?? "Coverage obat template gagal dihitung.");
                var item = new TrxPrescriptionItem { Id = Guid.NewGuid(), PrescriptionId = prescription.Id, DrugId = drug.Id, CreateDateTime = now, CreateBy = actorUserId, IsActive = true };
                CopyTemplateItem(item, source, drug, coverage);
                _dbContext.Set<TrxPrescriptionItem>().Add(item);
            }

            foreach (var source in template.Compounds.OrderBy(x => x.SortOrder))
            {
                var compound = new TrxPrescriptionCompound
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescription.Id,
                    CompoundName = source.CompoundName,
                    CompoundForm = source.CompoundForm,
                    TotalPackage = source.TotalPackage,
                    PackageUnitMeasurementId = source.PackageUnitMeasurementId,
                    DosePerUse = source.DosePerUse,
                    DoseUnitMeasurementId = source.DoseUnitMeasurementId,
                    FrequencyCode = source.FrequencyCode,
                    FrequencyText = source.FrequencyText,
                    FrequencyPerDay = source.FrequencyPerDay,
                    DurationValue = source.DurationValue,
                    DurationUnit = source.DurationUnit,
                    IsAsNeeded = source.IsAsNeeded,
                    AdministrationTime = source.AdministrationTime,
                    Signa = source.Signa,
                    CompoundingInstruction = source.CompoundingInstruction,
                    AdministrationInstruction = source.AdministrationInstruction,
                    DoctorNote = source.DoctorNote,
                    SortOrder = source.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.Set<TrxPrescriptionCompound>().Add(compound);

                foreach (var sourceItem in source.Items.OrderBy(x => x.SortOrder))
                {
                    var drug = sourceItem.Drug ?? await _dbContext.Set<MstDrug>().FirstAsync(x => x.Id == sourceItem.DrugId, cancellationToken);
                    var coverage = await _coverageService.ResolveDrugAsync(prescription.EncounterId, drug.Id, sourceItem.TotalQuantity, prescription.PrescriptionDateTime, cancellationToken);
                    if (!coverage.IsValid) throw new InvalidOperationException(coverage.ErrorMessage ?? "Coverage bahan racikan gagal dihitung.");
                    var item = new TrxPrescriptionCompoundItem { Id = Guid.NewGuid(), PrescriptionCompoundId = compound.Id, DrugId = drug.Id, CreateDateTime = now, CreateBy = actorUserId, IsActive = true };
                    CopyTemplateCompoundItem(item, sourceItem, drug, coverage);
                    _dbContext.Set<TrxPrescriptionCompoundItem>().Add(item);
                }
            }

            template.UsageCount += 1;
            template.LastUsedAt = now;
            template.UpdateDateTime = now;
            template.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            var aggregate = await _aggregateService.RebuildAsync(prescription.Id, actorUserId, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ApplyPrescriptionTemplateResponse
            {
                TemplateId = template.Id,
                PrescriptionId = prescription.Id,
                AddedRegularItemCount = template.RegularItemCount,
                AddedCompoundCount = template.CompoundCount,
                AddedCompoundIngredientCount = template.CompoundIngredientCount,
                TotalPrice = aggregate.TotalPrice,
                CoveredAmount = aggregate.CoveredAmount,
                PatientPayAmount = aggregate.PatientPayAmount,
                IsNeedApproval = aggregate.IsNeedApproval,
                IsApproved = aggregate.IsApproved
            };
        }

        private async Task ValidateDoctorAsync(Guid doctorId, CancellationToken ct)
        {
            if (!await _dbContext.Set<MstDoctor>().AnyAsync(x => x.Id == doctorId && x.IsActive && !x.IsDelete, ct))
                throw new InvalidOperationException("Dokter pemilik template tidak ditemukan atau tidak aktif.");
        }

        private async Task ValidateTemplateContentAsync(List<PrescriptionTemplateItemRequest> items, List<PrescriptionTemplateCompoundRequest> compounds, CancellationToken ct)
        {
            var drugIds = items.Select(x => x.DrugId).Concat(compounds.SelectMany(x => x.Items.Select(i => i.DrugId))).Distinct().ToList();
            var validCount = await _dbContext.Set<MstDrug>().CountAsync(x => drugIds.Contains(x.Id) && x.IsActive && x.IsPrescribable && !x.IsDelete, ct);
            if (validCount != drugIds.Count) throw new InvalidOperationException("Terdapat obat template yang tidak ditemukan, tidak aktif, atau tidak dapat diresepkan.");
            if (compounds.Any(c => c.Items.Count == 0)) throw new InvalidOperationException("Setiap racikan template wajib memiliki minimal satu bahan.");
        }

        private void ApplyGraph(MstPrescriptionTemplate template, List<PrescriptionTemplateItemRequest> items, List<PrescriptionTemplateCompoundRequest> compounds, Guid actor, DateTime now)
        {
            foreach (var x in items) template.Items.Add(new MstPrescriptionTemplateItem
            {
                Id = Guid.NewGuid(),
                PrescriptionTemplateId = template.Id,
                DrugId = x.DrugId,
                Dose = x.Dose,
                DoseUnitMeasurementId = x.DoseUnitMeasurementId,
                FrequencyCode = Normalize(x.FrequencyCode),
                FrequencyText = Normalize(x.FrequencyText),
                FrequencyPerDay = x.FrequencyPerDay,
                DurationValue = x.DurationValue,
                DurationUnit = Normalize(x.DurationUnit),
                IsAsNeeded = x.IsAsNeeded,
                AdministrationTime = Normalize(x.AdministrationTime),
                Signa = Normalize(x.Signa),
                AdministrationInstruction = Normalize(x.AdministrationInstruction),
                DoctorNote = Normalize(x.DoctorNote),
                Quantity = x.Quantity,
                DispenseUnitMeasurementId = x.DispenseUnitMeasurementId,
                SortOrder = x.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor
            });
            foreach (var x in compounds)
            {
                var c = new MstPrescriptionTemplateCompound
                {
                    Id = Guid.NewGuid(),
                    PrescriptionTemplateId = template.Id,
                    CompoundName = x.CompoundName.Trim(),
                    CompoundForm = Normalize(x.CompoundForm),
                    TotalPackage = x.TotalPackage,
                    PackageUnitMeasurementId = x.PackageUnitMeasurementId,
                    DosePerUse = x.DosePerUse,
                    DoseUnitMeasurementId = x.DoseUnitMeasurementId,
                    FrequencyCode = Normalize(x.FrequencyCode),
                    FrequencyText = Normalize(x.FrequencyText),
                    FrequencyPerDay = x.FrequencyPerDay,
                    DurationValue = x.DurationValue,
                    DurationUnit = Normalize(x.DurationUnit),
                    IsAsNeeded = x.IsAsNeeded,
                    AdministrationTime = Normalize(x.AdministrationTime),
                    Signa = Normalize(x.Signa),
                    CompoundingInstruction = Normalize(x.CompoundingInstruction),
                    AdministrationInstruction = Normalize(x.AdministrationInstruction),
                    DoctorNote = Normalize(x.DoctorNote),
                    SortOrder = x.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actor
                };
                foreach (var i in x.Items) c.Items.Add(new MstPrescriptionTemplateCompoundItem
                {
                    Id = Guid.NewGuid(),
                    PrescriptionTemplateCompoundId = c.Id,
                    DrugId = i.DrugId,
                    AmountPerPackage = i.AmountPerPackage,
                    TotalQuantity = i.TotalQuantity,
                    QuantityUnitMeasurementId = i.QuantityUnitMeasurementId,
                    IngredientInstruction = Normalize(i.IngredientInstruction),
                    SortOrder = i.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actor
                });
                template.Compounds.Add(c);
            }
        }

        private static void RebuildCounts(MstPrescriptionTemplate x)
        {
            x.RegularItemCount = x.Items.Count(i => !i.IsDelete && i.IsActive);
            x.CompoundCount = x.Compounds.Count(c => !c.IsDelete && c.IsActive);
            x.CompoundIngredientCount = x.Compounds.Where(c => !c.IsDelete && c.IsActive).Sum(c => c.Items.Count(i => !i.IsDelete && i.IsActive));
            x.TotalItemCount = x.RegularItemCount + x.CompoundIngredientCount;
        }

        private async Task<string> GenerateCodeAsync(DateTime now, CancellationToken ct)
        {
            var prefix = $"RXT-{now:yyyyMMdd}";
            var count = await _dbContext.Set<MstPrescriptionTemplate>().CountAsync(x => x.TemplateCode.StartsWith(prefix), ct);
            return $"{prefix}-{count + 1:D5}";
        }

        private async Task SoftDeletePrescriptionContentAsync(Guid prescriptionId, Guid actor, DateTime now, CancellationToken ct)
        {
            var items = await _dbContext.Set<TrxPrescriptionItem>().Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete).ToListAsync(ct);
            foreach (var x in items) SoftDelete(x, actor, now);
            var compounds = await _dbContext.Set<TrxPrescriptionCompound>().Include(x => x.Items).Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete).ToListAsync(ct);
            foreach (var c in compounds) { foreach (var i in c.Items.Where(x => !x.IsDelete)) SoftDelete(i, actor, now); SoftDelete(c, actor, now); }
        }

        private static void CopyTemplateItem(TrxPrescriptionItem e, MstPrescriptionTemplateItem s, MstDrug d, InsuranceCoverageResult c)
        {
            e.DrugCodeSnapshot = d.DrugCode; e.DrugNameSnapshot = d.DrugName; e.GenericNameSnapshot = d.GenericName; e.DrugCategoryNameSnapshot = d.DrugCategory?.DrugCategoryName;
            e.DrugFormSnapshot = d.DrugForm; e.StrengthSnapshot = d.Strength; e.RouteSnapshot = d.Route; e.IsFormularySnapshot = d.IsFormulary; e.IsGenericSnapshot = d.IsGeneric;
            e.IsAntibioticSnapshot = d.IsAntibiotic; e.IsNarcoticSnapshot = d.IsNarcotic; e.IsPsychotropicSnapshot = d.IsPsychotropic; e.IsHighAlertSnapshot = d.IsHighAlert;
            e.Dose = s.Dose; e.DoseUnitMeasurementId = s.DoseUnitMeasurementId; e.DoseUnitNameSnapshot = s.DoseUnitMeasurement?.MeasurementName; e.DoseUnitSymbolSnapshot = s.DoseUnitMeasurement?.MeasurementSymbol;
            e.FrequencyCode = s.FrequencyCode; e.FrequencyText = s.FrequencyText; e.FrequencyPerDay = s.FrequencyPerDay; e.DurationValue = s.DurationValue; e.DurationUnit = s.DurationUnit;
            e.IsAsNeeded = s.IsAsNeeded; e.AdministrationTime = s.AdministrationTime; e.Signa = s.Signa; e.AdministrationInstruction = s.AdministrationInstruction; e.DoctorNote = s.DoctorNote;
            e.Quantity = s.Quantity; e.DispenseUnitMeasurementId = s.DispenseUnitMeasurementId; e.DispenseUnitNameSnapshot = s.DispenseUnitMeasurement?.MeasurementName; e.DispenseUnitSymbolSnapshot = s.DispenseUnitMeasurement?.MeasurementSymbol; e.SortOrder = s.SortOrder;
            ApplyCoverage(e, c);
        }

        private static void CopyTemplateCompoundItem(TrxPrescriptionCompoundItem e, MstPrescriptionTemplateCompoundItem s, MstDrug d, InsuranceCoverageResult c)
        {
            e.DrugCodeSnapshot = d.DrugCode; e.DrugNameSnapshot = d.DrugName; e.GenericNameSnapshot = d.GenericName; e.DrugCategoryNameSnapshot = d.DrugCategory?.DrugCategoryName;
            e.DrugFormSnapshot = d.DrugForm; e.StrengthSnapshot = d.Strength; e.RouteSnapshot = d.Route; e.IsFormularySnapshot = d.IsFormulary; e.IsGenericSnapshot = d.IsGeneric;
            e.IsAntibioticSnapshot = d.IsAntibiotic; e.IsNarcoticSnapshot = d.IsNarcotic; e.IsPsychotropicSnapshot = d.IsPsychotropic; e.IsHighAlertSnapshot = d.IsHighAlert;
            e.AmountPerPackage = s.AmountPerPackage; e.TotalQuantity = s.TotalQuantity; e.QuantityUnitMeasurementId = s.QuantityUnitMeasurementId;
            e.QuantityUnitNameSnapshot = s.QuantityUnitMeasurement?.MeasurementName; e.QuantityUnitSymbolSnapshot = s.QuantityUnitMeasurement?.MeasurementSymbol;
            e.IngredientInstruction = s.IngredientInstruction; e.SortOrder = s.SortOrder; ApplyCoverage(e, c);
        }

        private static void ApplyCoverage(TrxPrescriptionItem e, InsuranceCoverageResult c)
        { e.TariffId = c.TariffId; e.InsuranceTariffId = c.InsuranceTariffId; e.InsuranceCoverageRuleId = c.InsuranceCoverageRuleId; e.HospitalUnitPrice = c.HospitalUnitPrice; e.ContractUnitPrice = c.ContractUnitPrice; e.UnitPrice = c.UnitPrice; e.TotalPrice = c.TotalPrice; e.PricingSource = c.PricingSource; e.IsCoverageApplicable = c.IsCoverageApplicable; e.IsCoveredByInsurance = c.IsCovered; e.CoverageStatus = c.CoverageStatus; e.CoveragePercent = c.CoveragePercent; e.CoveredAmount = c.CoveredAmount; e.PatientPayAmount = c.PatientPayAmount; e.CoPaymentAmount = c.CoPaymentAmount; e.IsNeedApproval = c.IsNeedApproval; e.IsApproved = !c.IsNeedApproval; e.IsNeedGuaranteeLetter = c.IsNeedGuaranteeLetter; e.IsAllowExcessPaymentByPatient = c.IsAllowExcessPaymentByPatient; e.CoverageNote = c.CoverageNote; }
        private static void ApplyCoverage(TrxPrescriptionCompoundItem e, InsuranceCoverageResult c)
        { e.TariffId = c.TariffId; e.InsuranceTariffId = c.InsuranceTariffId; e.InsuranceCoverageRuleId = c.InsuranceCoverageRuleId; e.HospitalUnitPrice = c.HospitalUnitPrice; e.ContractUnitPrice = c.ContractUnitPrice; e.UnitPrice = c.UnitPrice; e.TotalPrice = c.TotalPrice; e.PricingSource = c.PricingSource; e.IsCoverageApplicable = c.IsCoverageApplicable; e.IsCoveredByInsurance = c.IsCovered; e.CoverageStatus = c.CoverageStatus; e.CoveragePercent = c.CoveragePercent; e.CoveredAmount = c.CoveredAmount; e.PatientPayAmount = c.PatientPayAmount; e.CoPaymentAmount = c.CoPaymentAmount; e.IsNeedApproval = c.IsNeedApproval; e.IsApproved = !c.IsNeedApproval; e.IsNeedGuaranteeLetter = c.IsNeedGuaranteeLetter; e.IsAllowExcessPaymentByPatient = c.IsAllowExcessPaymentByPatient; e.CoverageNote = c.CoverageNote; }

        private static void SoftDelete(IdentityModel x, Guid actor, DateTime now) { x.IsDelete = true; x.DeleteDateTime = now; x.DeleteBy = actor; x.UpdateDateTime = now; x.UpdateBy = actor; }
        private static string? Normalize(string? x) => string.IsNullOrWhiteSpace(x) ? null : x.Trim();
    }
}
