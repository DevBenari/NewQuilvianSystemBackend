using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMstDiagnosisGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiagnosisGroupId",
                schema: "public",
                table: "MstDiagnosis",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstDiagnosisGroup",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DtdNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CodeRangeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GroupName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsImmunization = table.Column<bool>(type: "boolean", nullable: false),
                    IsAccidentCause = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstDiagnosisGroup", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisGroupId",
                schema: "public",
                table: "MstDiagnosis",
                column: "DiagnosisGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisGroup_DtdNumber",
                schema: "public",
                table: "MstDiagnosisGroup",
                column: "DtdNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisGroup_GroupName",
                schema: "public",
                table: "MstDiagnosisGroup",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisGroup_SourceCode",
                schema: "public",
                table: "MstDiagnosisGroup",
                column: "SourceCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDiagnosis_MstDiagnosisGroup_DiagnosisGroupId",
                schema: "public",
                table: "MstDiagnosis",
                column: "DiagnosisGroupId",
                principalSchema: "public",
                principalTable: "MstDiagnosisGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDiagnosis_MstDiagnosisGroup_DiagnosisGroupId",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropTable(
                name: "MstDiagnosisGroup",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_DiagnosisGroupId",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "DiagnosisGroupId",
                schema: "public",
                table: "MstDiagnosis");
        }
    }
}
