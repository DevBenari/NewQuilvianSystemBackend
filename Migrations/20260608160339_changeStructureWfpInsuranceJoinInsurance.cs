using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeStructureWfpInsuranceJoinInsurance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_BpjsKesehatanNumber",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_BpjsKetenagakerjaanNumber",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_IsBpjsKesehatanEnabled_IsBpjsKetenagakerjaanEn~",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_WorkforceProfileId",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropColumn(
                name: "PrivateInsuranceProvider",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveStartDate",
                schema: "public",
                table: "WfpInsurance",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveEndDate",
                schema: "public",
                table: "WfpInsurance",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CancelBy",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "PrivateInsuranceProviderId",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceId",
                schema: "public",
                table: "MstWorkforceProfile",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_PrivateInsuranceProviderId",
                schema: "public",
                table: "WfpInsurance",
                column: "PrivateInsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_WorkforceProfileId",
                schema: "public",
                table: "WfpInsurance",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_WorkforceProfileId_EffectiveStartDate_Effectiv~",
                schema: "public",
                table: "WfpInsurance",
                columns: new[] { "WorkforceProfileId", "EffectiveStartDate", "EffectiveEndDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_WorkforceProfileId_IsActive_IsDelete",
                schema: "public",
                table: "WfpInsurance",
                columns: new[] { "WorkforceProfileId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_WorkforceProfileId_IsPrivateInsuranceEnabled_P~",
                schema: "public",
                table: "WfpInsurance",
                columns: new[] { "WorkforceProfileId", "IsPrivateInsuranceEnabled", "PrivateInsuranceProviderId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceProfile_InsuranceId",
                schema: "public",
                table: "MstWorkforceProfile",
                column: "InsuranceId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstWorkforceProfile_WfpInsurance_InsuranceId",
                schema: "public",
                table: "MstWorkforceProfile",
                column: "InsuranceId",
                principalSchema: "public",
                principalTable: "WfpInsurance",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WfpInsurance_MstInsuranceProvider_PrivateInsuranceProviderId",
                schema: "public",
                table: "WfpInsurance",
                column: "PrivateInsuranceProviderId",
                principalSchema: "public",
                principalTable: "MstInsuranceProvider",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstWorkforceProfile_WfpInsurance_InsuranceId",
                schema: "public",
                table: "MstWorkforceProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpInsurance_MstInsuranceProvider_PrivateInsuranceProviderId",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_PrivateInsuranceProviderId",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_WorkforceProfileId",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_WorkforceProfileId_EffectiveStartDate_Effectiv~",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_WorkforceProfileId_IsActive_IsDelete",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_WfpInsurance_WorkforceProfileId_IsPrivateInsuranceEnabled_P~",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkforceProfile_InsuranceId",
                schema: "public",
                table: "MstWorkforceProfile");

            migrationBuilder.DropColumn(
                name: "PrivateInsuranceProviderId",
                schema: "public",
                table: "WfpInsurance");

            migrationBuilder.DropColumn(
                name: "InsuranceId",
                schema: "public",
                table: "MstWorkforceProfile");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveStartDate",
                schema: "public",
                table: "WfpInsurance",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveEndDate",
                schema: "public",
                table: "WfpInsurance",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CancelBy",
                schema: "public",
                table: "WfpInsurance",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PrivateInsuranceProvider",
                schema: "public",
                table: "WfpInsurance",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_BpjsKesehatanNumber",
                schema: "public",
                table: "WfpInsurance",
                column: "BpjsKesehatanNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_BpjsKetenagakerjaanNumber",
                schema: "public",
                table: "WfpInsurance",
                column: "BpjsKetenagakerjaanNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_IsBpjsKesehatanEnabled_IsBpjsKetenagakerjaanEn~",
                schema: "public",
                table: "WfpInsurance",
                columns: new[] { "IsBpjsKesehatanEnabled", "IsBpjsKetenagakerjaanEnabled", "IsPrivateInsuranceEnabled", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_WorkforceProfileId",
                schema: "public",
                table: "WfpInsurance",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
