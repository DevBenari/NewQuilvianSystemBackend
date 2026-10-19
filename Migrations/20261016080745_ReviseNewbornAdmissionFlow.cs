using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class ReviseNewbornAdmissionFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InheritedFromPaymentSourceAt",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InheritedFromPaymentSourceId",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryCaseId",
                schema: "public",
                table: "MstPatient",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewbornRegistrationFingerprint",
                schema: "public",
                table: "MstPatient",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewbornRegistrationKeyHash",
                schema: "public",
                table: "MstPatient",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryCaseId",
                schema: "public",
                table: "InpEpisode",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PatNewbornBirthSequence",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastAllocatedBirthOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PatNewbornBirthSequence", x => x.Id);
                    table.CheckConstraint("CK_PatNewbornBirthSequence_LastAllocatedBirthOrder_Positive", "\"LastAllocatedBirthOrder\" > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegPatientEncounterGuarantor_InheritedSource",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                column: "InheritedFromPaymentSourceId",
                filter: "\"InheritedFromPaymentSourceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_MstPatient_DeliveryCase_BirthOrder",
                schema: "public",
                table: "MstPatient",
                columns: new[] { "DeliveryCaseId", "BirthOrder" },
                unique: true,
                filter: "\"DeliveryCaseId\" IS NOT NULL AND \"BirthOrder\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_MstPatient_NewbornRegistrationKeyHash",
                schema: "public",
                table: "MstPatient",
                column: "NewbornRegistrationKeyHash",
                unique: true,
                filter: "\"NewbornRegistrationKeyHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_DeliveryCaseId",
                schema: "public",
                table: "InpEpisode",
                column: "DeliveryCaseId",
                filter: "\"DeliveryCaseId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_PatNewbornBirthSequence_DeliveryCaseId",
                schema: "public",
                table: "PatNewbornBirthSequence",
                column: "DeliveryCaseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatNewbornBirthSequence",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_RegPatientEncounterGuarantor_InheritedSource",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "UX_MstPatient_DeliveryCase_BirthOrder",
                schema: "public",
                table: "MstPatient");

            migrationBuilder.DropIndex(
                name: "UX_MstPatient_NewbornRegistrationKeyHash",
                schema: "public",
                table: "MstPatient");

            migrationBuilder.DropIndex(
                name: "IX_InpEpisode_DeliveryCaseId",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "InheritedFromPaymentSourceAt",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "InheritedFromPaymentSourceId",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "DeliveryCaseId",
                schema: "public",
                table: "MstPatient");

            migrationBuilder.DropColumn(
                name: "NewbornRegistrationFingerprint",
                schema: "public",
                table: "MstPatient");

            migrationBuilder.DropColumn(
                name: "NewbornRegistrationKeyHash",
                schema: "public",
                table: "MstPatient");

            migrationBuilder.DropColumn(
                name: "DeliveryCaseId",
                schema: "public",
                table: "InpEpisode");
        }
    }
}
