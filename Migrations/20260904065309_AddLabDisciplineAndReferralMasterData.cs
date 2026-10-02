using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabDisciplineAndReferralMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LabDiscipline",
                schema: "public",
                table: "MstProcedure",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstReferralInstitution",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstitutionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InstitutionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstReferralInstitution", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstReferralDoctor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralInstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstReferralDoctor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstReferralDoctor_MstReferralInstitution_ReferralInstitutio~",
                        column: x => x.ReferralInstitutionId,
                        principalSchema: "public",
                        principalTable: "MstReferralInstitution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_LabDiscipline",
                schema: "public",
                table: "MstProcedure",
                column: "LabDiscipline",
                filter: "\"LabDiscipline\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralDoctor_DoctorName",
                schema: "public",
                table: "MstReferralDoctor",
                column: "DoctorName");

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralDoctor_ReferralInstitutionId",
                schema: "public",
                table: "MstReferralDoctor",
                column: "ReferralInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralInstitution_InstitutionCode",
                schema: "public",
                table: "MstReferralInstitution",
                column: "InstitutionCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstReferralInstitution_InstitutionName",
                schema: "public",
                table: "MstReferralInstitution",
                column: "InstitutionName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstReferralDoctor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstReferralInstitution",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstProcedure_LabDiscipline",
                schema: "public",
                table: "MstProcedure");

            migrationBuilder.DropColumn(
                name: "LabDiscipline",
                schema: "public",
                table: "MstProcedure");
        }
    }
}
