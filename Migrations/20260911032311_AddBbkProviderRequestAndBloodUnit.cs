using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBbkProviderRequestAndBloodUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BbkProviderRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BloodOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_BbkProviderRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkProviderRequest_BbkBloodOrder_BloodOrderId",
                        column: x => x.BloodOrderId,
                        principalSchema: "public",
                        principalTable: "BbkBloodOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkProviderRequest_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BbkBloodUnitReceipt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "integer", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BbkBloodUnitReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnitReceipt_BbkProviderRequest_ProviderRequestId",
                        column: x => x.ProviderRequestId,
                        principalSchema: "public",
                        principalTable: "BbkProviderRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BbkBloodUnit",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PmiBagNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsExcess = table.Column<bool>(type: "boolean", nullable: false),
                    UnitStatus = table.Column<int>(type: "integer", nullable: false),
                    IssuedToPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedViaEmergency = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_BbkBloodUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnit_BbkBloodUnitReceipt_ReceiptId",
                        column: x => x.ReceiptId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnitReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnit_BbkProviderRequest_ProviderRequestId",
                        column: x => x.ProviderRequestId,
                        principalSchema: "public",
                        principalTable: "BbkProviderRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnit_MstBloodComponent_BloodComponentId",
                        column: x => x.BloodComponentId,
                        principalSchema: "public",
                        principalTable: "MstBloodComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnit_MstPatient_IssuedToPatientId",
                        column: x => x.IssuedToPatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnit_BloodComponentId",
                schema: "public",
                table: "BbkBloodUnit",
                column: "BloodComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnit_IssuedToPatientId",
                schema: "public",
                table: "BbkBloodUnit",
                column: "IssuedToPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnit_PmiBagNumber",
                schema: "public",
                table: "BbkBloodUnit",
                column: "PmiBagNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnit_ProviderRequestId",
                schema: "public",
                table: "BbkBloodUnit",
                column: "ProviderRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnit_ReceiptId",
                schema: "public",
                table: "BbkBloodUnit",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnit_UnitStatus",
                schema: "public",
                table: "BbkBloodUnit",
                column: "UnitStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitReceipt_ProviderRequestId",
                schema: "public",
                table: "BbkBloodUnitReceipt",
                column: "ProviderRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitReceipt_ProviderRequestId_Sequence",
                schema: "public",
                table: "BbkBloodUnitReceipt",
                columns: new[] { "ProviderRequestId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitReceipt_ReceivedAt",
                schema: "public",
                table: "BbkBloodUnitReceipt",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BbkProviderRequest_BloodOrderId",
                schema: "public",
                table: "BbkProviderRequest",
                column: "BloodOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkProviderRequest_BloodOrderId_Active",
                schema: "public",
                table: "BbkProviderRequest",
                column: "BloodOrderId",
                unique: true,
                filter: "\"RequestStatus\" IN (0, 1) AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BbkProviderRequest_PatientId",
                schema: "public",
                table: "BbkProviderRequest",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkProviderRequest_RequestNumber",
                schema: "public",
                table: "BbkProviderRequest",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BbkProviderRequest_RequestStatus",
                schema: "public",
                table: "BbkProviderRequest",
                column: "RequestStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BbkBloodUnit",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BbkBloodUnitReceipt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BbkProviderRequest",
                schema: "public");
        }
    }
}
