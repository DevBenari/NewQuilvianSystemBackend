using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;
using W = QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services.AssetImportWorkbook;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

internal sealed class ValidatedImportRow
{
    public int RowNumber { get; init; }
    public AssetImportParsedRow Source { get; init; } = new();
    public List<string> Errors { get; } = [];
    public Guid GroupId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public Guid OwnershipTypeId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? ModelType { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? DistributionPermitNumber { get; set; }
    public int? ManufactureYear { get; set; }
    public string? DistributorName { get; set; }
    public string? SupplierName { get; set; }
    public DateOnly AcquisitionDate { get; set; }
    public DateOnly ReadyForUseDate { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public decimal? AccumulatedDepreciation { get; set; }
    public Guid OrganizationUnitId { get; set; }
    public Guid LocationId { get; set; }
    public Guid CustodianEmployeeId { get; set; }
    public string PlacementLabel { get; set; } = string.Empty;
}

public sealed partial class AssetImportService
{
    private async Task<List<ValidatedImportRow>> ValidateAsync(
        IReadOnlyList<AssetImportParsedRow> rows,
        Guid legalEntityId,
        DateOnly cutoverDate,
        CancellationToken cancellationToken)
    {
        var parameter = await _parameterQuery.GetRequiredParameterAtAsync(legalEntityId, cutoverDate, cancellationToken);

        var groups = (await _context.MstAssetGroups
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .Select(x => new ImportGroup(x.Id, x.InventoryCode, x.Classification!.IsMedical, x.Category!.IsDepreciable))
                .ToListAsync(cancellationToken))
            .ToDictionary(x => x.InventoryCode.ToUpperInvariant());
        var ownership = (await _context.MstAssetOwnershipTypes
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .Select(x => new { x.Id, x.OwnershipCode, x.IsCapitalizable })
                .ToListAsync(cancellationToken))
            .ToDictionary(x => x.OwnershipCode.ToUpperInvariant());
        var units = (await _context.MstOrganizationUnits
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .Select(x => new { x.Id, x.UnitCode, x.UnitName })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.UnitCode.ToUpperInvariant())
            .ToDictionary(x => x.Key, x => x.First());
        var locations = (await _context.MstAssetLocations
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .Select(x => new { x.Id, x.LocationCode, x.LocationName, x.OrganizationUnitId })
                .ToListAsync(cancellationToken))
            .ToDictionary(x => x.LocationCode.ToUpperInvariant());

        var custodianKeys = rows.Select(x => x.Text(W.CustodianNumber)).Where(x => x.Length > 0).Distinct().ToList();
        var employees = await _context.MstEmployees
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete && (custodianKeys.Contains(x.EmployeeNumber) || custodianKeys.Contains(x.EmployeeCode)))
            .Select(x => new { x.Id, x.EmployeeNumber, x.EmployeeCode })
            .ToListAsync(cancellationToken);
        var employeeByKey = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var employee in employees)
        {
            employeeByKey.TryAdd(employee.EmployeeNumber, employee.Id);
            employeeByKey.TryAdd(employee.EmployeeCode, employee.Id);
        }

        var serials = rows.Select(x => x.Text(W.SerialNumber).ToUpperInvariant()).Where(x => x.Length > 0).Distinct().ToList();
        var existingSerials = (await _context.AstAssets
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.RecordStatus != AssetRecordStatus.Dibatalkan && x.SerialNumber != null && serials.Contains(x.SerialNumber.ToUpper()))
                .Select(x => new { x.Brand, x.SerialNumber })
                .ToListAsync(cancellationToken))
            .Select(x => SerialKey(x.Brand, x.SerialNumber))
            .ToHashSet();

        var policies = new Dictionary<(Guid, DateOnly), AstDepreciationPolicy?>();
        var seenSerials = new Dictionary<string, int>();
        var thisYear = AssetRegisterSettingReader.Today().Year;
        var result = new List<ValidatedImportRow>(rows.Count);

        foreach (var source in rows)
        {
            var row = new ValidatedImportRow { RowNumber = source.RowNumber, Source = source };
            result.Add(row);

            row.AssetName = Required(row, W.AssetName, "Nama Aset", 250) ?? string.Empty;
            row.Description = Optional(row, W.Description, "Keterangan", 1000);
            row.Brand = Optional(row, W.Brand, "Merek", 100);
            row.ModelType = Optional(row, W.ModelType, "Tipe/Model", 100);
            row.SerialNumber = Optional(row, W.SerialNumber, "Nomor Seri", 100);
            row.Manufacturer = Optional(row, W.Manufacturer, "Produsen", 150);
            row.DistributionPermitNumber = Optional(row, W.DistributionPermitNumber, "Nomor Izin Edar", 60);
            row.DistributorName = Optional(row, W.DistributorName, "Distributor", 150);
            row.SupplierName = Optional(row, W.SupplierName, "Pemasok", 200);

            var yearText = source.Text(W.ManufactureYear);
            if (yearText.Length > 0)
            {
                if (int.TryParse(yearText, out var year) && year >= 1900 && year <= thisYear)
                {
                    row.ManufactureYear = year;
                }
                else
                {
                    AddError(row, AssetRegisterErrorCodes.Field, AssetRegisterMessages.ImportYear);
                }
            }

            ImportGroup? group = null;
            var groupCode = Required(row, W.GroupCode, "Kode Kelompok", 11);
            if (groupCode != null)
            {
                if (groups.TryGetValue(groupCode.ToUpperInvariant(), out var found))
                {
                    group = found;
                    row.GroupId = found.Id;
                    row.InventoryCode = found.InventoryCode;
                }
                else
                {
                    AddError(row, AssetRegisterErrorCodes.ImportGroup, string.Format(AssetRegisterMessages.ImportGroupFormat, groupCode));
                }
            }

            bool? capitalizable = null;
            var ownershipCode = Required(row, W.OwnershipCode, "Kode Kepemilikan", 30);
            if (ownershipCode != null)
            {
                if (ownership.TryGetValue(ownershipCode.ToUpperInvariant(), out var found))
                {
                    row.OwnershipTypeId = found.Id;
                    capitalizable = found.IsCapitalizable;
                }
                else
                {
                    AddError(row, AssetRegisterErrorCodes.ImportGroup, string.Format(AssetRegisterMessages.ImportOwnershipFormat, ownershipCode));
                }
            }

            if (group is { IsMedical: true } &&
                new[] { row.Brand, row.ModelType, row.SerialNumber, row.Manufacturer, row.DistributionPermitNumber }.Any(string.IsNullOrWhiteSpace))
            {
                AddError(row, AssetRegisterErrorCodes.MedicalAttributes, AssetRegisterMessages.MedicalAttributes);
            }

            var acquisitionDate = RequiredDate(row, W.AcquisitionDate, "Tanggal Perolehan");
            var readyDate = RequiredDate(row, W.ReadyForUseDate, "Tanggal Siap Pakai");
            if (acquisitionDate is { } acquired && readyDate is { } ready)
            {
                row.AcquisitionDate = acquired;
                row.ReadyForUseDate = ready;
                if (ready < acquired)
                {
                    AddError(row, AssetRegisterErrorCodes.Dates, AssetRegisterMessages.Dates);
                }

                if (ready > cutoverDate)
                {
                    AddError(row, AssetRegisterErrorCodes.ImportReadyDate, AssetRegisterMessages.ImportReadyDate);
                }
            }

            row.AcquisitionCost = RequiredAmount(row, W.AcquisitionCost, "Harga Perolehan");
            if (row.AcquisitionCost is { } cost && (cost <= 0m || !AssetValuationMoney.HasAtMostTwoDecimals(cost)))
            {
                AddError(row, AssetRegisterErrorCodes.AcquisitionCost, AssetRegisterMessages.AcquisitionCost);
            }

            row.AccumulatedDepreciation = RequiredAmount(row, W.AccumulatedDepreciation, "Akumulasi Penyusutan");
            if (row.AccumulatedDepreciation is { } negative && negative < 0m)
            {
                AddError(row, AssetRegisterErrorCodes.ImportAccumulation, AssetValuationMessages.OpeningAccumulation);
            }

            var unitCode = Required(row, W.UnitCode, "Kode Unit", 50);
            var locationCode = Required(row, W.LocationCode, "Kode Lokasi", 30);
            var custodian = Required(row, W.CustodianNumber, "NIP Penanggung Jawab", 50);
            var unit = unitCode != null && units.TryGetValue(unitCode.ToUpperInvariant(), out var foundUnit) ? foundUnit : null;
            if (unitCode != null && unit == null)
            {
                AddError(row, AssetRegisterErrorCodes.ImportPlacement, string.Format(AssetRegisterMessages.ImportUnitFormat, unitCode));
            }

            if (locationCode != null)
            {
                if (locations.TryGetValue(locationCode.ToUpperInvariant(), out var location) && unit != null && location.OrganizationUnitId == unit.Id)
                {
                    row.LocationId = location.Id;
                    row.OrganizationUnitId = unit.Id;
                    row.PlacementLabel = $"{unit.UnitName} · {location.LocationName}";
                }
                else if (unit != null || unitCode == null)
                {
                    AddError(row, AssetRegisterErrorCodes.ImportPlacement, string.Format(AssetRegisterMessages.ImportLocationFormat, locationCode, unitCode ?? "-"));
                }
            }

            if (custodian != null)
            {
                if (employeeByKey.TryGetValue(custodian, out var employeeId))
                {
                    row.CustodianEmployeeId = employeeId;
                }
                else
                {
                    AddError(row, AssetRegisterErrorCodes.ImportPlacement, string.Format(AssetRegisterMessages.ImportCustodianFormat, custodian));
                }
            }

            if (row.SerialNumber != null)
            {
                var key = SerialKey(row.Brand, row.SerialNumber);
                if (existingSerials.Contains(key) || seenSerials.ContainsKey(key))
                {
                    AddError(row, AssetRegisterErrorCodes.ImportSerial, AssetRegisterMessages.ImportSerial);
                }

                seenSerials.TryAdd(key, row.RowNumber);
            }

            if (group != null && capitalizable is { } isCapitalizable && row.AcquisitionCost is > 0m && row.AccumulatedDepreciation is >= 0m && acquisitionDate != null && readyDate != null)
            {
                var startPeriod = AssetBookService.GetDepreciationStartPeriod(row.ReadyForUseDate);
                AstDepreciationPolicy? policy = null;
                var depreciable = group.IsDepreciableCategory && isCapitalizable && row.AcquisitionCost.Value >= parameter.CapitalizationThreshold;
                if (depreciable && !policies.TryGetValue((group.Id, startPeriod), out policy))
                {
                    policy = await _parameterQuery.GetPolicyAtAsync(group.Id, startPeriod, cancellationToken);
                    policies[(group.Id, startPeriod)] = policy;
                }

                var code = AssetBookService.CheckOpeningBalance(
                    row.AcquisitionCost.Value,
                    row.AccumulatedDepreciation.Value,
                    isCapitalizable,
                    group.IsDepreciableCategory,
                    parameter.CapitalizationThreshold,
                    depreciable ? policy?.UsefulLifeMonths ?? 0 : 0,
                    depreciable ? policy?.ResidualValuePercent ?? 0m : 0m,
                    row.ReadyForUseDate,
                    cutoverDate);
                if (code == AssetValuationErrorCodes.OpeningAccumulation)
                {
                    AddError(row, AssetRegisterErrorCodes.ImportAccumulation, AssetValuationMessages.OpeningAccumulation);
                }
                else if (code == AssetValuationErrorCodes.OpeningBalanceExhausted)
                {
                    AddError(row, AssetValuationErrorCodes.OpeningBalanceExhausted, AssetValuationMessages.OpeningBalanceExhausted);
                }
            }
        }

        return result;
    }

    private static string SerialKey(string? brand, string? serial) =>
        $"{(brand ?? string.Empty).Trim().ToUpperInvariant()}|{(serial ?? string.Empty).Trim().ToUpperInvariant()}";

    private static void AddError(ValidatedImportRow row, string code, string message) =>
        row.Errors.Add($"{code} {message}");

    private static string? Required(ValidatedImportRow row, string key, string label, int maxLength)
    {
        var value = row.Source.Text(key);
        if (value.Length == 0)
        {
            AddError(row, AssetRegisterErrorCodes.Field, string.Format(AssetRegisterMessages.ImportRequiredFormat, label));
            return null;
        }

        if (value.Length > maxLength)
        {
            AddError(row, AssetRegisterErrorCodes.Field, string.Format(AssetRegisterMessages.ImportLengthFormat, label, maxLength));
            return null;
        }

        return value;
    }

    private static string? Optional(ValidatedImportRow row, string key, string label, int maxLength)
    {
        var value = row.Source.Text(key);
        if (value.Length == 0)
        {
            return null;
        }

        if (value.Length > maxLength)
        {
            AddError(row, AssetRegisterErrorCodes.Field, string.Format(AssetRegisterMessages.ImportLengthFormat, label, maxLength));
            return null;
        }

        return value;
    }

    private static DateOnly? RequiredDate(ValidatedImportRow row, string key, string label)
    {
        var value = Required(row, key, label, 30);
        if (value == null)
        {
            return null;
        }

        if (W.TryParseDate(value, out var date))
        {
            return date;
        }

        AddError(row, AssetRegisterErrorCodes.Field, string.Format(AssetRegisterMessages.ImportDateFormat, label));
        return null;
    }

    private static decimal? RequiredAmount(ValidatedImportRow row, string key, string label)
    {
        var value = Required(row, key, label, 30);
        if (value == null)
        {
            return null;
        }

        if (W.TryParseAmount(value, out var amount))
        {
            return amount;
        }

        AddError(row, AssetRegisterErrorCodes.Field, string.Format(AssetRegisterMessages.ImportAmountFormat, label));
        return null;
    }

    private sealed record ImportGroup(Guid Id, string InventoryCode, bool IsMedical, bool IsDepreciableCategory);
}
