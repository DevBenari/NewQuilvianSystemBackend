using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnKioskDevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstKioskDevice_MstClinic_ClinicId",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKioskDevice_MstServiceUnit_ServiceUnitId",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropIndex(
                name: "IX_MstKioskDevice_ClinicId",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropIndex(
                name: "IX_MstKioskDevice_SerialNumber",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropIndex(
                name: "IX_MstKioskDevice_ServiceUnitId",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropIndex(
                name: "IX_MstKioskDevice_ServiceUnitId_ClinicId_IsAvailableForRegistr~",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "DeviceModel",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "IsAvailableForCheckIn",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "IsAvailableForPayment",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "IsAvailableForRegistration",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "ServiceUnitId",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "VendorName",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CancelBy",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_IsAllowWalkIn_IsAllowAppointment_IsAllowInsu~",
                schema: "public",
                table: "MstKioskDevice",
                columns: new[] { "IsAllowWalkIn", "IsAllowAppointment", "IsAllowInsuranceRegistration", "IsActive", "IsDelete" });

            migrationBuilder.Sql(
                 @"CREATE INDEX IF NOT EXISTS ""IX_MstKioskDevice_MacAddress""
                  ON public.""MstKioskDevice"" (""MacAddress"")
                  WHERE ""MacAddress"" IS NOT NULL;"
             );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstKioskDevice_IsAllowWalkIn_IsAllowAppointment_IsAllowInsu~",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.Sql(
                @"DROP INDEX IF EXISTS public.""IX_MstKioskDevice_MacAddress"";"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CancelBy",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClinicId",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceModel",
                schema: "public",
                table: "MstKioskDevice",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForCheckIn",
                schema: "public",
                table: "MstKioskDevice",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForPayment",
                schema: "public",
                table: "MstKioskDevice",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForRegistration",
                schema: "public",
                table: "MstKioskDevice",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                schema: "public",
                table: "MstKioskDevice",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceUnitId",
                schema: "public",
                table: "MstKioskDevice",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorName",
                schema: "public",
                table: "MstKioskDevice",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_ClinicId",
                schema: "public",
                table: "MstKioskDevice",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_SerialNumber",
                schema: "public",
                table: "MstKioskDevice",
                column: "SerialNumber",
                filter: "\"SerialNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_ServiceUnitId",
                schema: "public",
                table: "MstKioskDevice",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_ServiceUnitId_ClinicId_IsAvailableForRegistr~",
                schema: "public",
                table: "MstKioskDevice",
                columns: new[] { "ServiceUnitId", "ClinicId", "IsAvailableForRegistration", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_MstKioskDevice_MstClinic_ClinicId",
                schema: "public",
                table: "MstKioskDevice",
                column: "ClinicId",
                principalSchema: "public",
                principalTable: "MstClinic",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKioskDevice_MstServiceUnit_ServiceUnitId",
                schema: "public",
                table: "MstKioskDevice",
                column: "ServiceUnitId",
                principalSchema: "public",
                principalTable: "MstServiceUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
