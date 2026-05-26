using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddPatientManagementMasterData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstMembershipTier",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TierCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TierName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TierType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CardTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CardColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CardImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSelectableInKiosk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSelectableInAdmission = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsManagedByMarketingOnly = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RegistrationDiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    ConsultationDiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    ProcedureDiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    LaboratoryDiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    RadiologyDiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    PharmacyDiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    PriorityQueue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FreeAnnualCheckup = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FreeParking = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ValidityMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 12),
                    MinimumSpendAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BenefitDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstMembershipTier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstPatient",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MedicalRecordNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PatientStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RegistrationSource = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NickName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BirthPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    Religion = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaritalStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BloodType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IdentityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdentityNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostalCodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PhotoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsMember = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DefaultMembershipTierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivePatientMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsNewborn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MotherPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    BirthOrder = table.Column<int>(type: "integer", nullable: true),
                    BirthWeightGram = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BirthLengthCm = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BirthTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DeliveryMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeceased = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeceasedDate = table.Column<DateTime>(type: "date", nullable: true),
                    MergedToPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    MergeReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstPatient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPatient_MstCity_CityId",
                        column: x => x.CityId,
                        principalSchema: "public",
                        principalTable: "MstCity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatient_MstCountry_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "public",
                        principalTable: "MstCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatient_MstDistrict_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "public",
                        principalTable: "MstDistrict",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatient_MstMembershipTier_DefaultMembershipTierId",
                        column: x => x.DefaultMembershipTierId,
                        principalSchema: "public",
                        principalTable: "MstMembershipTier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatient_MstPatient_MergedToPatientId",
                        column: x => x.MergedToPatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatient_MstPatient_MotherPatientId",
                        column: x => x.MotherPatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatient_MstPostalCode_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalSchema: "public",
                        principalTable: "MstPostalCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatient_MstProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalSchema: "public",
                        principalTable: "MstProvince",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPatientEmergencyContact",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdentityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdentityNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsResponsiblePerson = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSameAddressAsPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstPatientEmergencyContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPatientEmergencyContact_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPatientIdentityDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdentityNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssuedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "date", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsFromKioskScan = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstPatientIdentityDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPatientIdentityDocument_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPatientMembership",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipTierId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MembershipStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    JoinDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiredDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAutoCreated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCreatedFromKiosk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCreatedFromAdmission = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCreatedByMarketing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PointBalance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalSpendAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    LastUpgradeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastDowngradeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpgradeDowngradeReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstPatientMembership", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPatientMembership_MstMembershipTier_MembershipTierId",
                        column: x => x.MembershipTierId,
                        principalSchema: "public",
                        principalTable: "MstMembershipTier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatientMembership_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPatientRelationship",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelationshipType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RelatedPersonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RelatedPersonIdentityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RelatedPersonIdentityNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedPersonPhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RelatedPersonWhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RelatedPersonEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RelatedPersonAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEmergencyContact = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsResponsiblePerson = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLegalGuardian = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstPatientRelationship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPatientRelationship_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatientRelationship_MstPatient_RelatedPatientId",
                        column: x => x.RelatedPatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstMembershipTier_IsDefault_IsActive_IsDelete",
                schema: "public",
                table: "MstMembershipTier",
                columns: new[] { "IsDefault", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstMembershipTier_IsSelectableInKiosk_IsSelectableInAdmissi~",
                schema: "public",
                table: "MstMembershipTier",
                columns: new[] { "IsSelectableInKiosk", "IsSelectableInAdmission", "IsManagedByMarketingOnly", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstMembershipTier_TierCode",
                schema: "public",
                table: "MstMembershipTier",
                column: "TierCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstMembershipTier_TierName",
                schema: "public",
                table: "MstMembershipTier",
                column: "TierName");

            migrationBuilder.CreateIndex(
                name: "IX_MstMembershipTier_TierType",
                schema: "public",
                table: "MstMembershipTier",
                column: "TierType");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_ActivePatientMembershipId",
                schema: "public",
                table: "MstPatient",
                column: "ActivePatientMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_CityId",
                schema: "public",
                table: "MstPatient",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_CountryId_ProvinceId_CityId_DistrictId_PostalCod~",
                schema: "public",
                table: "MstPatient",
                columns: new[] { "CountryId", "ProvinceId", "CityId", "DistrictId", "PostalCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_DefaultMembershipTierId",
                schema: "public",
                table: "MstPatient",
                column: "DefaultMembershipTierId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_DistrictId",
                schema: "public",
                table: "MstPatient",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_Email",
                schema: "public",
                table: "MstPatient",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_FullName",
                schema: "public",
                table: "MstPatient",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_FullName_BirthDate_Gender",
                schema: "public",
                table: "MstPatient",
                columns: new[] { "FullName", "BirthDate", "Gender" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_IdentityNumber",
                schema: "public",
                table: "MstPatient",
                column: "IdentityNumber",
                unique: true,
                filter: "\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_IsMember_IsNewborn_IsActive_IsDelete",
                schema: "public",
                table: "MstPatient",
                columns: new[] { "IsMember", "IsNewborn", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_MedicalRecordNumber",
                schema: "public",
                table: "MstPatient",
                column: "MedicalRecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_MergedToPatientId",
                schema: "public",
                table: "MstPatient",
                column: "MergedToPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_MotherPatientId",
                schema: "public",
                table: "MstPatient",
                column: "MotherPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_PatientCode",
                schema: "public",
                table: "MstPatient",
                column: "PatientCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_PatientType_PatientStatus_IsActive_IsDelete",
                schema: "public",
                table: "MstPatient",
                columns: new[] { "PatientType", "PatientStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_PhoneNumber",
                schema: "public",
                table: "MstPatient",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_PostalCodeId",
                schema: "public",
                table: "MstPatient",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_ProvinceId",
                schema: "public",
                table: "MstPatient",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_WhatsAppNumber",
                schema: "public",
                table: "MstPatient",
                column: "WhatsAppNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientEmergencyContact_ContactName",
                schema: "public",
                table: "MstPatientEmergencyContact",
                column: "ContactName");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientEmergencyContact_IdentityNumber",
                schema: "public",
                table: "MstPatientEmergencyContact",
                column: "IdentityNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientEmergencyContact_PatientId",
                schema: "public",
                table: "MstPatientEmergencyContact",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientEmergencyContact_PatientId_IsPrimary",
                schema: "public",
                table: "MstPatientEmergencyContact",
                columns: new[] { "PatientId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientEmergencyContact_PatientId_IsResponsiblePerson_Is~",
                schema: "public",
                table: "MstPatientEmergencyContact",
                columns: new[] { "PatientId", "IsResponsiblePerson", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientEmergencyContact_PhoneNumber",
                schema: "public",
                table: "MstPatientEmergencyContact",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientEmergencyContact_WhatsAppNumber",
                schema: "public",
                table: "MstPatientEmergencyContact",
                column: "WhatsAppNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientIdentityDocument_IdentityNumber",
                schema: "public",
                table: "MstPatientIdentityDocument",
                column: "IdentityNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientIdentityDocument_IsFromKioskScan_IsActive_IsDelete",
                schema: "public",
                table: "MstPatientIdentityDocument",
                columns: new[] { "IsFromKioskScan", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientIdentityDocument_PatientId",
                schema: "public",
                table: "MstPatientIdentityDocument",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientIdentityDocument_PatientId_IdentityType_IdentityN~",
                schema: "public",
                table: "MstPatientIdentityDocument",
                columns: new[] { "PatientId", "IdentityType", "IdentityNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientIdentityDocument_PatientId_IsPrimary",
                schema: "public",
                table: "MstPatientIdentityDocument",
                columns: new[] { "PatientId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientIdentityDocument_PatientId_IsVerified_IsActive_Is~",
                schema: "public",
                table: "MstPatientIdentityDocument",
                columns: new[] { "PatientId", "IsVerified", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_MemberNumber",
                schema: "public",
                table: "MstPatientMembership",
                column: "MemberNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_MembershipTierId",
                schema: "public",
                table: "MstPatientMembership",
                column: "MembershipTierId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_MembershipTierId_MembershipStatus_IsAc~",
                schema: "public",
                table: "MstPatientMembership",
                columns: new[] { "MembershipTierId", "MembershipStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_PatientId",
                schema: "public",
                table: "MstPatientMembership",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_PatientId_IsPrimary",
                schema: "public",
                table: "MstPatientMembership",
                columns: new[] { "PatientId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_PatientId_MembershipStatus_IsActive_Is~",
                schema: "public",
                table: "MstPatientMembership",
                columns: new[] { "PatientId", "MembershipStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientRelationship_PatientId",
                schema: "public",
                table: "MstPatientRelationship",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientRelationship_PatientId_IsEmergencyContact_IsRespo~",
                schema: "public",
                table: "MstPatientRelationship",
                columns: new[] { "PatientId", "IsEmergencyContact", "IsResponsiblePerson", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientRelationship_PatientId_IsPrimary",
                schema: "public",
                table: "MstPatientRelationship",
                columns: new[] { "PatientId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientRelationship_PatientId_RelationshipType_IsActive_~",
                schema: "public",
                table: "MstPatientRelationship",
                columns: new[] { "PatientId", "RelationshipType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientRelationship_RelatedPatientId",
                schema: "public",
                table: "MstPatientRelationship",
                column: "RelatedPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientRelationship_RelatedPersonIdentityNumber",
                schema: "public",
                table: "MstPatientRelationship",
                column: "RelatedPersonIdentityNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientRelationship_RelationshipType",
                schema: "public",
                table: "MstPatientRelationship",
                column: "RelationshipType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPatientEmergencyContact",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPatientIdentityDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPatientMembership",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPatientRelationship",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPatient",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMembershipTier",
                schema: "public");
        }
    }
}
