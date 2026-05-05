using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeChildEmployeeDocterExternalUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.RenameColumn(
                name: "ContractNumber",
                schema: "public",
                table: "MstExternalUser",
                newName: "TaxNumber");

            migrationBuilder.RenameColumn(
                name: "TaxNumber",
                schema: "public",
                table: "MstEmployee",
                newName: "WorkLocation");

            migrationBuilder.RenameColumn(
                name: "BpjsHealthNumber",
                schema: "public",
                table: "MstEmployee",
                newName: "Religion");

            migrationBuilder.RenameColumn(
                name: "BpjsEmploymentNumber",
                schema: "public",
                table: "MstEmployee",
                newName: "MaritalStatus");

            migrationBuilder.RenameColumn(
                name: "StrNumber",
                schema: "public",
                table: "MstDoctor",
                newName: "SubSpecialistName");

            migrationBuilder.RenameColumn(
                name: "StrExpiredDate",
                schema: "public",
                table: "MstDoctor",
                newName: "JoinDate");

            migrationBuilder.RenameColumn(
                name: "SipNumber",
                schema: "public",
                table: "MstDoctor",
                newName: "MedicalStaffGroup");

            migrationBuilder.RenameColumn(
                name: "SipExpiredDate",
                schema: "public",
                table: "MstDoctor",
                newName: "ContractStartDate");

            migrationBuilder.AddColumn<string>(
                name: "BusinessLicenseNumber",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonName",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalStatus",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                schema: "public",
                table: "MstExternalUser",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactAddress",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelation",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeLevel",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityType",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProbationEndDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResignDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResignReason",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                schema: "public",
                table: "MstDoctor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                schema: "public",
                table: "MstDoctor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityType",
                schema: "public",
                table: "MstDoctor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForAppointment",
                schema: "public",
                table: "MstDoctor",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                schema: "public",
                table: "MstDoctor",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DctFeeProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FollowUpFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TelemedicineFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DoctorSharePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    FeeCalculationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsFeeActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_DctFeeProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DctFeeProfile_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DctLicense",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_DctLicense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DctLicense_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DctPracticeProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolyclinicName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultConsultationDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    MaxPatientPerSession = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AllowOnlineAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AllowWalkInAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AllowTelemedicine = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PracticeNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_DctPracticeProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DctPracticeProfile_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpBankAccount",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BankBranch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_EmpBankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpBankAccount_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_EmpDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpDocument_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpInsuranceProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BpjsHealthNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BpjsHealthRegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsBpjsHealthActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BpjsEmploymentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BpjsEmploymentRegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsBpjsEmploymentActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PrivateInsuranceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrivateInsuranceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrivateInsuranceStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrivateInsuranceEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_EmpInsuranceProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpInsuranceProfile_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpPayrollProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalaryType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FixedAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MealAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TransportAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PositionAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OtherAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FixedDeduction = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsOvertimeEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPayrollActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_EmpPayrollProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpPayrollProfile_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpTaxProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PtkpStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPph21Active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTaxPaidByCompany = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TaxRegisteredName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaxRegisteredAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TaxRegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_EmpTaxProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpTaxProfile_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtUserContract",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContractName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContractType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentTerm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ScopeOfWork = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ExtUserContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtUserContract_MstExternalUser_ExternalUserId",
                        column: x => x.ExternalUserId,
                        principalSchema: "public",
                        principalTable: "MstExternalUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtUserDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_ExtUserDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtUserDocument_MstExternalUser_ExternalUserId",
                        column: x => x.ExternalUserId,
                        principalSchema: "public",
                        principalTable: "MstExternalUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_CompanyCode",
                schema: "public",
                table: "MstExternalUser",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_Email",
                schema: "public",
                table: "MstExternalUser",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_IsActive",
                schema: "public",
                table: "MstExternalUser",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_AttendanceNumber",
                schema: "public",
                table: "MstEmployee",
                column: "AttendanceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_Email",
                schema: "public",
                table: "MstEmployee",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeStatus",
                schema: "public",
                table: "MstEmployee",
                column: "EmployeeStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_IdentityNumber",
                schema: "public",
                table: "MstEmployee",
                column: "IdentityNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_IsActive",
                schema: "public",
                table: "MstEmployee",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_IdentityNumber",
                schema: "public",
                table: "MstDoctor",
                column: "IdentityNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_IsActive",
                schema: "public",
                table: "MstDoctor",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DctFeeProfile_DoctorId",
                schema: "public",
                table: "DctFeeProfile",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DctLicense_DoctorId",
                schema: "public",
                table: "DctLicense",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DctLicense_LicenseNumber",
                schema: "public",
                table: "DctLicense",
                column: "LicenseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DctLicense_LicenseType",
                schema: "public",
                table: "DctLicense",
                column: "LicenseType");

            migrationBuilder.CreateIndex(
                name: "IX_DctPracticeProfile_DoctorId",
                schema: "public",
                table: "DctPracticeProfile",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpBankAccount_AccountNumber",
                schema: "public",
                table: "EmpBankAccount",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpBankAccount_EmployeeId",
                schema: "public",
                table: "EmpBankAccount",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpDocument_DocumentNumber",
                schema: "public",
                table: "EmpDocument",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpDocument_DocumentType",
                schema: "public",
                table: "EmpDocument",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_EmpDocument_EmployeeId",
                schema: "public",
                table: "EmpDocument",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpInsuranceProfile_BpjsEmploymentNumber",
                schema: "public",
                table: "EmpInsuranceProfile",
                column: "BpjsEmploymentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpInsuranceProfile_BpjsHealthNumber",
                schema: "public",
                table: "EmpInsuranceProfile",
                column: "BpjsHealthNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpInsuranceProfile_EmployeeId",
                schema: "public",
                table: "EmpInsuranceProfile",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpPayrollProfile_EmployeeId",
                schema: "public",
                table: "EmpPayrollProfile",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpPayrollProfile_PayrollNumber",
                schema: "public",
                table: "EmpPayrollProfile",
                column: "PayrollNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTaxProfile_EmployeeId",
                schema: "public",
                table: "EmpTaxProfile",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpTaxProfile_TaxNumber",
                schema: "public",
                table: "EmpTaxProfile",
                column: "TaxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserContract_ContractNumber",
                schema: "public",
                table: "ExtUserContract",
                column: "ContractNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserContract_ContractType",
                schema: "public",
                table: "ExtUserContract",
                column: "ContractType");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserContract_ExternalUserId",
                schema: "public",
                table: "ExtUserContract",
                column: "ExternalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserDocument_DocumentNumber",
                schema: "public",
                table: "ExtUserDocument",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserDocument_DocumentType",
                schema: "public",
                table: "ExtUserDocument",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserDocument_ExternalUserId",
                schema: "public",
                table: "ExtUserDocument",
                column: "ExternalUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DctFeeProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DctLicense",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DctPracticeProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpBankAccount",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpInsuranceProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpPayrollProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTaxProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExtUserContract",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExtUserDocument",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_CompanyCode",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_Email",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_IsActive",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_AttendanceNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_Email",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeStatus",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_IdentityNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_IsActive",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_IdentityNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_IsActive",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "BusinessLicenseNumber",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "ContactPersonName",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "ExternalStatus",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "AttendanceNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "BloodType",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "District",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "EmergencyContactAddress",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelation",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "GradeLevel",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "NickName",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "ProbationEndDate",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "Province",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "ResignDate",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "ResignReason",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "Village",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "IsAvailableForAppointment",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.RenameColumn(
                name: "TaxNumber",
                schema: "public",
                table: "MstExternalUser",
                newName: "ContractNumber");

            migrationBuilder.RenameColumn(
                name: "WorkLocation",
                schema: "public",
                table: "MstEmployee",
                newName: "TaxNumber");

            migrationBuilder.RenameColumn(
                name: "Religion",
                schema: "public",
                table: "MstEmployee",
                newName: "BpjsHealthNumber");

            migrationBuilder.RenameColumn(
                name: "MaritalStatus",
                schema: "public",
                table: "MstEmployee",
                newName: "BpjsEmploymentNumber");

            migrationBuilder.RenameColumn(
                name: "SubSpecialistName",
                schema: "public",
                table: "MstDoctor",
                newName: "StrNumber");

            migrationBuilder.RenameColumn(
                name: "MedicalStaffGroup",
                schema: "public",
                table: "MstDoctor",
                newName: "SipNumber");

            migrationBuilder.RenameColumn(
                name: "JoinDate",
                schema: "public",
                table: "MstDoctor",
                newName: "StrExpiredDate");

            migrationBuilder.RenameColumn(
                name: "ContractStartDate",
                schema: "public",
                table: "MstDoctor",
                newName: "SipExpiredDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                schema: "public",
                table: "MstExternalUser",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                schema: "public",
                table: "MstExternalUser",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
