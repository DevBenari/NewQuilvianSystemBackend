using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnEmployee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpOrganizationAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpOrganizationAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpOrganizationAssignment_DepartmentId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpOrganizationAssignment_EmployeeId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpOrganizationAssignment_EmployeeId_DepartmentId_PositionI~",
                schema: "public",
                table: "EmpOrganizationAssignment",
                columns: new[] { "EmployeeId", "DepartmentId", "PositionId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpOrganizationAssignment_EmployeeId_IsPrimary",
                schema: "public",
                table: "EmpOrganizationAssignment",
                columns: new[] { "EmployeeId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_EmpOrganizationAssignment_PositionId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "PositionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpOrganizationAssignment",
                schema: "public");
        }
    }
}
