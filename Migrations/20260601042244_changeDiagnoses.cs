using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeDiagnoses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDrug_MstTariff_DefaultTariffId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.RenameColumn(
                name: "DefaultTariffId",
                schema: "public",
                table: "MstDrug",
                newName: "StrengthMeasurementId");

            migrationBuilder.RenameIndex(
                name: "IX_MstDrug_DefaultTariffId",
                schema: "public",
                table: "MstDrug",
                newName: "IX_MstDrug_StrengthMeasurementId");

            migrationBuilder.AddColumn<Guid>(
                name: "BaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultDoseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DispenseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowFractionalDispense",
                schema: "public",
                table: "MstDrug",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBatchTracked",
                schema: "public",
                table: "MstDrug",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompoundIngredientAllowed",
                schema: "public",
                table: "MstDrug",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpiryDateTracked",
                schema: "public",
                table: "MstDrug",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStockManaged",
                schema: "public",
                table: "MstDrug",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StockUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StrengthValue",
                schema: "public",
                table: "MstDrug",
                type: "numeric(18,6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstDiagnosisChapter",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChapterCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChapterName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DiagnosisCodeRangeStart = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DiagnosisCodeRangeEnd = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IcdVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "ICD-10"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDiagnosisChapter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstDrugStorageLocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentStorageLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    StorageLocationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StorageLocationName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StorageLocationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    LocationGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RoomName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RackCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShelfCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BinCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MinimumTemperatureCelsius = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    MaximumTemperatureCelsius = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    MinimumHumidityPercent = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    MaximumHumidityPercent = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsMainWarehouse = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPharmacyLocation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsColdChain = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsControlledDrugStorage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHighAlertStorage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsQuarantineLocation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAllowReceiving = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowDispensing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowTransferIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowTransferOut = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDrugStorageLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDrugStorageLocation_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugStorageLocation_MstDrugStorageLocation_ParentStorage~",
                        column: x => x.ParentStorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugStorageLocation_MstRoom_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "MstRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugStorageLocation_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstMeasurement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MeasurementName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MeasurementSymbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MeasurementType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    MeasurementGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsBaseUnit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDecimalAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DecimalPrecision = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    IsForDrug = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsForLaboratory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsForVitalSign = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsForGeneralUse = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstMeasurement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstDiagnosis",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagnosisChapterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentDiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiagnosisName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DiagnosisNameEnglish = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DiagnosisGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DiagnosisCategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DiagnosisType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "ICD10"),
                    IcdVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "ICD-10"),
                    IsSelectableForClinicalUse = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsBillable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPrimaryDiagnosisAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSecondaryDiagnosisAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsChronicDisease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsInfectiousDisease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsExternalCause = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPregnancyRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsMentalHealthRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRareDisease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    GenderRestriction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MinimumAgeYear = table.Column<int>(type: "integer", nullable: true),
                    MaximumAgeYear = table.Column<int>(type: "integer", nullable: true),
                    ExternalDiagnosisCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IntegrationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDiagnosis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDiagnosis_MstDiagnosis_ParentDiagnosisId",
                        column: x => x.ParentDiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDiagnosis_MstDiagnosisChapter_DiagnosisChapterId",
                        column: x => x.DiagnosisChapterId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosisChapter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDrugStockPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockPolicyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StockPolicyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MinimumStockQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    MaximumStockQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    ReorderPointQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    ReorderQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    SafetyStockQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    CriticalStockQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExpiryWarningDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 90),
                    NearExpiryWarningDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    IsAutoReorderEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAllowNegativeStock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBatchRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsExpiryDateRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsStockOpnameRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StockOpnameIntervalDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDrugStockPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDrugStockPolicy_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugStockPolicy_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugStockPolicy_MstDrugStorageLocation_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugStockPolicy_MstMeasurement_StockUnitMeasurementId",
                        column: x => x.StockUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugStockPolicy_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDrugUnitConversion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConversionName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FromMeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToMeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromQuantity = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    ToQuantity = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    ConversionFactor = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    ConversionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBidirectional = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForPurchase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsForStock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForDispensing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForPrescription = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForCompound = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDrugUnitConversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDrugUnitConversion_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugUnitConversion_MstMeasurement_FromMeasurementId",
                        column: x => x.FromMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugUnitConversion_MstMeasurement_ToMeasurementId",
                        column: x => x.ToMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstMeasurementConversion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromMeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToMeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    IsBidirectional = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsStandardConversion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ConversionGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FormulaNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstMeasurementConversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstMeasurementConversion_MstMeasurement_FromMeasurementId",
                        column: x => x.FromMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstMeasurementConversion_MstMeasurement_ToMeasurementId",
                        column: x => x.ToMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_BaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "BaseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_BaseUnitMeasurementId_DispenseUnitMeasurementId_Sto~",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "BaseUnitMeasurementId", "DispenseUnitMeasurementId", "StockUnitMeasurementId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DefaultDoseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "DefaultDoseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DispenseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "DispenseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DrugCategoryId_DrugForm_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "DrugCategoryId", "DrugForm", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DrugForm",
                schema: "public",
                table: "MstDrug",
                column: "DrugForm");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsCompoundIngredientAllowed_IsAllowFractionalDispen~",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsCompoundIngredientAllowed", "IsAllowFractionalDispense", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsStockManaged_IsBatchTracked_IsExpiryDateTracked_I~",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsStockManaged", "IsBatchTracked", "IsExpiryDateTracked", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_NationalDrugCode",
                schema: "public",
                table: "MstDrug",
                column: "NationalDrugCode",
                filter: "\"NationalDrugCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_PurchaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "PurchaseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_Route",
                schema: "public",
                table: "MstDrug",
                column: "Route");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_StockUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "StockUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisChapterId",
                schema: "public",
                table: "MstDiagnosis",
                column: "DiagnosisChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisChapterId_IsActive_IsDelete",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "DiagnosisChapterId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisCode",
                schema: "public",
                table: "MstDiagnosis",
                column: "DiagnosisCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisName",
                schema: "public",
                table: "MstDiagnosis",
                column: "DiagnosisName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisType",
                schema: "public",
                table: "MstDiagnosis",
                column: "DiagnosisType");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisType_IcdVersion_IsSelectableForClinic~",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "DiagnosisType", "IcdVersion", "IsSelectableForClinicalUse", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_ExternalDiagnosisCode",
                schema: "public",
                table: "MstDiagnosis",
                column: "ExternalDiagnosisCode",
                filter: "\"ExternalDiagnosisCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_GenderRestriction_MinimumAgeYear_MaximumAgeYear",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "GenderRestriction", "MinimumAgeYear", "MaximumAgeYear" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IcdVersion",
                schema: "public",
                table: "MstDiagnosis",
                column: "IcdVersion");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IntegrationCode",
                schema: "public",
                table: "MstDiagnosis",
                column: "IntegrationCode",
                filter: "\"IntegrationCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IsBillable_IsPrimaryDiagnosisAllowed_IsSeconda~",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "IsBillable", "IsPrimaryDiagnosisAllowed", "IsSecondaryDiagnosisAllowed", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IsChronicDisease_IsInfectiousDisease_IsExterna~",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "IsChronicDisease", "IsInfectiousDisease", "IsExternalCause", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IsPregnancyRelated_IsMentalHealthRelated_IsRar~",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "IsPregnancyRelated", "IsMentalHealthRelated", "IsRareDisease", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_ParentDiagnosisId",
                schema: "public",
                table: "MstDiagnosis",
                column: "ParentDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_ParentDiagnosisId_IsActive_IsDelete",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "ParentDiagnosisId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_ShortName",
                schema: "public",
                table: "MstDiagnosis",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisChapter_ChapterCode",
                schema: "public",
                table: "MstDiagnosisChapter",
                column: "ChapterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisChapter_ChapterName",
                schema: "public",
                table: "MstDiagnosisChapter",
                column: "ChapterName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisChapter_DiagnosisCodeRangeStart_DiagnosisCodeRa~",
                schema: "public",
                table: "MstDiagnosisChapter",
                columns: new[] { "DiagnosisCodeRangeStart", "DiagnosisCodeRangeEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisChapter_IcdVersion",
                schema: "public",
                table: "MstDiagnosisChapter",
                column: "IcdVersion");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisChapter_IcdVersion_IsActive_IsDelete",
                schema: "public",
                table: "MstDiagnosisChapter",
                columns: new[] { "IcdVersion", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_ClinicId",
                schema: "public",
                table: "MstDrugStockPolicy",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_DrugId",
                schema: "public",
                table: "MstDrugStockPolicy",
                column: "DrugId",
                unique: true,
                filter: "\"StorageLocationId\" IS NULL AND \"ServiceUnitId\" IS NULL AND \"ClinicId\" IS NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_DrugId_ServiceUnitId_ClinicId",
                schema: "public",
                table: "MstDrugStockPolicy",
                columns: new[] { "DrugId", "ServiceUnitId", "ClinicId" },
                unique: true,
                filter: "\"StorageLocationId\" IS NULL AND \"ClinicId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_DrugId_StockUnitMeasurementId_IsActive_I~",
                schema: "public",
                table: "MstDrugStockPolicy",
                columns: new[] { "DrugId", "StockUnitMeasurementId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_DrugId_StorageLocationId",
                schema: "public",
                table: "MstDrugStockPolicy",
                columns: new[] { "DrugId", "StorageLocationId" },
                unique: true,
                filter: "\"StorageLocationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_EffectiveStartDate_EffectiveEndDate_IsAc~",
                schema: "public",
                table: "MstDrugStockPolicy",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_ExpiryWarningDays_NearExpiryWarningDays_~",
                schema: "public",
                table: "MstDrugStockPolicy",
                columns: new[] { "ExpiryWarningDays", "NearExpiryWarningDays", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_IsAutoReorderEnabled_IsAllowNegativeStoc~",
                schema: "public",
                table: "MstDrugStockPolicy",
                columns: new[] { "IsAutoReorderEnabled", "IsAllowNegativeStock", "IsBatchRequired", "IsExpiryDateRequired", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_MinimumStockQuantity_MaximumStockQuantit~",
                schema: "public",
                table: "MstDrugStockPolicy",
                columns: new[] { "MinimumStockQuantity", "MaximumStockQuantity", "ReorderPointQuantity", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_ServiceUnitId",
                schema: "public",
                table: "MstDrugStockPolicy",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_StockPolicyCode",
                schema: "public",
                table: "MstDrugStockPolicy",
                column: "StockPolicyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_StockUnitMeasurementId",
                schema: "public",
                table: "MstDrugStockPolicy",
                column: "StockUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStockPolicy_StorageLocationId",
                schema: "public",
                table: "MstDrugStockPolicy",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_ClinicId",
                schema: "public",
                table: "MstDrugStorageLocation",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_IsAllowReceiving_IsAllowDispensing_I~",
                schema: "public",
                table: "MstDrugStorageLocation",
                columns: new[] { "IsAllowReceiving", "IsAllowDispensing", "IsAllowTransferIn", "IsAllowTransferOut", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_IsColdChain_IsControlledDrugStorage_~",
                schema: "public",
                table: "MstDrugStorageLocation",
                columns: new[] { "IsColdChain", "IsControlledDrugStorage", "IsHighAlertStorage", "IsQuarantineLocation", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_IsDefault_IsMainWarehouse_IsPharmacy~",
                schema: "public",
                table: "MstDrugStorageLocation",
                columns: new[] { "IsDefault", "IsMainWarehouse", "IsPharmacyLocation", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_ParentStorageLocationId",
                schema: "public",
                table: "MstDrugStorageLocation",
                column: "ParentStorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_ParentStorageLocationId_StorageLocat~",
                schema: "public",
                table: "MstDrugStorageLocation",
                columns: new[] { "ParentStorageLocationId", "StorageLocationName", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_RoomId",
                schema: "public",
                table: "MstDrugStorageLocation",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_ServiceUnitId",
                schema: "public",
                table: "MstDrugStorageLocation",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_ServiceUnitId_ClinicId_StorageLocati~",
                schema: "public",
                table: "MstDrugStorageLocation",
                columns: new[] { "ServiceUnitId", "ClinicId", "StorageLocationType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_StorageLocationCode",
                schema: "public",
                table: "MstDrugStorageLocation",
                column: "StorageLocationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_StorageLocationName",
                schema: "public",
                table: "MstDrugStorageLocation",
                column: "StorageLocationName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugStorageLocation_StorageLocationType",
                schema: "public",
                table: "MstDrugStorageLocation",
                column: "StorageLocationType");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugUnitConversion_ConversionCode",
                schema: "public",
                table: "MstDrugUnitConversion",
                column: "ConversionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugUnitConversion_DrugId",
                schema: "public",
                table: "MstDrugUnitConversion",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugUnitConversion_DrugId_ConversionType_IsActive_IsDele~",
                schema: "public",
                table: "MstDrugUnitConversion",
                columns: new[] { "DrugId", "ConversionType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugUnitConversion_DrugId_FromMeasurementId_ToMeasuremen~",
                schema: "public",
                table: "MstDrugUnitConversion",
                columns: new[] { "DrugId", "FromMeasurementId", "ToMeasurementId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugUnitConversion_DrugId_IsForPurchase_IsForStock_IsFor~",
                schema: "public",
                table: "MstDrugUnitConversion",
                columns: new[] { "DrugId", "IsForPurchase", "IsForStock", "IsForDispensing", "IsForPrescription", "IsForCompound", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugUnitConversion_EffectiveStartDate_EffectiveEndDate_I~",
                schema: "public",
                table: "MstDrugUnitConversion",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugUnitConversion_FromMeasurementId",
                schema: "public",
                table: "MstDrugUnitConversion",
                column: "FromMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugUnitConversion_ToMeasurementId",
                schema: "public",
                table: "MstDrugUnitConversion",
                column: "ToMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurement_IsForDrug_IsForLaboratory_IsForVitalSign_IsF~",
                schema: "public",
                table: "MstMeasurement",
                columns: new[] { "IsForDrug", "IsForLaboratory", "IsForVitalSign", "IsForGeneralUse", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurement_MeasurementCode",
                schema: "public",
                table: "MstMeasurement",
                column: "MeasurementCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurement_MeasurementName",
                schema: "public",
                table: "MstMeasurement",
                column: "MeasurementName");

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurement_MeasurementSymbol",
                schema: "public",
                table: "MstMeasurement",
                column: "MeasurementSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurement_MeasurementType",
                schema: "public",
                table: "MstMeasurement",
                column: "MeasurementType");

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurement_MeasurementType_MeasurementGroupName_IsActiv~",
                schema: "public",
                table: "MstMeasurement",
                columns: new[] { "MeasurementType", "MeasurementGroupName", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurementConversion_ConversionGroupName_IsStandardConv~",
                schema: "public",
                table: "MstMeasurementConversion",
                columns: new[] { "ConversionGroupName", "IsStandardConversion", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurementConversion_FromMeasurementId",
                schema: "public",
                table: "MstMeasurementConversion",
                column: "FromMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurementConversion_FromMeasurementId_ToMeasurementId",
                schema: "public",
                table: "MstMeasurementConversion",
                columns: new[] { "FromMeasurementId", "ToMeasurementId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstMeasurementConversion_ToMeasurementId",
                schema: "public",
                table: "MstMeasurementConversion",
                column: "ToMeasurementId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrug_MstMeasurement_BaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "BaseUnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrug_MstMeasurement_DefaultDoseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "DefaultDoseUnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrug_MstMeasurement_DispenseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "DispenseUnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrug_MstMeasurement_PurchaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "PurchaseUnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrug_MstMeasurement_StockUnitMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "StockUnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrug_MstMeasurement_StrengthMeasurementId",
                schema: "public",
                table: "MstDrug",
                column: "StrengthMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDrug_MstMeasurement_BaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDrug_MstMeasurement_DefaultDoseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDrug_MstMeasurement_DispenseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDrug_MstMeasurement_PurchaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDrug_MstMeasurement_StockUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDrug_MstMeasurement_StrengthMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropTable(
                name: "MstDiagnosis",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDrugStockPolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDrugUnitConversion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMeasurementConversion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDiagnosisChapter",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDrugStorageLocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMeasurement",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_BaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_BaseUnitMeasurementId_DispenseUnitMeasurementId_Sto~",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_DefaultDoseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_DispenseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_DrugCategoryId_DrugForm_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_DrugForm",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_IsCompoundIngredientAllowed_IsAllowFractionalDispen~",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_IsStockManaged_IsBatchTracked_IsExpiryDateTracked_I~",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_NationalDrugCode",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_PurchaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_Route",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_StockUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "BaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "DefaultDoseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "DispenseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "IsAllowFractionalDispense",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "IsBatchTracked",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "IsCompoundIngredientAllowed",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "IsExpiryDateTracked",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "IsStockManaged",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "PurchaseUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "StockUnitMeasurementId",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "StrengthValue",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.RenameColumn(
                name: "StrengthMeasurementId",
                schema: "public",
                table: "MstDrug",
                newName: "DefaultTariffId");

            migrationBuilder.RenameIndex(
                name: "IX_MstDrug_StrengthMeasurementId",
                schema: "public",
                table: "MstDrug",
                newName: "IX_MstDrug_DefaultTariffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrug_MstTariff_DefaultTariffId",
                schema: "public",
                table: "MstDrug",
                column: "DefaultTariffId",
                principalSchema: "public",
                principalTable: "MstTariff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
