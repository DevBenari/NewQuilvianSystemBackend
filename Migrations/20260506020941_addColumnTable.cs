using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addColumnTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MstDepartment_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MstPosition_PositionId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstPosition_PositionId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstPosition_PositionId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_DepartmentId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeStatus",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_FullName",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_DepartmentId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_FullName",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IdentityNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                schema: "public",
                table: "MstEmployee",
                newName: "PrimaryPositionId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                schema: "public",
                table: "MstEmployee",
                newName: "PrimaryDepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_MstEmployee_PositionId",
                schema: "public",
                table: "MstEmployee",
                newName: "IX_MstEmployee_PrimaryPositionId");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                schema: "public",
                table: "MstDoctor",
                newName: "PrimaryPositionId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                schema: "public",
                table: "MstDoctor",
                newName: "PrimaryDepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_MstDoctor_PositionId",
                schema: "public",
                table: "MstDoctor",
                newName: "IX_MstDoctor_PrimaryPositionId");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "AspNetUsers",
                newName: "PrimaryPositionId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "AspNetUsers",
                newName: "PrimaryDepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_PositionId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_PrimaryPositionId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResignDate",
                schema: "public",
                table: "MstEmployee",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProbationEndDate",
                schema: "public",
                table: "MstEmployee",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                schema: "public",
                table: "MstEmployee",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstEmployee",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "MstEmployee",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractStartDate",
                schema: "public",
                table: "MstEmployee",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractEndDate",
                schema: "public",
                table: "MstEmployee",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfessionType",
                schema: "public",
                table: "MstEmployee",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                schema: "public",
                table: "MstDoctor",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstDoctor",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "MstDoctor",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstDoctor",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractStartDate",
                schema: "public",
                table: "MstDoctor",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractEndDate",
                schema: "public",
                table: "MstDoctor",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "MustChangePassword",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsGeolocationBypassEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AspNetUserOrganization",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_AspNetUserOrganization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserOrganization_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserOrganization_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserOrganization_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee",
                column: "EmployeeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_PrimaryDepartmentId_PrimaryPositionId",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_Email",
                schema: "public",
                table: "MstDoctor",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_PrimaryDepartmentId_PrimaryPositionId",
                schema: "public",
                table: "MstDoctor",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PrimaryDepartmentId_PrimaryPositionId",
                table: "AspNetUsers",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_DepartmentId_PositionId",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "DepartmentId", "PositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_PositionId",
                schema: "public",
                table: "AspNetUserOrganization",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_UserId",
                schema: "public",
                table: "AspNetUserOrganization",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "UserId", "DepartmentId", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_UserId_IsActive_IsDelete",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "UserId", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MstDepartment_PrimaryDepartmentId",
                table: "AspNetUsers",
                column: "PrimaryDepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MstPosition_PrimaryPositionId",
                table: "AspNetUsers",
                column: "PrimaryPositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstDepartment_PrimaryDepartmentId",
                schema: "public",
                table: "MstDoctor",
                column: "PrimaryDepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstPosition_PrimaryPositionId",
                schema: "public",
                table: "MstDoctor",
                column: "PrimaryPositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstDepartment_PrimaryDepartmentId",
                schema: "public",
                table: "MstEmployee",
                column: "PrimaryDepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstPosition_PrimaryPositionId",
                schema: "public",
                table: "MstEmployee",
                column: "PrimaryPositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MstDepartment_PrimaryDepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MstPosition_PrimaryPositionId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstDepartment_PrimaryDepartmentId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstPosition_PrimaryPositionId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstDepartment_PrimaryDepartmentId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstPosition_PrimaryPositionId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropTable(
                name: "AspNetUserOrganization",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_PrimaryDepartmentId_PrimaryPositionId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_Email",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_PrimaryDepartmentId_PrimaryPositionId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PrimaryDepartmentId_PrimaryPositionId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfessionType",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PrimaryPositionId",
                schema: "public",
                table: "MstEmployee",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "PrimaryDepartmentId",
                schema: "public",
                table: "MstEmployee",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_MstEmployee_PrimaryPositionId",
                schema: "public",
                table: "MstEmployee",
                newName: "IX_MstEmployee_PositionId");

            migrationBuilder.RenameColumn(
                name: "PrimaryPositionId",
                schema: "public",
                table: "MstDoctor",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "PrimaryDepartmentId",
                schema: "public",
                table: "MstDoctor",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_MstDoctor_PrimaryPositionId",
                schema: "public",
                table: "MstDoctor",
                newName: "IX_MstDoctor_PositionId");

            migrationBuilder.RenameColumn(
                name: "PrimaryPositionId",
                table: "AspNetUsers",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "PrimaryDepartmentId",
                table: "AspNetUsers",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_PrimaryPositionId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_PositionId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResignDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProbationEndDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstEmployee",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "MstEmployee",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractStartDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractEndDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                schema: "public",
                table: "MstDoctor",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstDoctor",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "MstDoctor",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstDoctor",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractStartDate",
                schema: "public",
                table: "MstDoctor",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractEndDate",
                schema: "public",
                table: "MstDoctor",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "MustChangePassword",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsGeolocationBypassEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "AspNetUsers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_DepartmentId",
                schema: "public",
                table: "MstEmployee",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeStatus",
                schema: "public",
                table: "MstEmployee",
                column: "EmployeeStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_FullName",
                schema: "public",
                table: "MstEmployee",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_DepartmentId",
                schema: "public",
                table: "MstDoctor",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_FullName",
                schema: "public",
                table: "MstDoctor",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IdentityNumber",
                table: "AspNetUsers",
                column: "IdentityNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MstDepartment_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MstPosition_PositionId",
                table: "AspNetUsers",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstDoctor",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstPosition_PositionId",
                schema: "public",
                table: "MstDoctor",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstEmployee",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstPosition_PositionId",
                schema: "public",
                table: "MstEmployee",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
