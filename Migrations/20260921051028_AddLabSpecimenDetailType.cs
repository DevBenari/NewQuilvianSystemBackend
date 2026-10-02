using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabSpecimenDetailType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabSpecimenDetailType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabSpecimenTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DetailTypeNameId = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DetailTypeNameEn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SubTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SnomedCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_LabSpecimenDetailType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabSpecimenDetailType_LabSpecimenType_LabSpecimenTypeId",
                        column: x => x.LabSpecimenTypeId,
                        principalSchema: "public",
                        principalTable: "LabSpecimenType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabSpecimenDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabSpecimenId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabSpecimenDetailTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailNameSnapshot = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("PK_LabSpecimenDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabSpecimenDetail_LabSpecimenDetailType_LabSpecimenDetailTy~",
                        column: x => x.LabSpecimenDetailTypeId,
                        principalSchema: "public",
                        principalTable: "LabSpecimenDetailType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabSpecimenDetail_LabSpecimen_LabSpecimenId",
                        column: x => x.LabSpecimenId,
                        principalSchema: "public",
                        principalTable: "LabSpecimen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimenDetail_LabSpecimenDetailTypeId",
                schema: "public",
                table: "LabSpecimenDetail",
                column: "LabSpecimenDetailTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimenDetail_SpecimenId_DetailTypeId",
                schema: "public",
                table: "LabSpecimenDetail",
                columns: new[] { "LabSpecimenId", "LabSpecimenDetailTypeId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimenDetailType_DetailTypeCode",
                schema: "public",
                table: "LabSpecimenDetailType",
                column: "DetailTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimenDetailType_DetailTypeNameEn",
                schema: "public",
                table: "LabSpecimenDetailType",
                column: "DetailTypeNameEn");

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimenDetailType_DetailTypeNameId",
                schema: "public",
                table: "LabSpecimenDetailType",
                column: "DetailTypeNameId");

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimenDetailType_LabSpecimenTypeId",
                schema: "public",
                table: "LabSpecimenDetailType",
                column: "LabSpecimenTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimenDetailType_SnomedCode",
                schema: "public",
                table: "LabSpecimenDetailType",
                column: "SnomedCode",
                unique: true,
                filter: "\"IsDelete\" = false AND \"SnomedCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimenDetailType_SubTypeName",
                schema: "public",
                table: "LabSpecimenDetailType",
                column: "SubTypeName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabSpecimenDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LabSpecimenDetailType",
                schema: "public");
        }
    }
}
