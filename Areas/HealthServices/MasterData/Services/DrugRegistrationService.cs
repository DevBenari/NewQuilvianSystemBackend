using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik validasi dan pembuatan data induk obat. Controller hanya meneruskan kontrak API;
    /// service ini yang mengatur persistence dan meminta kode obat dari provider nomor bersama.
    /// </summary>
    public class DrugRegistrationService
    {
        private const string DrugSequenceKey = "MST_DRUG";
        private const string DrugNumberPrefix = "DRG-RSMMC";
        private const string LegacyCodePrefix = DrugNumberPrefix + "-";
        private const int DrugSequenceDigits = 5;

        private static readonly HashSet<string> DrugFormOptions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Tablet",
            "Capsule",
            "Syrup",
            "Injection",
            "Cream",
            "Drop",
            "Inhaler",
            "Other"
        };

        private static readonly HashSet<string> RouteOptions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Oral",
            "IV",
            "IM",
            "SC",
            "Topical",
            "Inhalation",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public DrugRegistrationService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        public async Task<DrugRegistrationResult> CreateAsync(
            CreateDrugRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return Failed(validation.ErrorMessage ?? "Data drug tidak valid.");
            }

            string drugCode;

            try
            {
                var legacyHighWatermark = await GetLegacyHighWatermarkAsync(cancellationToken);

                drugCode = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: DrugSequenceKey,
                        Prefix: DrugNumberPrefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: DrugSequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow,
                        MinimumValue: legacyHighWatermark),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return Failed("Kode obat gagal diterbitkan. Hubungi administrator sistem.");
            }

            var now = DateTime.UtcNow;
            var entity = new MstDrug
            {
                Id = Guid.NewGuid(),
                DrugCategoryId = request.DrugCategoryId,
                DrugCode = drugCode,
                DrugName = request.DrugName.Trim(),
                GenericName = NormalizeNullableString(request.GenericName),
                BrandName = NormalizeNullableString(request.BrandName),
                ManufacturerName = NormalizeNullableString(request.ManufacturerName),
                DrugForm = NormalizeDrugForm(request.DrugForm),
                Strength = NormalizeNullableString(request.Strength),
                StrengthValue = request.StrengthValue,
                StrengthMeasurementId = NormalizeNullableGuid(request.StrengthMeasurementId),
                BaseUnitMeasurementId = NormalizeNullableGuid(request.BaseUnitMeasurementId),
                DispenseUnitMeasurementId = NormalizeNullableGuid(request.DispenseUnitMeasurementId),
                PurchaseUnitMeasurementId = NormalizeNullableGuid(request.PurchaseUnitMeasurementId),
                StockUnitMeasurementId = NormalizeNullableGuid(request.StockUnitMeasurementId),
                DefaultDoseUnitMeasurementId = NormalizeNullableGuid(request.DefaultDoseUnitMeasurementId),
                Route = NormalizeRoute(request.Route),
                IsFormulary = request.IsFormulary,
                IsGeneric = request.IsGeneric,
                IsAntibiotic = request.IsAntibiotic,
                IsNarcotic = request.IsNarcotic,
                IsPsychotropic = request.IsPsychotropic,
                IsHighAlert = request.IsHighAlert,
                IsChronicDiseaseDrug = request.IsChronicDiseaseDrug,
                IsVaccine = request.IsVaccine,
                IsConsumable = request.IsConsumable,
                IsCompoundIngredientAllowed = request.IsCompoundIngredientAllowed,
                IsStockManaged = request.IsStockManaged,
                IsBatchTracked = request.IsBatchTracked,
                IsExpiryDateTracked = request.IsExpiryDateTracked,
                IsAllowFractionalDispense = request.IsAllowFractionalDispense,
                IsNeedPrescription = request.IsNeedPrescription,
                IsPrescribable = request.IsPrescribable,
                IsNeedApproval = request.IsNeedApproval,
                Indication = NormalizeClinicalText(request.Indication),
                Contraindication = NormalizeClinicalText(request.Contraindication),
                SideEffect = NormalizeClinicalText(request.SideEffect),
                WarningPrecaution = NormalizeClinicalText(request.WarningPrecaution),
                DosageInformation = NormalizeClinicalText(request.DosageInformation),
                DrugInteraction = NormalizeClinicalText(request.DrugInteraction),
                AdministrationInstruction = NormalizeClinicalText(request.AdministrationInstruction),
                StorageInstruction = NormalizeClinicalText(request.StorageInstruction),
                PregnancyCategory = NormalizeClinicalText(request.PregnancyCategory),
                LactationNote = NormalizeClinicalText(request.LactationNote),
                PediatricNote = NormalizeClinicalText(request.PediatricNote),
                GeriatricNote = NormalizeClinicalText(request.GeriatricNote),
                ExternalDrugCode = NormalizeNullableString(request.ExternalDrugCode),
                IntegrationCode = NormalizeNullableString(request.IntegrationCode),
                BpomRegistrationNumber = NormalizeNullableString(request.BpomRegistrationNumber),
                NationalDrugCode = NormalizeNullableString(request.NationalDrugCode),
                SortOrder = request.SortOrder,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            _dbContext.Set<MstDrug>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new DrugRegistrationResult(
                DrugRegistrationStatus.Success,
                entity,
                "Drug berhasil dibuat.");
        }

        public Task<DrugRegistrationResult> RegisterNonFormularyAsync(
            CreateNonFormularyDrugRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // VAL-DOK-53b.
            if (request.DrugCategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.DrugName))
            {
                return Task.FromResult(Failed("Nama dan kategori obat wajib diisi."));
            }

            var baseUnit = NormalizeNullableGuid(request.BaseUnitMeasurementId);
            var dispenseUnit = NormalizeNullableGuid(request.DispenseUnitMeasurementId);
            var doseUnit = NormalizeNullableGuid(request.DefaultDoseUnitMeasurementId);

            return CreateAsync(
                new CreateDrugRequest
                {
                    DrugCategoryId = request.DrugCategoryId,
                    DrugName = request.DrugName.Trim(),
                    GenericName = request.GenericName,
                    DrugForm = request.DrugForm,
                    Strength = request.Strength,
                    BaseUnitMeasurementId = baseUnit,
                    DispenseUnitMeasurementId = dispenseUnit,
                    DefaultDoseUnitMeasurementId = doseUnit,
                    // RWI-DEC-134 butir (2). Nilai ini ditentukan server, bukan client.
                    IsFormulary = false,
                    IsPrescribable = baseUnit.HasValue && dispenseUnit.HasValue && doseUnit.HasValue
                },
                actorUserId,
                cancellationToken);
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateDrugRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.DrugCategoryId == Guid.Empty)
                return (false, "Drug category wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.DrugName))
                return (false, "Nama drug wajib diisi.");

            if (request.StrengthValue.HasValue && request.StrengthValue.Value < 0)
                return (false, "Nilai strength tidak boleh kurang dari 0.");

            if (!string.IsNullOrWhiteSpace(request.DrugForm)
                && !DrugFormOptions.Contains(request.DrugForm.Trim()))
            {
                return (false, "Drug form tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!string.IsNullOrWhiteSpace(request.Route)
                && !RouteOptions.Contains(request.Route.Trim()))
            {
                return (false, "Route tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            var categoryExists = await _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.DrugCategoryId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (!categoryExists)
                return (false, "Drug category tidak ditemukan atau tidak aktif.");

            if (request.IsPrescribable)
            {
                if (!request.BaseUnitMeasurementId.HasValue || request.BaseUnitMeasurementId.Value == Guid.Empty)
                    return (false, "Base unit measurement wajib dipilih untuk obat yang dapat diresepkan.");

                if (!request.DispenseUnitMeasurementId.HasValue || request.DispenseUnitMeasurementId.Value == Guid.Empty)
                    return (false, "Dispense unit measurement wajib dipilih untuk obat yang dapat diresepkan.");

                if (!request.DefaultDoseUnitMeasurementId.HasValue || request.DefaultDoseUnitMeasurementId.Value == Guid.Empty)
                    return (false, "Default dose unit measurement wajib dipilih untuk obat yang dapat diresepkan.");
            }

            if (request.StrengthValue.HasValue
                && request.StrengthValue.Value > 0
                && (!request.StrengthMeasurementId.HasValue
                    || request.StrengthMeasurementId.Value == Guid.Empty))
            {
                return (false, "Strength measurement wajib dipilih jika strength value diisi.");
            }

            var measurementIds = new List<Guid?>
                {
                    request.StrengthMeasurementId,
                    request.BaseUnitMeasurementId,
                    request.DispenseUnitMeasurementId,
                    request.PurchaseUnitMeasurementId,
                    request.StockUnitMeasurementId,
                    request.DefaultDoseUnitMeasurementId
                }
                .Where(x => x.HasValue && x.Value != Guid.Empty)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            if (measurementIds.Count > 0)
            {
                var validMeasurementCount = await _dbContext.Set<MstMeasurement>()
                    .AsNoTracking()
                    .CountAsync(
                        x => measurementIds.Contains(x.Id)
                            && !x.IsDelete
                            && x.IsActive
                            && x.IsForDrug,
                        cancellationToken);

                if (validMeasurementCount != measurementIds.Count)
                {
                    return (false, "Measurement yang dipilih tidak valid, tidak aktif, atau tidak ditandai untuk obat.");
                }
            }

            if (request.IsAllowFractionalDispense && request.DispenseUnitMeasurementId.HasValue)
            {
                var dispenseUnitAllowsDecimal = await _dbContext.Set<MstMeasurement>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == request.DispenseUnitMeasurementId.Value
                            && !x.IsDelete
                            && x.IsActive
                            && x.IsForDrug
                            && x.IsDecimalAllowed,
                        cancellationToken);

                if (!dispenseUnitAllowsDecimal)
                {
                    return (false, "Dispense unit harus mengizinkan nilai desimal jika fractional dispense diaktifkan.");
                }
            }

            var normalizedName = request.DrugName.Trim().ToLower();
            var normalizedStrength = NormalizeComparableText(request.Strength);
            var normalizedDrugForm = NormalizeComparableText(request.DrugForm);
            var normalizedBrandName = NormalizeComparableText(request.BrandName);

            var duplicateNameQuery = _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete
                    && x.DrugCategoryId == request.DrugCategoryId
                    && x.DrugName.ToLower() == normalizedName
                    && (x.Strength ?? string.Empty).Trim().ToLower() == normalizedStrength
                    && (x.DrugForm ?? string.Empty).Trim().ToLower() == normalizedDrugForm
                    && (x.BrandName ?? string.Empty).Trim().ToLower() == normalizedBrandName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync(cancellationToken))
                return (false, "Drug dengan nama, kategori, strength, bentuk, dan brand tersebut sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(request.ExternalDrugCode))
            {
                var externalCode = request.ExternalDrugCode.Trim().ToLower();

                var duplicateExternalCodeQuery = _dbContext.Set<MstDrug>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete
                        && x.ExternalDrugCode != null
                        && x.ExternalDrugCode.ToLower() == externalCode);

                if (excludeId.HasValue)
                    duplicateExternalCodeQuery = duplicateExternalCodeQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateExternalCodeQuery.AnyAsync(cancellationToken))
                    return (false, "External drug code sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(request.IntegrationCode))
            {
                var integrationCode = request.IntegrationCode.Trim().ToLower();

                var duplicateIntegrationCodeQuery = _dbContext.Set<MstDrug>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete
                        && x.IntegrationCode != null
                        && x.IntegrationCode.ToLower() == integrationCode);

                if (excludeId.HasValue)
                    duplicateIntegrationCodeQuery = duplicateIntegrationCodeQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateIntegrationCodeQuery.AnyAsync(cancellationToken))
                    return (false, "Integration code sudah digunakan.");
            }

            return (true, null);
        }

        /// <summary>
        /// Menyelaraskan deret baru dengan kode lama satu kali secara aman. Nilai lama hanya
        /// menjadi batas bawah; kenaikannya tetap dilakukan atomik oleh NumberSeriesAllocator.
        /// </summary>
        private async Task<long> GetLegacyHighWatermarkAsync(
            CancellationToken cancellationToken)
        {
            var existingCodes = await _dbContext.Set<MstDrug>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.DrugCode.StartsWith(LegacyCodePrefix))
                .Select(x => x.DrugCode)
                .ToListAsync(cancellationToken);

            return existingCodes
                .Select(ExtractSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .DefaultIfEmpty(0)
                .Max();
        }

        private static long? ExtractSequenceNumber(string drugCode)
        {
            if (string.IsNullOrWhiteSpace(drugCode)
                || !drugCode.StartsWith(LegacyCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return long.TryParse(drugCode[LegacyCodePrefix.Length..], out var number)
                ? number
                : null;
        }

        private static DrugRegistrationResult Failed(string message) =>
            new(DrugRegistrationStatus.Invalid, null, message);

        private static Guid? NormalizeNullableGuid(Guid? value) =>
            value.HasValue && value.Value != Guid.Empty ? value : null;

        private static string? NormalizeNullableString(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeClinicalText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Trim();
        }

        private static string NormalizeComparableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLower();

        private static string? NormalizeDrugForm(string? value) =>
            NormalizeOption(value, DrugFormOptions);

        private static string? NormalizeRoute(string? value) =>
            NormalizeOption(value, RouteOptions);

        private static string? NormalizeOption(string? value, IEnumerable<string> options)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();

            return options.FirstOrDefault(
                    x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase))
                ?? trimmed;
        }
    }

    public enum DrugRegistrationStatus
    {
        Success = 0,
        Invalid = 1
    }

    public sealed record DrugRegistrationResult(
        DrugRegistrationStatus Status,
        MstDrug? Entity,
        string Message);
}
