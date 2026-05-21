using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeStruktureCredentialLicense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_LicenseType_License~",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_RequirementCode_IsA~",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevokedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevokedReason",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNote",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_ExpiredDate_IsActive_IsDelete",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "ExpiredDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_LicenseType_ExpiredDate_VerificationSt~",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "LicenseType", "ExpiredDate", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_RejectedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_RevokedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_VerifiedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_IsVerified_IsActive~",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "WorkforceProfileId", "IsVerified", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_LicenseType_IsPrima~",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "WorkforceProfileId", "LicenseType", "IsPrimary" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsPrimary\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_LicenseType_License~",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "WorkforceProfileId", "LicenseType", "LicenseNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_VerificationStatus_~",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "WorkforceProfileId", "VerificationStatus", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_WfpCredentialLicense_AspNetUsers_RejectedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                column: "RejectedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpCredentialLicense_AspNetUsers_RevokedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                column: "RevokedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpCredentialLicense_AspNetUsers_VerifiedByUserId",
                schema: "public",
                table: "WfpCredentialLicense",
                column: "VerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WfpCredentialLicense_AspNetUsers_RejectedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpCredentialLicense_AspNetUsers_RevokedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpCredentialLicense_AspNetUsers_VerifiedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_ExpiredDate_IsActive_IsDelete",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_LicenseType_ExpiredDate_VerificationSt~",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_RejectedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_RevokedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_VerifiedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_IsVerified_IsActive~",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_LicenseType_IsPrima~",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_LicenseType_License~",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_VerificationStatus_~",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "RejectedReason",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "RevokedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "RevokedReason",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "VerificationNote",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_LicenseType_License~",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "WorkforceProfileId", "LicenseType", "LicenseNumber", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_RequirementCode_IsA~",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "WorkforceProfileId", "RequirementCode", "IsActive", "IsDelete" });
        }
    }
}
