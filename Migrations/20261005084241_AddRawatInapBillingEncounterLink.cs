using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRawatInapBillingEncounterLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilInvoiceEncounterLink",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RanapInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkedEncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkReason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SourceReferralId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_BilInvoiceEncounterLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilInvoiceEncounterLink_BilInpatientEventReceipt_ReceiptId",
                        column: x => x.ReceiptId,
                        principalSchema: "public",
                        principalTable: "BilInpatientEventReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilInvoiceEncounterLink_BilInvoice_RanapInvoiceId",
                        column: x => x.RanapInvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilInvoiceEncounterLink_InpAdmissionReferral_SourceReferral~",
                        column: x => x.SourceReferralId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionReferral",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilInvoiceEncounterLink_RegPatientEncounter_LinkedEncounter~",
                        column: x => x.LinkedEncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceEncounterLink_LinkedEncounterId",
                schema: "public",
                table: "BilInvoiceEncounterLink",
                column: "LinkedEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceEncounterLink_ReceiptId",
                schema: "public",
                table: "BilInvoiceEncounterLink",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceEncounterLink_SourceReferralId",
                schema: "public",
                table: "BilInvoiceEncounterLink",
                column: "SourceReferralId");

            migrationBuilder.CreateIndex(
                name: "UX_BilInvoiceEncounterLink_Invoice_Encounter",
                schema: "public",
                table: "BilInvoiceEncounterLink",
                columns: new[] { "RanapInvoiceId", "LinkedEncounterId" },
                unique: true,
                filter: "NOT \"IsDelete\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilInvoiceEncounterLink",
                schema: "public");
        }
    }
}
