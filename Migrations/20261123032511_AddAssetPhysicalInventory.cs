using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetPhysicalInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AstStockTakeSession",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlannedEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpectedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ScannedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StartedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CountingClosedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CountingClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AstStockTakeSession", x => x.Id);
                    table.CheckConstraint("CK_AstStockTakeSession_Dates", "\"PlannedEndDate\" >= \"PlannedStartDate\"");
                    table.ForeignKey(
                        name: "FK_AstStockTakeSession_MstAssetLocation_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeSession_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstStockTakeExpectedItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedCustodianEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedCondition = table.Column<int>(type: "integer", nullable: false),
                    LoanBorrowerOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AstStockTakeExpectedItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstStockTakeExpectedItem_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeExpectedItem_AstStockTakeSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "AstStockTakeSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeExpectedItem_MstAssetLocation_ExpectedLocationId",
                        column: x => x.ExpectedLocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeExpectedItem_MstEmployee_ExpectedCustodianEmplo~",
                        column: x => x.ExpectedCustodianEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeExpectedItem_MstOrganizationUnit_LoanBorrowerOr~",
                        column: x => x.LoanBorrowerOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstStockTakeScan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScannedCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    IsManualEntry = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsUnregisteredFinding = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FindingDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FoundLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservedCondition = table.Column<int>(type: "integer", nullable: false),
                    PhotoAttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ScannedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_AstStockTakeScan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstStockTakeScan_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeScan_AstAttachment_PhotoAttachmentId",
                        column: x => x.PhotoAttachmentId,
                        principalSchema: "public",
                        principalTable: "AstAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeScan_AstStockTakeSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "AstStockTakeSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeScan_MstAssetLocation_FoundLocationId",
                        column: x => x.FoundLocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeScan_MstEmployee_ScannedByEmployeeId",
                        column: x => x.ScannedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstStockTakeTeamMember",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsLeader = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_AstStockTakeTeamMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstStockTakeTeamMember_AstStockTakeSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "AstStockTakeSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeTeamMember_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstStockTakeResult",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    IsLocationMismatch = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConditionMismatch = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsForeignAsset = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExpectedLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    FoundLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpectedCondition = table.Column<int>(type: "integer", nullable: true),
                    ObservedCondition = table.Column<int>(type: "integer", nullable: true),
                    TeamNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AstStockTakeResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstStockTakeResult_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeResult_AstStockTakeScan_ScanId",
                        column: x => x.ScanId,
                        principalSchema: "public",
                        principalTable: "AstStockTakeScan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeResult_AstStockTakeSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "AstStockTakeSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeResult_MstAssetLocation_ExpectedLocationId",
                        column: x => x.ExpectedLocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeResult_MstAssetLocation_FoundLocationId",
                        column: x => x.FoundLocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstStockTakeFollowUp",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TargetDocumentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TargetDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetDocumentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DismissedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DismissedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DismissReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AstStockTakeFollowUp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstStockTakeFollowUp_AstStockTakeResult_ResultId",
                        column: x => x.ResultId,
                        principalSchema: "public",
                        principalTable: "AstStockTakeResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstStockTakeFollowUp_AstStockTakeSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "AstStockTakeSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeExpectedItem_AssetId",
                schema: "public",
                table: "AstStockTakeExpectedItem",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeExpectedItem_ExpectedCustodianEmployeeId",
                schema: "public",
                table: "AstStockTakeExpectedItem",
                column: "ExpectedCustodianEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeExpectedItem_ExpectedLocationId",
                schema: "public",
                table: "AstStockTakeExpectedItem",
                column: "ExpectedLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeExpectedItem_LoanBorrowerOrganizationUnitId",
                schema: "public",
                table: "AstStockTakeExpectedItem",
                column: "LoanBorrowerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeExpectedItem_Session_Asset",
                schema: "public",
                table: "AstStockTakeExpectedItem",
                columns: new[] { "SessionId", "AssetId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeFollowUp_Result_Type",
                schema: "public",
                table: "AstStockTakeFollowUp",
                columns: new[] { "ResultId", "Type" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeFollowUp_SessionId",
                schema: "public",
                table: "AstStockTakeFollowUp",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeFollowUp_Status",
                schema: "public",
                table: "AstStockTakeFollowUp",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeFollowUp_Type",
                schema: "public",
                table: "AstStockTakeFollowUp",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeResult_AssetId",
                schema: "public",
                table: "AstStockTakeResult",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeResult_ExpectedLocationId",
                schema: "public",
                table: "AstStockTakeResult",
                column: "ExpectedLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeResult_FoundLocationId",
                schema: "public",
                table: "AstStockTakeResult",
                column: "FoundLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeResult_ScanId",
                schema: "public",
                table: "AstStockTakeResult",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeResult_Session_Category",
                schema: "public",
                table: "AstStockTakeResult",
                columns: new[] { "SessionId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeScan_AssetId",
                schema: "public",
                table: "AstStockTakeScan",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeScan_FoundLocationId",
                schema: "public",
                table: "AstStockTakeScan",
                column: "FoundLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeScan_PhotoAttachmentId",
                schema: "public",
                table: "AstStockTakeScan",
                column: "PhotoAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeScan_ScannedByEmployeeId",
                schema: "public",
                table: "AstStockTakeScan",
                column: "ScannedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeScan_Session_Asset_Latest",
                schema: "public",
                table: "AstStockTakeScan",
                columns: new[] { "SessionId", "AssetId", "IsLatest" });

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeSession_ActivePerUnit",
                schema: "public",
                table: "AstStockTakeSession",
                column: "OrganizationUnitId",
                unique: true,
                filter: "\"Status\" IN (2, 3, 4) AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeSession_LocationId",
                schema: "public",
                table: "AstStockTakeSession",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeSession_PlannedEndDate",
                schema: "public",
                table: "AstStockTakeSession",
                column: "PlannedEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeSession_SessionNumber",
                schema: "public",
                table: "AstStockTakeSession",
                column: "SessionNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeSession_Status",
                schema: "public",
                table: "AstStockTakeSession",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeTeamMember_EmployeeId",
                schema: "public",
                table: "AstStockTakeTeamMember",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeTeamMember_LeaderPerSession",
                schema: "public",
                table: "AstStockTakeTeamMember",
                column: "SessionId",
                unique: true,
                filter: "\"IsLeader\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstStockTakeTeamMember_Session_Employee",
                schema: "public",
                table: "AstStockTakeTeamMember",
                columns: new[] { "SessionId", "EmployeeId" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AstStockTakeExpectedItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstStockTakeFollowUp",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstStockTakeTeamMember",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstStockTakeResult",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstStockTakeScan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstStockTakeSession",
                schema: "public");
        }
    }
}
