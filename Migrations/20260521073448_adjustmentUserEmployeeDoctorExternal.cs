using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class adjustmentUserEmployeeDoctorExternal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeStatus_ProfessionType_IsActive_IsDelete",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_Email",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_IdentityNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DoctorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ExternalUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExternalStatus",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessEndDate",
                schema: "public",
                table: "MstExternalUser",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessPurpose",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessStartDate",
                schema: "public",
                table: "MstExternalUser",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                schema: "public",
                table: "MstExternalUser",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                schema: "public",
                table: "MstExternalUser",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EngagementType",
                schema: "public",
                table: "MstExternalUser",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ExternalUserStatus",
                schema: "public",
                table: "MstExternalUser",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ExternalUserType",
                schema: "public",
                table: "MstExternalUser",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdentityType",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostalCodeId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryDepartmentId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryPositionId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkLocation",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(@"
                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""Religion"" DROP DEFAULT;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""Religion"" TYPE integer
                USING CASE
                    WHEN ""Religion"" IS NULL THEN 0
                    WHEN trim(""Religion""::text) = '' THEN 0
                    WHEN ""Religion""::text ~ '^\d+$' THEN ""Religion""::integer
                    WHEN ""Religion""::text = 'Unknown' THEN 0
                    WHEN ""Religion""::text = 'Islam' THEN 1
                    WHEN ""Religion""::text = 'ProtestantChristian' THEN 2
                    WHEN ""Religion""::text = 'CatholicChristian' THEN 3
                    WHEN ""Religion""::text = 'Hindu' THEN 4
                    WHEN ""Religion""::text = 'Buddhist' THEN 5
                    WHEN ""Religion""::text = 'Confucian' THEN 6
                    WHEN ""Religion""::text = 'Other' THEN 99
                    ELSE 0
                END;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""Religion"" SET NOT NULL;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""Religion"" SET DEFAULT 0;
            ");

                        migrationBuilder.AlterColumn<int>(
                            name: "ProfessionType",
                            schema: "public",
                            table: "MstEmployee",
                            type: "integer",
                            nullable: false,
                            defaultValue: 1,
                            oldClrType: typeof(int),
                            oldType: "integer");

                        migrationBuilder.Sql(@"
                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""MaritalStatus"" DROP DEFAULT;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""MaritalStatus"" TYPE integer
                USING CASE
                    WHEN ""MaritalStatus"" IS NULL THEN 0
                    WHEN trim(""MaritalStatus""::text) = '' THEN 0
                    WHEN ""MaritalStatus""::text ~ '^\d+$' THEN ""MaritalStatus""::integer
                    WHEN ""MaritalStatus""::text = 'Unknown' THEN 0
                    WHEN ""MaritalStatus""::text = 'Single' THEN 1
                    WHEN ""MaritalStatus""::text = 'Married' THEN 2
                    WHEN ""MaritalStatus""::text = 'Divorced' THEN 3
                    WHEN ""MaritalStatus""::text = 'Widowed' THEN 4
                    WHEN ""MaritalStatus""::text = 'Separated' THEN 5
                    ELSE 0
                END;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""MaritalStatus"" SET NOT NULL;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""MaritalStatus"" SET DEFAULT 0;
            ");

                        migrationBuilder.Sql(@"
                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""EmploymentType"" DROP DEFAULT;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""EmploymentType"" TYPE integer
                USING CASE
                    WHEN ""EmploymentType"" IS NULL THEN 2
                    WHEN trim(""EmploymentType""::text) = '' THEN 2
                    WHEN ""EmploymentType""::text ~ '^\d+$' THEN ""EmploymentType""::integer
                    WHEN ""EmploymentType""::text = 'Unknown' THEN 0
                    WHEN ""EmploymentType""::text = 'Permanent' THEN 1
                    WHEN ""EmploymentType""::text = 'Contract' THEN 2
                    WHEN ""EmploymentType""::text = 'Probation' THEN 3
                    WHEN ""EmploymentType""::text = 'Internship' THEN 4
                    WHEN ""EmploymentType""::text = 'PartTime' THEN 5
                    WHEN ""EmploymentType""::text = 'Outsourced' THEN 6
                    WHEN ""EmploymentType""::text = 'DailyWorker' THEN 7
                    WHEN ""EmploymentType""::text = 'Consultant' THEN 8
                    WHEN ""EmploymentType""::text = 'Volunteer' THEN 9
                    ELSE 2
                END;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""EmploymentType"" SET NOT NULL;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""EmploymentType"" SET DEFAULT 2;
            ");

                        migrationBuilder.AlterColumn<int>(
                            name: "EmployeeStatus",
                            schema: "public",
                            table: "MstEmployee",
                            type: "integer",
                            nullable: false,
                            defaultValue: 1,
                            oldClrType: typeof(int),
                            oldType: "integer");

                        migrationBuilder.Sql(@"
                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""BloodType"" DROP DEFAULT;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""BloodType"" TYPE integer
                USING CASE
                    WHEN ""BloodType"" IS NULL THEN 0
                    WHEN trim(""BloodType""::text) = '' THEN 0
                    WHEN ""BloodType""::text ~ '^\d+$' THEN ""BloodType""::integer
                    WHEN ""BloodType""::text = 'Unknown' THEN 0
                    WHEN ""BloodType""::text = 'APositive' THEN 1
                    WHEN ""BloodType""::text = 'ANegative' THEN 2
                    WHEN ""BloodType""::text = 'BPositive' THEN 3
                    WHEN ""BloodType""::text = 'BNegative' THEN 4
                    WHEN ""BloodType""::text = 'ABPositive' THEN 5
                    WHEN ""BloodType""::text = 'ABNegative' THEN 6
                    WHEN ""BloodType""::text = 'OPositive' THEN 7
                    WHEN ""BloodType""::text = 'ONegative' THEN 8
                    WHEN ""BloodType""::text = 'NotDisclosed' THEN 9
                    ELSE 0
                END;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""BloodType"" SET NOT NULL;

                ALTER TABLE public.""MstEmployee""
                ALTER COLUMN ""BloodType"" SET DEFAULT 0;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "ProfessionType",
                schema: "public",
                table: "MstEmployee",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeStatus",
                schema: "public",
                table: "MstEmployee",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstDoctor",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
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

            migrationBuilder.AddColumn<int>(
                name: "BloodType",
                schema: "public",
                table: "MstDoctor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                schema: "public",
                table: "MstDoctor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                schema: "public",
                table: "MstDoctor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CredentialingDate",
                schema: "public",
                table: "MstDoctor",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                schema: "public",
                table: "MstDoctor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorNumber",
                schema: "public",
                table: "MstDoctor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DoctorStatus",
                schema: "public",
                table: "MstDoctor",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DoctorType",
                schema: "public",
                table: "MstDoctor",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "EmploymentType",
                schema: "public",
                table: "MstDoctor",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "GradeLevel",
                schema: "public",
                table: "MstDoctor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaritalStatus",
                schema: "public",
                table: "MstDoctor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                schema: "public",
                table: "MstDoctor",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostalCodeId",
                schema: "public",
                table: "MstDoctor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PracticeType",
                schema: "public",
                table: "MstDoctor",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProbationEndDate",
                schema: "public",
                table: "MstDoctor",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceId",
                schema: "public",
                table: "MstDoctor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Religion",
                schema: "public",
                table: "MstDoctor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResignDate",
                schema: "public",
                table: "MstDoctor",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResignReason",
                schema: "public",
                table: "MstDoctor",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkLocation",
                schema: "public",
                table: "MstDoctor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_CityId",
                schema: "public",
                table: "MstExternalUser",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_ContractStartDate_ContractEndDate_AccessSta~",
                schema: "public",
                table: "MstExternalUser",
                columns: new[] { "ContractStartDate", "ContractEndDate", "AccessStartDate", "AccessEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_CountryId_ProvinceId_CityId_DistrictId_Post~",
                schema: "public",
                table: "MstExternalUser",
                columns: new[] { "CountryId", "ProvinceId", "CityId", "DistrictId", "PostalCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_DistrictId",
                schema: "public",
                table: "MstExternalUser",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_ExternalUserType_ExternalUserStatus_Engagem~",
                schema: "public",
                table: "MstExternalUser",
                columns: new[] { "ExternalUserType", "ExternalUserStatus", "EngagementType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_PhoneNumber",
                schema: "public",
                table: "MstExternalUser",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_PostalCodeId",
                schema: "public",
                table: "MstExternalUser",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_PrimaryDepartmentId_PrimaryPositionId",
                schema: "public",
                table: "MstExternalUser",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_PrimaryPositionId",
                schema: "public",
                table: "MstExternalUser",
                column: "PrimaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_ProvinceId",
                schema: "public",
                table: "MstExternalUser",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_WhatsAppNumber",
                schema: "public",
                table: "MstExternalUser",
                column: "WhatsAppNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser",
                column: "WorkforceProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeStatus_ProfessionType_EmploymentType_Is~",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "EmployeeStatus", "ProfessionType", "EmploymentType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_CityId",
                schema: "public",
                table: "MstDoctor",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_CountryId_ProvinceId_CityId_DistrictId_PostalCode~",
                schema: "public",
                table: "MstDoctor",
                columns: new[] { "CountryId", "ProvinceId", "CityId", "DistrictId", "PostalCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_DistrictId",
                schema: "public",
                table: "MstDoctor",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_DoctorNumber",
                schema: "public",
                table: "MstDoctor",
                column: "DoctorNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_DoctorStatus_DoctorType_PracticeType_EmploymentTy~",
                schema: "public",
                table: "MstDoctor",
                columns: new[] { "DoctorStatus", "DoctorType", "PracticeType", "EmploymentType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_Email",
                schema: "public",
                table: "MstDoctor",
                column: "Email",
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_FullName",
                schema: "public",
                table: "MstDoctor",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_IdentityNumber",
                schema: "public",
                table: "MstDoctor",
                column: "IdentityNumber",
                unique: true,
                filter: "\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_IsAvailableForAppointment",
                schema: "public",
                table: "MstDoctor",
                column: "IsAvailableForAppointment");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_PhoneNumber",
                schema: "public",
                table: "MstDoctor",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_PostalCodeId",
                schema: "public",
                table: "MstDoctor",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_PrimaryDepartmentId_PrimaryPositionId_IsActive_Is~",
                schema: "public",
                table: "MstDoctor",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_ProvinceId",
                schema: "public",
                table: "MstDoctor",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_SpecialistName_SubSpecialistName_MedicalStaffGroup",
                schema: "public",
                table: "MstDoctor",
                columns: new[] { "SpecialistName", "SubSpecialistName", "MedicalStaffGroup" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_WhatsAppNumber",
                schema: "public",
                table: "MstDoctor",
                column: "WhatsAppNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor",
                column: "WorkforceProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DoctorId",
                table: "AspNetUsers",
                column: "DoctorId",
                unique: true,
                filter: "\"DoctorId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId",
                unique: true,
                filter: "\"EmployeeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ExternalUserId",
                table: "AspNetUsers",
                column: "ExternalUserId",
                unique: true,
                filter: "\"ExternalUserId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstCity_CityId",
                schema: "public",
                table: "MstDoctor",
                column: "CityId",
                principalSchema: "public",
                principalTable: "MstCity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstCountry_CountryId",
                schema: "public",
                table: "MstDoctor",
                column: "CountryId",
                principalSchema: "public",
                principalTable: "MstCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstDistrict_DistrictId",
                schema: "public",
                table: "MstDoctor",
                column: "DistrictId",
                principalSchema: "public",
                principalTable: "MstDistrict",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstPostalCode_PostalCodeId",
                schema: "public",
                table: "MstDoctor",
                column: "PostalCodeId",
                principalSchema: "public",
                principalTable: "MstPostalCode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstProvince_ProvinceId",
                schema: "public",
                table: "MstDoctor",
                column: "ProvinceId",
                principalSchema: "public",
                principalTable: "MstProvince",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstExternalUser_MstCity_CityId",
                schema: "public",
                table: "MstExternalUser",
                column: "CityId",
                principalSchema: "public",
                principalTable: "MstCity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstExternalUser_MstCountry_CountryId",
                schema: "public",
                table: "MstExternalUser",
                column: "CountryId",
                principalSchema: "public",
                principalTable: "MstCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstExternalUser_MstDepartment_PrimaryDepartmentId",
                schema: "public",
                table: "MstExternalUser",
                column: "PrimaryDepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstExternalUser_MstDistrict_DistrictId",
                schema: "public",
                table: "MstExternalUser",
                column: "DistrictId",
                principalSchema: "public",
                principalTable: "MstDistrict",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstExternalUser_MstPosition_PrimaryPositionId",
                schema: "public",
                table: "MstExternalUser",
                column: "PrimaryPositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstExternalUser_MstPostalCode_PostalCodeId",
                schema: "public",
                table: "MstExternalUser",
                column: "PostalCodeId",
                principalSchema: "public",
                principalTable: "MstPostalCode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstExternalUser_MstProvince_ProvinceId",
                schema: "public",
                table: "MstExternalUser",
                column: "ProvinceId",
                principalSchema: "public",
                principalTable: "MstProvince",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstCity_CityId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstCountry_CountryId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstDistrict_DistrictId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstPostalCode_PostalCodeId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstProvince_ProvinceId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstExternalUser_MstCity_CityId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropForeignKey(
                name: "FK_MstExternalUser_MstCountry_CountryId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropForeignKey(
                name: "FK_MstExternalUser_MstDepartment_PrimaryDepartmentId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropForeignKey(
                name: "FK_MstExternalUser_MstDistrict_DistrictId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropForeignKey(
                name: "FK_MstExternalUser_MstPosition_PrimaryPositionId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropForeignKey(
                name: "FK_MstExternalUser_MstPostalCode_PostalCodeId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropForeignKey(
                name: "FK_MstExternalUser_MstProvince_ProvinceId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_CityId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_ContractStartDate_ContractEndDate_AccessSta~",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_CountryId_ProvinceId_CityId_DistrictId_Post~",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_DistrictId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_ExternalUserType_ExternalUserStatus_Engagem~",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_PhoneNumber",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_PostalCodeId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_PrimaryDepartmentId_PrimaryPositionId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_PrimaryPositionId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_ProvinceId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_WhatsAppNumber",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeStatus_ProfessionType_EmploymentType_Is~",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_CityId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_CountryId_ProvinceId_CityId_DistrictId_PostalCode~",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_DistrictId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_DoctorNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_DoctorStatus_DoctorType_PracticeType_EmploymentTy~",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_Email",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_FullName",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_IdentityNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_IsAvailableForAppointment",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_PhoneNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_PostalCodeId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_PrimaryDepartmentId_PrimaryPositionId_IsActive_Is~",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_ProvinceId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_SpecialistName_SubSpecialistName_MedicalStaffGroup",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_WhatsAppNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DoctorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ExternalUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AccessEndDate",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "AccessPurpose",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "AccessStartDate",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "CountryId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "EngagementType",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "ExternalUserStatus",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "ExternalUserType",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "PostalCodeId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "PrimaryDepartmentId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "PrimaryPositionId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "WorkLocation",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "BloodType",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "CountryId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "CredentialingDate",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "DoctorNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "DoctorStatus",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "DoctorType",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "GradeLevel",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "NickName",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "PostalCodeId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "PracticeType",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "ProbationEndDate",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "Religion",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "ResignDate",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "ResignReason",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "WorkLocation",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ExternalStatus",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Religion",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unknown",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ProfessionType",
                schema: "public",
                table: "MstEmployee",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "MaritalStatus",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unknown",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "EmploymentType",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 2);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeStatus",
                schema: "public",
                table: "MstEmployee",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "BloodType",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unknown",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstDoctor",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeStatus_ProfessionType_IsActive_IsDelete",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "EmployeeStatus", "ProfessionType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_Email",
                schema: "public",
                table: "MstDoctor",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_IdentityNumber",
                schema: "public",
                table: "MstDoctor",
                column: "IdentityNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DoctorId",
                table: "AspNetUsers",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ExternalUserId",
                table: "AspNetUsers",
                column: "ExternalUserId");
        }
    }
}
