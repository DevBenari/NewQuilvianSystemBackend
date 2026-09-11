using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBbkBloodOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BbkBloodOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestingDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderSource = table.Column<int>(type: "integer", nullable: false),
                    InputByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BbkBloodOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodOrder_MstDoctor_RequestingDoctorId",
                        column: x => x.RequestingDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodOrder_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodOrder_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodOrder_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BbkTransitionHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ReasonNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BbkTransitionHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BbkBloodOrderLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BbkBloodOrderLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodOrderLine_BbkBloodOrder_BloodOrderId",
                        column: x => x.BloodOrderId,
                        principalSchema: "public",
                        principalTable: "BbkBloodOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodOrderLine_MstBloodComponent_BloodComponentId",
                        column: x => x.BloodComponentId,
                        principalSchema: "public",
                        principalTable: "MstBloodComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrder_EncounterId",
                schema: "public",
                table: "BbkBloodOrder",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrder_OrderNumber",
                schema: "public",
                table: "BbkBloodOrder",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrder_PatientId_OrderStatus",
                schema: "public",
                table: "BbkBloodOrder",
                columns: new[] { "PatientId", "OrderStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrder_RequestingDoctorId",
                schema: "public",
                table: "BbkBloodOrder",
                column: "RequestingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrder_ServiceUnitId",
                schema: "public",
                table: "BbkBloodOrder",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrderLine_BloodComponentId",
                schema: "public",
                table: "BbkBloodOrderLine",
                column: "BloodComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrderLine_BloodOrderId",
                schema: "public",
                table: "BbkBloodOrderLine",
                column: "BloodOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransitionHistory_CorrelationId",
                schema: "public",
                table: "BbkTransitionHistory",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransitionHistory_EntityId",
                schema: "public",
                table: "BbkTransitionHistory",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransitionHistory_OccurredAt",
                schema: "public",
                table: "BbkTransitionHistory",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransitionHistory_Scope",
                schema: "public",
                table: "BbkTransitionHistory",
                column: "Scope");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BbkBloodOrderLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BbkTransitionHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BbkBloodOrder",
                schema: "public");
        }
    }
}
