using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetValuationProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AstDepreciationRun",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<DateOnly>(type: "date", nullable: false),
                    AssetCount = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustmentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProcessedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AstDepreciationRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstDepreciationRun_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstSubledgerAccountSetup",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostControlAccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccumulatedDepreciationControlAccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccumulatedImpairmentControlAccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_AstSubledgerAccountSetup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstSubledgerAccountSetup_MstAssetGroup_AssetGroupId",
                        column: x => x.AssetGroupId,
                        principalSchema: "public",
                        principalTable: "MstAssetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstUnitCostCenter",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AstUnitCostCenter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstUnitCostCenter_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstUnitCostCenter_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstValueChangeRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AdditionalLifeMonths = table.Column<int>(type: "integer", nullable: true),
                    NewUsefulLifeMonths = table.Column<int>(type: "integer", nullable: true),
                    NewResidualValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    EvidenceAttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AppliedEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AstValueChangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstValueChangeRequest_AstAssetValueEntry_AppliedEntryId",
                        column: x => x.AppliedEntryId,
                        principalSchema: "public",
                        principalTable: "AstAssetValueEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstValueChangeRequest_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstValueChangeRequest_AstAttachment_EvidenceAttachmentId",
                        column: x => x.EvidenceAttachmentId,
                        principalSchema: "public",
                        principalTable: "AstAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstDepreciationRunLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustmentReason = table.Column<int>(type: "integer", nullable: false),
                    AdjustmentPeriod = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_AstDepreciationRunLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstDepreciationRunLine_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstDepreciationRunLine_AstDepreciationRun_RunId",
                        column: x => x.RunId,
                        principalSchema: "public",
                        principalTable: "AstDepreciationRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstDepreciationRunLine_MstAssetGroup_AssetGroupId",
                        column: x => x.AssetGroupId,
                        principalSchema: "public",
                        principalTable: "MstAssetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstDepreciationRunLine_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstDepreciationRunLine_MstOrganizationUnit_OrganizationUnit~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AstDepreciationRun_Entity_Period",
                schema: "public",
                table: "AstDepreciationRun",
                columns: new[] { "LegalEntityId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AstDepreciationRunLine_AssetGroupId",
                schema: "public",
                table: "AstDepreciationRunLine",
                column: "AssetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AstDepreciationRunLine_AssetId",
                schema: "public",
                table: "AstDepreciationRunLine",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstDepreciationRunLine_CostCenterId",
                schema: "public",
                table: "AstDepreciationRunLine",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_AstDepreciationRunLine_OrganizationUnitId",
                schema: "public",
                table: "AstDepreciationRunLine",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AstDepreciationRunLine_Run_Asset_Reason",
                schema: "public",
                table: "AstDepreciationRunLine",
                columns: new[] { "RunId", "AssetId", "AdjustmentReason" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AstDepreciationRunLine_Run_Group_CostCenter",
                schema: "public",
                table: "AstDepreciationRunLine",
                columns: new[] { "RunId", "AssetGroupId", "CostCenterId" });

            migrationBuilder.CreateIndex(
                name: "IX_AstSubledgerAccountSetup_AssetGroupId",
                schema: "public",
                table: "AstSubledgerAccountSetup",
                column: "AssetGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AstUnitCostCenter_CostCenterId",
                schema: "public",
                table: "AstUnitCostCenter",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_AstUnitCostCenter_OrganizationUnitId",
                schema: "public",
                table: "AstUnitCostCenter",
                column: "OrganizationUnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AstValueChangeRequest_AppliedEntryId",
                schema: "public",
                table: "AstValueChangeRequest",
                column: "AppliedEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_AstValueChangeRequest_ChangeType",
                schema: "public",
                table: "AstValueChangeRequest",
                column: "ChangeType");

            migrationBuilder.CreateIndex(
                name: "IX_AstValueChangeRequest_EvidenceAttachmentId",
                schema: "public",
                table: "AstValueChangeRequest",
                column: "EvidenceAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstValueChangeRequest_OpenPerAsset",
                schema: "public",
                table: "AstValueChangeRequest",
                column: "AssetId",
                unique: true,
                filter: "\"Status\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstValueChangeRequest_RequestNumber",
                schema: "public",
                table: "AstValueChangeRequest",
                column: "RequestNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstValueChangeRequest_Status",
                schema: "public",
                table: "AstValueChangeRequest",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AstDepreciationRunLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstSubledgerAccountSetup",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstUnitCostCenter",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstValueChangeRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstDepreciationRun",
                schema: "public");
        }
    }
}
