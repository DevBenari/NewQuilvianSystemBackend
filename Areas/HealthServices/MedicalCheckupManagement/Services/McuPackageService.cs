using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using IdentityModel = QuilvianSystemBackend.Models.IdentityModel;
using E = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services.McuErrorCodes;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Katalog paket MCU: paket, komponen, persyaratan, dan harga perusahaan (<c>BE-MCU-005</c>,
    /// <c>MCU-API-v1</c> grup Mcu Package, <c>VAL-MCU-01</c>..<c>08</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Harga tidak pernah disalin ke paket.</b> Paket menunjuk baris <c>MstTariff</c> ber-
    /// <c>IsPackageTariff</c> (<c>MCU-DEC-025</c>), dan harga perusahaan menunjuk baris tarif
    /// khususnya. Nominal selalu dibaca dari tarif, sehingga perubahan harga cukup dilakukan pemilik
    /// tarif.
    /// </para>
    /// <para>
    /// <b>Total nilai pengembalian dijaga terhadap harga umum</b> (<c>VAL-MCU-04</c>,
    /// <c>MCU-DEC-015</c>). Contoh: Paket Basic Rp 350.000 berisi Darah Lengkap Rp 100.000, Gula
    /// Darah Puasa Rp 40.000, dan Thorax Rp 250.000 — total Rp 390.000 — ditolak, karena membatalkan
    /// seluruh komponen akan mengembalikan uang lebih banyak daripada yang dibayar peserta.
    /// </para>
    /// <para>
    /// <b>Paket dan komponen tidak dihapus fisik.</b> Paket dinonaktifkan; komponen yang dibuang dari
    /// paket ditandai terhapus, karena <c>McuOrderComponent.PackageComponentId</c> order lama masih
    /// menunjuknya.
    /// </para>
    /// </remarks>
    public class McuPackageService
    {
        public const string PriceSourceGeneral = "General";
        public const string PriceSourceCompany = "Company";

        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private static readonly CultureInfo Indonesia = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _dbContext;
        private int _componentSequence;

        public McuPackageService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        public static McuPackageFilterMetadataResponse BuildFilterMetadata() => new()
        {
            StatusOptions =
            [
                new() { Value = "true", Label = "Aktif" },
                new() { Value = "false", Label = "Nonaktif" }
            ],
            ComponentTypeOptions = Enum.GetValues<McuComponentType>()
                .Select(x => new McuOptionItemResponse { Value = x.ToString(), Label = ComponentTypeLabel(x) })
                .ToList(),
            SortOptions =
            [
                new() { Value = "packageCode", Label = "Kode paket" },
                new() { Value = "packageName", Label = "Nama paket" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" }
            ],
            SortDirections = ["asc", "desc"],
            PageSizeOptions = [10, 25, 50, 100]
        };

        public async Task<PagedResult<McuPackageListResponse>> GetPackagesAsync(
            McuPackagePagedQuery query,
            CancellationToken cancellationToken)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var rows = _dbContext.McuPackages.AsNoTracking().Where(x => !x.IsDelete);

            var search = Normalize(query.Search)?.ToLower();
            if (search != null)
            {
                rows = rows.Where(x => x.PackageCode.ToLower().Contains(search) || x.PackageName.ToLower().Contains(search));
            }

            if (query.IsActive.HasValue)
            {
                rows = rows.Where(x => x.IsActive == query.IsActive.Value);
            }

            var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            rows = query.SortBy?.Trim().ToLowerInvariant() switch
            {
                "packagename" => descending ? rows.OrderByDescending(x => x.PackageName) : rows.OrderBy(x => x.PackageName),
                "createdatetime" => descending ? rows.OrderByDescending(x => x.CreateDateTime) : rows.OrderBy(x => x.CreateDateTime),
                _ => descending ? rows.OrderByDescending(x => x.PackageCode) : rows.OrderBy(x => x.PackageCode)
            };

            var totalData = await rows.CountAsync(cancellationToken);

            var items = await rows
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new McuPackageListResponse
                {
                    Id = x.Id,
                    PackageCode = x.PackageCode,
                    PackageName = x.PackageName,
                    GeneralTariffId = x.GeneralTariffId,
                    GeneralTariffName = x.GeneralTariff != null ? x.GeneralTariff.TariffName : null,
                    GeneralPrice = x.GeneralTariff != null ? x.GeneralTariff.NormalPrice : 0m,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            // Jumlah komponen dan nilai pengembalian dihitung di memori: penjumlahan decimal tidak
            // didukung semua provider, dan satu halaman paling banyak 100 paket.
            var ids = items.Select(x => x.Id).ToList();
            var components = await _dbContext.McuPackageComponents.AsNoTracking()
                .Where(x => ids.Contains(x.PackageId) && !x.IsDelete)
                .Select(x => new { x.PackageId, x.RefundAmount })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                var own = components.Where(x => x.PackageId == item.Id).ToList();
                item.ComponentCount = own.Count;
                item.TotalRefundAmount = own.Sum(x => x.RefundAmount);
            }

            return new PagedResult<McuPackageListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Paket aktif beserta harga yang berlaku pada tanggal pemeriksaan: harga perusahaan bila
        /// perusahaan punya harga yang berlaku pada tanggal itu, selain itu harga umum
        /// (<c>MCU-DEC-020</c>, <c>MCU-DEC-025</c>).
        /// </summary>
        public async Task<List<McuPackageOptionResponse>> GetOptionsAsync(
            McuPackageOptionsQuery query,
            CancellationToken cancellationToken)
        {
            var date = query.Date ?? DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

            var packages = await _dbContext.McuPackages.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.PackageName)
                .Select(x => new
                {
                    x.Id,
                    x.PackageCode,
                    x.PackageName,
                    x.GeneralTariffId,
                    GeneralPrice = x.GeneralTariff != null ? x.GeneralTariff.NormalPrice : 0m
                })
                .ToListAsync(cancellationToken);

            var packageIds = packages.Select(x => x.Id).ToList();

            var companyPrices = query.CompanyGuarantorId.HasValue
                ? await _dbContext.McuPackageCompanyPrices.AsNoTracking()
                    .Where(x => !x.IsDelete
                        && packageIds.Contains(x.PackageId)
                        && x.CompanyGuarantorId == query.CompanyGuarantorId.Value
                        && x.EffectiveStartDate <= date
                        && (x.EffectiveEndDate == null || x.EffectiveEndDate >= date))
                    .Select(x => new
                    {
                        x.Id,
                        x.PackageId,
                        x.TariffId,
                        Price = x.Tariff != null ? x.Tariff.NormalPrice : 0m,
                        x.EffectiveStartDate,
                        x.EffectiveEndDate
                    })
                    .ToListAsync(cancellationToken)
                : [];

            var requirements = await _dbContext.McuPackageComponentRequirements.AsNoTracking()
                .Where(x => !x.IsDelete
                    && x.PackageComponent != null
                    && !x.PackageComponent.IsDelete
                    && packageIds.Contains(x.PackageComponent.PackageId))
                .Select(x => new
                {
                    x.PackageComponent!.PackageId,
                    ProcedureName = x.PackageComponent.Procedure != null ? x.PackageComponent.Procedure.ProcedureName : null,
                    ExaminationTypeName = x.PackageComponent.ExaminationType != null ? x.PackageComponent.ExaminationType.ExaminationTypeName : null,
                    x.RequirementText,
                    x.IsConfirmationRequired,
                    x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return packages.Select(package =>
            {
                // Tumpang tindih ditolak saat harga dibuat (VAL-MCU-05), jadi paling banyak satu.
                var company = companyPrices.FirstOrDefault(x => x.PackageId == package.Id);

                return new McuPackageOptionResponse
                {
                    Id = package.Id,
                    PackageCode = package.PackageCode,
                    PackageName = package.PackageName,
                    TariffId = company?.TariffId ?? package.GeneralTariffId,
                    Price = company?.Price ?? package.GeneralPrice,
                    PriceSource = company != null ? PriceSourceCompany : PriceSourceGeneral,
                    PriceSourceLabel = company != null ? "Harga perusahaan" : "Harga umum",
                    CompanyPriceId = company?.Id,
                    EffectiveStartDate = company?.EffectiveStartDate,
                    EffectiveEndDate = company?.EffectiveEndDate,
                    Requirements = requirements
                        .Where(x => x.PackageId == package.Id)
                        .OrderBy(x => x.CreateDateTime)
                        .Select(x => new McuPackageOptionRequirementResponse
                        {
                            ComponentName = x.ProcedureName ?? x.ExaminationTypeName ?? string.Empty,
                            RequirementText = x.RequirementText,
                            IsConfirmationRequired = x.IsConfirmationRequired
                        })
                        .ToList()
                };
            }).ToList();
        }

        public async Task<McuPackageDetailResponse?> GetPackageAsync(Guid id, CancellationToken cancellationToken)
        {
            var package = await _dbContext.McuPackages.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new McuPackageDetailResponse
                {
                    Id = x.Id,
                    PackageCode = x.PackageCode,
                    PackageName = x.PackageName,
                    Description = x.Description,
                    GeneralTariffId = x.GeneralTariffId,
                    GeneralTariffCode = x.GeneralTariff != null ? x.GeneralTariff.TariffCode : null,
                    GeneralTariffName = x.GeneralTariff != null ? x.GeneralTariff.TariffName : null,
                    GeneralPrice = x.GeneralTariff != null ? x.GeneralTariff.NormalPrice : 0m,
                    IsActive = x.IsActive
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (package == null)
                return null;

            var components = await _dbContext.McuPackageComponents.AsNoTracking()
                .Where(x => x.PackageId == id && !x.IsDelete)
                .OrderBy(x => x.ComponentType)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.Id,
                    x.ComponentType,
                    x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    x.RadModalityId,
                    RadModalityName = x.RadModality != null ? x.RadModality.ModalityName : null,
                    x.ExaminationTypeId,
                    ExaminationTypeName = x.ExaminationType != null ? x.ExaminationType.ExaminationTypeName : null,
                    x.RefundAmount,
                    x.MedicalFeeAmount
                })
                .ToListAsync(cancellationToken);

            var componentIds = components.Select(x => x.Id).ToList();
            var requirements = await _dbContext.McuPackageComponentRequirements.AsNoTracking()
                .Where(x => componentIds.Contains(x.PackageComponentId) && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.PackageComponentId, x.RequirementText, x.IsConfirmationRequired })
                .ToListAsync(cancellationToken);

            package.Components = components.Select(x => new McuPackageComponentResponse
            {
                Id = x.Id,
                ComponentType = x.ComponentType,
                ComponentTypeLabel = ComponentTypeLabel(x.ComponentType),
                ComponentName = x.ProcedureName ?? x.ExaminationTypeName ?? string.Empty,
                ProcedureId = x.ProcedureId,
                ProcedureCode = x.ProcedureCode,
                RadModalityId = x.RadModalityId,
                RadModalityName = x.RadModalityName,
                ExaminationTypeId = x.ExaminationTypeId,
                RefundAmount = x.RefundAmount,
                MedicalFeeAmount = x.MedicalFeeAmount,
                Requirements = requirements
                    .Where(r => r.PackageComponentId == x.Id)
                    .Select(r => new McuPackageRequirementResponse
                    {
                        Id = r.Id,
                        RequirementText = r.RequirementText,
                        IsConfirmationRequired = r.IsConfirmationRequired
                    })
                    .ToList()
            }).ToList();

            package.TotalRefundAmount = package.Components.Sum(x => x.RefundAmount);

            var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());
            package.CompanyPrices = (await _dbContext.McuPackageCompanyPrices.AsNoTracking()
                    .Where(x => x.PackageId == id && !x.IsDelete)
                    .Select(x => new McuCompanyPriceResponse
                    {
                        Id = x.Id,
                        PackageId = x.PackageId,
                        CompanyGuarantorId = x.CompanyGuarantorId,
                        CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : null,
                        TariffId = x.TariffId,
                        TariffName = x.Tariff != null ? x.Tariff.TariffName : null,
                        Price = x.Tariff != null ? x.Tariff.NormalPrice : 0m,
                        EffectiveStartDate = x.EffectiveStartDate,
                        EffectiveEndDate = x.EffectiveEndDate
                    })
                    .ToListAsync(cancellationToken))
                .OrderBy(x => x.CompanyGuarantorName)
                .ThenBy(x => x.EffectiveStartDate)
                .ToList();

            foreach (var price in package.CompanyPrices)
            {
                price.IsCurrentlyEffective = price.EffectiveStartDate <= today
                    && (price.EffectiveEndDate == null || price.EffectiveEndDate >= today);
            }

            return package;
        }

        // =====================================================================
        // Paket
        // =====================================================================

        public async Task<McuResult<McuPackageDetailResponse>> CreatePackageAsync(
            CreateMcuPackageRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var validation = await ValidatePackageAsync(null, request, cancellationToken);
            if (validation.Error != null)
                return Fail<McuPackageDetailResponse>(validation.Error);

            var now = DateTime.UtcNow;
            var package = new McuPackage
            {
                PackageCode = validation.PackageCode,
                PackageName = request.PackageName.Trim(),
                Description = Normalize(request.Description),
                GeneralTariffId = request.GeneralTariffId,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.McuPackages.Add(package);

            foreach (var component in request.Components)
            {
                AddComponent(package.Id, component, actorUserId, now);
            }

            var saveError = await SaveAsync(cancellationToken);
            if (saveError != null)
                return Fail<McuPackageDetailResponse>(saveError);

            return McuResult<McuPackageDetailResponse>.Ok((await GetPackageAsync(package.Id, cancellationToken))!);
        }

        public async Task<McuResult<McuPackageDetailResponse>> UpdatePackageAsync(
            Guid id,
            UpdateMcuPackageRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var package = await _dbContext.McuPackages
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (package == null)
                return McuResult<McuPackageDetailResponse>.NotFound("Paket MCU tidak ditemukan.");

            var validation = await ValidatePackageAsync(id, request, cancellationToken);
            if (validation.Error != null)
                return Fail<McuPackageDetailResponse>(validation.Error);

            var now = DateTime.UtcNow;

            package.PackageCode = validation.PackageCode;
            package.PackageName = request.PackageName.Trim();
            package.Description = Normalize(request.Description);
            package.GeneralTariffId = request.GeneralTariffId;
            package.UpdateDateTime = now;
            package.UpdateBy = actorUserId;

            var existing = await _dbContext.McuPackageComponents
                .Include(x => x.Requirements.Where(r => !r.IsDelete))
                .Where(x => x.PackageId == id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            // Pasangkan dulu menurut Id, lalu sisanya menurut pemeriksaan yang sama. Dengan begitu
            // prosedur yang tetap ada di paket tidak pernah dihapus lalu dibuat ulang — order lama
            // tetap menunjuk baris yang sama.
            var unmatched = existing.ToList();
            var pairs = new List<(McuPackageComponentRequest Request, McuPackageComponent? Existing)>();

            foreach (var item in request.Components)
            {
                var byId = item.Id.HasValue
                    ? unmatched.FirstOrDefault(x => x.Id == item.Id.Value && SameExamination(x, item))
                    : null;

                if (byId != null)
                    unmatched.Remove(byId);

                pairs.Add((item, byId));
            }

            for (var i = 0; i < pairs.Count; i++)
            {
                if (pairs[i].Existing != null)
                    continue;

                var byExamination = unmatched.FirstOrDefault(x => SameExamination(x, pairs[i].Request));
                if (byExamination == null)
                    continue;

                unmatched.Remove(byExamination);
                pairs[i] = (pairs[i].Request, byExamination);
            }

            foreach (var (item, component) in pairs)
            {
                if (component == null)
                {
                    AddComponent(id, item, actorUserId, now);
                    continue;
                }

                var changed = component.RadModalityId != RadModality(item)
                    || component.RefundAmount != item.RefundAmount
                    || component.MedicalFeeAmount != item.MedicalFeeAmount;

                component.RadModalityId = RadModality(item);
                component.RefundAmount = item.RefundAmount;
                component.MedicalFeeAmount = item.MedicalFeeAmount;

                if (!SameRequirements(component.Requirements, item.Requirements))
                {
                    foreach (var requirement in component.Requirements)
                    {
                        MarkDeleted(requirement, actorUserId, now);
                    }

                    AddRequirements(component.Id, item.Requirements, actorUserId, now);
                    changed = true;
                }

                if (changed)
                {
                    component.UpdateDateTime = now;
                    component.UpdateBy = actorUserId;
                }
            }

            foreach (var removed in unmatched)
            {
                MarkDeleted(removed, actorUserId, now);
                foreach (var requirement in removed.Requirements)
                {
                    MarkDeleted(requirement, actorUserId, now);
                }
            }

            var saveError = await SaveAsync(cancellationToken);
            if (saveError != null)
                return Fail<McuPackageDetailResponse>(saveError);

            return McuResult<McuPackageDetailResponse>.Ok((await GetPackageAsync(id, cancellationToken))!);
        }

        public async Task<McuResult<McuPackageStatusResponse>> SetStatusAsync(
            Guid id,
            McuPackageStatusRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var package = await _dbContext.McuPackages
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (package == null)
                return McuResult<McuPackageStatusResponse>.NotFound("Paket MCU tidak ditemukan.");

            if (package.IsActive != request.IsActive)
            {
                package.IsActive = request.IsActive;
                package.UpdateDateTime = DateTime.UtcNow;
                package.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return McuResult<McuPackageStatusResponse>.Ok(new McuPackageStatusResponse
            {
                Id = package.Id,
                PackageCode = package.PackageCode,
                IsActive = package.IsActive
            });
        }

        // =====================================================================
        // Harga perusahaan
        // =====================================================================

        public async Task<McuResult<McuCompanyPriceResponse>> AddCompanyPriceAsync(
            Guid packageId,
            CreateMcuCompanyPriceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var packageExists = await _dbContext.McuPackages.AsNoTracking()
                .AnyAsync(x => x.Id == packageId && !x.IsDelete, cancellationToken);

            if (!packageExists)
                return McuResult<McuCompanyPriceResponse>.NotFound("Paket MCU tidak ditemukan.");

            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value < request.EffectiveStartDate)
            {
                return McuResult<McuCompanyPriceResponse>.Invalid(
                    E.InvalidRequest, "Tanggal akhir berlaku tidak boleh sebelum tanggal mulai berlaku.");
            }

            var company = await _dbContext.Set<MstCompanyGuarantor>().AsNoTracking()
                .Where(x => x.Id == request.CompanyGuarantorId && !x.IsDelete && x.IsActive)
                .Select(x => new { x.CompanyGuarantorName })
                .SingleOrDefaultAsync(cancellationToken);

            if (company == null)
            {
                return McuResult<McuCompanyPriceResponse>.Invalid(
                    E.InvalidRequest, "Perusahaan penjamin tidak ditemukan atau tidak aktif.");
            }

            var tariffError = await ValidatePackageTariffAsync(request.TariffId, cancellationToken);
            if (tariffError != null)
                return Fail<McuCompanyPriceResponse>(tariffError);

            // Dua admin yang menambah harga untuk paket dan perusahaan yang sama secara bersamaan
            // sama-sama lolos pemeriksaan tumpang tindih di memori. Kunci ini mengantrekan keduanya
            // sehingga yang kedua membaca harga yang pertama.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));",
                    [$"MCU_COMPANY_PRICE_{packageId:N}_{request.CompanyGuarantorId:N}"],
                    cancellationToken);
            }

            var newEnd = request.EffectiveEndDate ?? DateOnly.MaxValue;
            var overlaps = await _dbContext.McuPackageCompanyPrices.AsNoTracking()
                .Where(x => !x.IsDelete
                    && x.PackageId == packageId
                    && x.CompanyGuarantorId == request.CompanyGuarantorId
                    && x.EffectiveStartDate <= newEnd
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= request.EffectiveStartDate))
                .Select(x => new { x.EffectiveStartDate, x.EffectiveEndDate })
                .ToListAsync(cancellationToken);

            if (overlaps.Count > 0)
            {
                var first = overlaps.OrderBy(x => x.EffectiveStartDate).First();
                var start = first.EffectiveStartDate > request.EffectiveStartDate ? first.EffectiveStartDate : request.EffectiveStartDate;
                var end = Min(first.EffectiveEndDate, request.EffectiveEndDate);

                return McuResult<McuCompanyPriceResponse>.Invalid(
                    E.Val05,
                    $"{company.CompanyGuarantorName} sudah punya harga untuk paket ini pada {FormatPeriod(start, end)}.");
            }

            var price = new McuPackageCompanyPrice
            {
                PackageId = packageId,
                CompanyGuarantorId = request.CompanyGuarantorId,
                TariffId = request.TariffId,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.McuPackageCompanyPrices.Add(price);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var detail = (await GetPackageAsync(packageId, cancellationToken))!;
            return McuResult<McuCompanyPriceResponse>.Ok(detail.CompanyPrices.Single(x => x.Id == price.Id));
        }

        /// <summary>
        /// Mengakhiri masa berlaku harga perusahaan. Hanya memendekkan: memperpanjang masa berlaku
        /// dapat menimpa harga lain yang mulai sesudahnya, dan untuk itu admin menambah harga baru.
        /// </summary>
        public async Task<McuResult<McuCompanyPriceEndResponse>> EndCompanyPriceAsync(
            Guid packageId,
            Guid priceId,
            EndMcuCompanyPriceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var price = await _dbContext.McuPackageCompanyPrices
                .SingleOrDefaultAsync(x => x.Id == priceId && x.PackageId == packageId && !x.IsDelete, cancellationToken);

            if (price == null)
                return McuResult<McuCompanyPriceEndResponse>.NotFound("Harga perusahaan untuk paket ini tidak ditemukan.");

            if (request.EffectiveEndDate < price.EffectiveStartDate)
            {
                return McuResult<McuCompanyPriceEndResponse>.Invalid(
                    E.InvalidRequest, "Tanggal akhir berlaku tidak boleh sebelum tanggal mulai berlaku.");
            }

            if (price.EffectiveEndDate.HasValue && request.EffectiveEndDate > price.EffectiveEndDate.Value)
            {
                return McuResult<McuCompanyPriceEndResponse>.Invalid(
                    E.InvalidRequest,
                    $"Harga ini sudah berakhir {FormatDate(price.EffectiveEndDate.Value)}; masa berlakunya tidak dapat diperpanjang.");
            }

            price.EffectiveEndDate = request.EffectiveEndDate;
            price.UpdateDateTime = DateTime.UtcNow;
            price.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return McuResult<McuCompanyPriceEndResponse>.Ok(new McuCompanyPriceEndResponse
            {
                Id = price.Id,
                PackageId = price.PackageId,
                EffectiveStartDate = price.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate
            });
        }

        // =====================================================================
        // Validasi paket — VAL-MCU-01..08
        // =====================================================================

        private sealed record Failure(string Code, string Message);

        private sealed record PackageValidation(string PackageCode, Failure? Error);

        private async Task<PackageValidation> ValidatePackageAsync(
            Guid? packageId,
            CreateMcuPackageRequest request,
            CancellationToken cancellationToken)
        {
            static PackageValidation Reject(string code, string message) => new(string.Empty, new Failure(code, message));

            var code = request.PackageCode?.Trim().ToUpperInvariant() ?? string.Empty;
            if (code.Length == 0)
                return Reject(E.InvalidRequest, "Kode paket wajib diisi.");
            if (code.Length > 30)
                return Reject(E.InvalidRequest, "Kode paket paling banyak 30 karakter.");
            if (code.Any(char.IsWhiteSpace))
                return Reject(E.InvalidRequest, "Kode paket tidak boleh mengandung spasi.");

            var name = request.PackageName?.Trim() ?? string.Empty;
            if (name.Length == 0)
                return Reject(E.InvalidRequest, "Nama paket wajib diisi.");
            if (name.Length > 150)
                return Reject(E.InvalidRequest, "Nama paket paling banyak 150 karakter.");
            if (Normalize(request.Description)?.Length > 1000)
                return Reject(E.InvalidRequest, "Keterangan paket paling banyak 1000 karakter.");

            var components = request.Components ?? [];
            if (components.Count == 0)
                return Reject(E.Val02, "Paket harus berisi sekurang-kurangnya satu pemeriksaan.");

            foreach (var component in components)
            {
                if (component.RefundAmount < 0 || component.MedicalFeeAmount < 0)
                    return Reject(E.Val08, "Nilai tidak boleh negatif.");

                foreach (var requirement in component.Requirements ?? [])
                {
                    var text = requirement.RequirementText?.Trim() ?? string.Empty;
                    if (text.Length == 0)
                        return Reject(E.InvalidRequest, "Teks persyaratan wajib diisi.");
                    if (text.Length > 300)
                        return Reject(E.InvalidRequest, "Teks persyaratan paling banyak 300 karakter.");
                }
            }

            // VAL-MCU-06: isian sesuai jenis, dan master yang ditunjuk memang sah.
            var procedureIds = components.Where(x => x.ProcedureId.HasValue).Select(x => x.ProcedureId!.Value).Distinct().ToList();
            var procedures = await _dbContext.Set<MstProcedure>().AsNoTracking()
                .Where(x => procedureIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .Select(x => new { x.Id, x.ProcedureName, x.IsLaboratory, x.IsRadiology })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var modalityIds = components.Where(x => x.RadModalityId.HasValue).Select(x => x.RadModalityId!.Value).Distinct().ToList();
            var modalities = await _dbContext.Set<MstRadModality>().AsNoTracking()
                .Where(x => modalityIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var examinationTypeIds = components.Where(x => x.ExaminationTypeId.HasValue).Select(x => x.ExaminationTypeId!.Value).Distinct().ToList();
            var examinationTypes = await _dbContext.McuExaminationTypes.AsNoTracking()
                .Where(x => examinationTypeIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .Select(x => new { x.Id, x.ExaminationTypeName })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            foreach (var component in components)
            {
                switch (component.ComponentType)
                {
                    case McuComponentType.Laboratory:
                        if (!component.ProcedureId.HasValue
                            || !procedures.TryGetValue(component.ProcedureId.Value, out var lab)
                            || !lab.IsLaboratory)
                        {
                            return Reject(E.Val06, "Komponen Laboratorium wajib memilih prosedur laboratorium.");
                        }
                        break;

                    case McuComponentType.Radiology:
                        if (!component.ProcedureId.HasValue
                            || !procedures.TryGetValue(component.ProcedureId.Value, out var rad)
                            || !rad.IsRadiology
                            || !component.RadModalityId.HasValue
                            || !modalities.Contains(component.RadModalityId.Value))
                        {
                            return Reject(E.Val06, "Komponen Radiologi wajib memilih prosedur dan modalitas.");
                        }
                        break;

                    case McuComponentType.McuExamination:
                        if (!component.ExaminationTypeId.HasValue
                            || !examinationTypes.ContainsKey(component.ExaminationTypeId.Value))
                        {
                            return Reject(E.Val06, "Komponen Pemeriksaan MCU wajib memilih jenis pemeriksaan.");
                        }
                        break;

                    default:
                        return Reject(E.InvalidRequest, "Jenis komponen tidak dikenal.");
                }
            }

            // VAL-MCU-07: satu prosedur atau satu jenis pemeriksaan hanya sekali per paket.
            var duplicate = components
                .Select(x => new { Key = ExaminationKey(x), Component = x })
                .GroupBy(x => x.Key)
                .FirstOrDefault(x => x.Count() > 1);

            if (duplicate != null)
            {
                var component = duplicate.First().Component;
                var duplicateName = component.ComponentType == McuComponentType.McuExamination
                    ? examinationTypes[component.ExaminationTypeId!.Value].ExaminationTypeName
                    : procedures[component.ProcedureId!.Value].ProcedureName;

                return Reject(E.Val07, $"{duplicateName} sudah ada di paket ini.");
            }

            // VAL-MCU-01
            var codeTaken = await _dbContext.McuPackages.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.PackageCode == code && x.Id != packageId, cancellationToken);
            if (codeTaken)
                return Reject(E.Val01, $"Kode paket {code} sudah dipakai.");

            // VAL-MCU-03
            var tariffError = await ValidatePackageTariffAsync(request.GeneralTariffId, cancellationToken);
            if (tariffError != null)
                return new PackageValidation(string.Empty, tariffError);

            // VAL-MCU-04
            var generalPrice = await _dbContext.Set<MstTariff>().AsNoTracking()
                .Where(x => x.Id == request.GeneralTariffId)
                .Select(x => x.NormalPrice)
                .SingleAsync(cancellationToken);

            var totalRefund = components.Sum(x => x.RefundAmount);
            if (totalRefund > generalPrice)
            {
                return Reject(
                    E.Val04,
                    $"Total nilai pengembalian {FormatRupiah(totalRefund)} melebihi harga paket {FormatRupiah(generalPrice)}.");
            }

            return new PackageValidation(code, null);
        }

        /// <summary><c>VAL-MCU-03</c>: tarif harus tarif paket yang aktif.</summary>
        private async Task<Failure?> ValidatePackageTariffAsync(Guid tariffId, CancellationToken cancellationToken)
        {
            var valid = await _dbContext.Set<MstTariff>().AsNoTracking()
                .AnyAsync(x => x.Id == tariffId && !x.IsDelete && x.IsActive && x.IsPackageTariff, cancellationToken);

            return valid ? null : new Failure(E.Val03, "Tarif yang dipilih bukan tarif paket aktif.");
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private void AddComponent(Guid packageId, McuPackageComponentRequest item, Guid actorUserId, DateTime now)
        {
            var component = new McuPackageComponent
            {
                PackageId = packageId,
                ComponentType = item.ComponentType,
                ProcedureId = item.ComponentType == McuComponentType.McuExamination ? null : item.ProcedureId,
                RadModalityId = RadModality(item),
                ExaminationTypeId = item.ComponentType == McuComponentType.McuExamination ? item.ExaminationTypeId : null,
                RefundAmount = item.RefundAmount,
                MedicalFeeAmount = item.MedicalFeeAmount,
                // Dinaikkan per komponen: detail paket mengurutkan menurut waktu dibuat, dan komponen
                // yang disimpan bersamaan tidak boleh tertukar urutannya.
                CreateDateTime = now.AddTicks(_componentSequence++),
                CreateBy = actorUserId
            };

            _dbContext.McuPackageComponents.Add(component);
            AddRequirements(component.Id, item.Requirements, actorUserId, now);
        }

        private void AddRequirements(
            Guid componentId,
            IEnumerable<McuPackageRequirementRequest>? requirements,
            Guid actorUserId,
            DateTime now)
        {
            // CreateDateTime dinaikkan per butir agar urutan yang diisi admin tetap terbaca.
            var tick = 0;
            foreach (var requirement in requirements ?? [])
            {
                _dbContext.McuPackageComponentRequirements.Add(new McuPackageComponentRequirement
                {
                    PackageComponentId = componentId,
                    RequirementText = requirement.RequirementText.Trim(),
                    IsConfirmationRequired = requirement.IsConfirmationRequired,
                    CreateDateTime = now.AddTicks(tick++),
                    CreateBy = actorUserId
                });
            }
        }

        private static void MarkDeleted(IdentityModel row, Guid actorUserId, DateTime now)
        {
            row.IsDelete = true;
            row.DeleteDateTime = now;
            row.DeleteBy = actorUserId;
        }

        private static Guid? RadModality(McuPackageComponentRequest item) =>
            item.ComponentType == McuComponentType.Radiology ? item.RadModalityId : null;

        private static (McuComponentType, Guid?) ExaminationKey(McuPackageComponentRequest item) =>
            item.ComponentType == McuComponentType.McuExamination
                ? (item.ComponentType, item.ExaminationTypeId)
                : (McuComponentType.Laboratory, item.ProcedureId);

        private static bool SameExamination(McuPackageComponent existing, McuPackageComponentRequest item) =>
            existing.ComponentType == item.ComponentType
            && existing.ProcedureId == (item.ComponentType == McuComponentType.McuExamination ? null : item.ProcedureId)
            && existing.ExaminationTypeId == (item.ComponentType == McuComponentType.McuExamination ? item.ExaminationTypeId : null);

        private static bool SameRequirements(
            IEnumerable<McuPackageComponentRequirement> existing,
            IEnumerable<McuPackageRequirementRequest>? requested)
        {
            var left = existing.OrderBy(x => x.CreateDateTime).Select(x => (x.RequirementText, x.IsConfirmationRequired)).ToList();
            var right = (requested ?? []).Select(x => (x.RequirementText.Trim(), x.IsConfirmationRequired)).ToList();
            return left.SequenceEqual(right);
        }

        /// <summary>
        /// Menyimpan perubahan. Dua admin yang menyimpan kode atau pemeriksaan yang sama secara
        /// bersamaan sama-sama lolos pemeriksaan di memori; index unik menolak yang kedua.
        /// </summary>
        private async Task<Failure?> SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateException exception)
                when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var constraint = (exception.InnerException as PostgresException)?.ConstraintName ?? string.Empty;

                return constraint.Contains("PackageCode", StringComparison.Ordinal)
                    ? new Failure(E.Val01, "Kode paket sudah dipakai.")
                    : new Failure(E.Val07, "Pemeriksaan yang sama sudah ada di paket ini.");
            }
        }

        private static McuResult<T> Fail<T>(Failure failure) => McuResult<T>.Invalid(failure.Code, failure.Message);

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static DateOnly? Min(DateOnly? left, DateOnly? right) =>
            left == null ? right : right == null ? left : (left < right ? left : right);

        public static string ComponentTypeLabel(McuComponentType type) => type switch
        {
            McuComponentType.Laboratory => "Laboratorium",
            McuComponentType.Radiology => "Radiologi",
            McuComponentType.McuExamination => "Pemeriksaan MCU",
            _ => type.ToString()
        };

        /// <summary>Contoh <c>Rp 390.000</c>.</summary>
        public static string FormatRupiah(decimal value) => $"Rp {value.ToString("N0", Indonesia)}";

        private static string FormatDate(DateOnly date) => date.ToString("d MMMM yyyy", Indonesia);

        /// <summary>
        /// Contoh <c>1–31 Desember 2026</c>, <c>1 Oktober–31 Desember 2026</c>,
        /// <c>1 Oktober 2026 dan seterusnya</c>.
        /// </summary>
        public static string FormatPeriod(DateOnly start, DateOnly? end)
        {
            if (end == null)
                return $"{FormatDate(start)} dan seterusnya";

            var last = end.Value;
            if (start == last)
                return FormatDate(start);

            if (start.Year != last.Year)
                return $"{FormatDate(start)}–{FormatDate(last)}";

            if (start.Month != last.Month)
                return $"{start.ToString("d MMMM", Indonesia)}–{FormatDate(last)}";

            return $"{start.Day}–{FormatDate(last)}";
        }
    }
}
