using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeSetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpTransportAllowancePolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AllowanceMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "DailyAttendance"),
                    DefaultMonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DefaultDailyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DefaultNightAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    NightStartTime = table.Column<TimeSpan>(type: "time without time zone", nullable: true),
                    NightEndTime = table.Column<TimeSpan>(type: "time without time zone", nullable: true),
                    RequireAttendance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExcludeIfAbsent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExcludeIfLeave = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExcludeIfHoliday = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_EmpTransportAllowancePolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstCountry",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CountryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PhoneCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstCountry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstDepartment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DepartmentName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstDepartment", x => x.Id);
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
                name: "SysApplicationModule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AreaName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_SysApplicationModule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysAppVersion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BackendVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApiVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FrontendMinimumVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrontendRecommendedVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReleaseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReleaseDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_SysAppVersion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstProvince",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvinceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProvinceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_MstProvince", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstProvince_MstCountry_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "public",
                        principalTable: "MstCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPosition",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PositionName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstPosition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPosition_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SysControllerAccess",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControllerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RoutePath = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    VisibleInRoleAccess = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSystemOnly = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_SysControllerAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysControllerAccess_SysApplicationModule_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "SysApplicationModule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstCity",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CityName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CityType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("PK_MstCity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstCity_MstProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalSchema: "public",
                        principalTable: "MstProvince",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "SysActionAccess",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ControllerAccessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RoutePath = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AccessType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Read"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    VisibleInRoleAccess = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSystemOnly = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_SysActionAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysActionAccess_SysControllerAccess_ControllerAccessId",
                        column: x => x.ControllerAccessId,
                        principalSchema: "public",
                        principalTable: "SysControllerAccess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDistrict",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_MstDistrict", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDistrict_MstCity_CityId",
                        column: x => x.CityId,
                        principalSchema: "public",
                        principalTable: "MstCity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDoctor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DoctorType = table.Column<int>(type: "integer", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    BirthPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    IdentityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdentityNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SpecialistName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubSpecialistName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MedicalStaffGroup = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrimaryDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    JoinDate = table.Column<DateTime>(type: "date", nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsAvailableForAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstDoctor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDoctor_MstDepartment_PrimaryDepartmentId",
                        column: x => x.PrimaryDepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctor_MstPosition_PrimaryPositionId",
                        column: x => x.PrimaryPositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctor_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstExternalUser",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExternalUserType = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompanyCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactPersonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdentityNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TaxNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BusinessLicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_MstExternalUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstExternalUser_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "SysAccessPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControllerAccessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionAccessId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_SysAccessPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysAccessPolicy_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SysAccessPolicy_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SysAccessPolicy_SysActionAccess_ActionAccessId",
                        column: x => x.ActionAccessId,
                        principalSchema: "public",
                        principalTable: "SysActionAccess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SysAccessPolicy_SysControllerAccess_ControllerAccessId",
                        column: x => x.ControllerAccessId,
                        principalSchema: "public",
                        principalTable: "SysControllerAccess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPostalCode",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VillageName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
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
                    table.PrimaryKey("PK_MstPostalCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPostalCode_MstDistrict_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "public",
                        principalTable: "MstDistrict",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "MstEmployee",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NickName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    BirthPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: false),
                    Religion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Unknown"),
                    MaritalStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Unknown"),
                    BloodType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Unknown"),
                    IdentityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdentityNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostalCodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryDepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryPositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeStatus = table.Column<int>(type: "integer", nullable: false),
                    ProfessionType = table.Column<int>(type: "integer", nullable: false),
                    EmploymentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GradeLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WorkLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    JoinDate = table.Column<DateTime>(type: "date", nullable: false),
                    ProbationEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    ResignDate = table.Column<DateTime>(type: "date", nullable: true),
                    ResignReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    EmergencyContactAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstEmployee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstEmployee_MstCity_CityId",
                        column: x => x.CityId,
                        principalSchema: "public",
                        principalTable: "MstCity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstEmployee_MstCountry_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "public",
                        principalTable: "MstCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstEmployee_MstDepartment_PrimaryDepartmentId",
                        column: x => x.PrimaryDepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstEmployee_MstDistrict_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "public",
                        principalTable: "MstDistrict",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstEmployee_MstPosition_PrimaryPositionId",
                        column: x => x.PrimaryPositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstEmployee_MstPostalCode_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalSchema: "public",
                        principalTable: "MstPostalCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstEmployee_MstProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalSchema: "public",
                        principalTable: "MstProvince",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstEmployee_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsGeolocationBypassEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    GeolocationBypassReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    GeolocationBypassUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccessValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProfilePhotoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsFingerprintRegistrationEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FingerprintRegistrationReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FingerprintRegistrationEnabledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FingerprintRegistrationEnabledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_MstDepartment_PrimaryDepartmentId",
                        column: x => x.PrimaryDepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_MstExternalUser_ExternalUserId",
                        column: x => x.ExternalUserId,
                        principalSchema: "public",
                        principalTable: "MstExternalUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_MstPosition_PrimaryPositionId",
                        column: x => x.PrimaryPositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
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
                name: "EmpOrganizationAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_EmpOrganizationAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "EmpTransportAllowanceProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNightTransportEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllowanceMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "None"),
                    MonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DailyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    NightAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsProrated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPayrollComponent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_EmpTransportAllowanceProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpTransportAllowanceProfile_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserFingerprint",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    FingerPosition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateFormat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TemplateVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TemplateDataEncrypted = table.Column<byte[]>(type: "bytea", nullable: false),
                    TemplateHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SampleFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    QualityScore = table.Column<int>(type: "integer", nullable: true),
                    EnrollmentSampleCount = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegisteredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisteredIpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegisteredUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_AspNetUserFingerprint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserFingerprint_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserFingerprint_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserFingerprint_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserFingerprint_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstWorkSchedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScheduleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserType = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    WorkEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CheckInToleranceMinutes = table.Column<int>(type: "integer", nullable: false),
                    CheckOutToleranceMinutes = table.Column<int>(type: "integer", nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstWorkSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstWorkSchedule_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstWorkSchedule_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstWorkSchedule_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpTransportAllowanceTransaction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportAllowanceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransportAllowancePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodYearMonth = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AllowanceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Daily"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsGeneratedFromAttendance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNightShift = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    Notes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_EmpTransportAllowanceTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpTransportAllowanceTransaction_EmpTransportAllowancePolic~",
                        column: x => x.TransportAllowancePolicyId,
                        principalSchema: "public",
                        principalTable: "EmpTransportAllowancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpTransportAllowanceTransaction_EmpTransportAllowanceProfi~",
                        column: x => x.TransportAllowanceProfileId,
                        principalSchema: "public",
                        principalTable: "EmpTransportAllowanceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpTransportAllowanceTransaction_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpAttendance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    WorkEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CheckInToleranceMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsLate = table.Column<bool>(type: "boolean", nullable: false),
                    LateMinutes = table.Column<int>(type: "integer", nullable: false),
                    AttendanceStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Present"),
                    CheckInLatitude = table.Column<double>(type: "double precision", nullable: false),
                    CheckInLongitude = table.Column<double>(type: "double precision", nullable: false),
                    CheckInAccuracyMeters = table.Column<double>(type: "double precision", nullable: true),
                    CheckInDistanceMeters = table.Column<double>(type: "double precision", nullable: false),
                    CheckOutLatitude = table.Column<double>(type: "double precision", nullable: true),
                    CheckOutLongitude = table.Column<double>(type: "double precision", nullable: true),
                    CheckOutAccuracyMeters = table.Column<double>(type: "double precision", nullable: true),
                    CheckOutDistanceMeters = table.Column<double>(type: "double precision", nullable: true),
                    WorkDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsGeofenceBypassed = table.Column<bool>(type: "boolean", nullable: false),
                    GeofenceBypassReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    UserType = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    CheckInSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Login"),
                    CheckOutSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "CheckedIn"),
                    CheckInIpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CheckOutIpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CheckInUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CheckOutUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_EmpAttendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpAttendance_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpAttendance_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmpAttendance_MstWorkSchedule_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalSchema: "public",
                        principalTable: "MstWorkSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserFingerprint_DoctorId",
                schema: "public",
                table: "AspNetUserFingerprint",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserFingerprint_EmployeeId",
                schema: "public",
                table: "AspNetUserFingerprint",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserFingerprint_TemplateHash",
                schema: "public",
                table: "AspNetUserFingerprint",
                column: "TemplateHash");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserFingerprint_UserId",
                schema: "public",
                table: "AspNetUserFingerprint",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserFingerprint_UserId_FingerPosition",
                schema: "public",
                table: "AspNetUserFingerprint",
                columns: new[] { "UserId", "FingerPosition" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserFingerprint_UserId_IsActive_IsDelete",
                schema: "public",
                table: "AspNetUserFingerprint",
                columns: new[] { "UserId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserFingerprint_UserId_IsPrimary",
                schema: "public",
                table: "AspNetUserFingerprint",
                columns: new[] { "UserId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserFingerprint_WorkforceProfileId",
                schema: "public",
                table: "AspNetUserFingerprint",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

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
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "UserId", "DepartmentId", "PositionId", "EffectiveStartDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

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

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsActive",
                table: "AspNetUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsFingerprintRegistrationEnabled",
                table: "AspNetUsers",
                column: "IsFingerprintRegistrationEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsGeolocationBypassEnabled",
                table: "AspNetUsers",
                column: "IsGeolocationBypassEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PrimaryDepartmentId_PrimaryPositionId",
                table: "AspNetUsers",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PrimaryPositionId",
                table: "AspNetUsers",
                column: "PrimaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserCode",
                table: "AspNetUsers",
                column: "UserCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserType",
                table: "AspNetUsers",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WorkforceProfileId",
                table: "AspNetUsers",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

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
                name: "IX_EmpAttendance_AttendanceDate",
                schema: "public",
                table: "EmpAttendance",
                column: "AttendanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_AttendanceStatus",
                schema: "public",
                table: "EmpAttendance",
                column: "AttendanceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_DoctorId",
                schema: "public",
                table: "EmpAttendance",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_EmployeeId",
                schema: "public",
                table: "EmpAttendance",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_IsGeofenceBypassed",
                schema: "public",
                table: "EmpAttendance",
                column: "IsGeofenceBypassed");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_IsLate",
                schema: "public",
                table: "EmpAttendance",
                column: "IsLate");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_Status",
                schema: "public",
                table: "EmpAttendance",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_UserId_AttendanceDate",
                schema: "public",
                table: "EmpAttendance",
                columns: new[] { "UserId", "AttendanceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_WorkScheduleId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkScheduleId");

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
                name: "IX_EmpOrganizationAssignment_PositionId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "PositionId");

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
                name: "IX_EmpTransportAllowancePolicy_AllowanceMode",
                schema: "public",
                table: "EmpTransportAllowancePolicy",
                column: "AllowanceMode");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowancePolicy_IsActive_IsDelete",
                schema: "public",
                table: "EmpTransportAllowancePolicy",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowancePolicy_PolicyCode",
                schema: "public",
                table: "EmpTransportAllowancePolicy",
                column: "PolicyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowancePolicy_PolicyName",
                schema: "public",
                table: "EmpTransportAllowancePolicy",
                column: "PolicyName");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceProfile_AllowanceMode",
                schema: "public",
                table: "EmpTransportAllowanceProfile",
                column: "AllowanceMode");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceProfile_EffectiveStartDate_EffectiveEn~",
                schema: "public",
                table: "EmpTransportAllowanceProfile",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceProfile_EmployeeId",
                schema: "public",
                table: "EmpTransportAllowanceProfile",
                column: "EmployeeId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceProfile_IsEligible_IsNightTransportEli~",
                schema: "public",
                table: "EmpTransportAllowanceProfile",
                columns: new[] { "IsEligible", "IsNightTransportEligible", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_AttendanceId",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_EmployeeId_PeriodYearMonth~",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                columns: new[] { "EmployeeId", "PeriodYearMonth", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_EmployeeId_TransactionDate~",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                columns: new[] { "EmployeeId", "TransactionDate", "AllowanceType", "IsNightShift", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_PeriodYearMonth_Transactio~",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                columns: new[] { "PeriodYearMonth", "TransactionStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_ShiftId",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_TransactionDate_AllowanceT~",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                columns: new[] { "TransactionDate", "AllowanceType", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_TransportAllowancePolicyId",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                column: "TransportAllowancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_TransportAllowanceProfileId",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                column: "TransportAllowanceProfileId");

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

            migrationBuilder.CreateIndex(
                name: "IX_MstCity_CityType",
                schema: "public",
                table: "MstCity",
                column: "CityType");

            migrationBuilder.CreateIndex(
                name: "IX_MstCity_IsActive_IsDelete",
                schema: "public",
                table: "MstCity",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCity_ProvinceId_CityCode",
                schema: "public",
                table: "MstCity",
                columns: new[] { "ProvinceId", "CityCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstCity_ProvinceId_CityName",
                schema: "public",
                table: "MstCity",
                columns: new[] { "ProvinceId", "CityName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCountry_CountryCode",
                schema: "public",
                table: "MstCountry",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstCountry_CountryName",
                schema: "public",
                table: "MstCountry",
                column: "CountryName");

            migrationBuilder.CreateIndex(
                name: "IX_MstCountry_IsActive_IsDelete",
                schema: "public",
                table: "MstCountry",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDepartment_DepartmentCode",
                schema: "public",
                table: "MstDepartment",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDepartment_DepartmentName",
                schema: "public",
                table: "MstDepartment",
                column: "DepartmentName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDistrict_CityId_DistrictCode",
                schema: "public",
                table: "MstDistrict",
                columns: new[] { "CityId", "DistrictCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDistrict_CityId_DistrictName",
                schema: "public",
                table: "MstDistrict",
                columns: new[] { "CityId", "DistrictName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDistrict_IsActive_IsDelete",
                schema: "public",
                table: "MstDistrict",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_DoctorCode",
                schema: "public",
                table: "MstDoctor",
                column: "DoctorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_DoctorType",
                schema: "public",
                table: "MstDoctor",
                column: "DoctorType");

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
                name: "IX_MstDoctor_IsActive",
                schema: "public",
                table: "MstDoctor",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_PrimaryDepartmentId_PrimaryPositionId",
                schema: "public",
                table: "MstDoctor",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_PrimaryPositionId",
                schema: "public",
                table: "MstDoctor",
                column: "PrimaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_WorkforceProfileId",
                schema: "public",
                table: "MstDoctor",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_BloodType",
                schema: "public",
                table: "MstEmployee",
                column: "BloodType");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_CityId",
                schema: "public",
                table: "MstEmployee",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_CountryId_ProvinceId_CityId_DistrictId_PostalCo~",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "CountryId", "ProvinceId", "CityId", "DistrictId", "PostalCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_DistrictId",
                schema: "public",
                table: "MstEmployee",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_Email",
                schema: "public",
                table: "MstEmployee",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeCode",
                schema: "public",
                table: "MstEmployee",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee",
                column: "EmployeeNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeStatus_ProfessionType_IsActive_IsDelete",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "EmployeeStatus", "ProfessionType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_FullName",
                schema: "public",
                table: "MstEmployee",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_IdentityNumber",
                schema: "public",
                table: "MstEmployee",
                column: "IdentityNumber",
                unique: true,
                filter: "\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_IsActive",
                schema: "public",
                table: "MstEmployee",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_MaritalStatus",
                schema: "public",
                table: "MstEmployee",
                column: "MaritalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_PhoneNumber",
                schema: "public",
                table: "MstEmployee",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_PostalCodeId",
                schema: "public",
                table: "MstEmployee",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_PrimaryDepartmentId_PrimaryPositionId",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_PrimaryDepartmentId_PrimaryPositionId_IsActive_~",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_PrimaryPositionId",
                schema: "public",
                table: "MstEmployee",
                column: "PrimaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_ProvinceId",
                schema: "public",
                table: "MstEmployee",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_Religion",
                schema: "public",
                table: "MstEmployee",
                column: "Religion");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_WhatsAppNumber",
                schema: "public",
                table: "MstEmployee",
                column: "WhatsAppNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_CompanyCode",
                schema: "public",
                table: "MstExternalUser",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_CompanyName",
                schema: "public",
                table: "MstExternalUser",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_Email",
                schema: "public",
                table: "MstExternalUser",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_ExternalCode",
                schema: "public",
                table: "MstExternalUser",
                column: "ExternalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_ExternalUserType",
                schema: "public",
                table: "MstExternalUser",
                column: "ExternalUserType");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_FullName",
                schema: "public",
                table: "MstExternalUser",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_IsActive",
                schema: "public",
                table: "MstExternalUser",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_WorkforceProfileId",
                schema: "public",
                table: "MstExternalUser",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstPosition_DepartmentId_PositionCode",
                schema: "public",
                table: "MstPosition",
                columns: new[] { "DepartmentId", "PositionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPosition_DepartmentId_PositionName",
                schema: "public",
                table: "MstPosition",
                columns: new[] { "DepartmentId", "PositionName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPostalCode_DistrictId_PostalCode",
                schema: "public",
                table: "MstPostalCode",
                columns: new[] { "DistrictId", "PostalCode" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPostalCode_DistrictId_VillageName",
                schema: "public",
                table: "MstPostalCode",
                columns: new[] { "DistrictId", "VillageName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPostalCode_IsActive_IsDelete",
                schema: "public",
                table: "MstPostalCode",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPostalCode_PostalCode",
                schema: "public",
                table: "MstPostalCode",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstProvince_CountryId_ProvinceCode",
                schema: "public",
                table: "MstProvince",
                columns: new[] { "CountryId", "ProvinceCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstProvince_CountryId_ProvinceName",
                schema: "public",
                table: "MstProvince",
                columns: new[] { "CountryId", "ProvinceName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstProvince_IsActive_IsDelete",
                schema: "public",
                table: "MstProvince",
                columns: new[] { "IsActive", "IsDelete" });

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
                name: "IX_MstWorkSchedule_DepartmentId_PositionId",
                schema: "public",
                table: "MstWorkSchedule",
                columns: new[] { "DepartmentId", "PositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_IsActive",
                schema: "public",
                table: "MstWorkSchedule",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_IsDefault",
                schema: "public",
                table: "MstWorkSchedule",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_PositionId",
                schema: "public",
                table: "MstWorkSchedule",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_ScheduleCode",
                schema: "public",
                table: "MstWorkSchedule",
                column: "ScheduleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_UserId",
                schema: "public",
                table: "MstWorkSchedule",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_UserType",
                schema: "public",
                table: "MstWorkSchedule",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_ActionAccessId",
                schema: "public",
                table: "SysAccessPolicy",
                column: "ActionAccessId");

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_ControllerAccessId_ActionAccessId_IsAllowed~",
                schema: "public",
                table: "SysAccessPolicy",
                columns: new[] { "ControllerAccessId", "ActionAccessId", "IsAllowed", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_DepartmentId_PositionId_ControllerAccessId_~",
                schema: "public",
                table: "SysAccessPolicy",
                columns: new[] { "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_DepartmentId_PositionId_IsAllowed_IsActive_~",
                schema: "public",
                table: "SysAccessPolicy",
                columns: new[] { "DepartmentId", "PositionId", "IsAllowed", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_PositionId",
                schema: "public",
                table: "SysAccessPolicy",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_AccessType",
                schema: "public",
                table: "SysActionAccess",
                column: "AccessType");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_ActionName",
                schema: "public",
                table: "SysActionAccess",
                column: "ActionName");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_ControllerAccessId_ActionName",
                schema: "public",
                table: "SysActionAccess",
                columns: new[] { "ControllerAccessId", "ActionName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_IsActive",
                schema: "public",
                table: "SysActionAccess",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_IsSystemOnly",
                schema: "public",
                table: "SysActionAccess",
                column: "IsSystemOnly");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_VisibleInRoleAccess",
                schema: "public",
                table: "SysActionAccess",
                column: "VisibleInRoleAccess");

            migrationBuilder.CreateIndex(
                name: "IX_SysApplicationModule_IsActive",
                schema: "public",
                table: "SysApplicationModule",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SysApplicationModule_ModuleCode",
                schema: "public",
                table: "SysApplicationModule",
                column: "ModuleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_ApiVersion",
                schema: "public",
                table: "SysAppVersion",
                column: "ApiVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_BackendVersion",
                schema: "public",
                table: "SysAppVersion",
                column: "BackendVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_IsActive",
                schema: "public",
                table: "SysAppVersion",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_IsLatest",
                schema: "public",
                table: "SysAppVersion",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_ControllerName",
                schema: "public",
                table: "SysControllerAccess",
                column: "ControllerName");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_IsActive",
                schema: "public",
                table: "SysControllerAccess",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_IsSystemOnly",
                schema: "public",
                table: "SysControllerAccess",
                column: "IsSystemOnly");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_ModuleId_ControllerName",
                schema: "public",
                table: "SysControllerAccess",
                columns: new[] { "ModuleId", "ControllerName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_VisibleInRoleAccess",
                schema: "public",
                table: "SysControllerAccess",
                column: "VisibleInRoleAccess");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserFingerprint",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserOrganization",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

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
                name: "EmpAttendance",
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
                name: "EmpOrganizationAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpPayrollProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTaxProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTransportAllowanceTransaction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExtUserContract",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExtUserDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstWorkforceRequirement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SysAccessPolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SysAppVersion",
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
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "MstWorkSchedule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTransportAllowancePolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTransportAllowanceProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SysActionAccess",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "SysControllerAccess",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDoctor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstEmployee",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstExternalUser",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SysApplicationModule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPostalCode",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstWorkforceProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDistrict",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPosition",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCity",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDepartment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstProvince",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCountry",
                schema: "public");
        }
    }
}
