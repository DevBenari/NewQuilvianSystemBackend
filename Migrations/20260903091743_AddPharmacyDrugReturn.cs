using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyDrugReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxDrugReturn",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceDrugUsageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxDrugReturn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturn_MstDrugStorageLocation_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturn_MstWorkforceProfile_ReturnedByWorkforceId",
                        column: x => x.ReturnedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturn_MstWorkforceProfile_VerifiedByWorkforceId",
                        column: x => x.VerifiedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturn_TrxDrugUsage_SourceDrugUsageId",
                        column: x => x.SourceDrugUsageId,
                        principalSchema: "public",
                        principalTable: "TrxDrugUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturn_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxDrugReturnHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxDrugReturnHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturnHistory_TrxDrugReturn_DrugReturnId",
                        column: x => x.DrugReturnId,
                        principalSchema: "public",
                        principalTable: "TrxDrugReturn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrxDrugReturnItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MeasurementNameSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    AcceptedStatus = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxDrugReturnItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturnItem_MstDrugBatch_DrugBatchId",
                        column: x => x.DrugBatchId,
                        principalSchema: "public",
                        principalTable: "MstDrugBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturnItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturnItem_MstMeasurement_MeasurementId",
                        column: x => x.MeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugReturnItem_TrxDrugReturn_DrugReturnId",
                        column: x => x.DrugReturnId,
                        principalSchema: "public",
                        principalTable: "TrxDrugReturn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturn_EncounterId_ReturnedAt",
                schema: "public",
                table: "TrxDrugReturn",
                columns: new[] { "EncounterId", "ReturnedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturn_ReturnedByWorkforceId",
                schema: "public",
                table: "TrxDrugReturn",
                column: "ReturnedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturn_ReturnNumber",
                schema: "public",
                table: "TrxDrugReturn",
                column: "ReturnNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturn_SourceDrugUsageId",
                schema: "public",
                table: "TrxDrugReturn",
                column: "SourceDrugUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturn_Status_ReturnedAt",
                schema: "public",
                table: "TrxDrugReturn",
                columns: new[] { "Status", "ReturnedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturn_StorageLocationId_Status",
                schema: "public",
                table: "TrxDrugReturn",
                columns: new[] { "StorageLocationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturn_VerifiedByWorkforceId",
                schema: "public",
                table: "TrxDrugReturn",
                column: "VerifiedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturnHistory_Action_CorrelationId",
                schema: "public",
                table: "TrxDrugReturnHistory",
                columns: new[] { "Action", "CorrelationId" },
                unique: true,
                filter: "\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturnHistory_DrugReturnId_OccurredAt",
                schema: "public",
                table: "TrxDrugReturnHistory",
                columns: new[] { "DrugReturnId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturnItem_DrugBatchId",
                schema: "public",
                table: "TrxDrugReturnItem",
                column: "DrugBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturnItem_DrugId",
                schema: "public",
                table: "TrxDrugReturnItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturnItem_DrugReturnId_DrugBatchId",
                schema: "public",
                table: "TrxDrugReturnItem",
                columns: new[] { "DrugReturnId", "DrugBatchId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturnItem_MeasurementId",
                schema: "public",
                table: "TrxDrugReturnItem",
                column: "MeasurementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxDrugReturnHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxDrugReturnItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxDrugReturn",
                schema: "public");
        }
    }
}
