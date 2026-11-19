using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AstLoan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowerOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlannedReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OriginalPlannedReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsApprovalByRequester = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PickedUpBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PickedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReturnedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReturnConfirmedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReturnConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AstLoan", x => x.Id);
                    table.CheckConstraint("CK_AstLoan_DifferentUnits", "\"OwnerOrganizationUnitId\" <> \"BorrowerOrganizationUnitId\"");
                    table.CheckConstraint("CK_AstLoan_ReturnAfterStart", "\"PlannedReturnDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_AstLoan_MstOrganizationUnit_BorrowerOrganizationUnitId",
                        column: x => x.BorrowerOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstLoan_MstOrganizationUnit_OwnerOrganizationUnitId",
                        column: x => x.OwnerOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstTransfer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetCustodianEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    ReasonNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CorrectsTransferId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceStockTakeResultId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnitApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsUnitApprovalByRequester = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AssetHeadApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetHeadApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AstTransfer", x => x.Id);
                    table.CheckConstraint("CK_AstTransfer_DifferentUnits", "\"SourceOrganizationUnitId\" <> \"TargetOrganizationUnitId\"");
                    table.ForeignKey(
                        name: "FK_AstTransfer_AstTransfer_CorrectsTransferId",
                        column: x => x.CorrectsTransferId,
                        principalSchema: "public",
                        principalTable: "AstTransfer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstTransfer_MstAssetLocation_TargetLocationId",
                        column: x => x.TargetLocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstTransfer_MstEmployee_TargetCustodianEmployeeId",
                        column: x => x.TargetCustodianEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstTransfer_MstOrganizationUnit_SourceOrganizationUnitId",
                        column: x => x.SourceOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstTransfer_MstOrganizationUnit_TargetOrganizationUnitId",
                        column: x => x.TargetOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstJobDescription",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobDescriptionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    Purpose = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("PK_MstJobDescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstJobDescription_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstLoanExtension",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestedReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AstLoanExtension", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstLoanExtension_AstLoan_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "public",
                        principalTable: "AstLoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstLoanItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionAtPickup = table.Column<int>(type: "integer", nullable: false),
                    ReturnCondition = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_AstLoanItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstLoanItem_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstLoanItem_AstLoan_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "public",
                        principalTable: "AstLoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstTransferItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionAtSubmit = table.Column<int>(type: "integer", nullable: false),
                    ConditionAtReceipt = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_AstTransferItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstTransferItem_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstTransferItem_AstTransfer_TransferId",
                        column: x => x.TransferId,
                        principalSchema: "public",
                        principalTable: "AstTransfer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstJobDescriptionItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobDescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("PK_MstJobDescriptionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstJobDescriptionItem_MstJobDescription_JobDescriptionId",
                        column: x => x.JobDescriptionId,
                        principalSchema: "public",
                        principalTable: "MstJobDescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AstLoan_BorrowerOrganizationUnitId",
                schema: "public",
                table: "AstLoan",
                column: "BorrowerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AstLoan_LoanNumber",
                schema: "public",
                table: "AstLoan",
                column: "LoanNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstLoan_OwnerOrganizationUnitId",
                schema: "public",
                table: "AstLoan",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AstLoan_RequestedBy",
                schema: "public",
                table: "AstLoan",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AstLoan_Status_PlannedReturnDate",
                schema: "public",
                table: "AstLoan",
                columns: new[] { "Status", "PlannedReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AstLoanExtension_OpenPerLoan",
                schema: "public",
                table: "AstLoanExtension",
                column: "LoanId",
                unique: true,
                filter: "\"Status\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstLoanExtension_Status",
                schema: "public",
                table: "AstLoanExtension",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AstLoanItem_AssetId",
                schema: "public",
                table: "AstLoanItem",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstLoanItem_Loan_Asset",
                schema: "public",
                table: "AstLoanItem",
                columns: new[] { "LoanId", "AssetId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_CorrectsTransferId",
                schema: "public",
                table: "AstTransfer",
                column: "CorrectsTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_Reason",
                schema: "public",
                table: "AstTransfer",
                column: "Reason");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_RequestedBy",
                schema: "public",
                table: "AstTransfer",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_SourceOrganizationUnitId",
                schema: "public",
                table: "AstTransfer",
                column: "SourceOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_SourceStockTakeResultId",
                schema: "public",
                table: "AstTransfer",
                column: "SourceStockTakeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_Status_Units",
                schema: "public",
                table: "AstTransfer",
                columns: new[] { "Status", "SourceOrganizationUnitId", "TargetOrganizationUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_TargetCustodianEmployeeId",
                schema: "public",
                table: "AstTransfer",
                column: "TargetCustodianEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_TargetLocationId",
                schema: "public",
                table: "AstTransfer",
                column: "TargetLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_TargetOrganizationUnitId",
                schema: "public",
                table: "AstTransfer",
                column: "TargetOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransfer_TransferNumber",
                schema: "public",
                table: "AstTransfer",
                column: "TransferNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransferItem_AssetId",
                schema: "public",
                table: "AstTransferItem",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstTransferItem_Transfer_Asset",
                schema: "public",
                table: "AstTransferItem",
                columns: new[] { "TransferId", "AssetId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstJobDescription_JobDescriptionCode",
                schema: "public",
                table: "MstJobDescription",
                column: "JobDescriptionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstJobDescription_PositionId_Status",
                schema: "public",
                table: "MstJobDescription",
                columns: new[] { "PositionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MstJobDescription_PositionId_VersionNumber",
                schema: "public",
                table: "MstJobDescription",
                columns: new[] { "PositionId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstJobDescriptionItem_JobDescriptionId_SectionType_Sequence",
                schema: "public",
                table: "MstJobDescriptionItem",
                columns: new[] { "JobDescriptionId", "SectionType", "Sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AstLoanExtension",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstLoanItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstTransferItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstJobDescriptionItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstLoan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstTransfer",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstJobDescription",
                schema: "public");
        }
    }
}
