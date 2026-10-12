using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralPartnerAgreementAndSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgreementNumberSnapshot",
                schema: "public",
                table: "RegEncounterReferral",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionCodeSnapshot",
                schema: "public",
                table: "RegEncounterReferral",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionNameSnapshot",
                schema: "public",
                table: "RegEncounterReferral",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                schema: "public",
                table: "MstReferralInstitution",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "MstReferralInstitution",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "public",
                table: "MstReferralInstitution",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalFacilityCode",
                schema: "public",
                table: "MstReferralInstitution",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstitutionType",
                schema: "public",
                table: "MstReferralInstitution",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PicName",
                schema: "public",
                table: "MstReferralInstitution",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceId",
                schema: "public",
                table: "MstReferralInstitution",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                schema: "public",
                table: "MstReferralInstitution",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MstReferralInstitutionAgreement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralInstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_MstReferralInstitutionAgreement", x => x.Id);
                    table.CheckConstraint("CK_MstReferralInstitutionAgreement_DateRange", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_MstReferralInstitutionAgreement_MstReferralInstitution_Refe~",
                        column: x => x.ReferralInstitutionId,
                        principalSchema: "public",
                        principalTable: "MstReferralInstitution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralInstitution_CityId",
                schema: "public",
                table: "MstReferralInstitution",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralInstitution_CreateDateTime",
                schema: "public",
                table: "MstReferralInstitution",
                column: "CreateDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralInstitution_ProvinceId",
                schema: "public",
                table: "MstReferralInstitution",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralInstitutionAgreement_ReferralInstitutionId_Agree~",
                schema: "public",
                table: "MstReferralInstitutionAgreement",
                columns: new[] { "ReferralInstitutionId", "AgreementNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralInstitutionAgreement_ReferralInstitutionId_Start~",
                schema: "public",
                table: "MstReferralInstitutionAgreement",
                columns: new[] { "ReferralInstitutionId", "StartDate", "EndDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_MstReferralInstitution_MstCity_CityId",
                schema: "public",
                table: "MstReferralInstitution",
                column: "CityId",
                principalSchema: "public",
                principalTable: "MstCity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstReferralInstitution_MstProvince_ProvinceId",
                schema: "public",
                table: "MstReferralInstitution",
                column: "ProvinceId",
                principalSchema: "public",
                principalTable: "MstProvince",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstReferralInstitution_MstCity_CityId",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropForeignKey(
                name: "FK_MstReferralInstitution_MstProvince_ProvinceId",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropTable(
                name: "MstReferralInstitutionAgreement",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstReferralInstitution_CityId",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropIndex(
                name: "IX_MstReferralInstitution_CreateDateTime",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropIndex(
                name: "IX_MstReferralInstitution_ProvinceId",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropColumn(
                name: "AgreementNumberSnapshot",
                schema: "public",
                table: "RegEncounterReferral");

            migrationBuilder.DropColumn(
                name: "InstitutionCodeSnapshot",
                schema: "public",
                table: "RegEncounterReferral");

            migrationBuilder.DropColumn(
                name: "InstitutionNameSnapshot",
                schema: "public",
                table: "RegEncounterReferral");

            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropColumn(
                name: "ExternalFacilityCode",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropColumn(
                name: "InstitutionType",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropColumn(
                name: "PicName",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                schema: "public",
                table: "MstReferralInstitution");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "public",
                table: "MstReferralInstitution");
        }
    }
}
