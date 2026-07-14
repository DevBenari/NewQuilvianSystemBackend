using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeClusterEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstNurseStationClusterStaff_NurseStationClusterId_EmployeeI~",
                schema: "public",
                table: "MstNurseStationClusterStaff");

            migrationBuilder.DropIndex(
                name: "IX_MstNurseStationClusterClinic_NurseStationClusterId_ClinicId~",
                schema: "public",
                table: "MstNurseStationClusterClinic");

            migrationBuilder.CreateTable(
                name: "MstNurseStationClusterStaffClinic",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseStationClusterStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstNurseStationClusterStaffClinic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstNurseStationClusterStaffClinic_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstNurseStationClusterStaffClinic_MstNurseStationClusterSta~",
                        column: x => x.NurseStationClusterStaffId,
                        principalSchema: "public",
                        principalTable: "MstNurseStationClusterStaff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaff_NurseStationClusterId_EmployeeId",
                schema: "public",
                table: "MstNurseStationClusterStaff",
                columns: new[] { "NurseStationClusterId", "EmployeeId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterClinic_NurseStationClusterId_ClinicId",
                schema: "public",
                table: "MstNurseStationClusterClinic",
                columns: new[] { "NurseStationClusterId", "ClinicId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaffClinic_ClinicId",
                schema: "public",
                table: "MstNurseStationClusterStaffClinic",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaffClinic_ClinicId_IsActive_IsDelete",
                schema: "public",
                table: "MstNurseStationClusterStaffClinic",
                columns: new[] { "ClinicId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaffClinic_NurseStationClusterStaff~1",
                schema: "public",
                table: "MstNurseStationClusterStaffClinic",
                columns: new[] { "NurseStationClusterStaffId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaffClinic_NurseStationClusterStaffI~",
                schema: "public",
                table: "MstNurseStationClusterStaffClinic",
                columns: new[] { "NurseStationClusterStaffId", "ClinicId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaffClinic_NurseStationClusterStaffId",
                schema: "public",
                table: "MstNurseStationClusterStaffClinic",
                column: "NurseStationClusterStaffId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstNurseStationClusterStaffClinic",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstNurseStationClusterStaff_NurseStationClusterId_EmployeeId",
                schema: "public",
                table: "MstNurseStationClusterStaff");

            migrationBuilder.DropIndex(
                name: "IX_MstNurseStationClusterClinic_NurseStationClusterId_ClinicId",
                schema: "public",
                table: "MstNurseStationClusterClinic");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaff_NurseStationClusterId_EmployeeI~",
                schema: "public",
                table: "MstNurseStationClusterStaff",
                columns: new[] { "NurseStationClusterId", "EmployeeId", "IsDelete" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterClinic_NurseStationClusterId_ClinicId~",
                schema: "public",
                table: "MstNurseStationClusterClinic",
                columns: new[] { "NurseStationClusterId", "ClinicId", "IsDelete" },
                unique: true);
        }
    }
}
