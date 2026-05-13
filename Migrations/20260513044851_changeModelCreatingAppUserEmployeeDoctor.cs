using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeModelCreatingAppUserEmployeeDoctor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmpOrganizationAssignment_MstDepartment_DepartmentId",
                schema: "public",
                table: "EmpOrganizationAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpOrganizationAssignment_MstEmployee_EmployeeId",
                schema: "public",
                table: "EmpOrganizationAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpOrganizationAssignment_MstPosition_PositionId",
                schema: "public",
                table: "EmpOrganizationAssignment");

            migrationBuilder.DropIndex(
                name: "IX_EmpOrganizationAssignment_EmployeeId_DepartmentId_PositionI~",
                schema: "public",
                table: "EmpOrganizationAssignment");

            migrationBuilder.DropIndex(
                name: "IX_EmpOrganizationAssignment_EmployeeId_IsPrimary",
                schema: "public",
                table: "EmpOrganizationAssignment");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserOrganization_UserId_IsActive_IsDelete",
                schema: "public",
                table: "AspNetUserOrganization");

            migrationBuilder.DropColumn(
                name: "PersonType",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.RenameColumn(
                name: "HospitalId",
                table: "AspNetUsers",
                newName: "WorkforceProfileId");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstEmployee",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstDoctor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveStartDate",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveEndDate",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                schema: "public",
                table: "EmpAttendance",
                type: "integer",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstWorkforceProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PrimaryDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstWorkforceProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstWorkforceProfile_MstDepartment_PrimaryDepartmentId",
                        column: x => x.PrimaryDepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstWorkforceProfile_MstPosition_PrimaryPositionId",
                        column: x => x.PrimaryPositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstWorkforceRequirement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    RequirementCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequirementName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsMultipleAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFileRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNumberRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsIssueDateRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsExpiredDateRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerificationRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstWorkforceRequirement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WfpBankAccount",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BankBranch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpBankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpBankAccount_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpCertification",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CertificationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Issuer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiredDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsLifetime = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpCertification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpCertification_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpCredentialLicense",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LicenseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Issuer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiredDate = table.Column<DateTime>(type: "date", nullable: false),
                    PracticeLocation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpCredentialLicense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpCredentialLicense_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "date", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "date", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpDocument_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpEducation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EducationLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InstitutionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Major = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    GraduationYear = table.Column<int>(type: "integer", nullable: true),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpEducation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpEducation_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpOrganizationAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpOrganizationAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpOrganizationAssignment_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOrganizationAssignment_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOrganizationAssignment_MstWorkforceProfile_WorkforceProf~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpTrainingRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrainingType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TrainingName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Organizer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreditPoint = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpTrainingRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpTrainingRecord_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WorkforceProfileId",
                table: "AspNetUsers",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "UserId", "DepartmentId", "PositionId", "EffectiveStartDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceProfile_DisplayName",
                schema: "public",
                table: "MstWorkforceProfile",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceProfile_Email",
                schema: "public",
                table: "MstWorkforceProfile",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceProfile_PrimaryDepartmentId",
                schema: "public",
                table: "MstWorkforceProfile",
                column: "PrimaryDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceProfile_PrimaryPositionId",
                schema: "public",
                table: "MstWorkforceProfile",
                column: "PrimaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceProfile_ProfileCode",
                schema: "public",
                table: "MstWorkforceProfile",
                column: "ProfileCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceProfile_UserType",
                schema: "public",
                table: "MstWorkforceProfile",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceProfile_UserType_IsActive_IsDelete",
                schema: "public",
                table: "MstWorkforceProfile",
                columns: new[] { "UserType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_IsActi~",
                schema: "public",
                table: "MstWorkforceRequirement",
                columns: new[] { "UserType", "RequirementCategory", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_Requir~",
                schema: "public",
                table: "MstWorkforceRequirement",
                columns: new[] { "UserType", "RequirementCategory", "RequirementCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId",
                schema: "public",
                table: "WfpBankAccount",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_AccountNumber_IsDelete",
                schema: "public",
                table: "WfpBankAccount",
                columns: new[] { "WorkforceProfileId", "AccountNumber", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_IsPrimary",
                schema: "public",
                table: "WfpBankAccount",
                columns: new[] { "WorkforceProfileId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCertification_WorkforceProfileId",
                schema: "public",
                table: "WfpCertification",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCertification_WorkforceProfileId_CertificationName_Certi~",
                schema: "public",
                table: "WfpCertification",
                columns: new[] { "WorkforceProfileId", "CertificationName", "CertificateNumber", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCertification_WorkforceProfileId_RequirementCode_IsActiv~",
                schema: "public",
                table: "WfpCertification",
                columns: new[] { "WorkforceProfileId", "RequirementCode", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId",
                schema: "public",
                table: "WfpCredentialLicense",
                column: "WorkforceProfileId");

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

            migrationBuilder.CreateIndex(
                name: "IX_WfpDocument_WorkforceProfileId",
                schema: "public",
                table: "WfpDocument",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDocument_WorkforceProfileId_DocumentType_DocumentNumber_~",
                schema: "public",
                table: "WfpDocument",
                columns: new[] { "WorkforceProfileId", "DocumentType", "DocumentNumber", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpDocument_WorkforceProfileId_RequirementCode_IsActive_IsD~",
                schema: "public",
                table: "WfpDocument",
                columns: new[] { "WorkforceProfileId", "RequirementCode", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpEducation_WorkforceProfileId",
                schema: "public",
                table: "WfpEducation",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpEducation_WorkforceProfileId_EducationLevel_GraduationYe~",
                schema: "public",
                table: "WfpEducation",
                columns: new[] { "WorkforceProfileId", "EducationLevel", "GraduationYear", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpEducation_WorkforceProfileId_RequirementCode_IsActive_Is~",
                schema: "public",
                table: "WfpEducation",
                columns: new[] { "WorkforceProfileId", "RequirementCode", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOrganizationAssignment_DepartmentId",
                schema: "public",
                table: "WfpOrganizationAssignment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOrganizationAssignment_PositionId",
                schema: "public",
                table: "WfpOrganizationAssignment",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOrganizationAssignment_WorkforceProfileId",
                schema: "public",
                table: "WfpOrganizationAssignment",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOrganizationAssignment_WorkforceProfileId_DepartmentId_P~",
                schema: "public",
                table: "WfpOrganizationAssignment",
                columns: new[] { "WorkforceProfileId", "DepartmentId", "PositionId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOrganizationAssignment_WorkforceProfileId_IsPrimary",
                schema: "public",
                table: "WfpOrganizationAssignment",
                columns: new[] { "WorkforceProfileId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTrainingRecord_WorkforceProfileId",
                schema: "public",
                table: "WfpTrainingRecord",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTrainingRecord_WorkforceProfileId_RequirementCode_IsActi~",
                schema: "public",
                table: "WfpTrainingRecord",
                columns: new[] { "WorkforceProfileId", "RequirementCode", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpTrainingRecord_WorkforceProfileId_TrainingType_StartDate~",
                schema: "public",
                table: "WfpTrainingRecord",
                columns: new[] { "WorkforceProfileId", "TrainingType", "StartDate", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MstWorkforceProfile_WorkforceProfileId",
                table: "AspNetUsers",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpAttendance_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmpOrganizationAssignment_MstDepartment_DepartmentId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpOrganizationAssignment_MstEmployee_EmployeeId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "EmployeeId",
                principalSchema: "public",
                principalTable: "MstEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpOrganizationAssignment_MstPosition_PositionId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDoctor_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstExternalUser_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MstWorkforceProfile_WorkforceProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpAttendance_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpOrganizationAssignment_MstDepartment_DepartmentId",
                schema: "public",
                table: "EmpOrganizationAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpOrganizationAssignment_MstEmployee_EmployeeId",
                schema: "public",
                table: "EmpOrganizationAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpOrganizationAssignment_MstPosition_PositionId",
                schema: "public",
                table: "EmpOrganizationAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDoctor_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_MstExternalUser_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropTable(
                name: "MstWorkforceRequirement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpBankAccount",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpCertification",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpCredentialLicense",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpEducation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpOrganizationAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpTrainingRecord",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstWorkforceProfile",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_WorkforceProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~",
                schema: "public",
                table: "AspNetUserOrganization");

            migrationBuilder.DropColumn(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "UserType",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.RenameColumn(
                name: "WorkforceProfileId",
                table: "AspNetUsers",
                newName: "HospitalId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveStartDate",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveEndDate",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "EmpOrganizationAssignment",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "PersonType",
                schema: "public",
                table: "EmpAttendance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

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
                name: "IX_AspNetUserOrganization_UserId_IsActive_IsDelete",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "UserId", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmpOrganizationAssignment_MstDepartment_DepartmentId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpOrganizationAssignment_MstEmployee_EmployeeId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "EmployeeId",
                principalSchema: "public",
                principalTable: "MstEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpOrganizationAssignment_MstPosition_PositionId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
