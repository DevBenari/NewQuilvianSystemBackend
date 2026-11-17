using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

[assembly: AccessExplicitPermission(
    moduleCode: "CORPORATE_ASSET_MANAGEMENT_REGISTER",
    resourceName: "AssetRegister",
    actionName: "ReadAllUnits",
    displayName: "Read All Units",
    accessType: AccessTypes.Read,
    Description = "Melihat dan mengelola aset seluruh unit, termasuk aset yang belum ditempatkan",
    SortOrder = 7)]
