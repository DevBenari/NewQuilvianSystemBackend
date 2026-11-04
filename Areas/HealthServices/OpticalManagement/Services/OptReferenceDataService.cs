using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Data referensi Optik (<c>BE-OPT-005</c>, <c>OPT-API-v1</c> grup Opt Reference Data): daftar pilihan berlabel
    /// Indonesia, katalog barang Optik beserta stok depo Optik, profil atribut barang, dan pengaturan unit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Barang tetap milik Master Data dan stok tetap milik Farmasi.</b> Kelas ini hanya menulis
    /// <see cref="OptItemProfile"/>, <see cref="OptSetting"/>, dan <see cref="OptSettingRevision"/>. Stok dibaca lewat
    /// <see cref="DrugStockService.GetAvailableQuantitiesAsync"/> — aturan siap-pakai, belum kedaluwarsa, dan dikurangi
    /// yang ditahan sama dengan pengeluaran — tidak pernah dihitung sendiri (<c>OPT-DEC-004</c>).
    /// </para>
    /// <para>
    /// <b>Pengaturan berlaku ke depan.</b> Mengubah pengaturan tidak menyentuh dokumen yang sudah dibuat, karena
    /// dokumen membekukan nilainya sendiri (<c>DownPaymentMinPercentSnapshot</c>, <c>ValidityMonthsSnapshot</c>,
    /// snapshot garansi). Setiap perubahan menulis satu <see cref="OptSettingRevision"/> berisi nilai lama → baru
    /// (<c>QBE-AUD-001</c>). <b>Contoh:</b> <c>PickupReminderDays: 30 → 21</c>.
    /// </para>
    /// </remarks>
    public class OptReferenceDataService
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;
        private readonly DrugStockService _drugStockService;

        public OptReferenceDataService(ApplicationDbContext dbContext, DrugStockService drugStockService)
        {
            _dbContext = dbContext;
            _drugStockService = drugStockService;
        }

        // ---------------------------------------------------------------- metadata

        public static OptMetadataResponse GetMetadata() => new()
        {
            ItemCategories = Options<OptItemCategory>(OptLabels.ItemCategory),
            LensDesigns = Options<OptLensDesign>(OptLabels.LensDesign),
            LensSupplies = Options<OptLensSupply>(OptLabels.LensSupply),
            WorkshopTypes = Options<OptWorkshopType>(OptLabels.WorkshopType),
            OrderKinds = Options<OptOrderKind>(OptLabels.OrderKind),
            OrderStatuses = Options<OptOrderStatus>(OptLabels.OrderStatus),
            OrderItemStatuses = Options<OptOrderItemStatus>(OptLabels.OrderItemStatus),
            RefractionExamStatuses = Options<OptRefractionExamStatus>(OptLabels.RefractionExamStatus),
            PrescriptionTypes = Options<OptPrescriptionType>(OptLabels.PrescriptionType),
            PrescriptionOrigins = Options<OptPrescriptionOrigin>(OptLabels.PrescriptionOrigin),
            CancellationStatuses = Options<OptCancellationStatus>(OptLabels.CancellationStatus),
            ComplaintTypes = Options<OptComplaintType>(OptLabels.ComplaintType),
            WarrantyClaimStatuses = Options<OptWarrantyClaimStatus>(OptLabels.WarrantyClaimStatus),
        };

        private static List<OptEnumOptionResponse> Options<TEnum>(Func<TEnum, string> label) where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>()
                .Select(x => new OptEnumOptionResponse { Value = Convert.ToInt32(x), Name = x.ToString(), Label = label(x) })
                .ToList();

        // ---------------------------------------------------------------- katalog barang

        public async Task<PagedResult<OptItemProfileResponse>> GetItemCatalogAsync(
            OptItemCatalogQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var rows = from profile in _dbContext.OptItemProfiles.AsNoTracking()
                       join drug in _dbContext.Set<MstDrug>().AsNoTracking() on profile.DrugId equals drug.Id
                       where !profile.IsDelete && !drug.IsDelete
                       select new { profile, drug };

            if (query.ItemCategory is { } kategori)
                rows = rows.Where(x => x.profile.ItemCategory == kategori);
            if (query.IsActive is { } aktif)
                rows = rows.Where(x => x.profile.IsActive == aktif);
            if (query.LensIndex is { } indeks)
                rows = rows.Where(x => x.profile.LensIndex == indeks);
            if (!string.IsNullOrWhiteSpace(query.Color))
            {
                var warna = query.Color.Trim().ToLower();
                rows = rows.Where(x => x.profile.Color != null && x.profile.Color.ToLower().Contains(warna));
            }
            if (!string.IsNullOrWhiteSpace(query.FrameSize))
            {
                var ukuran = query.FrameSize.Trim();
                rows = rows.Where(x => x.profile.FrameSize != null && x.profile.FrameSize.Contains(ukuran));
            }
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var kata = query.Search.Trim().ToLower();
                rows = rows.Where(x => x.drug.DrugCode.ToLower().Contains(kata)
                    || x.drug.DrugName.ToLower().Contains(kata)
                    || (x.drug.BrandName != null && x.drug.BrandName.ToLower().Contains(kata)));
            }

            var total = await rows.CountAsync(cancellationToken);

            var page = await rows
                .OrderBy(x => x.profile.ItemCategory)
                .ThenBy(x => x.drug.DrugName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OptItemProfileResponse
                {
                    ProfileId = x.profile.Id,
                    DrugId = x.drug.Id,
                    DrugCode = x.drug.DrugCode,
                    DrugName = x.drug.DrugName,
                    BrandName = x.drug.BrandName,
                    ItemCategory = x.profile.ItemCategory,
                    Color = x.profile.Color,
                    FrameSize = x.profile.FrameSize,
                    FrameMaterial = x.profile.FrameMaterial,
                    LensIndex = x.profile.LensIndex,
                    LensDesign = x.profile.LensDesign,
                    LensCoating = x.profile.LensCoating,
                    ContactLensBaseCurve = x.profile.ContactLensBaseCurve,
                    ContactLensDiameter = x.profile.ContactLensDiameter,
                    ContactLensReplacement = x.profile.ContactLensReplacement,
                    IsActive = x.profile.IsActive,
                    IsDrugActive = x.drug.IsActive,
                })
                .ToListAsync(cancellationToken);

            var depoId = await FindOpticalDepotIdAsync(cancellationToken);
            Dictionary<Guid, decimal>? stok = null;
            if (depoId is { } lokasi && page.Count > 0)
                stok = await _drugStockService.GetAvailableQuantitiesAsync(lokasi, page.Select(x => x.DrugId).ToList(), cancellationToken);

            foreach (var item in page)
            {
                item.ItemCategoryLabel = OptLabels.ItemCategory(item.ItemCategory);
                item.LensDesignLabel = item.LensDesign is { } desain ? OptLabels.LensDesign(desain) : null;
                item.AvailableQuantity = stok?.GetValueOrDefault(item.DrugId);
            }

            return new PagedResult<OptItemProfileResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = page,
            };
        }

        /// <summary>
        /// Depo Optik: lokasi penyimpanan Farmasi yang tertaut ke unit Optik dan boleh melayani. Yang bertanda
        /// bawaan didahulukan. <c>null</c> bila belum didaftarkan Farmasi (<c>OPT-APR-005</c>).
        /// </summary>
        public Task<Guid?> FindOpticalDepotIdAsync(CancellationToken cancellationToken = default) =>
            (from lokasi in _dbContext.Set<MstDrugStorageLocation>().AsNoTracking()
             join unit in _dbContext.Set<MstServiceUnit>().AsNoTracking() on lokasi.ServiceUnitId equals unit.Id
             where unit.ServiceUnitType == ServiceUnitType.Optical && !unit.IsDelete
                   && !lokasi.IsDelete && lokasi.IsActive && lokasi.IsAllowDispensing
             orderby lokasi.IsDefault descending, lokasi.StorageLocationCode
             select (Guid?)lokasi.Id)
            .FirstOrDefaultAsync(cancellationToken);

        // ---------------------------------------------------------------- profil barang

        public async Task<OptResult<OptItemProfileResponse>> UpsertItemProfileAsync(
            Guid drugId, OptItemProfileUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            if (request.ItemCategory is not { } kategori || !Enum.IsDefined(kategori))
                return OptResult<OptItemProfileResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih kategori barang Optik.");
            if (request.LensDesign is { } desain && !Enum.IsDefined(desain))
                return OptResult<OptItemProfileResponse>.Invalid(OptErrorCodes.InvalidRequest, "Desain lensa tidak dikenal.");

            var drug = await _dbContext.Set<MstDrug>().AsNoTracking()
                .Where(x => x.Id == drugId && !x.IsDelete)
                .Select(x => new { x.Id })
                .FirstOrDefaultAsync(cancellationToken);
            if (drug is null)
                return OptResult<OptItemProfileResponse>.NotFound("Barang tidak ditemukan di master barang.");

            var now = DateTime.UtcNow;
            var profile = await _dbContext.OptItemProfiles.FirstOrDefaultAsync(x => x.DrugId == drugId, cancellationToken);
            if (profile is null)
            {
                profile = new OptItemProfile { DrugId = drugId, CreateDateTime = now, CreateBy = actorUserId };
                _dbContext.OptItemProfiles.Add(profile);
            }
            else
            {
                profile.UpdateDateTime = now;
                profile.UpdateBy = actorUserId;
                profile.IsDelete = false;
            }

            profile.ItemCategory = kategori;
            profile.Color = Clean(request.Color);
            profile.FrameSize = Clean(request.FrameSize);
            profile.FrameMaterial = Clean(request.FrameMaterial);
            profile.LensIndex = request.LensIndex;
            profile.LensDesign = request.LensDesign;
            profile.LensCoating = Clean(request.LensCoating);
            profile.ContactLensBaseCurve = request.ContactLensBaseCurve;
            profile.ContactLensDiameter = request.ContactLensDiameter;
            profile.ContactLensReplacement = Clean(request.ContactLensReplacement);
            profile.IsActive = request.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var item = await ProjectProfileAsync(profile.Id, cancellationToken);
            return OptResult<OptItemProfileResponse>.Ok(item!);
        }

        private async Task<OptItemProfileResponse?> ProjectProfileAsync(Guid profileId, CancellationToken cancellationToken)
        {
            var row = await (from profile in _dbContext.OptItemProfiles.AsNoTracking()
                             join drug in _dbContext.Set<MstDrug>().AsNoTracking() on profile.DrugId equals drug.Id
                             where profile.Id == profileId
                             select new OptItemProfileResponse
                             {
                                 ProfileId = profile.Id,
                                 DrugId = drug.Id,
                                 DrugCode = drug.DrugCode,
                                 DrugName = drug.DrugName,
                                 BrandName = drug.BrandName,
                                 ItemCategory = profile.ItemCategory,
                                 Color = profile.Color,
                                 FrameSize = profile.FrameSize,
                                 FrameMaterial = profile.FrameMaterial,
                                 LensIndex = profile.LensIndex,
                                 LensDesign = profile.LensDesign,
                                 LensCoating = profile.LensCoating,
                                 ContactLensBaseCurve = profile.ContactLensBaseCurve,
                                 ContactLensDiameter = profile.ContactLensDiameter,
                                 ContactLensReplacement = profile.ContactLensReplacement,
                                 IsActive = profile.IsActive,
                                 IsDrugActive = drug.IsActive,
                             })
                .FirstOrDefaultAsync(cancellationToken);

            if (row is null)
                return null;

            row.ItemCategoryLabel = OptLabels.ItemCategory(row.ItemCategory);
            row.LensDesignLabel = row.LensDesign is { } desain ? OptLabels.LensDesign(desain) : null;
            var depoId = await FindOpticalDepotIdAsync(cancellationToken);
            if (depoId is { } lokasi)
                row.AvailableQuantity = (await _drugStockService.GetAvailableQuantitiesAsync(lokasi, [row.DrugId], cancellationToken))
                    .GetValueOrDefault(row.DrugId);
            return row;
        }

        // ---------------------------------------------------------------- pengaturan

        public async Task<OptResult<OptSettingResponse>> GetSettingAsync(Guid? serviceUnitId, CancellationToken cancellationToken = default)
        {
            var setting = await FindSettingQuery(serviceUnitId).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            return setting is null
                ? OptResult<OptSettingResponse>.NotFound("Pengaturan Optik belum tersedia. Pastikan unit layanan Optik sudah terdaftar.")
                : OptResult<OptSettingResponse>.Ok(await ProjectSettingAsync(setting, cancellationToken));
        }

        public async Task<OptResult<OptSettingResponse>> UpdateSettingAsync(
            OptSettingUpdateRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var galat = ValidateSetting(request);
            if (galat is not null)
                return galat;

            var setting = await FindSettingQuery(request.ServiceUnitId).FirstOrDefaultAsync(cancellationToken);
            if (setting is null)
                return OptResult<OptSettingResponse>.NotFound("Pengaturan Optik belum tersedia. Pastikan unit layanan Optik sudah terdaftar.");

            if (request.ExpectedVersion != setting.Version)
                return Stale();

            var perubahan = new List<string>();
            void Ubah<T>(string nama, T lama, T baru, Action<T> set) where T : IEquatable<T>
            {
                if (lama.Equals(baru)) return;
                perubahan.Add($"{nama}: {Format(lama)} → {Format(baru)}");
                set(baru);
            }

            Ubah(nameof(OptSetting.DownPaymentMinPercent), setting.DownPaymentMinPercent, request.DownPaymentMinPercent, v => setting.DownPaymentMinPercent = v);
            Ubah(nameof(OptSetting.PickupReminderDays), setting.PickupReminderDays, request.PickupReminderDays, v => setting.PickupReminderDays = v);
            Ubah(nameof(OptSetting.NotCollectedCloseDays), setting.NotCollectedCloseDays, request.NotCollectedCloseDays, v => setting.NotCollectedCloseDays = v);
            Ubah(nameof(OptSetting.PrescriptionValidityMonths), setting.PrescriptionValidityMonths, request.PrescriptionValidityMonths, v => setting.PrescriptionValidityMonths = v);
            Ubah(nameof(OptSetting.WarrantyFitDays), setting.WarrantyFitDays, request.WarrantyFitDays, v => setting.WarrantyFitDays = v);
            Ubah(nameof(OptSetting.WarrantyLensDefectMonths), setting.WarrantyLensDefectMonths, request.WarrantyLensDefectMonths, v => setting.WarrantyLensDefectMonths = v);
            Ubah(nameof(OptSetting.WarrantyFrameDefectMonths), setting.WarrantyFrameDefectMonths, request.WarrantyFrameDefectMonths, v => setting.WarrantyFrameDefectMonths = v);
            Ubah(nameof(OptSetting.ContactLensBaseCurveMin), setting.ContactLensBaseCurveMin, request.ContactLensBaseCurveMin, v => setting.ContactLensBaseCurveMin = v);
            Ubah(nameof(OptSetting.ContactLensBaseCurveMax), setting.ContactLensBaseCurveMax, request.ContactLensBaseCurveMax, v => setting.ContactLensBaseCurveMax = v);
            Ubah(nameof(OptSetting.ContactLensDiameterMin), setting.ContactLensDiameterMin, request.ContactLensDiameterMin, v => setting.ContactLensDiameterMin = v);
            Ubah(nameof(OptSetting.ContactLensDiameterMax), setting.ContactLensDiameterMax, request.ContactLensDiameterMax, v => setting.ContactLensDiameterMax = v);

            if (perubahan.Count > 0)
            {
                var now = DateTime.UtcNow;
                setting.Version += 1;
                setting.UpdateDateTime = now;
                setting.UpdateBy = actorUserId;
                _dbContext.OptSettingRevisions.Add(new OptSettingRevision
                {
                    SettingId = setting.Id,
                    ChangedFieldsSummary = string.Join("; ", perubahan),
                    ChangedBy = actorUserId,
                    ChangedAt = now,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                });

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Stale();
                }
            }

            return OptResult<OptSettingResponse>.Ok(await ProjectSettingAsync(setting, cancellationToken));
        }

        /// <summary>Aturan <c>OPT-VAL-070</c>..<c>073</c>.</summary>
        public static OptResult<OptSettingResponse>? ValidateSetting(OptSettingUpdateRequest r)
        {
            if (r.DownPaymentMinPercent < 0m || r.DownPaymentMinPercent > 100m)
                return OptResult<OptSettingResponse>.Invalid(OptErrorCodes.Val070, "Persentase uang muka harus 0–100.");
            if (r.PickupReminderDays < 1 || r.PickupReminderDays > 365)
                return OptResult<OptSettingResponse>.Invalid(OptErrorCodes.Val071, "Batas pengingat harus 1–365 hari.");
            if (r.NotCollectedCloseDays <= r.PickupReminderDays)
                return OptResult<OptSettingResponse>.Invalid(OptErrorCodes.Val071, "Batas tutup harus lebih lama dari batas pengingat.");
            if (r.PrescriptionValidityMonths < 1 || r.PrescriptionValidityMonths > 60)
                return OptResult<OptSettingResponse>.Invalid(OptErrorCodes.Val072, "Usia resep harus 1–60 bulan.");
            if (r.WarrantyFitDays < 0 || r.WarrantyLensDefectMonths < 0 || r.WarrantyFrameDefectMonths < 0)
                return OptResult<OptSettingResponse>.Invalid(OptErrorCodes.InvalidRequest, "Masa garansi tidak boleh negatif.");
            if (r.ContactLensBaseCurveMin <= 0m || r.ContactLensBaseCurveMin >= r.ContactLensBaseCurveMax
                || r.ContactLensDiameterMin <= 0m || r.ContactLensDiameterMin >= r.ContactLensDiameterMax)
                return OptResult<OptSettingResponse>.Invalid(OptErrorCodes.Val073, "Nilai minimum harus lebih kecil dari maksimum.");
            return null;
        }

        private IQueryable<OptSetting> FindSettingQuery(Guid? serviceUnitId) =>
            from setting in _dbContext.OptSettings
            join unit in _dbContext.Set<MstServiceUnit>() on setting.ServiceUnitId equals unit.Id
            where !setting.IsDelete && !unit.IsDelete && unit.ServiceUnitType == ServiceUnitType.Optical
                  && (serviceUnitId == null || unit.Id == serviceUnitId)
            orderby unit.CreateDateTime
            select setting;

        private async Task<OptSettingResponse> ProjectSettingAsync(OptSetting s, CancellationToken cancellationToken)
        {
            var unitName = await _dbContext.Set<MstServiceUnit>().AsNoTracking()
                .Where(x => x.Id == s.ServiceUnitId).Select(x => x.ServiceUnitName).FirstOrDefaultAsync(cancellationToken);
            var revisions = await _dbContext.OptSettingRevisions.AsNoTracking()
                .Where(x => x.SettingId == s.Id && !x.IsDelete)
                .OrderByDescending(x => x.ChangedAt)
                .Take(20)
                .Select(x => new OptSettingRevisionResponse { ChangedFieldsSummary = x.ChangedFieldsSummary, ChangedBy = x.ChangedBy, ChangedAt = x.ChangedAt })
                .ToListAsync(cancellationToken);

            return new OptSettingResponse
            {
                Id = s.Id,
                ServiceUnitId = s.ServiceUnitId,
                ServiceUnitName = unitName ?? string.Empty,
                DownPaymentMinPercent = s.DownPaymentMinPercent,
                PickupReminderDays = s.PickupReminderDays,
                NotCollectedCloseDays = s.NotCollectedCloseDays,
                PrescriptionValidityMonths = s.PrescriptionValidityMonths,
                WarrantyFitDays = s.WarrantyFitDays,
                WarrantyLensDefectMonths = s.WarrantyLensDefectMonths,
                WarrantyFrameDefectMonths = s.WarrantyFrameDefectMonths,
                ContactLensBaseCurveMin = s.ContactLensBaseCurveMin,
                ContactLensBaseCurveMax = s.ContactLensBaseCurveMax,
                ContactLensDiameterMin = s.ContactLensDiameterMin,
                ContactLensDiameterMax = s.ContactLensDiameterMax,
                Version = s.Version,
                Revisions = revisions,
            };
        }

        private static OptResult<OptSettingResponse> Stale() =>
            OptResult<OptSettingResponse>.Conflict(OptErrorCodes.ConcurrencyConflict,
                "Pengaturan sudah diubah petugas lain. Muat ulang halaman.");

        private static string Format<T>(T value) => value switch
        {
            decimal d => d.ToString("0.##", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
        };

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
